using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SalesOverviewDto
    {
        public string Period { get; set; }
        public List<ChartDataPoint<DateTime, decimal>> RevenueChartData { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; }
        public List<ChartDataPoint<string, int>> OrderStatusDistribution { get; set; }
    }

    public class ChartDataPoint<TX, TY>
    {
        public TX X { get; set; }
        public TY Y { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
