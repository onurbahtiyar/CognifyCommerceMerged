
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfShipperDal: EfEntityRepositoryBase<Shipper, ContextDb>, IShipperDal
{
    public EfShipperDal(ContextDb context) : base(context)
    {
    }
}
