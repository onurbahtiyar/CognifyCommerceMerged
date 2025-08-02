export enum OrderStatus {
    PendingConfirmation = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Returned = 5,
}

export const OrderStatusMap = new Map<number, string>([
    [OrderStatus.PendingConfirmation, 'Onay Bekliyor'],
    [OrderStatus.Processing, 'Hazırlanıyor'],
    [OrderStatus.Shipped, 'Kargoya Verildi'],
    [OrderStatus.Delivered, 'Teslim Edildi'],
    [OrderStatus.Cancelled, 'İptal Edildi'],
    [OrderStatus.Returned, 'İade Edildi'],
]);