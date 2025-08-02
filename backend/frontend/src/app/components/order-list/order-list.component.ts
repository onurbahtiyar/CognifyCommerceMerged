import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
// Servisler
import { OrderService } from 'src/app/services/order.service';
import { CustomerService } from 'src/app/services/customer.service';
import { ProductService, ProductDto } from 'src/app/services/product.service'; // ProductDto import'u düzeltildi
// Modeller ve Enumlar
import { OrderDto, OrderAddDto, ShipperDto, OrderUpdateShippingDto, OrderUpdateStatusDto } from 'src/app/models/order.model';
import { OrderStatus, OrderStatusMap } from 'src/app/models/order-status.enum';
import { CustomerDto } from 'src/app/models/customer.model';

interface CartItem extends ProductDto {
  quantity: number;
  originalStock: number;
}

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
})
export class OrderListComponent implements OnInit {
  // Liste ve Yükleme Durumları
  loading = true;
  orders: OrderDto[] = [];
  
  // Panel Yönetimi
  isPanelOpen = false;
  panelMode: 'add' | 'view' = 'view';
  
  // Detay Görüntüleme için
  selectedOrder: OrderDto | null = null;
  shippers: ShipperDto[] = [];
  updateShippingDto: OrderUpdateShippingDto = { orderId: 0, shipperId: 0, shippingTrackingCode: '' };
  updateStatusDto: OrderUpdateStatusDto = { orderId: 0, newStatus: 0 };
  orderStatusList = Array.from(OrderStatusMap.entries()).map(([key, value]) => ({ key, value }));

  // Yeni Sipariş Oluşturma için
  allCustomers: CustomerDto[] = [];
  filteredCustomers: CustomerDto[] = [];
  customerSearchText = '';
  
  allProducts: ProductDto[] = [];
  filteredProducts: ProductDto[] = [];
  productSearchText = '';
  
  newOrder: Partial<OrderAddDto> = { customerId: 0, shippingAddress: '', items: [] };
  cartItems: CartItem[] = [];
  cartTotal = 0;

  constructor(
    private orderService: OrderService,
    private customerService: CustomerService,
    private productService: ProductService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadInitialData();
  }

  loadInitialData(): void {
    this.loading = true;
    Promise.all([
      this.orderService.getAllOrders().toPromise(),
      this.customerService.getAllCustomers().toPromise(),
      this.productService.getAllProducts().toPromise(),
      this.orderService.getAllActiveShippers().toPromise()
    ]).then(([ordersRes, customersRes, productsRes, shippersRes]) => {
      this.orders = ordersRes?.data || [];
      this.allCustomers = customersRes?.data || [];
      this.allProducts = productsRes?.data || [];
      this.shippers = shippersRes?.data || [];
    }).catch(err => {
      this.toastr.error('Başlangıç verileri yüklenirken bir hata oluştu.', 'Hata');
    }).finally(() => {
      this.loading = false;
    });
  }

  // Panel Yönetimi
  openAddPanel(): void {
    this.resetAddForm();
    this.panelMode = 'add';
    this.isPanelOpen = true;
  }

  openViewPanel(order: OrderDto): void {
    this.selectedOrder = order;
    this.updateShippingDto = { orderId: order.orderId, shipperId: 0, shippingTrackingCode: '' };
    this.updateStatusDto = { orderId: order.orderId, newStatus: order.orderStatusCode };
    this.panelMode = 'view';
    this.isPanelOpen = true;
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedOrder = null;
  }

  // Yeni Sipariş Metotları
  resetAddForm() {
    this.newOrder = { customerId: 0, shippingAddress: '', items: [] };
    this.cartItems = [];
    this.cartTotal = 0;
    this.customerSearchText = '';
    this.productSearchText = '';
    this.filteredCustomers = [];
    this.filteredProducts = [];
  }

  filterCustomers() {
    if (!this.customerSearchText) {
      this.filteredCustomers = [];
      return;
    }
    const search = this.customerSearchText.toLowerCase();
    this.filteredCustomers = this.allCustomers.filter(c =>
      c.firstName.toLowerCase().includes(search) ||
      c.lastName.toLowerCase().includes(search) ||
      c.email.toLowerCase().includes(search)
    );
  }

  selectCustomer(customer: CustomerDto) {
    this.newOrder.customerId = customer.customerId;
    this.newOrder.shippingAddress = customer.address || '';
    this.customerSearchText = '';
    this.filteredCustomers = [];
  }

  getSelectedCustomerName(): string {
    const customer = this.allCustomers.find(c => c.customerId === this.newOrder.customerId);
    return customer ? `${customer.firstName} ${customer.lastName}` : '';
  }

  filterProducts() {
    if (!this.productSearchText) {
      this.filteredProducts = [];
      return;
    }
    const search = this.productSearchText.toLowerCase();
    this.filteredProducts = this.allProducts.filter(p => p.name.toLowerCase().includes(search) && p.stock > 0);
  }

  addProductToCart(product: ProductDto) {
    const existingItem = this.cartItems.find(item => item.productId === product.productId);
    if (existingItem) {
      if (existingItem.quantity < product.stock) {
        existingItem.quantity++;
      } else {
        this.toastr.warning('Maksimum stok adedine ulaşıldı.', 'Uyarı');
      }
    } else {
      this.cartItems.push({ ...product, quantity: 1, originalStock: product.stock });
    }
    this.productSearchText = '';
    this.calculateTotal();
  }

  removeFromCart(productId: number) {
    this.cartItems = this.cartItems.filter(item => item.productId !== productId);
    this.calculateTotal();
  }

  calculateTotal() {
    this.cartTotal = this.cartItems.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  }

  createOrder() {
    if(!this.newOrder.customerId || this.cartItems.length === 0 || !this.newOrder.shippingAddress) {
      this.toastr.error('Lütfen tüm gerekli alanları doldurun.', 'Eksik Bilgi');
      return;
    }
    const orderData: OrderAddDto = {
      customerId: this.newOrder.customerId!,
      shippingAddress: this.newOrder.shippingAddress!,
      items: this.cartItems.map(item => ({ productId: item.productId, quantity: item.quantity }))
    };

    this.orderService.addOrder(orderData).subscribe({
      next: res => {
        if(res.success && res.data) {
          this.toastr.success(`#${res.data.orderId} numaralı sipariş başarıyla oluşturuldu.`, 'Başarılı');
          this.orders.unshift(res.data);
          this.allProducts.forEach(p => {
             const cartItem = this.cartItems.find(ci => ci.productId === p.productId);
             if(cartItem) p.stock -= cartItem.quantity;
          });
          this.closePanel();
        } else {
          this.toastr.error(res.message, 'Hata');
        }
      },
      error: err => this.toastr.error(err.error.message || 'Sipariş oluşturulurken bir sunucu hatası oluştu.', 'Hata')
    });
  }

  // Detay Görüntüleme Metotları
  updateShippingInfo() {
    this.orderService.updateShipping(this.updateShippingDto).subscribe({
      next: res => {
        if(res.success) {
          this.toastr.success(res.message, 'Başarılı');
          this.loadInitialData();
          this.closePanel();
        } else {
          this.toastr.error(res.message, 'Hata');
        }
      },
      error: err => this.toastr.error(err.error.message || 'Sunucu hatası.', 'Hata')
    });
  }

  changeStatus() {
     this.orderService.updateStatus(this.updateStatusDto).subscribe({
      next: res => {
        if(res.success) {
          this.toastr.success(res.message, 'Başarılı');
          this.loadInitialData();
          this.closePanel();
        } else {
          this.toastr.error(res.message, 'Hata');
        }
      },
      error: err => this.toastr.error(err.error.message || 'Sunucu hatası.', 'Hata')
    });
  }

  canBeCancelled(status: number): boolean {
    return status !== OrderStatus.Delivered && status !== OrderStatus.Shipped && status !== OrderStatus.Cancelled;
  }
  
  confirmCancel() {
    if(!this.selectedOrder) return;
    if(confirm(`#${this.selectedOrder.orderId} numaralı siparişi iptal etmek istediğinizden emin misiniz? Bu işlem geri alınamaz ve stoklar iade edilir.`)) {
        this.orderService.cancelOrder(this.selectedOrder.orderId).subscribe({
            next: res => {
                 if(res.success) {
                    this.toastr.success(res.message, 'Başarılı');
                    this.loadInitialData();
                    this.closePanel();
                 } else {
                    this.toastr.error(res.message, 'Hata');
                 }
            },
            error: err => this.toastr.error(err.error.message || 'Sunucu hatası.', 'Hata')
        });
    }
  }

  // Yardımcı Metotlar
  getStatusClass(status: number): string {
    switch (status) {
      case OrderStatus.PendingConfirmation: return 'bg-yellow-400/20 text-yellow-600';
      case OrderStatus.Processing: return 'bg-blue-400/20 text-blue-600';
      case OrderStatus.Shipped: return 'bg-indigo-400/20 text-indigo-600';
      case OrderStatus.Delivered: return 'bg-green-400/20 text-green-600';
      case OrderStatus.Cancelled:
      case OrderStatus.Returned: return 'bg-red-400/20 text-red-600';
      default: return 'bg-gray-400/20 text-gray-600';
    }
  }

  getOrderStatusText(statusCode: number): string {
    return OrderStatusMap.get(statusCode) || 'Bilinmeyen Durum';
  }
}