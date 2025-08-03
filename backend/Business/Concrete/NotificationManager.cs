using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class NotificationManager : INotificationService
    {
        private readonly INotificationDal _notificationDal;

        public NotificationManager(INotificationDal notificationDal)
        {
            _notificationDal = notificationDal;
        }

        public async Task<IResult> CreateNotificationAsync(Notification notification)
        {
            await _notificationDal.AddAsync(notification);
            return new SuccessResult("Bildirim oluşturuldu.");
        }

        public async Task<IDataResult<List<NotificationDto>>> GetUnreadNotificationsAsync()
        {
            var notifications = await _notificationDal.GetListAsync(n => !n.IsRead);
            var dtos = notifications.OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Topic = n.Topic.ToString(),
                    Type = n.Type.ToString(),
                    Message = n.Message,
                    RelatedEntityId = n.RelatedEntityId,
                    Url = n.Url,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead
                }).ToList();
            return new SuccessDataResult<List<NotificationDto>>(dtos);
        }

        public async Task<IResult> MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationDal.GetAsync(n => n.NotificationId == notificationId);
            if (notification == null) return new Result(false, "Bildirim bulunamadı.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _notificationDal.UpdateAsync(notification);
            }
            return new SuccessResult("Bildirim okundu olarak işaretlendi.");
        }

        public async Task<IResult> MarkAllAsReadAsync()
        {
            var unreadNotifications = await _notificationDal.GetListAsync(n => !n.IsRead);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _notificationDal.UpdateAsync(notification);
            }
            return new SuccessResult("Tüm bildirimler okundu olarak işaretlendi.");
        }

        public async Task<bool> DoesSimilarNotificationExistAsync(NotificationTopic topic, int relatedEntityId, TimeSpan since)
        {
            var thresholdDate = DateTime.Now.Subtract(since);
            var exists = await _notificationDal.GetAsync(n =>
                n.Topic == topic &&
                n.RelatedEntityId == relatedEntityId &&
                n.CreatedDate >= thresholdDate);

            return exists != null;
        }
    }
}
