using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ChatMessage:IEntity
    {
        public int MessageId { get; set; }
        public Guid SessionId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public bool IsDatabaseQuery { get; set; }
        public string? RelatedSql { get; set; }
        public string? ChartType { get; set; }
        public string? AdditionalData { get; set; }

        public DateTime CreatedAt { get; set; }
        public virtual ChatSession Session { get; set; }
    }
}
