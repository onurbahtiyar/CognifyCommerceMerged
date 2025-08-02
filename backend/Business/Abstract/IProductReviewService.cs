using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IProductReviewService
    {
        IDataResult<List<ProductReviewDto>> GetByProductId(int productId);
        IDataResult<List<ProductReviewDto>> GetUnapprovedReviews();
        IResult Add(ProductReviewAddDto dto);
        IResult AddAdminReply(AdminReplyAddDto dto);
        IResult ApproveReview(int reviewId);
        IResult Delete(int reviewId);
        IDataResult<List<ProductReviewDto>> GetAll();
    }
}
