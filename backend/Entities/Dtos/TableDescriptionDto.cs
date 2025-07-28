using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TableDescriptionDto:IDto
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string TableDescription { get; set; }
    }
}
