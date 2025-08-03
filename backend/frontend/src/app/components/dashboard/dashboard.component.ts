import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { DashboardService } from 'src/app/services/dashboard.service';
import { DashboardDto } from 'src/app/models/dashboard.model';

// Chart.js'in kendisini ve gerekli tüm bileşenleri import ediyoruz.
import { Chart, registerables } from 'chart.js';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('salesCanvas') salesCanvas: ElementRef | undefined;

  dashboardData: DashboardDto | null = null;
  loading = true;
  error: string | null = null;
  
  private salesChart: Chart | null = null;

  constructor(
    private dashboardService: DashboardService,
    private toastr: ToastrService
  ) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.salesChart?.destroy();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    this.dashboardService.getDashboardSummary().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.dashboardData = response.data;
          
          setTimeout(() => {
            this.createSalesChart(); 
          }, 0);

        } else {
          this.error = response.message || 'Dashboard verileri alınamadı.';
          if (this.error) {
            this.toastr.error(this.error, 'Hata');
          }
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Sunucuyla iletişim kurulamadı.';
        if (this.error) {
           this.toastr.error(this.error, 'Sunucu Hatası');
        }
        this.loading = false;
      }
    });
  }

  createSalesChart(): void {
    if (!this.dashboardData?.salesOverview?.revenueChartData || !this.salesCanvas) {
      console.error('Grafik oluşturulamadı: Veri veya canvas elementi bulunamadı.');
      return;
    }
    
    if (this.salesChart) {
      this.salesChart.destroy();
    }

    const chartLabels = this.dashboardData.salesOverview.revenueChartData.map(dp => 
        new Date(dp.x).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })
    );
    const chartDataPoints = this.dashboardData.salesOverview.revenueChartData.map(dp => dp.y);

    const data = {
      labels: chartLabels,
      datasets: [{
        label: 'Satışlar (TL)',
        data: chartDataPoints,
        fill: true,
        borderColor: 'rgb(14, 165, 233)',
        backgroundColor: 'rgba(14, 165, 233, 0.1)',
        tension: 0.4,
        pointBackgroundColor: 'rgb(14, 165, 233)',
        pointHoverRadius: 6,
        pointHoverBorderWidth: 2,
        pointHoverBorderColor: 'white'
      }]
    };

    this.salesChart = new Chart(this.salesCanvas.nativeElement, {
      type: 'line',
      data: data,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(Number(value))
            }
          }
        },
        plugins: {
          legend: {
            display: false,
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleFont: { weight: 'bold' },
            callbacks: {
              label: (context) => {
                const label = context.dataset.label || '';
                const value = context.parsed.y;
                return `${label}: ${new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value)}`;
              }
            }
          }
        },
        interaction: {
            intersect: false,
            mode: 'index',
        },
      }
    });
  }
}