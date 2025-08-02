using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Enums
{
    public enum OrderStatus
    {
        PendingConfirmation = 0, // Onay Bekliyor
        Processing = 1,          // Hazırlanıyor
        Shipped = 2,             // Kargoya Verildi
        Delivered = 3,           // Teslim Edildi
        Cancelled = 4,           // İptal Edildi
        Returned = 5             // İade Edildi
    }
}
