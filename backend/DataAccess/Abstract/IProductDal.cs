using Core.DataAccess;
using Entities.Concrete.EntityFramework.Entities;

public interface IProductDal : IEntityRepository<Product>
{
}