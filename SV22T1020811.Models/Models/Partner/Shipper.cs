namespace SV22T1020811.Models.Partner
{
    /// <summary>
    /// Người giao hàng
    /// </summary>
    public class Shipper
    {
        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int ShipperID { get; set; }
        /// <summary>
        /// Tên người giao hàng
        /// </summary>
        public string ShipperName { get; set; } = string.Empty;
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? Phone { get; set; }
    }
}
