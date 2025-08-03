using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ChatSession:IEntity
    {
        public Guid SessionId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        public ChatSession()
        {
            Messages = new HashSet<ChatMessage>();
        }
    }
}
