// Genel API Yanıt Modeli (zaten varsa kullanın)
import { ApiResponse } from './api-response.model';

// Ana Dashboard Veri Yapısı
export interface DashboardDto {
  generatedAt: string;
  summaryStats: SummaryStatsDto;
  actionItems: ActionItemsDto;
  salesOverview: SalesOverviewDto;
  productInsights: ProductInsightsDto;
  financials: FinancialsDto;
  customerActivity: CustomerActivityDto;
}

// Alt Modeller
export interface SummaryStatsDto {
  totalRevenue: Metric<number>;
  totalExpenses: Metric<number>;
  netProfit: Metric<number>;
  totalOrders: Metric<number>;
  totalCustomers: Metric<number>;
  totalProducts: Metric<number>;
}

export interface ActionItemsDto {
  ordersToShip: CountMetric;
  lowStockItems: CountMetric;
  pendingReviews: CountMetric;
}

export interface SalesOverviewDto {
  period: string;
  revenueChartData: ChartDataPoint<string, number>[];
  recentOrders: RecentOrderDto[];
  orderStatusDistribution: ChartDataPoint<string, number>[];
}

export interface ProductInsightsDto {
  bestSellingProducts: BestSellingProductDto[];
  lowStockProducts: LowStockProductDto[];
  categoryDistribution: ChartDataPoint<string, number>[];
}

export interface FinancialsDto {
  expenseBreakdown: ChartDataPoint<string, number>[];
  recentExpenses: RecentExpenseDto[];
}

export interface CustomerActivityDto {
  recentCustomers: RecentCustomerDto[];
  overallProductRating: RatingDto;
}

// Tekrar kullanılabilir yardımcı arayüzler
export interface Metric<T> {
  value: T;
  label: string;
}

export interface CountMetric {
  count: number;
  label:string;
  details: string;
}

export interface ChartDataPoint<TX, TY> {
  x: TX; // C# tarafında X ve Y olarak tanımladıysanız burada da aynı ismi kullanın
  y: TY;
}

export interface RecentOrderDto {
  orderId: number;
  customerName: string;
  totalAmount: number;
  status: string;
  orderDate: string;
}

export interface BestSellingProductDto {
  productId: number;
  productName: string;
  unitsSold: number;
  revenueGenerated: number;
}

export interface LowStockProductDto {
  productId: number;
  productName: string;
  stockLevel: number;
}

export interface RecentExpenseDto {
  expenseId: number;
  description: string;
  category: string;
  amount: number;
  expenseDate: string;
}

export interface RecentCustomerDto {
  customerId: number;
  fullName: string;
  email: string;
  joinDate: string;
}

export interface RatingDto {
  average: number;
  totalReviews: number;
}