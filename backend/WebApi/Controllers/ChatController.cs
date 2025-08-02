using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("stream")]
        public async Task Stream([FromQuery] string prompt)
        {
            Response.Headers.Add("Content-Type", "text/event-stream; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            await foreach (var chunk in _chatService.StreamChatAsync(prompt)
                                                    .WithCancellation(HttpContext.RequestAborted))
            {
                if (!string.IsNullOrEmpty(chunk))
                {
                    await Response.WriteAsync($"data: {chunk}\n\n");

                    await Response.Body.FlushAsync();
                }
            }
        }
    }
}