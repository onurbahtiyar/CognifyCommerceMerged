import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ApiResponse } from 'src/app/models/api-response.model';
import { CustomerDto, CustomerAddDto, CustomerUpdateDto } from 'src/app/models/customer.model';
import { CustomerService } from 'src/app/services/customer.service';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
})
export class CustomerListComponent implements OnInit {
  @ViewChild('customerForm') customerForm: NgForm | undefined;

  customers: CustomerDto[] = [];
  
  isPanelOpen = false;
  panelMode: 'add' | 'edit' = 'add';
  loading = true;

  // Ekleme ve düzenleme için ortak, aktif model
  activeCustomer: any = {};

  constructor(
    private customerService: CustomerService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.loading = true;
    this.customerService.getAllCustomers().subscribe({
      next: (res) => {
        this.customers = res.success ? (res.data || []) : [];
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('Müşteriler yüklenirken bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }

  // --- Panel Yönetimi ---
  openAddPanel(): void {
    this.panelMode = 'add';
    this.activeCustomer = {} as CustomerAddDto; // Boş model
    this.isPanelOpen = true;
  }

  openEditPanel(customer: CustomerDto): void {
    this.panelMode = 'edit';
    this.activeCustomer = { ...customer }; // Klonlayarak orijinal veriyi koru
    this.isPanelOpen = true;
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.customerForm?.resetForm();
  }

  // --- CRUD İşlemleri ---
  saveCustomer(): void {
    if (this.customerForm?.invalid) {
      this.toastr.warning('Lütfen gerekli tüm alanları doldurun.', 'Uyarı');
      return;
    }

    if (this.panelMode === 'add') {
      this.addCustomer();
    } else {
      this.updateCustomer();
    }
  }
  
  private addCustomer(): void {
    this.customerService.addCustomer(this.activeCustomer).subscribe({
      next: (res: ApiResponse<CustomerDto>) => {
        if (res.success && res.data) {
          this.customers.unshift(res.data); // Yeni müşteriyi listenin başına ekle
          this.toastr.success('Müşteri başarıyla eklendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Müşteri eklenemedi.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  private updateCustomer(): void {
    const updateDto: CustomerUpdateDto = this.activeCustomer;
    this.customerService.updateCustomer(updateDto).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const idx = this.customers.findIndex(c => c.customerId === res.data!.customerId);
          if (idx !== -1) this.customers[idx] = res.data;
          this.toastr.success('Müşteri başarıyla güncellendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Güncelleme başarısız.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  confirmDelete(id: number): void {
    // Üretim ortamında SweetAlert2 gibi daha şık bir kütüphane kullanılması önerilir.
    if (!confirm('Bu müşteriyi kalıcı olarak silmek istediğinizden emin misiniz?')) return;
    
    this.customerService.deleteCustomer(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.customers = this.customers.filter(c => c.customerId !== id);
          this.toastr.success('Müşteri başarıyla silindi.', 'Başarılı');
        } else {
          this.toastr.error(res.message || 'Silme işlemi başarısız.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }
}