using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ActionItemsDto
    {
        public CountMetricDto OrdersToShip { get; set; }
        public CountMetricDto LowStockItems { get; set; }
        public CountMetricDto PendingReviews { get; set; }
    }

    public class CountMetricDto
    {
        public int Count { get; set; }
        public string Label { get; set; }
        public string Details { get; set; }
    }
}
