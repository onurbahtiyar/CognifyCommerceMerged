using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using Entities.Dtos;
using Entities.Enums;

namespace Business.Concrete
{
    public class DashboardManager : IDashboardService
    {
        private readonly IOrderService _orderService;
        private readonly IExpenseService _expenseService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IProductReviewService _productReviewService;

        public DashboardManager(
            IOrderService orderService,
            IExpenseService expenseService,
            ICustomerService customerService,
            IProductService productService,
            IProductReviewService productReviewService)
        {
            _orderService = orderService;
            _expenseService = expenseService;
            _customerService = customerService;
            _productService = productService;
            _productReviewService = productReviewService;
        }

        public IDataResult<DashboardDto> GetDashboardSummary()
        {
            try
            {
                var ordersResult = _orderService.GetAll();
                var expensesResult = _expenseService.GetAll();
                var customersResult = _customerService.GetAll();
                var productsResult = _productService.GetAll();
                var reviewsResult = _productReviewService.GetAll();

                var allOrders = ordersResult.Success ? ordersResult.Data : new List<OrderDto>();
                var allExpenses = expensesResult.Success ? expensesResult.Data : new List<ExpenseDto>();
                var allCustomers = customersResult.Success ? customersResult.Data : new List<CustomerDto>();
                var allProducts = productsResult.Success ? productsResult.Data : new List<ProductDto>();
                var allReviews = reviewsResult.Success ? reviewsResult.Data : new List<ProductReviewDto>();

                var dashboardDto = new DashboardDto
                {
                    SummaryStats = GetSummaryStats(allOrders, allExpenses, allCustomers, allProducts),
                    ActionItems = GetActionItems(allOrders, allProducts, _productReviewService.GetUnapprovedReviews().Data),
                    SalesOverview = GetSalesOverview(allOrders),
                    ProductInsights = GetProductInsights(allOrders, allProducts),
                    Financials = GetFinancials(allExpenses),
                    CustomerActivity = GetCustomerActivity(allCustomers, allReviews)
                };

                return new SuccessDataResult<DashboardDto>(dashboardDto, "Dashboard verileri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<DashboardDto>(null, $"Dashboard oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        private SummaryStatsDto GetSummaryStats(List<OrderDto> orders, List<ExpenseDto> expenses, List<CustomerDto> customers, List<ProductDto> products)
        {
            var totalRevenue = orders.Where(o => o.OrderStatusCode != (int)OrderStatus.Cancelled).Sum(o => o.TotalAmount);
            var totalExpenses = expenses.Sum(e => e.Amount);

            return new SummaryStatsDto
            {
                TotalRevenue = new MetricDto<decimal> { Value = totalRevenue, Label = "Toplam Gelir (TL)" },
                TotalExpenses = new MetricDto<decimal> { Value = totalExpenses, Label = "Toplam Masraf (TL)" },
                NetProfit = new MetricDto<decimal> { Value = totalRevenue - totalExpenses, Label = "Net Kâr (TL)" },
                TotalOrders = new MetricDto<int> { Value = orders.Count, Label = "Toplam Sipariş" },
                TotalCustomers = new MetricDto<int> { Value = customers.Count, Label = "Toplam Müşteri" },
                TotalProducts = new MetricDto<int> { Value = products.Count, Label = "Aktif Ürün Sayısı" }
            };
        }

        private ActionItemsDto GetActionItems(List<OrderDto> orders, List<ProductDto> products, List<ProductReviewDto> pendingReviews)
        {
            return new ActionItemsDto
            {
                OrdersToShip = new CountMetricDto { Count = orders.Count(o => o.OrderStatusCode == (int)OrderStatus.PendingConfirmation), Label = "Kargolanacak Sipariş", Details = "Onay bekleyen veya hazırlanan siparişler." },
                LowStockItems = new CountMetricDto { Count = products.Count(p => p.Stock < 10), Label = "Stoğu Azalan Ürün", Details = "Stoğu 10'un altına düşen ürünler." },
                PendingReviews = new CountMetricDto { Count = pendingReviews?.Count ?? 0, Label = "Onay Bekleyen Yorum", Details = "Müşterilerden gelen ve onaya bekleyen yeni yorumlar." }
            };
        }

        private SalesOverviewDto GetSalesOverview(List<OrderDto> orders)
        {
            return new SalesOverviewDto
            {
                Period = "Son 30 Gün",
                RevenueChartData = orders
                    .Where(o => o.OrderStatusCode != (int)OrderStatus.Cancelled && o.OrderDate >= DateTime.Now.AddDays(-30))
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new ChartDataPoint<DateTime, decimal> { X = g.Key, Y = g.Sum(o => o.TotalAmount) })
                    .OrderBy(p => p.X)
                    .ToList(),
                RecentOrders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(3)
                    .Select(o => new RecentOrderDto { OrderId = o.OrderId, CustomerName = o.CustomerFullName, TotalAmount = o.TotalAmount, Status = o.OrderStatus, OrderDate = o.OrderDate })
                    .ToList(),
                OrderStatusDistribution = orders
                    .GroupBy(o => o.OrderStatus)
                    .Select(g => new ChartDataPoint<string, int> { X = g.Key, Y = g.Count() })
                    .ToList()
            };
        }

        private ProductInsightsDto GetProductInsights(List<OrderDto> orders, List<ProductDto> products)
        {
            var soldItems = orders
                .Where(o => o.OrderStatusCode != (int)OrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    UnitsSold = g.Sum(i => i.Quantity),
                    RevenueGenerated = g.Sum(i => i.Quantity * i.UnitPrice),
                    ProductName = g.First().ProductName
                })
                .OrderByDescending(x => x.UnitsSold)
                .ToList();

            return new ProductInsightsDto
            {
                BestSellingProducts = soldItems.Take(3)
                    .Select(x => new BestSellingProductDto { ProductId = x.ProductId, ProductName = x.ProductName, UnitsSold = x.UnitsSold, RevenueGenerated = x.RevenueGenerated })
                    .ToList(),
                LowStockProducts = products.Where(p => p.Stock < 10)
                    .Select(p => new LowStockProductDto { ProductId = p.ProductId, ProductName = p.Name, StockLevel = p.Stock })
                    .ToList(),
                CategoryDistribution = products
                    .GroupBy(p => p.CategoryName ?? "Kategorisiz")
                    .Select(g => new ChartDataPoint<string, int> { X = g.Key, Y = g.Count() })
                    .ToList()
            };
        }

        private FinancialsDto GetFinancials(List<ExpenseDto> expenses)
        {
            return new FinancialsDto
            {
                ExpenseBreakdown = expenses
                    .GroupBy(e => e.ExpenseCategoryName)
                    .Select(g => new ChartDataPoint<string, decimal> { X = g.Key, Y = g.Sum(e => e.Amount) })
                    .ToList(),
                RecentExpenses = expenses
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(3)
                    .Select(e => new RecentExpenseDto { ExpenseId = e.ExpenseId, Description = e.Description, Category = e.ExpenseCategoryName, Amount = e.Amount, ExpenseDate = e.ExpenseDate })
                    .ToList()
            };
        }

        private CustomerActivityDto GetCustomerActivity(List<CustomerDto> customers, List<ProductReviewDto> reviews)
        {
            var customerReviews = reviews.Where(r => r.Rating > 0).ToList();

            return new CustomerActivityDto
            {
                RecentCustomers = customers
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(3)
                    .Select(c => new RecentCustomerDto { CustomerId = c.CustomerId, FullName = $"{c.FirstName} {c.LastName}", Email = c.Email, JoinDate = c.CreatedDate })
                    .ToList(),
                OverallProductRating = new RatingDto
                {
                    Average = customerReviews.Any() ? customerReviews.Average(r => r.Rating) : 0,
                    TotalReviews = customerReviews.Count
                }
            };
        }
    }
}