namespace SV22T1020811.Models.Sales
{
    /// <summary>
    /// Thông tin đơn hàng khi hiển thị trong danh sách tìm kiếm (DTO)
    /// </summary>
    public class OrderSearchInfo : Order
    {
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; } = "";
        /// <summary>
        /// Điện thoại khách hàng
        /// </summary>
        public string CustomerPhone { get; set; } = "";
        /// <summary>
        /// Tên nhân viên phụ trách đơn hàng
        /// </summary>
        public string EmployeeName { get; set; } = "";
        /// <summary>
        /// Tổng số tiền
        /// </summary>
        public decimal SumOfPrice { get; set; }
    }


}
