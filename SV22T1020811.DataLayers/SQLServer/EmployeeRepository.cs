using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.HR;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Employees
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IEmployeeRepository.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhân viên theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm phân trang</returns>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
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
                FROM Employees
                WHERE FullName LIKE @SearchValue
                   OR Email LIKE @SearchValue
            ";

            string dataSql = @"
                SELECT *
                FROM Employees
                WHERE FullName LIKE @SearchValue
                   OR Email LIKE @SearchValue
                ORDER BY FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Employee> data;

            if (input.PageSize == 0)
            {
                string sql = @"
                    SELECT *
                    FROM Employees
                    WHERE FullName LIKE @SearchValue
                       OR Email LIKE @SearchValue
                    ORDER BY FullName
                ";
                data = await connection.QueryAsync<Employee>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<Employee>(dataSql, parameters);
            }

            return new PagedResult<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một nhân viên dựa theo EmployeeID
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>Đối tượng Employee hoặc null nếu không tồn tại</returns>
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Employees
                WHERE EmployeeID = @EmployeeID
            ";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { EmployeeID = id });
        }

        /// <summary>
        /// Bổ sung một nhân viên mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần thêm</param>
        /// <returns>Mã EmployeeID vừa được tạo</returns>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            // Bổ sung Password và RoleNames vào cả danh sách cột và danh sách tham số @
            string sql = @"
        INSERT INTO Employees
        (
            FullName,
            BirthDate,
            Address,
            Phone,
            Email,
            Photo,
            IsWorking,
            Password,     -- Thêm cột này
            RoleNames     -- Thêm cột này
        )
        VALUES
        (
            @FullName,
            @BirthDate,
            @Address,
            @Phone,
            @Email,
            @Photo,
            @IsWorking,
            @Password,    -- Thêm tham số này
            @RoleNames     -- Thêm tham số này
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ";

            // Dapper sẽ tự khớp thuộc tính Password và RoleNames từ đối tượng 'data' vào SQL
            var id = await connection.ExecuteScalarAsync<int>(sql, data);
            return id;
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công, ngược lại false</returns>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Employees
                SET
                    FullName = @FullName,
                    BirthDate = @BirthDate,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Photo = @Photo,
                    IsWorking = @IsWorking
                WHERE EmployeeID = @EmployeeID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> UpdateRoleAsync(int id, string roleNames)
        {
            using var connection = new SqlConnection(_connectionString);

            // Chỉ cập nhật duy nhất cột RoleNames dựa vào ID
            string sql = @"
                UPDATE Employees
                SET RoleNames = @RoleNames
                WHERE EmployeeID = @EmployeeID
            ";

            int rows = await connection.ExecuteAsync(sql, new
            {
                EmployeeID = id,
                RoleNames = roleNames
            });

            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhân viên khỏi CSDL
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Employees
                WHERE EmployeeID = @EmployeeID
            ";

            int rows = await connection.ExecuteAsync(sql, new { EmployeeID = id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra nhân viên có đang được sử dụng
        /// trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE EmployeeID = @EmployeeID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { EmployeeID = id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// (Email chưa được sử dụng bởi nhân viên khác)
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhân viên mới.
        /// Nếu id != 0: Kiểm tra email của nhân viên có mã là id
        /// </param>
        /// <returns>true nếu email hợp lệ (chưa bị trùng)</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql;
            object parameters;

            if (id == 0)
            {
                // Nhân viên mới: kiểm tra email chưa tồn tại
                sql = @"
                    SELECT COUNT(*)
                    FROM Employees
                    WHERE Email = @Email
                ";
                parameters = new { Email = email };
            }
            else
            {
                // Nhân viên đã tồn tại: kiểm tra email không trùng với nhân viên khác
                sql = @"
                    SELECT COUNT(*)
                    FROM Employees
                    WHERE Email = @Email
                      AND EmployeeID <> @EmployeeID
                ";
                parameters = new { Email = email, EmployeeID = id };
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, parameters);

            // Email hợp lệ khi không có bản ghi nào trùng
            return count == 0;
        }
        public async Task<bool> ChangePasswordAsync(int id, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // CHỈ UPDATE CỘT PASSWORD
                string sql = @"UPDATE Employees 
                       SET Password = @Password 
                       WHERE EmployeeID = @EmployeeID";

                var parameters = new
                {
                    EmployeeID = id,
                    Password = newPassword // Truyền trực tiếp, không qua hàm mã hóa nào
                };

                return await connection.ExecuteAsync(sql, parameters) > 0;
            }
        }
    }
}
