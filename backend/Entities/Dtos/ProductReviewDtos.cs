using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ProductReviewDto
    {
        public int ReviewId { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; }

        public string ProductName { get; set; }
        public List<ProductReviewDto> Replies { get; set; }

        public ProductReviewDto()
        {
            Replies = new List<ProductReviewDto>();
        }
    }

    // Yeni bir yorum eklemek için kullanılacak DTO
    public class ProductReviewAddDto
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    // Admin'in bir yoruma yanıt vermesi için kullanılacak DTO
    public class AdminReplyAddDto
    {
        public int ParentReviewId { get; set; } // Hangi yoruma yanıt veriliyor
        public int CustomerId { get; set; } // Yanıtı veren admin'in Customer Id'si (veya User Id)
        public string Comment { get; set; }
    }
}
