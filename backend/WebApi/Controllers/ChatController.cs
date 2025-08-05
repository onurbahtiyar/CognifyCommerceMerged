using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        public async Task Stream([FromQuery] string prompt, [FromQuery] Guid? sessionId)
        {
            Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            await foreach (var chunk in _chatService.StreamChatAsync(prompt, sessionId)
                                                    .WithCancellation(HttpContext.RequestAborted))
            {
                if (!string.IsNullOrEmpty(chunk))
                {
                    await Response.WriteAsync($"data: {chunk}\n\n");
                    await Response.Body.FlushAsync();
                }
            }
        }

        [HttpGet("sessions")]
        public IActionResult GetAllSessions()
        {
            var result = _chatService.GetAllSessions();
            return Ok(result);
        }

        [HttpGet("sessions/{sessionId:guid}")]
        public IActionResult GetSessionById(Guid sessionId)
        {
            var result = _chatService.GetSessionById(sessionId);
            return Ok(result);
        }

        [HttpGet("sessions/{sessionId:guid}/messages")]
        public IActionResult GetMessages(Guid sessionId)
        {
            var result = _chatService.GetMessagesBySessionId(sessionId);
            return Ok(result);

        }

        [HttpDelete("sessions/{sessionId:guid}")]
        public IActionResult DeleteSession(Guid sessionId)
        {
            var result = _chatService.DeleteSession(sessionId);
            return Ok(result);
        }

    }
}