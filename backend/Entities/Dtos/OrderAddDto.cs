using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OrderAddDto : IDto
    {
        public int CustomerId { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderAddItemDto> Items { get; set; }
    }

    public class OrderAddItemDto : IDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
