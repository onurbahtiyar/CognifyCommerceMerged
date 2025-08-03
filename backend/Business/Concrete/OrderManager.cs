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
        private readonly IOrderItemDal _orderItemDal;

        public OrderManager(IOrderDal orderDal, IProductDal productDal, ICustomerDal customerDal, IOrderItemDal orderItemDal)
        {
            _orderDal = orderDal;
            _productDal = productDal;
            _customerDal = customerDal;
            _orderItemDal = orderItemDal;
        }

        public IDataResult<OrderDto> Add(OrderAddDto dto)
        {
            var customer = _customerDal.Get(c => c.CustomerId == dto.CustomerId && c.IsActive);
            if (customer == null) return new ErrorDataResult<OrderDto>(null, "Geçersiz müşteri.");

            if (dto.Items == null || !dto.Items.Any()) return new ErrorDataResult<OrderDto>(null, "Sipariş için en az bir ürün eklenmelidir.");

            var newOrder = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.Now,
                OrderStatus = (int)OrderStatus.PendingConfirmation,
                ShippingAddress = dto.ShippingAddress,
                TotalAmount = 0
            };

            _orderDal.Add(newOrder);

            decimal totalAmount = 0;
            var addedItems = new List<OrderItem>();

            try
            {
                foreach (var itemDto in dto.Items)
                {
                    var product = _productDal.Get(p => p.ProductId == itemDto.ProductId && p.IsActive);

                    if (product == null) throw new InvalidOperationException($"ID:{itemDto.ProductId} olan ürün bulunamadı.");
                    if (product.Stock < itemDto.Quantity) throw new InvalidOperationException($"'{product.Name}' ürünü için yeterli stok yok (Kalan Stok: {product.Stock}).");

                    var orderItem = new OrderItem
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = product.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price
                    };

                    _orderItemDal.Add(orderItem);
                    addedItems.Add(orderItem);

                    product.Stock -= itemDto.Quantity;
                    _productDal.Update(product);

                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;
                }

                newOrder.TotalAmount = totalAmount;
                _orderDal.Update(newOrder);
            }
            catch (Exception ex)
            {
                _orderDal.Delete(newOrder);
                return new ErrorDataResult<OrderDto>(null, $"Sipariş oluşturulurken bir hata oluştu ve işlem geri alındı: {ex.Message}");
            }

            var resultDto = MapToOrderDto(newOrder, addedItems);
            return new SuccessDataResult<OrderDto>(resultDto, "Sipariş başarıyla oluşturuldu.");
        }

        private OrderDto MapToOrderDto(Order order, List<OrderItem> items)
        {
            var customer = _customerDal.Get(c => c.CustomerId == order.CustomerId);
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerFullName = $"{customer?.FirstName} {customer?.LastName}",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = ((OrderStatus)order.OrderStatus).ToString(),
                OrderStatusCode = order.OrderStatus,
                ShippingAddress = order.ShippingAddress,
                Items = items.Select(oi =>
                {
                    var product = _productDal.GetNoTracking(p => p.ProductId == oi.ProductId);
                    return new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = product?.Name ?? "Ürün Silinmiş",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    };
                }).ToList()
            };
        }
        public IDataResult<List<OrderDto>> GetAll()
        {
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
            var order = _orderDal.Get(
                o => o.OrderId == orderId,
                o => o.Customer,
                o => o.Shipper,
                o => o.OrderItems
            );

            if (order == null) return new ErrorDataResult<OrderDto>(null, "Sipariş bulunamadı.");

            return new SuccessDataResult<OrderDto>(MapToOrderDto(order), "Sipariş detayı getirildi.");
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerFullName = $"{order.Customer?.FirstName} {order.Customer?.LastName}",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = ((OrderStatus)order.OrderStatus).ToString(),
                OrderStatusCode = order.OrderStatus,
                ShipperName = order.Shipper?.CompanyName,
                ShippingTrackingCode = order.ShippingTrackingCode,
                ShippingAddress = order.ShippingAddress,
                Items = order.OrderItems.Select(oi => {
                    var product = _productDal.GetNoTracking(p => p.ProductId == oi.ProductId);
                    return new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = product?.Name ?? "Ürün Silinmiş",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    };
                }).ToList()
            };
        }

        public IResult UpdateShippingInfo(OrderUpdateShippingDto dto)
        {
            var order = _orderDal.Get(o => o.OrderId == dto.OrderId);
            if (order == null) return new Result(false, "Sipariş bulunamadı.");

            order.ShipperId = dto.ShipperId;
            order.ShippingTrackingCode = dto.ShippingTrackingCode;
            order.OrderStatus = (int)OrderStatus.Shipped;

            _orderDal.Update(order);
            return new SuccessResult("Kargo bilgileri güncellendi ve sipariş durumu 'Kargoya Verildi' olarak ayarlandı.");
        }

        public IResult UpdateStatus(OrderUpdateStatusDto dto)
        {
            var order = _orderDal.Get(o => o.OrderId == dto.OrderId);
            if (order == null) return new Result(false, "Sipariş bulunamadı.");

            order.OrderStatus = dto.NewStatus;
            _orderDal.Update(order);
            return new SuccessResult("Sipariş durumu güncellendi.");
        }

        public IResult CancelOrder(int orderId)
        {
            var order = _orderDal.Get(o => o.OrderId == orderId, o => o.OrderItems);
            if (order == null) return new Result(false, "Sipariş bulunamadı.");

            if (order.OrderStatus == (int)OrderStatus.Delivered || order.OrderStatus == (int)OrderStatus.Shipped)
            {
                return new Result(false, "Kargoya verilmiş veya teslim edilmiş bir sipariş iptal edilemez. İade süreci başlatılmalıdır.");
            }

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