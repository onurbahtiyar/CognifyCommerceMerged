using Core.DataAccess;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Abstract
{
    public interface IChatMessageDal : IEntityRepository<ChatMessage>
    {
    }
}