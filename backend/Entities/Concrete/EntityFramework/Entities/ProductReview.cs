using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ProductReview: IEntity
    {
        public int ProductReviewId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int? ParentReviewId { get; set; } // Yanıt ise, ana yorumun ID'si
        public int Rating { get; set; } // 1-5 arası puan
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; } // Admin onayı
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ProductReview ParentReview { get; set; }
        public virtual ICollection<ProductReview> Replies { get; set; } // Bu yoruma verilen yanıtlar

        public ProductReview()
        {
            Replies = new HashSet<ProductReview>();
        }
    }
}
