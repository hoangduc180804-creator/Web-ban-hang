namespace SV22T1020811.Models.Partner
{
    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// Tên giao dịch
        /// </summary>
        public string ContactName { get; set; } = string.Empty;
        /// <summary>
        /// Tỉnh thành
        /// </summary>
        public string? Province { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Quốc gia
        /// </summary>
        public string Country { get; set; } = "";
    }
}
