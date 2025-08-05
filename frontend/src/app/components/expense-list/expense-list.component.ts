import { Component, OnInit, ViewChild, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ExpenseService } from '../../services/expense.service';
import { formatDate } from '@angular/common';
import { ExpenseDto, ExpenseCategoryDto, ExpenseAddDto, ExpenseUpdateDto } from 'src/app/models/expense.dto';

@Component({
    selector: 'app-expense-list',
    templateUrl: './expense-list.component.html',
    styleUrls: ['./expense-list.component.scss']
})
export class ExpenseListComponent implements OnInit, AfterViewInit {
    // Form referansları
    @ViewChild('addForm') addForm!: NgForm;
    @ViewChild('editForm') editForm!: NgForm;

    // Bileşen state'leri
    expenses: ExpenseDto[] = [];
    expenseCategories: ExpenseCategoryDto[] = [];
    loading = true;

    // Yandan açılır panel state'leri
    isPanelOpen = false;
    panelMode: 'add' | 'edit' = 'add';

    // Form modelleri
    newExpense: ExpenseAddDto = this.createEmptyExpense();
    selectedExpense: ExpenseUpdateDto | null = null;

    constructor(
        private expenseService: ExpenseService,
        private toastr: ToastrService,
        private cd: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        this.loadInitialData();
    }
    
    // Formlar DOM'a eklendikten sonra değişiklikleri takip etmek için
    ngAfterViewInit(): void {
        this.addForm?.statusChanges?.subscribe(() => this.cd.detectChanges());
        this.editForm?.statusChanges?.subscribe(() => this.cd.detectChanges());
    }

    // Başlangıç verilerini (kategoriler, sonra masraflar) yükle
    loadInitialData(): void {
        this.loading = true;
        this.expenseService.getAllCategories().subscribe({
            next: (res) => {
                if (res.success && res.data) {
                    this.expenseCategories = res.data;
                    this.loadExpenses(); // Kategoriler yüklenince masrafları yükle
                } else {
                    this.toastr.error('Masraf kategorileri yüklenemedi.', 'Hata');
                    this.loading = false;
                }
            },
            error: () => {
                this.toastr.error('Kategoriler yüklenirken sunucu hatası oluştu.', 'Hata');
                this.loading = false;
            }
        });
    }

    // Masrafları API'den yükle
    loadExpenses(): void {
        this.expenseService.getAllExpenses().subscribe({
            next: (res) => {
                this.expenses = res.success ? (res.data || []) : [];
                this.loading = false;
            },
            error: () => {
                this.toastr.error('Masraflar yüklenirken bir hata oluştu.', 'Hata');
                this.loading = false;
            }
        });
    }

    // Boş masraf modeli oluşturur (tarih varsayılan olarak bugün)
    createEmptyExpense(): ExpenseAddDto {
        return {
            description: '',
            amount: 0,
            expenseCategoryId: null,
            expenseDate: formatDate(new Date(), 'yyyy-MM-dd', 'en-US'), // HTML date input formatı
            notes: ''
        };
    }

    // --- Panel Yönetimi ---
    openAddPanel(): void {
        this.panelMode = 'add';
        this.newExpense = this.createEmptyExpense();
        this.isPanelOpen = true;
    }

    openEditPanel(expense: ExpenseDto): void {
        this.panelMode = 'edit';
        // Düzenleme için ExpenseUpdateDto'ya dönüştür
        this.selectedExpense = {
            expenseId: expense.expenseId,
            description: expense.description,
            amount: expense.amount,
            expenseCategoryId: expense.expenseCategoryId,
            expenseDate: formatDate(expense.expenseDate, 'yyyy-MM-dd', 'en-US'),
            notes: expense.notes
        };
        this.isPanelOpen = true;
    }

    closePanel(): void {
        this.isPanelOpen = false;
        // Formları temizle
        this.addForm?.resetForm();
        this.editForm?.resetForm();
    }

    // --- CRUD İşlemleri ---
    saveNewExpense(form: NgForm): void {
        if (form.invalid) {
            this.toastr.warning('Lütfen gerekli tüm alanları doldurun.', 'Uyarı');
            return;
        }

        this.expenseService.addExpense(this.newExpense).subscribe({
            next: (res) => {
                if (res.success && res.data) {
                    this.expenses.unshift(res.data); // Yeni ekleneni listenin başına koy
                    this.toastr.success('Masraf başarıyla eklendi.', 'Başarılı');
                    this.closePanel();
                } else {
                    this.toastr.error(res.message || 'Masraf eklenemedi.', 'Hata');
                }
            },
            error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
        });
    }

    updateExpense(form: NgForm): void {
        if (form.invalid || !this.selectedExpense) {
            this.toastr.warning('Formda eksik bilgi var veya masraf seçili değil.', 'Uyarı');
            return;
        }

        this.expenseService.updateExpense(this.selectedExpense).subscribe({
            next: (res) => {
                if (res.success && res.data) {
                    const index = this.expenses.findIndex(e => e.expenseId === res.data!.expenseId);
                    if (index !== -1) {
                        this.expenses[index] = res.data;
                    }
                    this.toastr.success('Masraf başarıyla güncellendi.', 'Başarılı');
                    this.closePanel();
                } else {
                    this.toastr.error(res.message || 'Güncelleme başarısız.', 'Hata');
                }
            },
            error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
        });
    }

    confirmDelete(id: number): void {
        // SweetAlert2 gibi bir kütüphane ile daha şık bir onay kutusu gösterilebilir.
        if (confirm('Bu masraf kaydını silmek istediğinizden emin misiniz?')) {
            this.expenseService.deleteExpense(id).subscribe({
                next: (res) => {
                    if (res.success) {
                        this.expenses = this.expenses.filter(e => e.expenseId !== id);
                        this.toastr.info('Masraf kaydı silindi.', 'Bilgi');
                    } else {
                        this.toastr.error(res.message || 'Silme işlemi başarısız.', 'Hata');
                    }
                },
                error: (err) => this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata')
            });
        }
    }
}