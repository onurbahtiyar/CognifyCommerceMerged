using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FinancialsDto
    {
        public List<ChartDataPoint<string, decimal>> ExpenseBreakdown { get; set; }
        public List<RecentExpenseDto> RecentExpenses { get; set; }
    }

    public class RecentExpenseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
    }
}
