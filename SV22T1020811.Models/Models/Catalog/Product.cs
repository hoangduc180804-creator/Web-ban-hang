namespace SV22T1020811.Models.Catalog
{
    /// <summary>
    /// Mặt hàng
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả mặt hàng
        /// </summary>
        public string? ProductDescription { get; set; }
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int? SupplierID { get; set; }
        /// <summary>
        /// Mã loại hàng
        /// </summary>
        public int? CategoryID { get; set; }
        /// <summary>
        /// Đơn vi tính
        /// </summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>
        /// Giá
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Tên file ảnh đại diện của mặt hàng (nếu có)
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// Mặt hàng hiện có đang được bán hay không?
        /// </summary>
        public bool IsSelling { get; set; }
    }
}
