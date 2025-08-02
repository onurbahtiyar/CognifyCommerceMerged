using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class OrderManager : IOrderService
    {
        private readonly IOrderDal _orderDal;
        private readonly IProductDal _productDal;
        private readonly ICustomerDal _customerDal;

        public OrderManager(IOrderDal orderDal, IProductDal productDal, ICustomerDal customerDal)
        {
            _orderDal = orderDal;
            _productDal = productDal;
            _customerDal = customerDal;
        }

        public IDataResult<OrderDto> Add(OrderAddDto dto)
        {
            // İş Kuralları
            var customer = _customerDal.Get(c => c.CustomerId == dto.CustomerId && c.IsActive);
            if (customer == null) return new ErrorDataResult<OrderDto>(null, "Geçersiz müşteri.");

            if (dto.Items == null || !dto.Items.Any()) return new ErrorDataResult<OrderDto>(null, "Sipariş için en az bir ürün eklenmelidir.");

            var newOrder = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.Now,
                OrderStatus = (int)OrderStatus.PendingConfirmation,
                ShippingAddress = dto.ShippingAddress,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // Bu döngüdeki her bir veritabanı işlemi (stok güncelleme)
            // SaveChanges çağrıldığında tek bir transaction olarak çalışır.
            foreach (var itemDto in dto.Items)
            {
                var product = _productDal.Get(p => p.ProductId == itemDto.ProductId && p.IsActive);
                if (product == null) return new ErrorDataResult<OrderDto>(null, $"ID:{itemDto.ProductId} olan ürün bulunamadı.");
                if (product.Stock < itemDto.Quantity) return new ErrorDataResult<OrderDto>(null, $"'{product.Name}' ürünü için yeterli stok yok (Kalan Stok: {product.Stock}).");

                var orderItem = new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price // Sipariş anındaki fiyatı kaydet
                };

                newOrder.OrderItems.Add(orderItem);
                totalAmount += itemDto.Quantity * product.Price;

                // Stok Düşürme
                product.Stock -= itemDto.Quantity;
                _productDal.Update(product);
            }

            newOrder.TotalAmount = totalAmount;
            _orderDal.Add(newOrder);

            // Başarılı olursa, oluşturulan siparişin detayını geri dön
            return GetById(newOrder.OrderId);
        }

        public IDataResult<List<OrderDto>> GetAll()
        {
            // DÜZELTME: String tabanlı include yerine Expression tabanlı include kullanıldı.
            // Not: OrderItems.Product gibi iç içe bir ilişki bu yapıyla doğrudan çekilemez.
            // Bu yüzden MapToOrderDto içinde manuel olarak doldurulur.
            var orders = _orderDal.GetList(
                null,
                o => o.Customer,
                o => o.Shipper,
                o => o.OrderItems
            );

            var orderDtos = orders.Select(o => MapToOrderDto(o)).ToList();
            return new SuccessDataResult<List<OrderDto>>(orderDtos, "Siparişler listelendi.");
        }

        public IDataResult<OrderDto> GetById(int orderId)
        {
            // DÜZELTME: String tabanlı include yerine Expression tabanlı include kullanıldı.
            var order = _orderDal.Get(
                o => o.OrderId == orderId,
                o => o.Customer,
                o => o.Shipper,
                o => o.OrderItems
            );

            if (order == null) return new ErrorDataResult<OrderDto>(null, "Sipariş bulunamadı.");

            return new SuccessDataResult<OrderDto>(MapToOrderDto(order), "Sipariş detayı getirildi.");
        }

        // DÜZELTME: Bu yardımcı metot, iç içe olan Product verisini manuel olarak çeker.
        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerFullName = $"{order.Customer?.FirstName} {order.Customer?.LastName}", // Null kontrolü eklendi.
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = ((OrderStatus)order.OrderStatus).ToString(),
                OrderStatusCode = order.OrderStatus,
                ShipperName = order.Shipper?.CompanyName,
                ShippingTrackingCode = order.ShippingTrackingCode,
                ShippingAddress = order.ShippingAddress,
                Items = order.OrderItems.Select(oi => {
                    // Her bir sipariş kalemi için ürün bilgisini veritabanından çekiyoruz.
                    // Bu, IEntityRepository'nin iç içe include (ThenInclude) sınırlamasını aşmamızı sağlar.
                    var product = _productDal.GetNoTracking(p => p.ProductId == oi.ProductId);
                    return new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = product?.Name ?? "Ürün Silinmiş", // Ürün silinmişse null gelmesini engelle.
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    };
                }).ToList()
            };
        }

        public IResult UpdateShippingInfo(OrderUpdateShippingDto dto)
        {
            var order = _orderDal.Get(o => o.OrderId == dto.OrderId);
            if (order == null) return new Result(false, "Sipariş bulunamadı."); // IResult uyumu için ErrorResult kullanıldı.

            order.ShipperId = dto.ShipperId;
            order.ShippingTrackingCode = dto.ShippingTrackingCode;
            order.OrderStatus = (int)OrderStatus.Shipped; // Kargo bilgisi girilince durumu otomatik "Kargoda" yap

            _orderDal.Update(order);
            return new SuccessResult("Kargo bilgileri güncellendi ve sipariş durumu 'Kargoya Verildi' olarak ayarlandı.");
        }

        public IResult UpdateStatus(OrderUpdateStatusDto dto)
        {
            var order = _orderDal.Get(o => o.OrderId == dto.OrderId);
            if (order == null) return new Result(false, "Sipariş bulunamadı."); // IResult uyumu için ErrorResult kullanıldı.

            // İade durumunda stok güncellemesi gibi ek kurallar eklenebilir.
            order.OrderStatus = dto.NewStatus;
            _orderDal.Update(order);
            return new SuccessResult("Sipariş durumu güncellendi.");
        }

        public IResult CancelOrder(int orderId)
        {
            // DÜZELTME: String tabanlı include yerine Expression tabanlı include kullanıldı.
            var order = _orderDal.Get(o => o.OrderId == orderId, o => o.OrderItems);
            if (order == null) return new Result(false, "Sipariş bulunamadı."); // IResult uyumu için ErrorResult kullanıldı.

            // Sadece belirli durumlardaki siparişler iptal edilebilir.
            if (order.OrderStatus == (int)OrderStatus.Delivered || order.OrderStatus == (int)OrderStatus.Shipped)
            {
                return new Result(false, "Kargoya verilmiş veya teslim edilmiş bir sipariş iptal edilemez. İade süreci başlatılmalıdır.");
            }

            // Stokları iade et
            foreach (var item in order.OrderItems)
            {
                var product = _productDal.Get(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _productDal.Update(product);
                }
            }

            order.OrderStatus = (int)OrderStatus.Cancelled;
            _orderDal.Update(order);

            return new SuccessResult("Sipariş iptal edildi ve ürün stokları iade edildi.");
        }
    }
}