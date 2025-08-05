using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceIntegrationsController : ControllerBase
    {
        private readonly IMarketplaceIntegrationService _marketplaceService;

        public MarketplaceIntegrationsController(IMarketplaceIntegrationService marketplaceService)
        {
            _marketplaceService = marketplaceService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _marketplaceService.GetAll();
            return Ok(result);
        }

        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _marketplaceService.GetById(id);
            return Ok(result);
        }

        [HttpPost("add")]
        public IActionResult Add(MarketplaceIntegrationAddDto dto)
        {
            var result = _marketplaceService.Add(dto);
            return Ok(result);
        }

        [HttpPut("update")]
        public IActionResult Update(MarketplaceIntegrationUpdateDto dto)
        {
            var result = _marketplaceService.Update(dto);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var result = _marketplaceService.Delete(id);
            return Ok(result);
        }
    }
}
