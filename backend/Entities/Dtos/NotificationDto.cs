using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Topic { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public int? RelatedEntityId { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}
