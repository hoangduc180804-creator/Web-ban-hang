using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;


namespace SV22T1020811.Datalayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Suppliers
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IGenericRepository cho đối tượng Supplier.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm phân trang</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            // 1. Chuẩn bị giá trị tìm kiếm (đảm bảo luôn có dấu % để LIKE hoạt động)
            var condition = string.IsNullOrEmpty(input.SearchValue) ? "%%" : "%" + input.SearchValue + "%";

            // 2. Thiết lập tham số cho Dapper (Tên bên trái phải khớp với @ trong SQL)
            var parameters = new
            {
                SearchValue = condition,
                Offset = input.Offset,
                PageSize = input.PageSize
            };

            // 3. Câu lệnh đếm tổng số dòng
            string countSql = @"
        SELECT COUNT(*)
        FROM Suppliers
        WHERE SupplierName LIKE @SearchValue
           OR ContactName LIKE @SearchValue
    ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // 4. Lấy dữ liệu (xử lý trường hợp PageSize = 0 để lấy tất cả)
            IEnumerable<Supplier> data;
            if (input.PageSize <= 0)
            {
                string sql = @"
            SELECT *
            FROM Suppliers
            WHERE SupplierName LIKE @SearchValue
               OR ContactName LIKE @SearchValue
            ORDER BY SupplierName
        ";
                data = await connection.QueryAsync<Supplier>(sql, parameters);
            }
            else
            {
                string dataSql = @"
            SELECT *
            FROM Suppliers
            WHERE SupplierName LIKE @SearchValue
               OR ContactName LIKE @SearchValue
            ORDER BY SupplierName
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";
                data = await connection.QueryAsync<Supplier>(dataSql, parameters);
            }

            // 5. Trả về kết quả
            return new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một nhà cung cấp dựa theo SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>Đối tượng Supplier hoặc null nếu không tồn tại</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Suppliers
                WHERE SupplierID = @SupplierID
            ";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { SupplierID = id });
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID vừa được tạo</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Suppliers
                (
                    SupplierName,
                    ContactName,
                    Province,
                    Address,
                    Phone,
                    Email
                )
                VALUES
                (
                    @SupplierName,
                    @ContactName,
                    @Province,
                    @Address,
                    @Phone,
                    @Email
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)id;
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công, ngược lại false</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Suppliers
                SET
                    SupplierName = @SupplierName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email
                WHERE SupplierID = @SupplierID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhà cung cấp khỏi CSDL
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Suppliers
                WHERE SupplierID = @SupplierID
            ";

            int rows = await connection.ExecuteAsync(sql, new { SupplierID = id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có đang được sử dụng
        /// trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM Products
                WHERE SupplierID = @SupplierID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { SupplierID = id });

            return count > 0;
        }
    }
}