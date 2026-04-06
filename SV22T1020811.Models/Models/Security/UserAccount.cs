namespace SV22T1020811.Models.Security
{
    /// <summary>
    /// Thông tin tài khoản người dùng
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// Mã tài khoản
        /// </summary>
        public string UserId { get; set; } = "";
        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// Tên hiển thị (thường là họ tên của người dùng, hoặc có thể là tên đăng nhập nếu không có họ tên)
        /// </summary>
        public string DisplayName { get; set; } = "";
        /// <summary>
        /// Địa chỉ email (nếu có)
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// Tên fie ảnh đại diện của người dùng (nếu có)
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Danh sách tên các vai trò/quyền của người dùng, được phân cách bởi dấu chấm phẩy (nếu có)
        /// </summary>
        public string RoleNames { get; set; } = "";
    }
}
