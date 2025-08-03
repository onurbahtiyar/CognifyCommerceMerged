using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IScheduledJobService
    {
        Task CheckForSupplyAlertsAsync();
        Task CheckForUnsoldProductsAsync();
        Task CheckForBadReviewClustersAsync();
    }
}
