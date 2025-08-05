using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Business.Concrete
{
    public class ProductReviewManager : IProductReviewService
    {
        private readonly IProductReviewDal _productReviewDal;
        private readonly ICustomerDal _customerDal;
        private readonly IGeminiService _geminiService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProductReviewManager(IProductReviewDal productReviewDal, ICustomerDal customerDal, IGeminiService geminiService, IServiceScopeFactory scopeFactory)
        {
            _productReviewDal = productReviewDal;
            _customerDal = customerDal;
            _geminiService = geminiService;
            _scopeFactory = scopeFactory;
        }

        public IResult Add(ProductReviewAddDto dto)
        {
            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                CustomerId = dto.CustomerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedDate = System.DateTime.Now,
                IsApproved = false,
                IsDeleted = false
            };
            _productReviewDal.Add(review);

            _ = GenerateAndSaveAutomatedReplyAsync(review);

            return new SuccessResult("Yorumunuz onaya gönderildi. Teşekkür ederiz.");
        }

        public IResult AddAdminReply(AdminReplyAddDto dto)
        {
            var parentReview = _productReviewDal.Get(r => r.ProductReviewId == dto.ParentReviewId);
            if (parentReview == null)
            {
                return new Result(false, "Yanıtlanacak ana yorum bulunamadı.");
            }

            var reply = new ProductReview
            {
                ProductId = parentReview.ProductId,
                CustomerId = dto.CustomerId,
                ParentReviewId = dto.ParentReviewId,
                Comment = dto.Comment,
                Rating = 0,
                CreatedDate = System.DateTime.Now,
                IsApproved = true,
                IsDeleted = false
            };
            _productReviewDal.Add(reply);
            return new SuccessResult("Yorum başarıyla yanıtlandı.");
        }

        public IResult ApproveReview(int reviewId)
        {
            var review = _productReviewDal.Get(r => r.ProductReviewId == reviewId);
            if (review == null)
            {
                return new Result(false, "Onaylanacak yorum bulunamadı.");
            }
            review.IsApproved = true;
            _productReviewDal.Update(review);
            return new SuccessResult("Yorum onaylandı.");
        }

        public IResult Delete(int reviewId)
        {
            var review = _productReviewDal.Get(r => r.ProductReviewId == reviewId);
            if (review == null)
            {
                return new Result(false, "Silinecek yorum bulunamadı.");
            }
            review.IsDeleted = true;
            _productReviewDal.Update(review);
            return new SuccessResult("Yorum silindi.");
        }

        public IDataResult<List<ProductReviewDto>> GetByProductId(int productId)
        {
            var reviews = _productReviewDal.GetList(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted,
                q => q.Customer, q => q.Replies
            );

            var topLevelReviews = reviews.Where(r => r.ParentReviewId == null).ToList();

            var resultDto = topLevelReviews.Select(r => new ProductReviewDto
            {
                ReviewId = r.ProductReviewId,
                CustomerName = r.Customer?.FirstName + " " + r.Customer?.LastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate,
                IsApproved = r.IsApproved,
                Replies = reviews.Where(reply => reply.ParentReviewId == r.ProductReviewId)
                                 .Select(reply => new ProductReviewDto
                                 {
                                     ReviewId = reply.ProductReviewId,
                                     CustomerName = "Mağaza Yanıtı",
                                     Rating = reply.Rating,
                                     Comment = reply.Comment,
                                     CreatedDate = reply.CreatedDate,
                                     IsApproved = reply.IsApproved
                                 }).ToList()
            }).ToList();

            return new SuccessDataResult<List<ProductReviewDto>>(resultDto, "Yorumlar listelendi.");
        }

        public IDataResult<List<ProductReviewDto>> GetUnapprovedReviews()
        {
            var reviews = _productReviewDal.GetList(
                r => !r.IsApproved && !r.IsDeleted,
                q => q.Customer
            ).Select(r => new ProductReviewDto
            {
                ReviewId = r.ProductReviewId,
                CustomerName = r.Customer?.FirstName + " " + r.Customer?.LastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate
            }).ToList();

            return new SuccessDataResult<List<ProductReviewDto>>(reviews, "Onay bekleyen yorumlar listelendi.");
        }

        public IDataResult<List<ProductReviewDto>> GetAll()
        {
            var allReviews = _productReviewDal.GetList(
                r => !r.IsDeleted,
                q => q.Product, q => q.Customer
            );

            var replies = allReviews.Where(r => r.ParentReviewId != null).ToList();

            var resultDto = allReviews
                .Where(r => r.ParentReviewId == null)
                .Select(r => new ProductReviewDto
                {
                    ReviewId = r.ProductReviewId,
                    CustomerName = r.Customer?.FirstName + " " + r.Customer?.LastName,
                    ProductName = r.Product?.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate,
                    IsApproved = r.IsApproved,
                    Replies = replies
                        .Where(reply => reply.ParentReviewId == r.ProductReviewId)
                        .Select(reply => new ProductReviewDto
                        {
                            ReviewId = reply.ProductReviewId,
                            CustomerName = "Mağaza Yanıtı",
                            Comment = reply.Comment,
                            CreatedDate = reply.CreatedDate,
                            IsApproved = true
                        }).ToList()
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return new SuccessDataResult<List<ProductReviewDto>>(resultDto, "Tüm yorumlar listelendi.");
        }

        private async Task GenerateAndSaveAutomatedReplyAsync(ProductReview customerReview)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var scopedProductReviewDal = scope.ServiceProvider.GetRequiredService<IProductReviewDal>();
                    var scopedGeminiService = scope.ServiceProvider.GetRequiredService<IGeminiService>();

                    var systemPrompt = @"
Sen, bir e-ticaret mağazasının müşteri hizmetleri asistanısın. Görevin, müşterilerin ürünlere yaptığı yorumlara profesyonel, empatik ve samimi bir dille yanıt vermektir.
Sana müşterinin verdiği Puan (1-5 arası) ve yaptığı Yorum metni verilecek.
KURALLAR:
- Yüksek Puan (4-5 Yıldız): Müşteriye olumlu geri bildirimi için teşekkür et. Yorumundan bir detayı (eğer varsa) överek yanıtını kişiselleştir.
- Düşük Puan (1-2 Yıldız): Müşterinin yaşadığı olumsuz deneyim için özür dile. Empati kurduğunu göster ve sorunu çözmek için yardıma hazır olduğunu belirt. Asla savunmacı olma.
- Orta Puan (3 Yıldız): Geri bildirimi için teşekkür et. Deneyimini daha iyi hale getirmek için ne yapabileceğimizi soran veya geri bildirimini dikkate alacağımızı belirten nazik bir ifade kullan.
- Kısa ve Öz Ol: Yanıtların 2-3 cümleyi geçmesin.
- Kapanış: Yanıtını 'İyi günler dileriz.' gibi nazik bir kapanış cümlesiyle bitir.
- Dil: Yanıtların mutlaka Türkçe olmalıdır.
- Format: Sadece ve sadece yanıt metnini döndür. Giriş veya açıklama yapma.

**ÖZEL DURUM: Konu Dışı ve Uygunsuz Yorumlar**
Eğer yorum ürünle ilgili değilse, özellikle bir çalışanımızın dış görünüşü hakkında övgü içeriyorsa veya telefon numarası gibi kişisel bilgilerini istiyorsa, bu özel kuralı uygula:
1.  Övgüyü nazikçe ve esprili bir dille kabul et.
2.  Çalışanlarımızın kişisel bilgilerini koruma politikamız gereği kesinlikle paylaşamayacağını belirt. Bu kısmı esprili ama net bir şekilde ifade et.
3.  Konuyu tekrar ürüne veya hizmete yönlendirerek profesyonel bir kapanış yap.

**Örnek Senaryo:**
- **Müşteri Yorumu:** 'Kuryeniz Onur Bey çok yakışıklıydı, numarasını alabilir miyim?'
- **İstenen Yanıt:** 'Onur Bey'in karizmasıyla sizi de etkilediğini duyduğumuza sevindik! Kendisi bizim de gözbebeğimiz. Ancak şirket politikamız gereği, süper kahramanlarımızın gizli kimliklerini ve telefon numaralarını sır gibi saklıyoruz. :) Yine de ürünümüzle ilgili düşüncelerinizi de duymayı çok isteriz! İyi günler dileriz.'
";

                    var userInput = $"Puan: {customerReview.Rating}/5\nYorum: \"{customerReview.Comment}\"";
                    var history = new List<(string Role, string Content)> { ("user", userInput) };

                    var responseBuilder = new StringBuilder();
                    var stream = scopedGeminiService.StreamGenerateContentAsync(
                        history: history,
                        systemPrompt: systemPrompt,
                        model: "gemini-1.5-flash-latest"
                    );

                    await foreach (var chunk in stream)
                    {
                        responseBuilder.Append(chunk);
                    }

                    var geminiReplyText = responseBuilder.ToString().Trim();

                    if (!string.IsNullOrWhiteSpace(geminiReplyText))
                    {
                        const int adminCustomerId = 1;
                        var replyEntity = new ProductReview
                        {
                            ProductId = customerReview.ProductId,
                            ParentReviewId = customerReview.ProductReviewId,
                            CustomerId = adminCustomerId,
                            Comment = geminiReplyText,
                            Rating = 0,
                            CreatedDate = DateTime.Now,
                            IsApproved = true,
                            IsDeleted = false
                        };

                        scopedProductReviewDal.Add(replyEntity);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Otomatik yorum yanıtlama hatası: {ex.Message}");
                }
            } 
        }

    }
}
