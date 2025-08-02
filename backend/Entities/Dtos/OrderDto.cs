using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OrderDto : IDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } // Enum'ın string karşılığı
        public int OrderStatusCode { get; set; } // Enum'ın int değeri
        public string? ShipperName { get; set; }
        public string? ShippingTrackingCode { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
