using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ChatMessageAddDto
    {
        public Guid SessionId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public bool IsDatabaseQuery { get; set; } = false;
        public string RelatedSql { get; set; }
        public string ChartType { get; set; }
        public string AdditionalData { get; set; }
    }
}
