using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using GenerativeAI;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("add")]
        public IActionResult Add(ProductAddDto dto)
        {
            IDataResult<ProductDto> result = _productService.Add(dto);
            return Ok(result);
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            IDataResult<List<ProductDto>> result = _productService.GetAll();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public IActionResult GetById(int id)
        {
            IDataResult<ProductDto> result = _productService.GetById(id);
            return Ok(result);
        }

        [HttpPut("update")]
        public IActionResult Update(ProductDto dto)
        {
            var result = _productService.Update(new ProductUpdateDto
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                Description = dto.Description,
                Stock = dto.Stock,
                Price = dto.Price,
                CategoryId = dto.CategoryId
            });
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var result = _productService.Delete(id);
            return Ok(result);
        }

        [HttpGet("priceanalysis/{productName}")]
        public async Task<IActionResult> GetPriceAnalysis(string productName)
        {
            var result = await _productService.GetPriceAnalysisAsync(productName);
            return Ok(result);
        }

        [HttpPost("enhance-temp-image")]
        public async Task<IActionResult> EnhanceTempImage([FromForm] IFormFile image, [FromForm] string instruction)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "Görsel yüklenmedi." });

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var result = await _productService.EnhanceImageTempAsync(imageBytes, instruction);
            return Ok(result);
        }

        [HttpPost("attach-image/{productId}")]
        public async Task<IActionResult> AttachTempImageToProduct(int productId, [FromBody] AttachTempImageRequest request)
        {
            var result = await _productService.AttachTempImageToProductAsync(productId, request.TempImageUrl);
            return Ok(result);
        }
    }
}
