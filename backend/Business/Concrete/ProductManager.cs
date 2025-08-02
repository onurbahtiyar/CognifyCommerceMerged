using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IProductDal _productDal;
        private readonly IGeminiService _geminiService;
        private readonly ICategoryDal _categoryDal;

        public ProductManager(IProductDal productDal, IGeminiService geminiService, ICategoryDal categoryDal)
        {
            _productDal = productDal;
            _geminiService = geminiService;
            _categoryDal = categoryDal;
        }

        public IDataResult<ProductDto> Add(ProductAddDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.Now,
                IsActive = true,
                ImageUrl = dto.ImageUrl
            };
            _productDal.Add(product);

            var resultDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = _categoryDal.Get(c => c.CategoryId == dto.CategoryId).Name,
                CreatedDate = product.CreatedDate,
                ImageUrl = product.ImageUrl
            };
            return new SuccessDataResult<ProductDto>(resultDto, "Ürün eklendi.");
        }

        public IDataResult<List<ProductDto>> GetAll()
        {
            var list = _productDal.GetList(
                p => p.IsActive,
                q => q.Category)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name,
                    CreatedDate = p.CreatedDate,
                    ImageUrl = p.ImageUrl
                })
                .ToList();

            return new SuccessDataResult<List<ProductDto>>(list, "Ürünler listelendi.");
        }

        public IDataResult<ProductDto> GetById(int productId)
        {
            var p = _productDal.Get(p => p.ProductId == productId && p.IsActive);
            if (p == null)
                return new ErrorDataResult<ProductDto>(null, "Ürün bulunamadı.");

            var dto = new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CreatedDate = p.CreatedDate
            };
            return new SuccessDataResult<ProductDto>(dto, "Ürün getirildi.");
        }

        public IDataResult<ProductDto> Update(ProductUpdateDto dto)
        {
            var existing = _productDal.Get(p => p.ProductId == dto.ProductId && p.IsActive);
            if (existing == null)
                return new ErrorDataResult<ProductDto>(null, "Ürün bulunamadı.");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Stock = dto.Stock;
            existing.CategoryId = dto.CategoryId;

            _productDal.Update(existing);

            var updatedDto = new ProductDto
            {
                ProductId = existing.ProductId,
                Name = existing.Name,
                Description = existing.Description,
                Price = existing.Price,
                Stock = existing.Stock,
                CategoryId = existing.CategoryId,
                CreatedDate = existing.CreatedDate,
                ImageUrl = existing.ImageUrl
            };
            return new SuccessDataResult<ProductDto>(updatedDto, "Ürün güncellendi.");
        }

        public IResult Delete(int productId)
        {
            var product = _productDal.Get(p => p.ProductId == productId && p.IsActive);
            if (product == null)
                return new Result(false, "Ürün bulunamadı.");

            product.IsActive = false;
            _productDal.Update(product);
            return new SuccessResult("Ürün silindi.");
        }

        public async Task<IDataResult<PriceAnalysisResult>> GetPriceAnalysisAsync(string productName)
        {
            var systemPrompt = @"
Sen, bir ürün fiyat araştırma asistanısın. Görevin, sana verilen ürün adını Google'da aramak.
- Kesinlikle En Düşük Fiyat, En Yüksek Fiyat ve Ortalama Fiyat bilgilerini bulmalısın.
- Bu bilgileri ve ürün linklerini içeren bir tabloyu Markdown formatında döndürmelisin.
- Cevabın sadece istenen bilgileri içermeli, başka hiçbir ek açıklama veya giriş cümlesi yapmamalısın.
- Fiyat formatı '13.598,52 TL' gibi olmalı.
- Fiyatları ve tabloyu şu formatta sun:

En Düşük Fiyat: **12.500,00 TL**
En Yüksek Fiyat: **14.000,00 TL**
Ortalama Fiyat: **13.250,00 TL**

| Mağaza        | Link                                                  |
|---------------|-------------------------------------------------------|
| Örnek Mağaza  | [Ürünü Görüntüle](http://ornek.com/urun)             |
| Başka Mağaza  | [Ürünü Görüntüle](http://baska.com/urun)             |
";

            var history = new List<(string Role, string Content)>
        {
            ("user", productName)
        };

            var responseBuilder = new StringBuilder();
            var stream = _geminiService.StreamGenerateContentAsync(
                history: history,
                systemPrompt: systemPrompt,
                model: "gemini-1.5-pro-latest"
            );

            await foreach (var chunk in stream)
            {
                responseBuilder.Append(chunk);
            }

            var fullResponse = responseBuilder.ToString();

            // Gelen Markdown metnini yeni DTO yapısına parse et
            var result = ParsePriceAnalysisResponse(fullResponse);

            // Ham yanıtı modele ekle
            result.RawResponse = fullResponse;

            return new SuccessDataResult<PriceAnalysisResult>(result);
        }

        public async Task<IDataResult<string>> EnhanceImageTempAsync(byte[] uploadedImage, string instruction, string targetBackground = "white")
        {
            byte[] enhancedImage;
            try
            {
                enhancedImage = await _geminiService.EnhanceImageAsync(uploadedImage, instruction, targetBackground: targetBackground);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<string>(null, $"Görsel geliştirme başarısız: {ex.Message}");
            }

            var fileName = $"temp_product_{Guid.NewGuid()}.jpg";
            var saveDirectory = Path.Combine("wwwroot", "uploads", "temp-products");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, fileName);

            await File.WriteAllBytesAsync(savePath, enhancedImage);

            var publicUrl = $"/uploads/temp-products/{fileName}";

            return new SuccessDataResult<string>(publicUrl, "Geliştirilmiş görsel oluşturuldu ve geçici olarak kaydedildi.");
        }

        public async Task<IDataResult<string>> AttachTempImageToProductAsync(int productId, string tempImageUrl)
        {
            // Ürün var mı kontrolü
            var product = _productDal.Get(p => p.ProductId == productId && p.IsActive);
            if (product == null)
                return new ErrorDataResult<string>(null, "Ürün bulunamadı.");

            if (string.IsNullOrWhiteSpace(tempImageUrl))
                return new ErrorDataResult<string>(null, "Geçici görsel URL'si boş.");

            // Beklenen prefix: /uploads/temp-products/
            var expectedPrefix = "/uploads/temp-products/";
            if (!tempImageUrl.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                return new ErrorDataResult<string>(null, "Geçici görsel URL'si geçersiz.");

            // Fiziksel yol üret (path traversal koruması)
            var trimmed = tempImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar); // uploads\temp-products\...
            var tempFullPath = Path.GetFullPath(Path.Combine("wwwroot", trimmed));

            // wwwroot/uploads/temp-products içinde mi emin olalım
            var allowedDir = Path.GetFullPath(Path.Combine("wwwroot", "uploads", "temp-products"));
            if (!tempFullPath.StartsWith(allowedDir))
                return new ErrorDataResult<string>(null, "Geçici görsel konumu yetkisiz.");

            if (!File.Exists(tempFullPath))
                return new ErrorDataResult<string>(null, "Geçici görsel bulunamadı.");

            // Uzantıyı koru, ama güvenli uzantı kontrolü yap
            var extension = Path.GetExtension(tempFullPath)?.ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                return new ErrorDataResult<string>(null, "Görsel uzantısı desteklenmiyor.");

            // Kalıcı isim ve dizin
            var newFileName = $"product_{Guid.NewGuid()}{extension}";
            var permanentDir = Path.Combine("wwwroot", "uploads", "products");
            Directory.CreateDirectory(permanentDir);
            var permanentFullPath = Path.Combine(permanentDir, newFileName);

            try
            {
                // Taşı: önce kopyala, sonra orijinali silerek güvenli davranış
                File.Copy(tempFullPath, permanentFullPath);
                // İsteğe bağlı: temp dosyayı sil
                File.Delete(tempFullPath);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<string>(null, $"Görsel taşınırken hata oluştu: {ex.Message}");
            }

            // Ürüne ilişkilendir
            product.ImageUrl = $"/uploads/products/{newFileName}";
            _productDal.Update(product);

            return new SuccessDataResult<string>(product.ImageUrl!, "Görsel ürüne başarıyla ilişkilendirildi.");
        }


        private PriceAnalysisResult ParsePriceAnalysisResponse(string markdownResponse)
        {
            var result = new PriceAnalysisResult();
            var lines = markdownResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Fiyatları string olarak (örn: "12.500,00 TL") yakalamak için Regex
            var priceRegex = new Regex(@":\s*\**\s*([\d.,\s]+TL)\**", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                // Fiyat satırlarını işle
                if (line.StartsWith("En Düşük Fiyat", StringComparison.OrdinalIgnoreCase))
                {
                    var match = priceRegex.Match(line);
                    if (match.Success)
                        result.MinPrice = match.Groups[1].Value.Trim();
                }
                else if (line.StartsWith("En Yüksek Fiyat", StringComparison.OrdinalIgnoreCase))
                {
                    var match = priceRegex.Match(line);
                    if (match.Success)
                        result.MaxPrice = match.Groups[1].Value.Trim();
                }
                else if (line.StartsWith("Ortalama Fiyat", StringComparison.OrdinalIgnoreCase))
                {
                    var match = priceRegex.Match(line);
                    if (match.Success)
                        result.AveragePrice = match.Groups[1].Value.Trim();
                }
                // Markdown tablosundaki satırları işle
                else if (line.Trim().StartsWith("|") && !line.Contains("---"))
                {
                    var rowRegex = new Regex(@"\|\s*(.*?)\s*\|\s*\[.*?\]\((.*?)\)\s*\|");
                    var match = rowRegex.Match(line);

                    if (match.Success && match.Groups.Count == 3)
                    {
                        var storeName = match.Groups[1].Value.Trim();
                        var url = match.Groups[2].Value.Trim();

                        // Başlık satırını atla ("| Mağaza | Link |")
                        if (!storeName.Equals("Mağaza", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Retailers.Add(new RetailerInfo
                            {
                                StoreName = storeName,
                                Url = url
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
