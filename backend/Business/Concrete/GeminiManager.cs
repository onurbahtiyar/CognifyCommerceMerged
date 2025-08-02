using Business.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                                        .Trim('"', ',')
                                        .Replace("\\n", "\n").Replace("\\t", " ");

                    yield return textValue;
                }
            }
        }


        public async Task<byte[]> EnhanceImageAsync(
        byte[] inputImage,
        string instruction,
        string targetBackground = "white",
        string model = "gemini-2.0-flash-preview-image-generation")
        {
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            string base64Image = Convert.ToBase64String(inputImage);

            var payload = new
            {
                generation_config = new
                {
                    response_modalities = new[] { "TEXT", "IMAGE" } // modelin istediği kombinasyon
                },
                contents = new[]
                {
            new
            {
                role = "user",
                parts = new object[]
                {
                    new
                    {
                        text = $"Görseli geliştir: {instruction}. Arka planı '{targetBackground}' yap ve e-ticaret için profesyonel görünsün."
                    },
                    new
                    {
                        inline_data = new
                        {
                            mime_type = "image/jpeg",
                            data = base64Image
                        }
                    }
                }
            }
        }
            };

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // manuel verdiğimiz alan isimleriyle uyuşsun
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonContent = JsonSerializer.Serialize(payload, serializerOptions);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini image enhance hatası: {response.StatusCode} - {responseString}");

            using var doc = JsonDocument.Parse(responseString);

            // Hem inline_data hem inlineData için esnek parse
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array)
            {
                foreach (var candidate in candidates.EnumerateArray())
                {
                    if (candidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var part in parts.EnumerateArray())
                        {
                            // önce camelCase "inlineData"
                            if (part.TryGetProperty("inlineData", out var inlineDataCamel))
                            {
                                if (TryExtractBase64FromInlineData(inlineDataCamel, out var base64Camel))
                                    return Convert.FromBase64String(base64Camel);
                            }

                            // sonra snake_case "inline_data"
                            if (part.TryGetProperty("inline_data", out var inlineDataSnake))
                            {
                                if (TryExtractBase64FromInlineData(inlineDataSnake, out var base64Snake))
                                    return Convert.FromBase64String(base64Snake);
                            }
                        }
                    }
                }
            }

            throw new Exception("Gelen yanıtta düzenlenmiş görsel bulunamadı.");
        }

        private bool TryExtractBase64FromInlineData(JsonElement inlineDataElement, out string base64)
        {
            base64 = string.Empty;

            // data alanını bul
            if (inlineDataElement.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.String)
            {
                base64 = dataElem.GetString()!;
                return !string.IsNullOrWhiteSpace(base64);
            }

            return false;
        }

    }
}
