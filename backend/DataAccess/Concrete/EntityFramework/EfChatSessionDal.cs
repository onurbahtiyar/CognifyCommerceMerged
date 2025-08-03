using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfChatSessionDal : EfEntityRepositoryBase<ChatSession, ContextDb>, IChatSessionDal
    {
        public EfChatSessionDal(ContextDb context) : base(context)
        {
        }
    }
}