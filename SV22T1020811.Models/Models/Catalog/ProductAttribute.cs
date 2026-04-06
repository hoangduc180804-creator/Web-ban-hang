namespace SV22T1020811.Models.Catalog
{
    /// <summary>
    /// Thuộc tính của mặt hàng
    /// </summary>
    public class ProductAttribute
    {
        /// <summary>
        /// Mã thuộc tính
        /// </summary>
        public long AttributeID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên thuộc tính (ví dụ: "Màu sắc", "Kích thước", "Chất liệu", ...)
        /// </summary>
        public string AttributeName { get; set; } = string.Empty;
        /// <summary>
        /// Giá trị thuộc tính
        /// </summary>
        public string AttributeValue { get; set; } = string.Empty;
        /// <summary>
        /// Thứ tự hiển thị thuộc tính (giá trị nhỏ sẽ hiển thị trước)
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
