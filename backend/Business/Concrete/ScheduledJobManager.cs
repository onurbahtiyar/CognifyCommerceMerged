using Business.Abstract;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ScheduledJobManager : IScheduledJobService
    {
        private readonly IProductDal _productDal;
        private readonly IOrderItemDal _orderItemDal;
        private readonly IProductReviewDal _productReviewDal;
        private readonly INotificationService _notificationService;
        private readonly IGeminiService _geminiService;

        public ScheduledJobManager(
            IProductDal productDal,
            IOrderItemDal orderItemDal,
            IProductReviewDal productReviewDal,
            INotificationService notificationService,
            IGeminiService geminiService)
        {
            _productDal = productDal;
            _orderItemDal = orderItemDal;
            _productReviewDal = productReviewDal;
            _notificationService = notificationService;
            _geminiService = geminiService;
        }

        public async Task CheckForSupplyAlertsAsync()
        {
            const int daysToAnalyze = 7;
            var products = await _productDal.GetListAsync(p => p.IsActive && p.Stock > 0);
            var thresholdDate = DateTime.Now.AddDays(-daysToAnalyze);

            var systemPrompt = @"
Sen, bir e-ticaret şirketinde deneyimli bir stok yönetimi analistisin. Görevin, sana sunulan ürün verilerini analiz edip kritik bir tedarik durumu olup olmadığına karar vermektir.
- Eğer durum acil bir tedarik gerektiriyorsa (örn: stok hızla tükeniyorsa), yönetici için kısa, net ve aksiyona yönelik bir bildirim metni oluştur.
- Eğer durum acil değilse veya normal seyrinde ise, YALNIZCA 'DURUM_OK' yanıtını ver. Başka hiçbir açıklama yapma.
- Bildirim metinlerin profesyonel ve teşvik edici olmalı.";

            foreach (var product in products)
            {
                var recentSales = await _orderItemDal.GetListAsync(oi => oi.Product.ProductId == product.ProductId && oi.Order.OrderDate >= thresholdDate);
                var totalSold = recentSales.Sum(oi => oi.Quantity);

                if (totalSold == 0) continue;

                var dailySalesRate = (double)totalSold / daysToAnalyze;
                var stockDaysLeft = (dailySalesRate > 0) ? product.Stock / dailySalesRate : 1000;

                var userInput = $@"
                    Ürün Analizi:
                    - Ürün Adı: '{product.Name}'
                    - Mevcut Stok: {product.Stock} adet
                    - Son {daysToAnalyze} Günde Satılan: {totalSold} adet
                    - Günlük Ortalama Satış: {dailySalesRate:F2} adet/gün
                    - Tahmini Tükenme Süresi: {stockDaysLeft:F1} gün
                    Lütfen bu veriyi analiz et ve kararını bildir.";

                var aiResponse = await GetAiAnalysisResponseAsync(systemPrompt, userInput);

                if (!string.IsNullOrWhiteSpace(aiResponse) && !aiResponse.Trim().Equals("DURUM_OK", StringComparison.OrdinalIgnoreCase))
                {
                    bool alreadyNotified = await _notificationService.DoesSimilarNotificationExistAsync(NotificationTopic.SupplyAlert, product.ProductId, TimeSpan.FromDays(3));
                    if (!alreadyNotified)
                    {
                        var notification = new Notification
                        {
                            Topic = NotificationTopic.SupplyAlert,
                            Type = NotificationType.Critical,
                            Message = aiResponse,
                            RelatedEntityId = product.ProductId,
                            Url = $"/admin/products/edit/{product.ProductId}"
                        };
                        await _notificationService.CreateNotificationAsync(notification);
                    }
                }
            }
        }

        public async Task CheckForUnsoldProductsAsync()
        {
            const int unsoldDaysThreshold = 30;
            var thresholdDate = DateTime.Now.AddDays(-unsoldDaysThreshold);

            var soldProductIds = _orderItemDal.GetList(oi => oi.Order.OrderDate >= thresholdDate).Select(oi => oi.ProductId).Distinct().ToHashSet();
            var unsoldProducts = await _productDal.GetListAsync(p => p.IsActive && p.Stock > 0 && !soldProductIds.Contains(p.ProductId));

            var systemPrompt = @"
Sen, bir e-ticaret firmasında pazarlama stratejistisin. Görevin, uzun süredir satılmayan ürünler için yaratıcı ve teşvik edici önerilerde bulunmaktır.
- Sana sunulan ürün bilgisine bakarak, yöneticiye ürünü canlandırmak için bir kampanya, indirim veya tanıtım yapmasını öneren kısa bir bildirim metni oluştur.
- Eğer bir önerin yoksa veya ürünün durumu normal ise, YALNIZCA 'DURUM_OK' yanıtını ver. Başka hiçbir açıklama yapma.";

            foreach (var product in unsoldProducts)
            {
                var userInput = $@"
                    Pazarlama Analizi:
                    - Ürün Adı: '{product.Name}'
                    - Mevcut Stok: {product.Stock} adet
                    - Durum: Bu ürün en az {unsoldDaysThreshold} gündür hiç satılmadı.
                    Bu ürünün satışlarını artırmak için yöneticiye ne önerebilirsin?";

                var aiResponse = await GetAiAnalysisResponseAsync(systemPrompt, userInput);

                if (!string.IsNullOrWhiteSpace(aiResponse) && !aiResponse.Trim().Equals("DURUM_OK", StringComparison.OrdinalIgnoreCase))
                {
                    bool alreadyNotified = await _notificationService.DoesSimilarNotificationExistAsync(NotificationTopic.UnsoldProduct, product.ProductId, TimeSpan.FromDays(15));
                    if (!alreadyNotified)
                    {
                        var notification = new Notification
                        {
                            Topic = NotificationTopic.UnsoldProduct,
                            Type = NotificationType.Warning,
                            Message = aiResponse,
                            RelatedEntityId = product.ProductId,
                            Url = $"/admin/products/edit/{product.ProductId}"
                        };
                        await _notificationService.CreateNotificationAsync(notification);
                    }
                }
            }
        }

        public async Task CheckForBadReviewClustersAsync()
        {
            const int badReviewDays = 7;
            const int badRating = 2;

            var systemPrompt = @"
Sen, bir müşteri memnuniyeti ve kalite kontrol yöneticisisin. Görevin, bir ürün hakkında gelen kötü yorumları analiz ederek ortak bir sorun olup olmadığını tespit etmektir.
- Yorumları incele. Eğer yorumlarda tekrar eden bir tema (örn: kargo hasarı, kalitesizlik, yanlış ürün) varsa, bu sorunu özetleyen ve yöneticiyi incelemeye davet eden bir bildirim metni oluştur.
- Eğer yorumlar birbiriyle alakasız veya münferit olaylar gibi görünüyorsa, YALNIZCA 'DURUM_OK' yanıtını ver. Başka hiçbir açıklama yapma.";

            var productsWithRecentBadReviews = (await _productReviewDal.GetListAsync(
                r => !r.IsDeleted && r.Rating <= badRating && r.CreatedDate >= DateTime.Now.AddDays(-badReviewDays),
                r => r.Product))
                .Where(r => r.Product != null)
                .GroupBy(r => r.Product);

            foreach (var group in productsWithRecentBadReviews)
            {
                var product = group.Key;
                var comments = group.Select(r => $"- (Puan: {r.Rating}/5) \"{r.Comment}\"").ToList();

                if (comments.Count < 2) continue;

                var userInput = $@"
                    Kalite Kontrol Analizi:
                    - Ürün Adı: '{product.Name}'
                    - Son {badReviewDays} günde gelen kötü yorumlar ({comments.Count} adet):
                    {string.Join("\n", comments)}
                    Bu yorumlarda ortak bir sorun deseni görüyor musun? Yöneticiye bir uyarı göndermeli miyiz?";

                var aiResponse = await GetAiAnalysisResponseAsync(systemPrompt, userInput);

                if (!string.IsNullOrWhiteSpace(aiResponse) && !aiResponse.Trim().Equals("DURUM_OK", StringComparison.OrdinalIgnoreCase))
                {
                    bool alreadyNotified = await _notificationService.DoesSimilarNotificationExistAsync(NotificationTopic.BadReviewCluster, product.ProductId, TimeSpan.FromDays(7));
                    if (!alreadyNotified)
                    {
                        var notification = new Notification
                        {
                            Topic = NotificationTopic.BadReviewCluster,
                            Type = NotificationType.Warning,
                            Message = aiResponse,
                            RelatedEntityId = product.ProductId,
                            Url = $"/admin/products/reviews/{product.ProductId}"
                        };
                        await _notificationService.CreateNotificationAsync(notification);
                    }
                }
            }
        }

        private async Task<string> GetAiAnalysisResponseAsync(string systemPrompt, string userInput)
        {
            var history = new List<(string Role, string Content)> { ("user", userInput) };
            var responseBuilder = new StringBuilder();

            await foreach (var chunk in _geminiService.StreamGenerateContentAsync(
                history,
                systemPrompt,
                "gemini-1.5-flash-latest"))
            {
                responseBuilder.Append(chunk);
            }

            return responseBuilder.ToString().Trim();
        }
    }
}