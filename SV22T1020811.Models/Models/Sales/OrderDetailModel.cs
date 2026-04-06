namespace SV22T1020811.Models.Sales
{
    public class OrderDetailModel
    {
        public OrderViewInfo? Order { get; set; }
        public List<OrderDetailViewInfo> Details { get; set; } = new List<OrderDetailViewInfo>();
    }
}