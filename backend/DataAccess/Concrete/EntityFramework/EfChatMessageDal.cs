using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfChatMessageDal : EfEntityRepositoryBase<ChatMessage, ContextDb>, IChatMessageDal
    {
        public EfChatMessageDal(ContextDb context) : base(context)
        {
        }
    }
}