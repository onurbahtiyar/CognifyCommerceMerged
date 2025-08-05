using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DashboardDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public SummaryStatsDto SummaryStats { get; set; }
        public ActionItemsDto ActionItems { get; set; }
        public SalesOverviewDto SalesOverview { get; set; }
        public ProductInsightsDto ProductInsights { get; set; }
        public FinancialsDto Financials { get; set; }
        public CustomerActivityDto CustomerActivity { get; set; }
    }
}
