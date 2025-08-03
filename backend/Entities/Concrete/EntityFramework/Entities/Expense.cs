using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class Expense: IEntity
    {
        [Key]
        public int ExpenseId { get; set; }

        public int ExpenseCategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Notes { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("ExpenseCategoryId")]
        public virtual ExpenseCategory ExpenseCategory { get; set; }
    }
}
