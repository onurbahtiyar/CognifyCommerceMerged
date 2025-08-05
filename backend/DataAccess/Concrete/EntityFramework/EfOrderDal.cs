
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfOrderDal: EfEntityRepositoryBase<Order, ContextDb>, IOrderDal
{
    public EfOrderDal(ContextDb context) : base(context)
    {
    }
}
