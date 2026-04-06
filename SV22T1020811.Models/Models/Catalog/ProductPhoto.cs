namespace SV22T1020811.Models.Catalog
{
    /// <summary>
    /// Ảnh của mặt hàng
    /// </summary>
    public class ProductPhoto
    {
        /// <summary>
        /// Mã ảnh
        /// </summary>
        public long PhotoID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = string.Empty;
        /// <summary>
        /// Mô tả ảnh
        /// </summary>
        ///[Required(ErrorMessage = "Vui lòng nhập mô tả cho ảnh")]
        public string Description { get; set; } = "";
        /// <summary>
        /// Thứ tự hiển thị (giá trị nhỏ sẽ hiển thị trước)
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Có ẩn ảnh đối với khách hàng hay không?
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
