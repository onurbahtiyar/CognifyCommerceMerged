using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.Dapper;
using Entities.Concrete;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public record ChartTypeInfo(
        string ChartJsType,
        string DisplayName,
        List<string> Keywords
    );

    public record ColumnMappingDto(string originalKey, string displayName);

    public class ChatManager : IChatService
    {
        private readonly IChatSessionDal _chatSessionDal;
        private readonly IChatMessageDal _chatMessageDal;
        private readonly IGeminiService _geminiService;
        private readonly GenericRepository _repositoryForSchema;
        private readonly string _systemPrompt;
        private readonly DatabaseSchemaDescriptionDto _schema;
        private readonly List<(string Role, string Content)> _history = new();

        private readonly List<ChartTypeInfo> _chartTypes = new()
        {
            new("bar", "Çubuk Grafik", new List<string> { "bar", "çubuk", "sütun" }),
            new("pie", "Pasta Grafik", new List<string> { "pie", "pasta" }),
            new("doughnut", "Halka Grafik", new List<string> { "doughnut", "halka", "simit" }),
            new("radar", "Radar Grafik", new List<string> { "radar" }),
            new("line", "Çizgi Grafik", new List<string> { "line", "çizgi" }),
            new("polarArea", "Kutup Alan Grafiği", new List<string> { "polar", "kutup" }),
            new("bubble", "Baloncuk Grafiği", new List<string> { "bubble", "baloncuk" }),
            new("scatter", "Dağılım Grafiği", new List<string> { "scatter", "dağılım", "serpilme" })
        };
        public ChatManager(
            IChatSessionDal chatSessionDal,
            IChatMessageDal chatMessageDal,
            IGeminiService geminiService,
            GenericRepository repositoryForSchema)
        {
            _chatSessionDal = chatSessionDal;
            _chatMessageDal = chatMessageDal;
            _geminiService = geminiService;
            _repositoryForSchema = repositoryForSchema;

            var schemaResult = _repositoryForSchema.GetDatabaseSchemaDescriptions();
            if (!schemaResult.Success)
            {
                throw new InvalidOperationException("Veritabanı şeması okunamadı: " + schemaResult.Message);
            }
            _schema = schemaResult.Data;
            _systemPrompt = BuildSystemPrompt(_schema);
        }

        #region Ana Akış ve CRUD Metotları
        public async IAsyncEnumerable<string> StreamChatAsync(string userPrompt, Guid? sessionId = null)
        {
            Guid currentSessionId;

            if (sessionId.HasValue && sessionId.Value != Guid.Empty)
            {
                currentSessionId = sessionId.Value;
                LoadHistoryFromDatabase(currentSessionId);
            }
            else
            {
                var sessionResult = CreateSession(userPrompt);
                currentSessionId = sessionResult.Data.SessionId;
            }

            yield return JsonConvert.SerializeObject(new { type = "session_info", sessionId = currentSessionId });

            _history.Add(("user", userPrompt));
            AddMessage(new ChatMessageAddDto { SessionId = currentSessionId, Role = "user", Content = userPrompt });

            var needsDatabase = await ShouldUseDatabaseAsync(userPrompt);

            if (needsDatabase)
            {

                Exception lastException = null;
                string dataResponseJson = null; 
                bool isSuccess = false;       
                const int maxAttempts = 5;

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        dataResponseJson = await HandleDatabaseQueryAsync(userPrompt, currentSessionId);

                        isSuccess = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Console.WriteLine($"ChatManager: Attempt {attempt}/{maxAttempts} failed for session {currentSessionId}. Error: {ex.Message}");

                        if (attempt < maxAttempts)
                        {
                            await Task.Delay(attempt * 1000);
                        }
                    }
                }

                if (isSuccess)
                {
                    yield return dataResponseJson;
                }
                else if (lastException != null)
                {
                    var errorExplanation = "Üzgünüm, isteğinizi işlerken tekrar eden bir sorunla karşılaştık.";
                    var detailedError = $"Tüm denemeler başarısız oldu. Lütfen daha sonra tekrar deneyin veya sistem yöneticisiyle iletişime geçin. Son Hata: {lastException.Message}";
                    var errorResponse = new
                    {
                        type = "fatal_error_response",
                        explanation = errorExplanation,
                        error = detailedError
                    };
                    yield return JsonConvert.SerializeObject(errorResponse);
                }

                yield break;

            }
            else
            {
                yield return JsonConvert.SerializeObject(new { type = "stream_start" });
                await foreach (var chunk in HandleNormalConversationAsync(currentSessionId, useDatabaseSystemPrompt: false))
                {
                    yield return chunk;
                }

                yield return JsonConvert.SerializeObject(new { type = "stream_end" });
            }
        }

        public IDataResult<ChatSessionDto> GetSessionById(Guid sessionId)
        {
            var session = _chatSessionDal.Get(s => s.SessionId == sessionId);
            if (session == null)
            {
                return new ErrorDataResult<ChatSessionDto>("Oturum bulunamadı.");
            }
            var sessionDto = new ChatSessionDto
            {
                SessionId = session.SessionId,
                Title = session.Title,
                CreatedAt = session.CreatedAt
            };
            return new SuccessDataResult<ChatSessionDto>(sessionDto, "Oturum bilgileri getirildi.");
        }
        public IDataResult<List<ChatSessionDto>> GetAllSessions()
        {
            var sessions = _chatSessionDal.GetList(null, q => q.OrderByDescending(s => s.CreatedAt));
            var sessionDtos = sessions.Select(s => new ChatSessionDto
            {
                SessionId = s.SessionId,
                Title = s.Title,
                CreatedAt = s.CreatedAt
            }).ToList();
            return new SuccessDataResult<List<ChatSessionDto>>(sessionDtos, "Tüm oturumlar listelendi.");
        }
        public IDataResult<List<ChatMessageDto>> GetMessagesBySessionId(Guid sessionId)
        {
            var messages = _chatMessageDal.GetList(m => m.SessionId == sessionId, q => q.OrderBy(m => m.CreatedAt));
            var messageDtos = messages.Select(m => new ChatMessageDto
            {
                MessageId = m.MessageId,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                IsDatabaseQuery = m.IsDatabaseQuery,
                RelatedSql = m.RelatedSql,
                ChartType = m.ChartType
            }).ToList();
            return new SuccessDataResult<List<ChatMessageDto>>(messageDtos, "Mesaj geçmişi getirildi.");
        }
        public IResult DeleteSession(Guid sessionId)
        {
            var sessionToDelete = _chatSessionDal.Get(s => s.SessionId == sessionId);
            if (sessionToDelete == null)
            {
                return new Result(false, "Silinecek oturum bulunamadı.");
            }
            _chatSessionDal.Delete(sessionToDelete);
            return new SuccessResult("Oturum ve ilgili mesajlar başarıyla silindi.");
        }
        #endregion

        #region Dahili (Private) Mantıksal Metotlar

        private async Task<string> HandleDatabaseQueryAsync(string originalUserPrompt, Guid sessionId)
        {
            var sqlResult = await GenerateAndExecuteSqlWithRetriesAsync(originalUserPrompt, 5);

            if (!sqlResult.Success)
            {
                var explanation = "Üzgünüm, isteğinizi işlerken bir sorunla karşılaştım. Yapay zeka sorguyu düzeltmeye çalıştı ancak başarılı olamadı.";
                var detailedError = $"Son hata: {sqlResult.ErrorMessage}. Lütfen isteğinizi farklı bir şekilde ifade etmeyi deneyin veya sistem yöneticisiyle iletişime geçin.";
                var errorJson = JsonConvert.SerializeObject(new { type = "error_response", explanation, error = detailedError });
                AddMessage(new ChatMessageAddDto { SessionId = sessionId, Role = "assistant", Content = errorJson, IsDatabaseQuery = true, RelatedSql = sqlResult.GeneratedSql, AdditionalData = sqlResult.ErrorMessage });
                _history.Add(("assistant", explanation + " " + detailedError));
                return errorJson;
            }

            var QueryData = sqlResult.QueryData;
            var GeneratedSql = sqlResult.GeneratedSql;

            try
            {
                var rawJsonData = JsonConvert.SerializeObject(QueryData);
                var presentationDecision = await GetPresentationDecision(originalUserPrompt, rawJsonData);
                var matchedChart = FindChartType(presentationDecision);

                string responseJson;
                string explanation;

                if (matchedChart != null)
                {
                    var chartJsonString = await GenerateChartJsonAsync(originalUserPrompt, rawJsonData, matchedChart);

                    dynamic chartPayload = JsonConvert.DeserializeObject(chartJsonString);

                    explanation = $"İsteğiniz doğrultusunda hazırlanan '{matchedChart.DisplayName}' aşağıdadır:";
                    responseJson = JsonConvert.SerializeObject(new { type = "chart_response", explanation, payload = chartPayload });
                    AddMessage(new ChatMessageAddDto { SessionId = sessionId, Role = "assistant", Content = responseJson, IsDatabaseQuery = true, RelatedSql = GeneratedSql, ChartType = matchedChart.ChartJsType, AdditionalData = rawJsonData });
                }
                else
                {
                    dynamic finalData = QueryData;
                    var columnMappings = await GetSimplifiedTableStructureAsync(originalUserPrompt, rawJsonData);

                    if (columnMappings != null && columnMappings.Count > 0)
                    {
                        var tempSimplifiedData = new List<dynamic>();
                        foreach (var rowAsDict in QueryData)
                        {
                            var newRow = new ExpandoObject() as IDictionary<string, object>;
                            foreach (var mapping in columnMappings)
                            {
                                if (rowAsDict.ContainsKey(mapping.originalKey) && rowAsDict[mapping.originalKey] != null)
                                {
                                    newRow[mapping.displayName] = rowAsDict[mapping.originalKey];
                                }
                            }
                            if (newRow.Count > 0) tempSimplifiedData.Add(newRow);
                        }
                        if (tempSimplifiedData.Count > 0)
                        {
                            finalData = tempSimplifiedData;
                        }
                    }

                    explanation = await GenerateExplanationForDataAsync(originalUserPrompt, JsonConvert.SerializeObject(finalData));
                    responseJson = JsonConvert.SerializeObject(new { type = "data_response", explanation, data = finalData });
                    AddMessage(new ChatMessageAddDto { SessionId = sessionId, Role = "assistant", Content = responseJson, IsDatabaseQuery = true, RelatedSql = GeneratedSql, ChartType = "table", AdditionalData = rawJsonData });
                }

                _history.Add(("assistant", explanation));
                return responseJson;
            }
            catch (Exception ex)
            {
                var explanation = "Üzgünüm, veriyi sunum için hazırlarken bir sorun oluştu.";
                var detailedError = $"Detay: {ex.Message}. Yapay zeka geçerli bir format üretememiş olabilir.";
                var errorJson = JsonConvert.SerializeObject(new { type = "error_response", explanation, error = detailedError });
                AddMessage(new ChatMessageAddDto { SessionId = sessionId, Role = "assistant", Content = errorJson, IsDatabaseQuery = true, RelatedSql = GeneratedSql, AdditionalData = ex.ToString() });
                _history.Add(("assistant", explanation + " " + detailedError));
                return errorJson;
            }
        }

        private async IAsyncEnumerable<string> HandleNormalConversationAsync(Guid sessionId, bool useDatabaseSystemPrompt = true)
        {
            var systemPromptToUse = useDatabaseSystemPrompt ? _systemPrompt : null;

            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(_history, systemPromptToUse))
            {
                if (string.IsNullOrEmpty(chunk)) continue;
                responseBuilder.Append(chunk);
                yield return JsonConvert.SerializeObject(new { type = "text_response", text = chunk });
            }
            var finalResponse = responseBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(finalResponse))
            {
                _history.Add(("assistant", finalResponse));
                AddMessage(new ChatMessageAddDto { SessionId = sessionId, Role = "assistant", Content = finalResponse });
            }
        }


        #endregion

        private async Task<(bool Success, string GeneratedSql, IEnumerable<IDictionary<string, object>> QueryData, string ErrorMessage)> GenerateAndExecuteSqlWithRetriesAsync(string originalUserPrompt, int maxAttempts)
        {
            string lastGeneratedSql = null;
            Exception lastException = null;
            var tempHistory = new List<(string, string)>(_history);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var sqlGenerationPrompt = (attempt == 1)
                        ? $"Kullanıcının son isteği: '{originalUserPrompt}'. Bu isteğe uygun MSSQL sorgusunu oluştur."
                        : $"Bu sorgu başarısız oldu. Hata: '{lastException.Message}'. Lütfen SADECE sana verilen veritabanı şemasını kullanarak sorguyu düzelt ve tekrar yaz.";
                    tempHistory.Add(("user", sqlGenerationPrompt));

                    var sqlBuilder = new StringBuilder();
                    await foreach (var chunk in _geminiService.StreamGenerateContentAsync(tempHistory, _systemPrompt))
                    {
                        sqlBuilder.Append(chunk);
                    }

                    lastGeneratedSql = SanitizeAndExtractSql(sqlBuilder.ToString());
                    if (string.IsNullOrWhiteSpace(lastGeneratedSql))
                        throw new Exception("Yapay zeka geçerli bir SQL sorgusu üretemedi.");

                    tempHistory.Add(("assistant", lastGeneratedSql));

                    var queryResult = _repositoryForSchema.QueryDynamic(lastGeneratedSql);
                    if (!queryResult.Success)
                        throw new Exception(queryResult.Message);

                    return (true, lastGeneratedSql, queryResult.Data, null);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }
            return (false, lastGeneratedSql, null, lastException?.Message ?? "Bilinmeyen bir SQL üretme/yürütme hatası oluştu.");
        }

        private IDataResult<ChatSession> CreateSession(string firstPrompt)
        {
            var newSession = new ChatSession
            {
                SessionId = Guid.NewGuid(),
                Title = firstPrompt.Length > 255 ? firstPrompt.Substring(0, 255) : firstPrompt,
                CreatedAt = DateTime.UtcNow
            };
            _chatSessionDal.Add(newSession);
            return new SuccessDataResult<ChatSession>(newSession);
        }

        private void AddMessage(ChatMessageAddDto dto)
        {
            var message = new ChatMessage
            {
                SessionId = dto.SessionId,
                Role = dto.Role,
                Content = dto.Content,
                IsDatabaseQuery = dto.IsDatabaseQuery,
                RelatedSql = dto.RelatedSql,
                ChartType = dto.ChartType,
                AdditionalData = dto.AdditionalData,
                CreatedAt = DateTime.UtcNow
            };
            _chatMessageDal.Add(message);
        }

        private void LoadHistoryFromDatabase(Guid sessionId)
        {
            _history.Clear();
            var messages = _chatMessageDal.GetList(m => m.SessionId == sessionId, q => q.OrderBy(m => m.CreatedAt));
            if (messages == null) return;
            foreach (var message in messages)
            {
                string historyContent = message.Content;
                if (message.Role == "assistant" && message.IsDatabaseQuery)
                {
                    try
                    {
                        dynamic parsedJson = JsonConvert.DeserializeObject<dynamic>(message.Content);
                        string explanation = parsedJson?.explanation;
                        if (!string.IsNullOrWhiteSpace(explanation))
                        {
                            historyContent = explanation;
                        }
                    }
                    catch (JsonException) { }
                }
                _history.Add((message.Role, historyContent));
            }
        }

        private async Task<List<ColumnMappingDto>> GetSimplifiedTableStructureAsync(string userPrompt, string rawJsonData)
        {
            var prompt = $@"
                Kullanıcının isteği: '{userPrompt}'
                Bu isteğe yanıt olarak veritabanından şu ham JSON verisi alındı: {rawJsonData}
                
                GÖREV: Bu ham veriyi kullanıcıya sunmak için sadeleştir.
                1. Kullanıcının sorusuyla en alakalı sütunları seç.
                2. 'Id', 'Guid', 'Password', 'IsDeleted', 'IsActive' gibi teknik veya hassas sütunları ÇIKAR.
                3. Seçtiğin sütunların başlıklarını (key) kullanıcı dostu, anlaşılır Türkçe isimlere çevir.
                4. Sonucu, aşağıdaki basit formatta, her eşleşmeyi YENİ BİR SATIRA yazarak döndür. Başka HİÇBİR ŞEY EKLEME.

                FORMAT:
                OrjinalSutunAdi1:Kullanıcı Dostu Ad 1
                OrjinalSutunAdi2:Kullanıcı Dostu Ad 2

                ÖRNEK ÇIKTI:
                Name:Ürün Adı
                UnitPrice:Birim Fiyatı
            ";

            var stringBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(new List<(string, string)> { ("user", prompt) }))
            {
                stringBuilder.Append(chunk);
            }

            var responseText = stringBuilder.ToString().Trim();
            var mappings = new List<ColumnMappingDto>();
            try
            {
                var lines = responseText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        var originalKey = parts[0].Trim();
                        var displayName = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(originalKey) && !string.IsNullOrWhiteSpace(displayName))
                        {
                            mappings.Add(new ColumnMappingDto(originalKey, displayName));
                        }
                    }
                }
                return mappings;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ShouldUseDatabaseAsync(string prompt)
        {
            var classificationPrompt = $@"
        Senin görevin, kullanıcının isteğinin bir veritabanı sorgusu gerektirip gerektirmediğini sınıflandırmaktır.
        # KAPSAM:
        - VERİTABANI GEREKTİREN İSTEKLER: Bu dükkanın ürünleri, müşterileri, siparişleri, satışları, stok durumu gibi konularla ilgili spesifik veri isteyen sorulardır.
        - VERİTABANI GEREKTİRMEYEN İSTEKLER: Genel sohbet, selamlaşma, teşekkür, konuşma geçmişiyle ilgili sorular, genel bilgi (tarih, bilim vb.), kod yazma veya yaratıcı metin oluşturma gibi, bu dükkanın özel verilerini içermeyen tüm diğer isteklerdir.

        # ÖRNEKLER:
        Kullanıcı: 'En çok satan 5 ürünü listele'
        Cevap: Evet

        Kullanıcı: '2 numaralı müşterinin son siparişi nedir?'
        Cevap: Evet
        
        Kullanıcı: 'Merhaba, nasılsın?'
        Cevap: Hayır

        Kullanıcı: 'Bana C# dilinde bir login örneği yazar mısın?'
        Cevap: Hayır

        Kullanıcı: 'Türkiye'nin başkenti neresidir?'
        Cevap: Hayır

        Kullanıcı: 'Az önce verdiğin cevabı özetler misin?'
        Cevap: Hayır

        # GÖREV:
        Şimdi aşağıdaki kullanıcı isteğini sınıflandır. Yalnızca 'Evet' veya 'Hayır' olarak yanıt ver.
        Kullanıcı: '{prompt}'
        Cevap:";

            var historyForClassification = new List<(string, string)> { ("user", classificationPrompt.Trim()) };
            var classificationResponse = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(historyForClassification, null, "gemini-1.5-flash-latest"))
            {
                classificationResponse.Append(chunk);
            }
            return classificationResponse.ToString().Trim().StartsWith("Evet", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> GetPresentationDecision(string userPrompt, string jsonData)
        {
            var allChartKeywords = string.Join(" | ", _chartTypes.SelectMany(c => c.Keywords).Distinct());
            var prompt = $@"
                GÖREV: Sağlanan kullanıcı isteği ve JSON verisine göre en uygun sunum formatını belirle.
                İSTEK: ""{userPrompt}""
                VERİ: ""{jsonData}""
                SEÇENEKLER: tablo | {allChartKeywords}
                KURALLAR:
                1. Yanıtın SADECE ve SADECE 'FORMAT: <seçim>' şeklinde olmalıdır.
                2. ASLA açıklama, gerekçe veya ek metin ekleme.
                3. Tek bir veri satırı varsa veya veri karşılaştırmaya uygun değilse 'tablo' seç.
                ÖRNEK ÇIKTI: FORMAT: tablo";

            var decisionBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(new List<(string, string)> { ("user", prompt) }))
            {
                decisionBuilder.Append(chunk);
            }
            return decisionBuilder.ToString().Trim().Split(':').Last().Trim().ToLower();
        }

        private async Task<string> GenerateChartJsonAsync(
            string userPrompt,
            string jsonData,
            ChartTypeInfo chartInfo,
            Exception previousError = null)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Kullanıcının isteği: '{userPrompt}'.");
            promptBuilder.AppendLine($"Veri: {jsonData}.");
            promptBuilder.AppendLine($"Bu veriyi bir '{chartInfo.ChartJsType}' grafiği için analiz et.");
            promptBuilder.AppendLine("GÖREV: Grafiğin etiketlerini (labels) ve veri setlerini (datasets) aşağıdaki basit formatta, her bir öğeyi yeni bir satıra yazarak döndür.");
            promptBuilder.AppendLine("--- FORMAT ---");
            promptBuilder.AppendLine("TITLE:Grafik Başlığı");
            promptBuilder.AppendLine("LABEL:Etiket1,Etiket2,Etiket3");
            promptBuilder.AppendLine("DATA:SayısalDeğer1,SayısalDeğer2,SayısalDeğer3");
            promptBuilder.AppendLine("--- KURALLAR ---");
            promptBuilder.AppendLine("1. Sadece yukarıdaki formatı kullan, başka hiçbir şey ekleme.");
            promptBuilder.AppendLine("2. Etiket ve veri sayıları EŞİT olmalıdır.");
            promptBuilder.AppendLine("3. Sayısal değerlerde binlik ayıracı (,) kullanma, ondalık için nokta (.) kullan.");
            promptBuilder.AppendLine("--- ÖRNEK ÇIKTI ---");
            promptBuilder.AppendLine("TITLE:Kategorilere Göre Ürün Sayısı");
            promptBuilder.AppendLine("LABEL:Elektronik,Giyim,Kozmetik");
            promptBuilder.AppendLine("DATA:15,32,8");

            var prompt = promptBuilder.ToString();

            var responseBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(new List<(string, string)> { ("user", prompt) }))
            {
                responseBuilder.Append(chunk);
            }

            try
            {
                var responseText = responseBuilder.ToString().Trim();
                var lines = responseText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                string title = "Grafik";
                List<string> labels = new List<string>();
                List<decimal> data = new List<decimal>();

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim().ToUpper();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "TITLE":
                            title = value;
                            break;
                        case "LABEL":
                            labels.AddRange(value.Split(',').Select(l => l.Trim()));
                            break;
                        case "DATA":
                            data.AddRange(value.Split(',').Select(d => decimal.TryParse(d.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num) ? num : 0));
                            break;
                    }
                }

                if (labels.Count == 0 || data.Count == 0 || labels.Count != data.Count)
                {
                    throw new Exception("Yapay zeka tutarsız veya eksik grafik verisi üretti.");
                }

                var chartJsPayload = new
                {
                    type = chartInfo.ChartJsType,
                    data = new
                    {
                        labels = labels,
                        datasets = new[]
                        {
                    new
                    {
                        label = title,
                        data = data
                    }
                }
                    }
                };

                return JsonConvert.SerializeObject(chartJsPayload);
            }
            catch (Exception ex)
            {
                throw new Exception("Grafik verisi işlenirken hata oluştu.", ex);
            }
        }

        private async Task<string> GenerateExplanationForDataAsync(string userPrompt, string jsonData)
        {
            var prompt = $"Veri: '{jsonData}'. Kullanıcı isteği: '{userPrompt}'. Bu veriyi özetleyen kısa, samimi bir giriş cümlesi oluştur.";
            var explanationBuilder = new StringBuilder();
            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(new List<(string, string)> { ("user", prompt) }, _systemPrompt))
            {
                explanationBuilder.Append(chunk);
            }
            return explanationBuilder.ToString().Trim();
        }

        private ChartTypeInfo FindChartType(string decision)
        {
            if (string.IsNullOrWhiteSpace(decision)) return null;
            return _chartTypes.FirstOrDefault(ct => ct.Keywords.Any(kw => decision.Contains(kw, StringComparison.OrdinalIgnoreCase)));
        }

        private string SanitizeAndExtractSql(string rawSqlOutput)
        {
            var match = Regex.Match(rawSqlOutput, "```sql\\s*(.*?)\\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : Regex.Replace(rawSqlOutput, @"\s+", " ").Trim();
        }

        private string SanitizeAndExtractJson(string rawJsonOutput)
        {
            var match = Regex.Match(rawJsonOutput, @"```json\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
            string extractedJson = match.Success ? match.Groups[1].Value : rawJsonOutput;
            int firstBracket = extractedJson.IndexOf('[');
            int firstBrace = extractedJson.IndexOf('{');
            if (firstBracket == -1 && firstBrace == -1) return string.Empty;
            int startIndex = (firstBracket > -1 && firstBrace > -1) ? Math.Min(firstBracket, firstBrace) : Math.Max(firstBracket, firstBrace);
            int lastBracket = extractedJson.LastIndexOf(']');
            int lastBrace = extractedJson.LastIndexOf('}');
            int endIndex = Math.Max(lastBracket, lastBrace);
            if (endIndex <= startIndex) return string.Empty;
            return extractedJson.Substring(startIndex, endIndex - startIndex + 1).Trim();
        }

        private string BuildSystemPrompt(DatabaseSchemaDescriptionDto schema)
        {
            var sb = new StringBuilder()
                .AppendLine("Sen, bir veritabanı uzmanı ve aynı zamanda kullanıcı dostu bir asistansın.")
                .AppendLine("Görevin, kullanıcıların doğal dilde sorduğu soruları anlayıp, aşağıdaki veritabanı şemasına göre SQL sorguları üretmek ve sonuçları onlara tercüme etmektir.")
                .AppendLine("\n--- KURALLAR ---")
                .AppendLine("1. YALNIZCA ve SADECE aşağıda sana verilen 'VERİTABANI ŞEMASI' içindeki tabloları ve sütunları kullanabilirsin. Asla bu şemanın dışında bir tablo veya sütun adı tahmin etme veya kullanma.")
                .AppendLine("2. SQL üretmen istendiğinde, YALNIZCA SQL kodunu ver. Başka hiçbir açıklama, yorum veya ` ``` ` bloğu ekleme.")
                .AppendLine("3. Bir SQL sonucunu yorumlaman ve açıklama metni oluşturman istendiğinde, sadece kısa ve samimi bir giriş cümlesi yaz.")
                .AppendLine("4. SQL sorgularında tablo ve sütun adlarını daima köşeli parantez içinde kullan: `[dbo].[Products]`.")
                .AppendLine("5. Kullanıcı metninde parantez içinde '(tür ID: değer)' formatında bir ifade görürsen, bu ifadeyi doğrudan bir WHERE koşuluna çevirmelisin. Bu, bir nesneye yapılan doğrudan referanstır.")
                .AppendLine("   - ÖRNEK 1: 'bana (müşteri ID: 123) adlı kişinin son siparişini göster' -> SORGUN ŞUNU İÇERMELİ: `WHERE [c].[CustomerId] = 123`")
                .AppendLine("   - ÖRNEK 2: 'içinde (ürün ID: 45) olan siparişler' -> SORGUN ŞUNU İÇERMELİ: `WHERE [oi].[ProductId] = 45`")
                .AppendLine("6. Kullanıcı 'grafik', 'chart', 'bar', 'pasta' gibi kelimeler kullanırsa, sunum formatını tablo yerine ilgili grafik olarak belirlemelisin.")
                .AppendLine("7. Bir grafik yanıtı oluşturman gerektiğinde, veriyi Chart.js uyumlu JSON formatına dönüştürerek sunmalısın. Yalnızca chartData JSON nesnesini döndür.")
                .AppendLine("8. Bir tablo verisi sunman gerektiğinde, ilk olarak benden gelen talimatla ham veriyi sadeleştirmelisin. Bu kural, sana özel olarak gönderilen talimatlarda detaylandırılmıştır.")
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