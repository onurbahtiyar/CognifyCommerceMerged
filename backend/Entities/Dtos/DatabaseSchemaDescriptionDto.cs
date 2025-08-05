using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DatabaseSchemaDescriptionDto
    {
        public List<TableDescriptionDto> Tables { get; set; }
        public List<ColumnInfoDto> Columns { get; set; }
        public List<ForeignKeyDto> ForeignKeys { get; set; }
    }
}
