using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Security;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu tài khoản khách hàng
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IUserAccountRepository cho tài khoản khách hàng.
    /// </summary>
    public class CustomerAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của khách hàng
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email của khách hàng)</param>
        /// <param name="password">Mật khẩu (đã được hash MD5)</param>
        /// <returns>
        /// Thông tin UserAccount nếu đăng nhập hợp lệ,
        /// ngược lại trả về null
        /// </returns>
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT c.CustomerID AS UserId,
                       c.Email AS UserName,
                       c.CustomerName AS DisplayName,
                       c.Email,
                       N'' AS Photo,
                       N'customer' AS RoleNames
                FROM Customers c
                WHERE c.Email = @UserName
                  AND c.Password = @Password
                  AND (c.IsLocked IS NULL OR c.IsLocked = 0)
            ";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql,
                new { UserName = userName, Password = password });
        }

        /// <summary>
        /// Đổi mật khẩu của tài khoản khách hàng
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email của khách hàng)</param>
        /// <param name="password">Mật khẩu mới (đã được hash MD5)</param>
        /// <returns>true nếu đổi mật khẩu thành công</returns>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customers
                SET Password = @Password
                WHERE Email = @UserName
            ";

            int rows = await connection.ExecuteAsync(sql, new { UserName = userName, Password = password });
            return rows > 0;
        }

        public async Task<bool> UpdateDisplayNameAsync(string userName, string displayName)
        {
            // Nếu khách hàng không cho đổi tên qua đây thì cứ return true hoặc thực hiện UPDATE tương tự Employee
            return await Task.FromResult(true);
        }
    }
}
