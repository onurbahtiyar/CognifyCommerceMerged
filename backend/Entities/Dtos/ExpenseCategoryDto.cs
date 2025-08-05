using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ExpenseCategoryDto
    {
        public int ExpenseCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
