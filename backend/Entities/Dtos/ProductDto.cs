using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ProductDto:IDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
    }

    public class RetailerInfo
    {
        public string StoreName { get; set; }
        public string Url { get; set; }
    }

    public class PriceAnalysisResult
    {
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public string AveragePrice { get; set; }
        public List<RetailerInfo> Retailers { get; set; } = new List<RetailerInfo>();

        public string RawResponse { get; set; }
    }
}
