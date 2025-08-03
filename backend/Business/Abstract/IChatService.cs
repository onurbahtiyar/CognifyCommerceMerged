using Core.Utilities.Result.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IChatService
    {
        IAsyncEnumerable<string> StreamChatAsync(string prompt, Guid? sessionId = null);

        IDataResult<ChatSessionDto> GetSessionById(Guid sessionId);

        IDataResult<List<ChatSessionDto>> GetAllSessions();

        IDataResult<List<ChatMessageDto>> GetMessagesBySessionId(Guid sessionId);

        IResult DeleteSession(Guid sessionId);
    }
}
