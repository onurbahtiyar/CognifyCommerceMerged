using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ExpenseCategory : IEntity
    {
        public ExpenseCategory()
        {
            Expenses = new HashSet<Expense>();
        }

        [Key]
        public int ExpenseCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public virtual ICollection<Expense> Expenses { get; set; }
    }
}
