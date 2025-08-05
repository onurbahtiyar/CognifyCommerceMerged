using Core.Utilities.Result.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface INotificationService
    {
        Task<IResult> CreateNotificationAsync(Notification notification);
        Task<IDataResult<List<NotificationDto>>> GetUnreadNotificationsAsync();
        Task<IResult> MarkAsReadAsync(int notificationId);
        Task<IResult> MarkAllAsReadAsync();
        Task<bool> DoesSimilarNotificationExistAsync(NotificationTopic topic, int relatedEntityId, TimeSpan since);
    }
}
