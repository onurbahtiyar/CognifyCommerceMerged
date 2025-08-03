using Core.DataAccess;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Abstract
{
    public interface IChatSessionDal : IEntityRepository<ChatSession>
    {
    }
}