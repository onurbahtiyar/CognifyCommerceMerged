using Business.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class GeminiManager: IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        public GeminiManager(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async IAsyncEnumerable<string> StreamGenerateContentAsync(
            List<(string Role, string Content)> history,
            string systemPrompt = null,
            string model = "gemini-1.5-flash-latest")
        {
            var requestUrl = $"{ApiBaseUrl}{model}:streamGenerateContent?key={_apiKey}";

            var requestPayload = new GeminiRequest
            {
                // API "assistant" yerine "model" rolünü bekler.
                // "system" rolü ise özel bir alana yazılır.
                Contents = history
                    .Where(m => m.Role != "system") // Sistem mesajları ana içerikten çıkarılır
                    .Select(m => new Content
                    {
                        Role = m.Role == "assistant" ? "model" : m.Role,
                        Parts = new List<Part> { new Part { Text = m.Content } }
                    }).ToList()
            };

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                requestPayload.SystemInstruction = new SystemInstruction
                {
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                };
            }

            // `System.Text.Json` için ayarlar
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var jsonContent = JsonSerializer.Serialize(requestPayload, serializerOptions);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            // Stream'den gelen veriyi satır satır oku. Gemini APIsi, JSON objelerini
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.Trim().StartsWith("\"text\":"))
                {
                    // "text": "içerik" formatını parse et
                    var textValue = line.Substring(line.IndexOf(':') + 1)
                                        .Trim()
                                        .Trim('"', ',') // Tırnak ve virgülleri temizle
                                        .Replace("\\n", "\n"); // Satır sonlarını düzelt
                    yield return textValue;
                }
            }
        }
    }
}
