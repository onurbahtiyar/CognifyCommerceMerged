using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OrderUpdateShippingDto : IDto
    {
        public int OrderId { get; set; }
        public int ShipperId { get; set; }
        public string ShippingTrackingCode { get; set; }
    }
}
