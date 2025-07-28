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
            // Server-Sent Events (SSE) için gerekli olan response başlıkları.
            Response.Headers.Add("Content-Type", "text/event-stream; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            // Servisten gelen her bir veri parçasını (chunk) alıp frontend'e yazdır.
            // WithCancellation, kullanıcı tarayıcıyı kapattığında veya bağlantı koptuğunda
            // sunucu tarafındaki işlemi iptal ederek kaynakların boşa harcanmasını önler.
            await foreach (var chunk in _chatService.StreamChatAsync(prompt)
                                                    .WithCancellation(HttpContext.RequestAborted))
            {
                // Sadece dolu olan chunk'ları frontend'e gönder.
                if (!string.IsNullOrEmpty(chunk))
                {
                    // SSE formatına uygun şekilde veriyi yaz: "data: {json_string}\n\n"
                    await Response.WriteAsync($"data: {chunk}\n\n");

                    // Verinin hemen gönderilmesi için response buffer'ını temizle (flush).
                    await Response.Body.FlushAsync();
                }
            }
        }
    }
}