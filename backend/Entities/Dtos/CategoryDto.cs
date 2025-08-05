using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CategoryDto:IDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
