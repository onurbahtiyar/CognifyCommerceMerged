using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;

        public ProductReviewsController(IProductReviewService productReviewService)
        {
            _productReviewService = productReviewService;
        }

        [HttpPost("add")]
        public IActionResult Add(ProductReviewAddDto dto)
        {
            var result = _productReviewService.Add(dto);
            return Ok(result);
        }

        [HttpPost("addadminreply")]
        public IActionResult AddAdminReply(AdminReplyAddDto dto)
        {
            var result = _productReviewService.AddAdminReply(dto);
            return Ok(result);
        }

        [HttpGet("getbyproduct/{productId}")]
        public IActionResult GetByProductId(int productId)
        {
            var result = _productReviewService.GetByProductId(productId);
            return Ok(result);
        }

        [HttpGet("getunapproved")]
        public IActionResult GetUnapprovedReviews()
        {
            var result = _productReviewService.GetUnapprovedReviews();
            return Ok(result);
        }

        [HttpPut("approve/{reviewId}")]
        public IActionResult ApproveReview(int reviewId)
        {
            var result = _productReviewService.ApproveReview(reviewId);
            return Ok(result);
        }

        [HttpDelete("delete/{reviewId}")]
        public IActionResult Delete(int reviewId)
        {
            var result = _productReviewService.Delete(reviewId);
            return Ok(result);
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _productReviewService.GetAll();
            return Ok(result);
        }
    }
}
