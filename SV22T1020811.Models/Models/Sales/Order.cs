namespace SV22T1020811.Models.Sales
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int? CustomerID { get; set; }
        /// <summary>
        /// Thời điểm đặt hàng (thời điểm tạo đơn hàng)
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Tỉnh/thành giao hàng
        /// </summary>
        public string? DeliveryProvince { get; set; }
        /// <summary>
        /// Địa chỉ giao hàng
        /// </summary>
        public string? DeliveryAddress { get; set; }
        /// <summary>
        /// Mã nhân viên xử lý đơn hàng (người nhận/duyệt đơn hàng)
        /// </summary>
        public int? EmployeeID { get; set; }
        /// <summary>
        /// Thời điểm duyệt đơn hàng (thời điểm nhân viên nhận/duyệt đơn hàng)
        /// </summary>
        public DateTime? AcceptTime { get; set; }
        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int? ShipperID { get; set; }
        /// <summary>
        /// Thời điểm người giao hàng nhận đơn hàng để giao
        /// </summary>
        public DateTime? ShippedTime { get; set; }
        /// <summary>
        /// Thời điểm kết thúc đơn hàng
        /// </summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// Trạng thái hiện tại của đơn hàng
        /// </summary>
        public OrderStatusEnum Status { get; set; }
    }
}
