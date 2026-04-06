using SV22T1020811.Models.Security;

namespace SV22T1020811.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu liên quan đến tài khoản
    /// </summary>
    public interface IUserAccountRepository
    {
        /// <summary>
        /// Kiểm tra xem tên đăng nhập và mật khẩu có hợp lệ không
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Trả về thông tin của tài khoản nếu thông tin đăng nhập hợp lệ,
        /// ngược lại trả về null
        /// </returns>
        Task<UserAccount?> AuthorizeAsync(string userName, string password);
        /// <summary>
        /// Đổi mật khẩu của tài khoản
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ChangePasswordAsync(string userName, string password);

        Task<bool> UpdateDisplayNameAsync(string userName, string displayName);
    }
}
