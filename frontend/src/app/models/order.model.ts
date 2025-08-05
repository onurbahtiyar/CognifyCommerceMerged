// Sipariş listesi ve detayında kullanılacak ana DTO
export interface OrderDto {
  orderId: number;
  customerId: number;
  customerFullName: string;
  orderDate: Date;
  totalAmount: number;
  orderStatus: string;
  orderStatusCode: number;
  shipperName?: string;
  shippingTrackingCode?: string;
  shippingAddress: string;
  items: OrderItemDto[];
}

// Sipariş içindeki her bir ürün kalemi
export interface OrderItemDto {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

// Yeni sipariş oluştururken API'ye gönderilecek DTO
export interface OrderAddDto {
  customerId: number;
  shippingAddress: string;
  items: OrderAddItemDto[];
}

export interface OrderAddItemDto {
  productId: number;
  quantity: number;
}

// Kargo bilgilerini güncelleme DTO'su
export interface OrderUpdateShippingDto {
  orderId: number;
  shipperId: number;
  shippingTrackingCode: string;
}

// Sipariş durumunu güncelleme DTO'su
export interface OrderUpdateStatusDto {
  orderId: number;
  newStatus: number;
}

// Kargo firması DTO'su
export interface ShipperDto {
  shipperId: number;
  companyName: string;
}