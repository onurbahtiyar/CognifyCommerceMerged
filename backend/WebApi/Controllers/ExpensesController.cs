using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpPost("add")]
        public IActionResult Add(ExpenseAddDto dto)
        {
            var result = _expenseService.Add(dto);
            return Ok(result);
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _expenseService.GetAll();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public IActionResult GetById(int id)
        {
            var result = _expenseService.GetById(id);
            return Ok(result);
        }

        [HttpPut("update")]
        public IActionResult Update(ExpenseUpdateDto dto)
        {
            var result = _expenseService.Update(dto);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var result = _expenseService.Delete(id);
            return Ok(result);
        }

        [HttpGet("getallcategories")]
        public IActionResult GetAllCategories()
        {
            var result = _expenseService.GetAllCategories();
            return Ok(result);
        }
    }
}
