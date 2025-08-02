import { Component, OnInit, ViewChild } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { CategoryService } from '../../services/category.service';
import { CategoryDto, CategoryAddDto, CategoryUpdateDto } from '../../models/category.model';
import { ApiResponse } from '../../models/api-response.model';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-category-list',
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss'],
})
export class CategoryListComponent implements OnInit {
  @ViewChild('addForm') addForm: NgForm | undefined;
  @ViewChild('editForm') editForm: NgForm | undefined;

  categories: CategoryDto[] = [];
  loading = false;
  
  // Panel state yönetimi
  isPanelOpen = false;
  panelMode: 'add' | 'edit' = 'add';
  
  // Form modelleri
  newCategory: CategoryAddDto = this.createEmptyCategory();
  selectedCategory: CategoryUpdateDto | null = null;

  constructor(
    private categoryService: CategoryService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getAllCategories().subscribe({
      next: (res: ApiResponse<CategoryDto[]>) => {
        if (res.success) {
          this.categories = res.data || [];
        } else {
          this.toastr.error(res.message || 'Kategoriler yüklenemedi.', 'Hata');
        }
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
        this.loading = false;
      }
    });
  }

  createEmptyCategory(): CategoryAddDto {
    return { name: '', description: '' };
  }

  // --- Panel Yönetimi ---
  openAddPanel(): void {
    this.panelMode = 'add';
    this.newCategory = this.createEmptyCategory();
    this.isPanelOpen = true;
  }

  openEditPanel(category: CategoryDto): void {
    this.panelMode = 'edit';
    this.selectedCategory = { ...category }; // Klonla
    this.isPanelOpen = true;
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.addForm?.resetForm();
    this.editForm?.resetForm();
  }

  // --- CRUD İşlemleri ---
  addCategory(): void {
    if (this.addForm?.invalid) return;
    this.categoryService.addCategory(this.newCategory).subscribe({
      next: (res: ApiResponse<CategoryDto>) => {
        if (res.success && res.data) {
          this.categories.unshift(res.data); // Başa ekle
          this.toastr.success('Kategori başarıyla eklendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Kategori eklenemedi.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  updateCategory(): void {
    if (this.editForm?.invalid || !this.selectedCategory) return;
    this.categoryService.updateCategory(this.selectedCategory).subscribe({
      next: (res: ApiResponse<CategoryDto>) => {
        if (res.success && res.data) {
          const idx = this.categories.findIndex(c => c.categoryId === res.data!.categoryId);
          if (idx !== -1) this.categories[idx] = res.data;
          this.toastr.success('Kategori başarıyla güncellendi.', 'Başarılı');
          this.closePanel();
        } else {
          this.toastr.error(res.message || 'Kategori güncellenemedi.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }

  confirmDelete(categoryId: number): void {
    if (!confirm('Bu kategoriyi ve bağlı tüm ürünleri kalıcı olarak silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.')) {
      return;
    }
    this.categoryService.deleteCategory(categoryId).subscribe({
      next: (res: ApiResponse<any>) => {
        if (res.success) {
          this.categories = this.categories.filter(c => c.categoryId !== categoryId);
          this.toastr.success('Kategori başarıyla silindi.', 'Başarılı');
        } else {
          this.toastr.error(res.message || 'Kategori silinemedi.', 'Hata');
        }
      },
      error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
    });
  }
}