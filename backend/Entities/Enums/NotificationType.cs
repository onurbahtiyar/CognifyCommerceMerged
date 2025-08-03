using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Enums
{
    public enum NotificationType
    {
        Info = 1,
        Warning = 2,
        Critical = 3,
        Success = 4
    }

    public enum NotificationTopic
    {
        SupplyAlert = 1,      // Tedarik Uyarısı (Hızlı tükenen ürün)
        UnsoldProduct = 2,    // Satılmayan Ürün
        BadReviewCluster = 3, // Kötü Yorum Kümelenmesi
        LowStock = 4          // Düşük Stok Uyarısı
    }
}
