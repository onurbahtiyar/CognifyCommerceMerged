using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _orderService.GetAll();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _orderService.GetById(id);
            return Ok(result);
        }

        [HttpPost("add")]
        public IActionResult Add(OrderAddDto dto)
        {
            var result = _orderService.Add(dto);
            return Ok(result);
        }

        [HttpPut("updatestatus")]
        public IActionResult UpdateStatus(OrderUpdateStatusDto dto)
        {
            var result = _orderService.UpdateStatus(dto);
            return Ok(result);
        }

        [HttpPut("updateshipping")]
        public IActionResult UpdateShipping(OrderUpdateShippingDto dto)
        {
            var result = _orderService.UpdateShippingInfo(dto);
            return Ok(result);
        }

        [HttpPut("cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            var result = _orderService.CancelOrder(id);
            return Ok(result);
        }

    }
}
