using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OrderUpdateStatusDto : IDto
    {
        public int OrderId { get; set; }
        public int NewStatus { get; set; } // OrderStatus enum'ının int değeri
    }
}
