using SV22T1020811.Models.Partner;
using SV22T1020811.Models.Sales;
using SV22T1020811.Models.Common;

namespace SV22T1020811.Shop.Models
{
    public class UserProfileViewModel
    {
        public Customer Customer { get; set; } = new Customer();
        public List<OrderViewInfo> RecentOrders { get; set; } = new List<OrderViewInfo>();
    }
}