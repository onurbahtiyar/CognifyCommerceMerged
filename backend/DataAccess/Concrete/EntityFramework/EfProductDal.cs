using Core.DataAccess.EntityFramework;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfProductDal : EfEntityRepositoryBase<Product, ContextDb>, IProductDal
    {
        public EfProductDal(ContextDb _context) : base(_context)
        {
        }
    }
}