using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IGeminiService
    {
        IAsyncEnumerable<string> StreamGenerateContentAsync(
            List<(string Role, string Content)> history,
            string systemPrompt = null,
            string model = "gemini-1.5-flash-latest");
    }
}
