using Core.Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class Notification: IEntity
    {
        [Key]
        public int NotificationId { get; set; }

        public NotificationTopic Topic { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Message { get; set; }

        public int? RelatedEntityId { get; set; }

        [MaxLength(255)]
        public string Url { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }
    }
}
