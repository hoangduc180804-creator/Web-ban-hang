using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer; 
using SV22T1020811.Models.Security;

namespace SV22T1020811.BusinessLayers
{
    /// <summary>
    /// Lớp các chức năng về bảo mật.
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository _employeeAccountDB;

        static SecurityDataService()
        {
            // Lấy chuỗi kết nối từ lớp Configuration mà bạn đã khởi tạo ở Program.cs
            string connectionString = Configuration.ConnectionString;

            // Khởi tạo Repository
            _employeeAccountDB = new EmployeeAccountRepository(connectionString);
        }

        /// <summary>
        /// Xác thực tài khoản nhân viên
        /// </summary>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            return await _employeeAccountDB.AuthorizeAsync(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public static async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            return await _employeeAccountDB.ChangePasswordAsync(userName, password);
        }

        public static async Task<bool> UpdateDisplayNameAsync(string userName, string displayName)
        {
            return await _employeeAccountDB.UpdateDisplayNameAsync(userName, displayName);
        }
    }
}