using Business.Abstract;
using DataAccess.Concrete.Dapper;
using Entities.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    /// <summary>
    /// Kullanıcı etkileşimlerini yöneten, Gemini AI ile iletişim kuran ve veritabanı işlemlerini yürüten ana sohbet servisi.
    /// </summary>
    public class ChatManager : IChatService
    {
        private readonly IGeminiService _geminiService;
        private readonly string _systemPrompt;
        private readonly GenericRepository _repository;
        private readonly DatabaseSchemaDescriptionDto _schema;
        private readonly List<(string Role, string Content)> _history = new();

        public ChatManager(GenericRepository repository, IGeminiService geminiService)
        {
            _repository = repository;
            _geminiService = geminiService;
            // Veritabanı şema açıklamalarını al
            var schemaResult = _repository.GetDatabaseSchemaDescriptions();
            if (!schemaResult.Success)
            {
                throw new InvalidOperationException("Veritabanı şeması okunamadı: " + schemaResult.Message);
            }
            _schema = schemaResult.Data;
            _systemPrompt = BuildSystemPrompt(_schema);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(string userPrompt)
        {
            _history.Add(("user", userPrompt));

            var needsDatabase = await ShouldUseDatabaseAsync(userPrompt);

            if (needsDatabase)
            {
                // 1. Veritabanı yanıtını tek bir JSON string'i olarak al.
                var dataResponseJson = await HandleDatabaseQueryAsync(userPrompt);

                // 2. Bu yanıtı, tek seferde gönderilecek bir "full_response" zarfına koy.
                var fullResponseWrapper = new
                {
                    type = "full_response",
                    // Payload'ı tekrar serialize etmemek için deserializeobject ile nesneye çevirip gömüyoruz.
                    payload = JsonConvert.DeserializeObject<dynamic>(dataResponseJson)
                };

                // 3. Çıktıyı serialize et ve tek bir parça olarak gönder.
                yield return JsonConvert.SerializeObject(fullResponseWrapper);
            }
            else
            {
                // 1. Önce stream'in başlayacağını bildiren mesajı gönder.
                yield return JsonConvert.SerializeObject(new { type = "stream_start" });

                // 2. Ardından normal sohbet parçalarını stream et.
                await foreach (var chunk in HandleNormalConversationAsync())
                {
                    yield return chunk;
                }
                yield return JsonConvert.SerializeObject(new { type = "stream_end" });
            }
        }

        private async Task<bool> ShouldUseDatabaseAsync(string prompt)
        {
            var classificationPrompt = $"Kullanıcının şu isteği veritabanından bilgi almayı gerektiriyor mu: '{prompt}'. Yalnızca 'Evet' veya 'Hayır' de.";
            var historyForClassification = new List<(string, string)> { ("user", classificationPrompt) };

            var classificationResponse = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(historyForClassification, null))
            {
                classificationResponse.Append(chunk);
            }
            return classificationResponse.ToString().Trim().StartsWith("Evet", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> HandleDatabaseQueryAsync(string originalUserPrompt)
        {
            try
            {
                var sqlGenerationPrompt = $"Kullanıcının şu isteği için bir SQL sorgusu oluştur: '{originalUserPrompt}'. Sadece SQL kodunu, başka hiçbir açıklama olmadan ver.";
                var tempHistoryForSql = new List<(string, string)>(_history) { ("user", sqlGenerationPrompt) };
                var sqlBuilder = new StringBuilder();
                await foreach (var chunk in _geminiService.StreamGenerateContentAsync(tempHistoryForSql, _systemPrompt))
                {
                    sqlBuilder.Append(chunk);
                }
                var generatedSql = sqlBuilder.ToString().Replace("```sql", "").Replace("```", "").Trim();

                var queryResult = _repository.QueryDynamic(generatedSql);
                if (!queryResult.Success)
                {
                    throw new Exception($"SQL çalıştırma hatası: {queryResult.Message}");
                }

                var jsonDataForExplanation = JsonConvert.SerializeObject(queryResult.Data);
                var explanationPrompt = $"Aşağıdaki JSON verisini ve kullanıcının şu orijinal isteğini analiz et: '{originalUserPrompt}'. Bu verinin ne anlama geldiğini anlatan kısa, samimi bir giriş cümlesi oluştur (örneğin 'İşte en çok satan 5 ürün:'). Sadece bu giriş cümlesini yaz, başka hiçbir şey ekleme.";
                _history.Add(("user", explanationPrompt));
                var explanationBuilder = new StringBuilder();
                await foreach (var chunk in _geminiService.StreamGenerateContentAsync(_history, _systemPrompt))
                {
                    explanationBuilder.Append(chunk);
                }
                var explanationText = explanationBuilder.ToString().Trim();
                _history.Add(("assistant", explanationText));

                var finalResponseObject = new
                {
                    type = "data_response",
                    explanation = explanationText,
                    data = queryResult.Data
                };
                return JsonConvert.SerializeObject(finalResponseObject);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    type = "error_response",
                    explanation = "Üzgünüm, isteğinizi işlerken bir sorunla karşılaştım.",
                    error = ex.Message
                };
                return JsonConvert.SerializeObject(errorResponse);
            }
        }

        private async IAsyncEnumerable<string> HandleNormalConversationAsync()
        {
            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(_history, _systemPrompt))
            {
                if (string.IsNullOrEmpty(chunk))
                {
                    continue;
                }

                responseBuilder.Append(chunk);
                var normalTextResponse = new { type = "text_response", text = chunk };
                yield return JsonConvert.SerializeObject(normalTextResponse);
            }
            _history.Add(("assistant", responseBuilder.ToString()));
        }

        private string BuildSystemPrompt(DatabaseSchemaDescriptionDto schema)
        {
            var sb = new StringBuilder()
                .AppendLine("Sen, bir veritabanı uzmanı ve aynı zamanda kullanıcı dostu bir asistansın.")
                .AppendLine("Görevin, kullanıcıların doğal dilde sorduğu soruları anlayıp, aşağıdaki veritabanı şemasına göre SQL sorguları üretmek ve sonuçları onlara tercüme etmektir.")
                .AppendLine("\n--- KURALLAR ---")
                .AppendLine("1. SQL üretmen istendiğinde, YALNIZCA SQL kodunu ver. Başka hiçbir açıklama, yorum veya ` ``` ` bloğu ekleme.")
                .AppendLine("2. Bir SQL sonucunu yorumlaman ve açıklama metni oluşturman istendiğinde, sadece kısa ve samimi bir giriş cümlesi yaz.")
                .AppendLine("3. SQL sorgularında tablo ve sütun adlarını daima köşeli parantez içinde kullan: `[dbo].[Products]`.")
                .AppendLine()
                .AppendLine("--- VERİTABANI ŞEMASI ---")
                .AppendLine("== Tablolar ==");
            foreach (var t in schema.Tables)
                sb.AppendLine($"- [{t.SchemaName}].[{t.TableName}]: {t.TableDescription}");
            sb.AppendLine().AppendLine("== Sütunlar ==");
            foreach (var c in schema.Columns)
                sb.AppendLine($"- [{c.SchemaName}].[{c.TableName}].[{c.ColumnName}] ({c.DataType}): {c.ColumnDescription}");
            sb.AppendLine().AppendLine("== İlişkiler (Foreign Keys) ==");
            foreach (var fk in schema.ForeignKeys)
                sb.AppendLine($"- [{fk.SourceSchema}].[{fk.SourceTable}].[{fk.SourceColumn}] -> [{fk.TargetSchema}].[{fk.TargetTable}].[{fk.TargetColumn}]");
            return sb.ToString();
        }
    }
}