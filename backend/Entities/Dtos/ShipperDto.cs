using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ShipperDto : IDto
    {
        public int ShipperId { get; set; }
        public string CompanyName { get; set; }
    }
}
