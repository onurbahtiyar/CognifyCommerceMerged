using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ChatSessionDto
    {
        public Guid SessionId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
