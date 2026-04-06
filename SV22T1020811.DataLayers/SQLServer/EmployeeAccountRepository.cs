using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Security;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu tài khoản nhân viên
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IUserAccountRepository cho tài khoản nhân viên.
    /// </summary>
    public class EmployeeAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public EmployeeAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của nhân viên
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email của nhân viên)</param>
        /// <param name="password">Mật khẩu (đã được hash MD5)</param>
        /// <returns>
        /// Thông tin UserAccount nếu đăng nhập hợp lệ,
        /// ngược lại trả về null
        /// </returns>
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            // SỬA LẠI SQL Ở ĐÂY: Thay N'employee' AS RoleNames bằng e.RoleNames
            string sql = @"
        SELECT e.EmployeeID AS UserId,
               e.Email AS UserName,
               e.FullName AS DisplayName,
               e.Email,
               e.Photo,
               e.RoleNames   -- Lấy giá trị thực tế từ cột RoleNames trong DB
        FROM Employees e
        WHERE e.Email = @UserName
          AND e.Password = @Password
          AND e.IsWorking = 1
    ";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql,
                new { UserName = userName, Password = password });
        }

        /// <summary>
        /// Đổi mật khẩu của tài khoản nhân viên
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email của nhân viên)</param>
        /// <param name="password">Mật khẩu mới (đã được hash MD5)</param>
        /// <returns>true nếu đổi mật khẩu thành công</returns>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Employees
                SET Password = @Password
                WHERE Email = @UserName
            ";

            int rows = await connection.ExecuteAsync(sql, new { UserName = userName, Password = password });
            return rows > 0;
        }

        public async Task<bool> UpdateDisplayNameAsync(string userName, string displayName)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
        UPDATE Employees 
        SET FullName = @DisplayName 
        WHERE Email = @UserName";

            int rows = await connection.ExecuteAsync(sql, new
            {
                UserName = userName,
                DisplayName = displayName
            });
            return rows > 0;
        }
    }
}
