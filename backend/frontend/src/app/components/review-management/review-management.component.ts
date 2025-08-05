import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';

// Gerekli Tüm Servis ve Modelleri Import Edin
import { ProductDto, ProductService } from 'src/app/services/product.service';
import { CustomerDto } from 'src/app/models/customer.model';
import { CustomerService } from 'src/app/services/customer.service';
import { AdminReplyAddDto, ProductReviewAddDto, ProductReviewDto } from 'src/app/models/product-review.model';
import { ProductReviewService } from 'src/app/services/product-review.service';

@Component({
  selector: 'app-review-management',
  templateUrl: './review-management.component.html',
  styleUrls: ['./review-management.component.scss']
})
export class ReviewManagementComponent implements OnInit {

  // === Veri Listeleri ve Durum ===
  masterReviews: ProductReviewDto[] = []; 
  filteredReviews: ProductReviewDto[] = [];
  products: ProductDto[] = [];
  customers: CustomerDto[] = []; // Müşteri listesi
  loading = false;
  currentFilter: 'all' | 'unapproved' | 'approved' = 'all'; 
  
  // === Modal State'leri ===
  isReplyModalOpen = false;
  selectedReviewForReply: ProductReviewDto | null = null;
  replyText = '';
  isReplying = false;

  isAddModalOpen = false;
  isSubmittingNewReview = false;
  newReview: ProductReviewAddDto = this.createEmptyReviewDto();
  hoveredRating = 0;

  constructor(
    private reviewService: ProductReviewService,
    private productService: ProductService,
    private customerService: CustomerService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadInitialData();
  }

  // Getter metotları (Sayıları HTML'de göstermek için)
  get allCount(): number { return this.masterReviews.length; }
  get unapprovedCount(): number { return this.masterReviews.filter(r => !r.isApproved).length; }
  get approvedCount(): number { return this.masterReviews.filter(r => r.isApproved).length; }

  // --- Veri Yükleme ve Filtreleme ---

  loadInitialData(): void {
    this.loading = true;
    
    // Ürünleri Yükle
    this.productService.getAllProducts().subscribe({
      next: res => { this.products = res.success ? (res.data || []) : []; },
      error: () => this.toastr.error('Ürünler yüklenemedi.', 'Hata')
    });

    // Müşterileri Yükle (Sizin servisiniz kullanılıyor)
    this.customerService.getAllCustomers().subscribe({
      next: res => { this.customers = res.success ? (res.data || []) : []; },
      error: () => this.toastr.error('Müşteriler yüklenemedi.', 'Hata')
    });

    // Yorumları Yükle
    this.loadAllReviews();
  }

  loadAllReviews(): void {
    this.reviewService.getAllReviews().subscribe({
      next: (res) => {
        this.masterReviews = res.success ? (res.data || []) : [];
        this.applyFilter(this.currentFilter);
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Yorumlar yüklenirken bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }
  
  applyFilter(filter: 'all' | 'unapproved' | 'approved'): void {
    this.currentFilter = filter;
    switch (filter) {
      case 'unapproved': this.filteredReviews = this.masterReviews.filter(r => !r.isApproved); break;
      case 'approved': this.filteredReviews = this.masterReviews.filter(r => r.isApproved); break;
      default: this.filteredReviews = [...this.masterReviews]; break;
    }
  }

  // --- CRUD İşlemleri ---
  approveReview(review: ProductReviewDto): void {
    this.reviewService.approveReview(review.reviewId).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastr.success('Yorum başarıyla onaylandı.', 'Başarılı');
          this.loadAllReviews();
        } else {
          this.toastr.error(res.message || 'Yorum onaylanamadı.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  confirmDelete(review: ProductReviewDto): void {
    if (!confirm(`'${review.customerName}' adlı kullanıcının yorumunu kalıcı olarak silmek istediğinizden emin misiniz?`)) return;
    this.reviewService.deleteReview(review.reviewId).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastr.success('Yorum başarıyla silindi.', 'Başarılı');
          this.loadAllReviews();
        } else {
          this.toastr.error(res.message || 'Silme işlemi başarısız.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  // --- Yanıtlama Modalı Yönetimi ---
  openReplyModal(review: ProductReviewDto): void {
    this.selectedReviewForReply = review;
    this.replyText = '';
    this.isReplyModalOpen = true;
  }

  closeReplyModal(): void {
    this.isReplyModalOpen = false;
    this.selectedReviewForReply = null;
    this.replyText = '';
  }

  submitReply(): void {
    if (!this.replyText.trim() || !this.selectedReviewForReply) return;
    
    this.isReplying = true;
    const adminUserId = 1;

    const dto: AdminReplyAddDto = {
      parentReviewId: this.selectedReviewForReply.reviewId,
      comment: this.replyText,
      customerId: adminUserId 
    };

    this.reviewService.addAdminReply(dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastr.success('Yorum başarıyla yanıtlandı.', 'Başarılı');
          this.closeReplyModal();
          this.loadAllReviews();
        } else {
          this.toastr.error(res.message || 'Yanıt gönderilemedi.', 'Hata');
        }
        this.isReplying = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
        this.isReplying = false;
      }
    });
  }

  // --- Yeni Yorum Ekleme Modalı Yönetimi ---
  
  createEmptyReviewDto(): ProductReviewAddDto {
    return { productId: 0, customerId: 0, rating: 0, comment: '' };
  }

  openAddModal(): void {
    this.newReview = this.createEmptyReviewDto();
    this.hoveredRating = 0;
    this.isAddModalOpen = true;
  }

  closeAddModal(): void {
    this.isAddModalOpen = false;
  }
  
  setRating(rating: number): void {
    this.newReview.rating = rating;
  }

  submitNewReview(form: NgForm): void {
    if (form.invalid || this.newReview.customerId === 0 || this.newReview.productId === 0 || this.newReview.rating === 0) {
      this.toastr.warning('Lütfen tüm alanları (Müşteri, Ürün, Puan, Yorum) doldurun.', 'Eksik Bilgi');
      return;
    }

    this.isSubmittingNewReview = true;
    this.reviewService.addReview(this.newReview).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastr.success(res.message, 'Başarılı');
          this.closeAddModal();
          this.loadAllReviews();
        } else {
          this.toastr.error(res.message, 'Hata');
        }
        this.isSubmittingNewReview = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Yorum gönderilirken bir hata oluştu.', 'Hata');
        this.isSubmittingNewReview = false;
      }
    });
  }
}