using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SummaryStatsDto
    {
        public MetricDto<decimal> TotalRevenue { get; set; }
        public MetricDto<decimal> TotalExpenses { get; set; }
        public MetricDto<decimal> NetProfit { get; set; }
        public MetricDto<int> TotalOrders { get; set; }
        public MetricDto<int> TotalCustomers { get; set; }
        public MetricDto<int> TotalProducts { get; set; }
    }

    public class MetricDto<T>
    {
        public T Value { get; set; }
        public string Label { get; set; }
    }
}
