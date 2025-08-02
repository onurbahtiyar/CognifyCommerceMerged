using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    // API'ye gönderilecek isteğin ana yapısı
    public class GeminiRequest
    {
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Model { get; set; }

        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; }

        [JsonPropertyName("system_instruction")]
        public SystemInstruction SystemInstruction { get; set; }

        [JsonPropertyName("generationConfig")]
        public GenerationConfig GenerationConfig { get; set; } = new GenerationConfig();
        [JsonPropertyName("tools")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Tool> Tools { get; set; }
    }

    // "user" veya "model" rolünü ve içeriği tutar
    public class Content
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    // Asıl metin içeriğini tutar
    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    // System prompt'u için özel yapı
    public class SystemInstruction
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    // Üretim ayarları (isteğe bağlı)
    public class GenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.9;

        [JsonPropertyName("topK")]
        public int TopK { get; set; } = 1;

        [JsonPropertyName("topP")]
        public double TopP { get; set; } = 1;

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 8192;
    }

    // Stream'den gelen her bir JSON bloğunu temsil eder
    public class GeminiStreamResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        // Kolay erişim için bir yardımcı özellik
        [JsonIgnore]
        public string Text => Candidates?.FirstOrDefault()?
                                       .Content?
                                       .Parts?.FirstOrDefault()?
                                       .Text;
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Tool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("input")]
        public object Input { get; set; }
    }

    public class GoogleSearch
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}
