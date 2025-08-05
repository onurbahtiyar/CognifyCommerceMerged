import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { MarketplaceService } from 'src/app/services/marketplace.service';
import { MarketplaceIntegrationDto, MarketplaceIntegrationUpdateDto, MarketplaceIntegrationAddDto } from 'src/app/models/marketplace.model';

// Arayüzde kullanılacak birleştirilmiş model
interface UIMarketplace {
    name: string; // Veritabanındaki eşleşme için (örn: "Trendyol")
    displayName: string; // Arayüzde gösterilecek isim (örn: "Trendyol")
    logoClass: string; // Font-awesome, bootstrap icons vb. için ikon sınıfı
    data: MarketplaceIntegrationDto | null; // API'den gelen veri
}

@Component({
  selector: 'app-marketplace-integrations',
  templateUrl: './marketplace-integrations.component.html',
})
export class MarketplaceIntegrationsComponent implements OnInit {

  loading = true;
  isPanelOpen = false;

  readonly PREDEFINED_MARKETPLACES = [
    { name: 'Trendyol', displayName: 'Trendyol', logoClass: 'bi bi-shop text-orange-500' },
    { name: 'Hepsiburada', displayName: 'Hepsiburada', logoClass: 'bi bi-shop-window text-purple-600' },
    { name: 'N11', displayName: 'N11', logoClass: 'bi bi-cart3 text-red-600' },
    { name: 'PTTAVM', displayName: 'PTTAVM', logoClass: 'bi bi-truck text-yellow-500' },
    { name: 'CicekSepeti', displayName: 'ÇiçekSepeti', logoClass: 'bi bi-flower1 text-pink-500' },
    { name: 'AmazonTR', displayName: 'Amazon Türkiye', logoClass: 'bi bi-amazon text-black' }
  ];

  uiMarketplaces: UIMarketplace[] = [];
  selectedMarketplace: UIMarketplace | null = null;
  
  integrationForm: MarketplaceIntegrationUpdateDto = this.getEmptyForm();

  constructor(
    private marketplaceService: MarketplaceService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadIntegrations();
  }

  loadIntegrations(): void {
    this.loading = true;
    this.marketplaceService.getAllIntegrations().subscribe({
      next: (res) => {
        if (res.success) {
          const apiData = res.data;
          this.uiMarketplaces = this.PREDEFINED_MARKETPLACES.map(predefined => {
            const dbRecord = apiData.find(db => db.marketplaceName === predefined.name);
            return {
              ...predefined,
              data: dbRecord || null
            };
          });
        } else {
          this.toastr.error(res.message, 'Hata');
        }
      },
      error: (err) => this.toastr.error('Entegrasyonlar yüklenirken bir hata oluştu.', 'Sunucu Hatası'),
      complete: () => this.loading = false
    });
  }

  openEditPanel(marketplace: UIMarketplace): void {
    this.selectedMarketplace = marketplace;
    if (marketplace.data) {
      this.integrationForm = { ...marketplace.data };
    } else {
      this.integrationForm = this.getEmptyForm();
      this.integrationForm.marketplaceName = marketplace.name;
    }
    this.isPanelOpen = true;
  }
  
  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedMarketplace = null;
    this.integrationForm = this.getEmptyForm();
  }
  
  saveChanges(): void {
    if (!this.integrationForm.apiKey) {
        this.toastr.warning('API Anahtarı alanı zorunludur.', 'Uyarı');
        return;
    }

    if (this.integrationForm.id && this.integrationForm.id > 0) {
      this.marketplaceService.updateIntegration(this.integrationForm).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastr.success(res.message, 'Başarılı');
            this.loadIntegrations();
            this.closePanel();
          } else {
            this.toastr.error(res.message, 'Hata');
          }
        },
        error: (err) => this.toastr.error('Güncelleme sırasında hata oluştu.', 'Hata')
      });
    } else {
      const addDto: MarketplaceIntegrationAddDto = {
          marketplaceName: this.integrationForm.marketplaceName,
          apiKey: this.integrationForm.apiKey,
          apiSecret: this.integrationForm.apiSecret,
          apiUrl: this.integrationForm.apiUrl,
          description: this.integrationForm.description,
          isActive: this.integrationForm.isActive
      };
      this.marketplaceService.addIntegration(addDto).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastr.success(res.message, 'Başarılı');
            this.loadIntegrations();
            this.closePanel();
          } else {
            this.toastr.error(res.message, 'Hata');
          }
        },
        error: (err) => this.toastr.error('Ekleme sırasında hata oluştu.', 'Hata')
      });
    }
  }

  private getEmptyForm(): MarketplaceIntegrationUpdateDto {
    return {
      id: 0,
      marketplaceName: '',
      apiKey: '',
      apiSecret: '',
      apiUrl: '',
      description: '',
      isActive: true
    };
  }
}