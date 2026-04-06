using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.Admin.Models;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;
using System.Diagnostics;

namespace SV22T1020811.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Trang chủ Dashboard - Lấy dữ liệu thật từ các Service Async
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // 1. Khởi tạo đầu vào tìm kiếm (Lấy trang 1, số lượng 1 để đếm tổng số dòng)
            var basicInput = new PaginationSearchInput { Page = 1, PageSize = 1, SearchValue = "" };

            // 2. Đầu vào cho Sản phẩm (Dùng ProductSearchInput theo Service của bạn)
            var productInput = new ProductSearchInput { Page = 1, PageSize = 1, SearchValue = "" };

            // 3. Đầu vào cho Đơn hàng (Lấy 5 đơn hàng mới nhất để hiển thị bảng)
            var orderInput = new OrderSearchInput
            {
                Page = 1,
                PageSize = 5,
                SearchValue = "",
                Status = 0 // Lấy tất cả trạng thái
            };

            // 4. Gọi Service lấy dữ liệu thật
            var productResult = await ProductDataService.ListProductsAsync(productInput);
            var customerResult = await PartnerDataService.ListCustomersAsync(basicInput);
            var orderResult = await SalesDataService.ListOrdersAsync(orderInput);

            // 5. Đổ dữ liệu vào ViewBag (Lưu ý dùng đúng RowCount và DataItems)
            ViewBag.ProductCount = productResult?.RowCount ?? 0;
            ViewBag.CustomerCount = customerResult?.RowCount ?? 0;
            ViewBag.OrderCount = orderResult?.RowCount ?? 0;

            // Danh sách 5 đơn hàng mới nhất
            ViewBag.RecentOrders = orderResult?.DataItems ?? new List<OrderViewInfo>();

            // Tính doanh thu tạm tính từ các đơn hàng đang hiển thị (SumOrderTotal)
            ViewBag.TotalRevenue = orderResult?.DataItems?.Sum(o => o.SumOrderTotal) ?? 0;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}