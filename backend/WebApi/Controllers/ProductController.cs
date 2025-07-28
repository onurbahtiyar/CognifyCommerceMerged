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
                Price = dto.Price
            });
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var result = _productService.Delete(id);
            return Ok(result);
        }
    }
}
