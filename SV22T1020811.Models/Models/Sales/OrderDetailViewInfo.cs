namespace SV22T1020811.Models.Sales
{
    /// <summary>
    /// DTO hiển thị thông tin chi tiết của mặt hàng trong đơn hàng
    /// </summary>
    public class OrderDetailViewInfo : OrderDetail
    {
        /// <summary>
        /// Tên hàng
        /// </summary>
        public string ProductName { get; set; } = "";
        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = "";
    }
}
