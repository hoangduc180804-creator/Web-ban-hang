namespace SV22T1020811.Models.Sales
{
    /// <summary>
    /// Thông tin chi tiết của mặt hàng được bán trong đơn hàng
    /// </summary>
    public class OrderDetail
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// Tổng số tiền
        /// </summary>
        public decimal TotalPrice => Quantity * SalePrice;        
    }
}
