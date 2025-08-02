import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ProductService, ProductDto, ProductAddDto } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { CategoryDto } from '../../models/category.model';
import { ApiResponse } from '../../models/api-response.model';
import { NgForm } from '@angular/forms';
import { PriceAnalysisResult } from 'src/app/models/price-analysis.model';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
})
export class ProductListComponent implements OnInit {
  // Form referansları
  @ViewChild('addForm') addForm: NgForm | undefined;
  @ViewChild('editForm') editForm: NgForm | undefined;

  products: ProductDto[] = [];
  categories: CategoryDto[] = [];

  // Ekleme ve Düzenleme için ortak state'ler
  isPanelOpen = false;
  panelMode: 'add' | 'edit' = 'add';
  loading = false;

  // Form modelleri
  newProduct: ProductAddDto = this.createEmptyProduct();
  selectedProduct: ProductDto | null = null;

  isAnalysisModalOpen = false;
  isAnalysisLoading = false;
  analysisResult: PriceAnalysisResult | null = null;
  analysisProductName = '';

  enhanceInstruction = 'Arka planı beyaz yap ve ürün öne çıksın';
  uploadedFile?: File;
  tempImageUrl: string | null = null;
  isEnhancing = false;

  environment = environment;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private toastr: ToastrService,
    private cd: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadInitialData();
  }

  // Başlangıç verilerini yükle
  loadInitialData(): void {
    this.loading = true;
    this.categoryService.getAllCategories().subscribe({
      next: (res) => {
        this.categories = res.success ? (res.data || []) : [];
        this.loadProducts();
      },
      error: () => {
        this.toastr.error('Kategoriler yüklenirken bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }

  ngAfterViewInit(): void {
    // form referansları sonradan bağlandığında değişiklikleri yakalayıp yeniden çalıştırıyoruz
    this.addForm?.statusChanges?.subscribe(() => this.cd.detectChanges());
    this.editForm?.statusChanges?.subscribe(() => this.cd.detectChanges());
  }

  loadProducts(): void {
    this.productService.getAllProducts().subscribe({
      next: (res) => {
        this.products = res.success ? (res.data || []) : [];
        this.loading = false;
      },
      error: () => {
        this.toastr.error('Ürünler yüklenirken bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }

  // Boş ürün modeli oluşturucu
  createEmptyProduct(): ProductAddDto {
    return { name: '', description: '', price: 0, stock: 0, categoryId: 0, imageUrl: '' };
  }

  // --- Panel Yönetimi ---
  openAddPanel(): void {
    this.panelMode = 'add';
    this.newProduct = this.createEmptyProduct();
    this.isPanelOpen = true;
  }

  openEditPanel(product: ProductDto): void {
    this.panelMode = 'edit';
    // Klonlayarak orijinal veriyi koru
    this.selectedProduct = { ...product };
    this.isPanelOpen = true;
  }

  closePanel(): void {
    this.isPanelOpen = false;
    // Formları temizle (isteğe bağlı)
    this.addForm?.resetForm();
    this.editForm?.resetForm();
  }

  addProduct(form: NgForm): void {
    if (form.invalid) {
      this.toastr.error('Lütfen tüm alanları doldurun.', 'Hata');
      return;
    }

    this.loading = true;
    this.productService.addProduct(this.newProduct).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.products.unshift(res.data);
          this.toastr.success('Ürün başarıyla eklendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Ürün eklenemedi.', 'Hata');
        }
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
      }
    });
  }

  updateProduct(form: NgForm): void {
    if (form.invalid || !this.selectedProduct) return;

    this.loading = true;
    this.productService.updateProduct(this.selectedProduct).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          const idx = this.products.findIndex(p => p.productId === res.data!.productId);
          if (idx !== -1) this.products[idx] = res.data;
          this.toastr.success('Ürün başarıyla güncellendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Güncelleme başarısız.', 'Hata');
        }
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
      }
    });
  }

  confirmDelete(id: number): void {
    // Daha iyi bir UX için SweetAlert2 gibi bir kütüphane kullanılabilir.
    if (!confirm('Bu ürünü kalıcı olarak silmek istediğinizden emin misiniz?')) return;

    this.productService.deleteProduct(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.products = this.products.filter(p => p.productId !== id);
          this.toastr.success('Ürün başarıyla silindi.', 'Başarılı');
        } else {
          this.toastr.error(res.message || 'Silme işlemi başarısız.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  openAnalysisModal(product: ProductDto): void {
    this.analysisProductName = product.name;
    this.isAnalysisModalOpen = true;
    this.isAnalysisLoading = true;
    this.analysisResult = null; // Önceki sonuçları temizle

    this.productService.getPriceAnalysis(product.name).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.analysisResult = res.data;
        } else {
          this.toastr.error(res.message || 'Analiz sonuçları alınamadı.', 'Hata');
          this.closeAnalysisModal(); // Hata varsa modalı kapat
        }
        this.isAnalysisLoading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Fiyat analizi sırasında sunucu hatası oluştu.', 'Hata');
        this.isAnalysisLoading = false;
        this.closeAnalysisModal(); // Hata varsa modalı kapat
      }
    });
  }

  closeAnalysisModal(): void {
    this.isAnalysisModalOpen = false;
    this.analysisResult = null;
  }

  openAnalysisModalForNewProduct(): void {
    if (!this.newProduct.name || !this.newProduct.name.trim()) {
      this.toastr.error('Analiz için ürün adı gerekli.', 'Uyarı');
      return;
    }

    this.analysisProductName = this.newProduct.name;
    this.isAnalysisModalOpen = true;
    this.isAnalysisLoading = true;
    this.analysisResult = null;

    this.productService.getPriceAnalysis(this.newProduct.name).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.analysisResult = res.data;
        } else {
          this.toastr.error(res.message || 'Analiz sonuçları alınamadı.', 'Hata');
          // modal açık kalsın, kullanıcı tekrar deneyebilir
        }
        this.isAnalysisLoading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Fiyat analizi sırasında sunucu hatası oluştu.', 'Hata');
        this.isAnalysisLoading = false;
      }
    });
  }

  onFileSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length) {
    this.uploadedFile = input.files[0];
  }
}

enhanceImageForNewProduct(): void {
  if (!this.uploadedFile) {
    this.toastr.error('Görsel seçilmedi.', 'Hata');
    return;
  }
  if (!this.newProduct.name?.trim()) {
    this.toastr.error('Analiz için ürün adı gerekli.', 'Uyarı');
    return;
  }

  this.isEnhancing = true;
  this.productService.enhanceTempImage(this.uploadedFile, this.enhanceInstruction).subscribe({
    next: (res) => {
      this.isEnhancing = false;
      if (res.success && res.data) {
        this.tempImageUrl = res.data;
        this.toastr.success('Geliştirilmiş görsel hazır. Önizleyip onaylayabilirsin.', 'Başarılı');
      } else {
        this.toastr.error(res.message || 'Görsel geliştirme başarısız.', 'Hata');
      }
    },
    error: (err) => {
      this.isEnhancing = false;
      this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
    }
  });
}

attachEnhancedImageToExisting(product: ProductDto): void {
  if (!this.tempImageUrl) {
    this.toastr.error('Geliştirilmiş görsel yok.', 'Hata');
    return;
  }

  this.productService.attachTempImageToProduct(product.productId, this.tempImageUrl).subscribe({
    next: (res) => {
      if (res.success && res.data) {
        // local listeyi güncelle
        const idx = this.products.findIndex(p => p.productId === product.productId);
        if (idx !== -1) this.products[idx].imageUrl = res.data;
        this.toastr.success('Görsel ürünle ilişkilendirildi.', 'Başarılı');
      } else {
        this.toastr.error(res.message || 'İlişkilendirme başarısız.', 'Hata');
      }
    },
    error: (err) => {
      this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
    }
  });
}

saveNewProductWithImage(form: NgForm): void {
  if (form.invalid) {
    this.toastr.error('Lütfen tüm alanları doldurun.', 'Hata');
    return;
  }

  if (this.tempImageUrl) {
    this.newProduct.imageUrl = this.tempImageUrl;
  }

  this.loading = true;
  this.productService.addProduct(this.newProduct).subscribe({
    next: (res) => {
      this.loading = false;
      if (res.success && res.data) {
        // eğer tempImageUrl varsa ürüne bağla
        if (this.tempImageUrl) {
          this.productService.attachTempImageToProduct(res.data.productId, this.tempImageUrl).subscribe({
            next: attachRes => {
              if (attachRes.success) {
                res.data.imageUrl = attachRes.data;
              }
              this.products.unshift(res.data);
              this.toastr.success('Ürün ve görsel başarıyla eklendi.', 'Başarılı');
              this.closePanel();
            },
            error: () => {
              this.products.unshift(res.data);
              this.toastr.warning('Ürün eklendi ama görsel ilişkilendirilemedi.', 'Uyarı');
              this.closePanel();
            }
          });
        } else {
          this.products.unshift(res.data);
          this.toastr.success('Ürün başarıyla eklendi.', 'Başarılı');
          this.closePanel();
        }
      } else {
        this.toastr.error(res.message || 'Ürün eklenemedi.', 'Hata');
      }
    },
    error: (err) => {
      this.loading = false;
      this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
    }
  });
}

}