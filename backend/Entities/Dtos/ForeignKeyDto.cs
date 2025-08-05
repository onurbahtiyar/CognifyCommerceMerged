using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ForeignKeyDto:IDto
    {
        public string SourceSchema { get; set; }
        public string SourceTable { get; set; }
        public string SourceColumn { get; set; }
        public string TargetSchema { get; set; }
        public string TargetTable { get; set; }
        public string TargetColumn { get; set; }
    }
}
