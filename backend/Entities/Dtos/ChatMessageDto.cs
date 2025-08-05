using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDatabaseQuery { get; set; }
        public string RelatedSql { get; set; }
        public string ChartType { get; set; }
    }
}
