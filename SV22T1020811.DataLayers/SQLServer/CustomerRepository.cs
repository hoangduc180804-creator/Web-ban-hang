using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Customers
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface ICustomerRepository.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách khách hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm phân trang</returns>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                SearchValue = string.IsNullOrEmpty(input.SearchValue) ? "%%" : "%" + input.SearchValue + "%",
                Offset = input.Offset,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Customers
                WHERE CustomerName LIKE @SearchValue
                   OR ContactName LIKE @SearchValue
                   OR Email LIKE @SearchValue
            ";

            string dataSql = @"
                SELECT *
                FROM Customers
                WHERE CustomerName LIKE @SearchValue
                   OR ContactName LIKE @SearchValue
                   OR Email LIKE @SearchValue
                ORDER BY CustomerName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Customer> data;

            if (input.PageSize == 0)
            {
                string sql = @"
                    SELECT *
                    FROM Customers
                    WHERE CustomerName LIKE @SearchValue
                       OR ContactName LIKE @SearchValue
                       OR Email LIKE @SearchValue
                    ORDER BY CustomerName
                ";
                data = await connection.QueryAsync<Customer>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<Customer>(dataSql, parameters);
            }

            return new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một khách hàng dựa theo CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>Đối tượng Customer hoặc null nếu không tồn tại</returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Customers
                WHERE CustomerID = @CustomerID
            ";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = id });
        }

        public async Task<int> AddAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
        INSERT INTO Customers
        (
            CustomerName,
            ContactName,
            Province,
            Address,
            Phone,
            Email,
            IsLocked,
            IsActive,
            Password  -- THÊM CỘT NÀY
        )
        VALUES
        (
            @CustomerName,
            @ContactName,
            @Province,
            @Address,
            @Phone,
            @Email,
            @IsLocked,
            @IsActive,
            @Password -- THÊM PARAMETER NÀY
        );

        SELECT CAST(SCOPE_IDENTITY() as int); -- Ép kiểu trực tiếp về int cho sạch
    ";

            // Thực thi với Dapper
            var id = await connection.ExecuteScalarAsync<int>(sql, data);
            return id;
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công, ngược lại false</returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customers
                SET
                    CustomerName = @CustomerName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    IsLocked = @IsLocked,
                    IsActive = @IsActive
                WHERE CustomerID = @CustomerID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một khách hàng khỏi CSDL
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Customers
                WHERE CustomerID = @CustomerID
            ";

            int rows = await connection.ExecuteAsync(sql, new { CustomerID = id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra khách hàng có đang được sử dụng
        /// trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE CustomerID = @CustomerID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { CustomerID = id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không
        /// (Email chưa được sử dụng bởi khách hàng khác)
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của khách hàng mới.
        /// Nếu id != 0: Kiểm tra email đối với khách hàng đã tồn tại
        /// </param>
        /// <returns>true nếu email hợp lệ (chưa bị trùng)</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql;
            object parameters;

            if (id == 0)
            {
                // Khách hàng mới: kiểm tra email chưa tồn tại
                sql = @"
                    SELECT COUNT(*)
                    FROM Customers
                    WHERE Email = @Email
                ";
                parameters = new { Email = email };
            }
            else
            {
                // Khách hàng đã tồn tại: kiểm tra email không trùng với khách hàng khác
                sql = @"
                    SELECT COUNT(*)
                    FROM Customers
                    WHERE Email = @Email
                      AND CustomerID <> @CustomerID
                ";
                parameters = new { Email = email, CustomerID = id };
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, parameters);

            // Email hợp lệ khi không có bản ghi nào trùng
            return count == 0;
        }

        /// <summary>
        /// Thay đổi mật khẩu của khách hàng
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int id, string newPassword)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Customers 
                   SET Password = @Password 
                   WHERE CustomerID = @CustomerID";

            var parameters = new
            {
                Password = newPassword,
                CustomerID = id
            };

            int rows = await connection.ExecuteAsync(sql, parameters);
            return rows > 0;
        }

        /// <summary>
        /// Xác thực khách hàng dựa vào Email và Mật khẩu
        /// </summary>
        public async Task<Customer?> AuthorizeAsync(string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT * FROM Customers 
                WHERE Email = @Email AND Password = @Password
            ";

            // Tìm và trả về khách hàng đầu tiên khớp thông tin
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new
            {
                Email = email,
                Password = password
            });
        }

    }
}
