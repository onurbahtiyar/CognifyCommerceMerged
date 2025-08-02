using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfProductReviewDal : EfEntityRepositoryBase<ProductReview, ContextDb>, IProductReviewDal
    {
        public EfProductReviewDal(ContextDb _context) : base(_context)
        {
        }
    }
}