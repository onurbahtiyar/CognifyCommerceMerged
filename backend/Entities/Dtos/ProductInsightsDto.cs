using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ProductInsightsDto
    {
        public List<BestSellingProductDto> BestSellingProducts { get; set; }
        public List<LowStockProductDto> LowStockProducts { get; set; }
        public List<ChartDataPoint<string, int>> CategoryDistribution { get; set; }
    }

    public class BestSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitsSold { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int StockLevel { get; set; }
    }
}
