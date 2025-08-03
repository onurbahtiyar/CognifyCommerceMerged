using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CustomerActivityDto
    {
        public List<RecentCustomerDto> RecentCustomers { get; set; }
        public RatingDto OverallProductRating { get; set; }
    }

    public class RecentCustomerDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class RatingDto
    {
        public double Average { get; set; }
        public int TotalReviews { get; set; }
    }
}
