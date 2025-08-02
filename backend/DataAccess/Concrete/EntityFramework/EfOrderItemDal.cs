
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfOrderItemDal: EfEntityRepositoryBase<OrderItem, ContextDb>, IOrderItemDal
{
    public EfOrderItemDal(ContextDb context) : base(context)
    {
    }
}
