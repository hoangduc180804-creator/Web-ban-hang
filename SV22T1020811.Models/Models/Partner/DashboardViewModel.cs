using SV22T1020811.Models.Sales; // Để dùng OrderViewInfo
using SV22T1020811.Models.Catalog;

namespace SV22T1020811.Models.Partner
{
    public class DashboardViewModel
    {
        public decimal DailyRevenue { get; set; }
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }

        // Danh sách đơn hàng mới nhất (Dùng OrderViewInfo để có đủ tên khách hàng)
        public List<OrderViewInfo> RecentOrders { get; set; } = new List<OrderViewInfo>();

        // Dữ liệu giả lập cho biểu đồ (Đức có thể xử lý sau)
        public decimal[] MonthlyRevenue { get; set; } = new decimal[6];
    }
}