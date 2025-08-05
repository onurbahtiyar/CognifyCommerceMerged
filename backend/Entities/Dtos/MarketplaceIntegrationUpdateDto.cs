using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class MarketplaceIntegrationUpdateDto
    {
        public int Id { get; set; }
        public string MarketplaceName { get; set; }
        public string ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? ApiUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
