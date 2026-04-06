using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Categories
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IGenericRepository cho đối tượng Category.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách loại hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm phân trang</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
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
                FROM Categories
                WHERE CategoryName LIKE @SearchValue
            ";

            string dataSql = @"
                SELECT *
                FROM Categories
                WHERE CategoryName LIKE @SearchValue
                ORDER BY CategoryName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Category> data;

            if (input.PageSize == 0)
            {
                string sql = @"
                    SELECT *
                    FROM Categories
                    WHERE CategoryName LIKE @SearchValue
                    ORDER BY CategoryName
                ";
                data = await connection.QueryAsync<Category>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<Category>(dataSql, parameters);
            }

            return new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một loại hàng dựa theo CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>Đối tượng Category hoặc null nếu không tồn tại</returns>
        public async Task<Category?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Categories
                WHERE CategoryID = @CategoryID
            ";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { CategoryID = id });
        }

        /// <summary>
        /// Bổ sung một loại hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần thêm</param>
        /// <returns>Mã CategoryID vừa được tạo</returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Categories
                (
                    CategoryName,
                    Description
                )
                VALUES
                (
                    @CategoryName,
                    @Description
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)id;
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công, ngược lại false</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Categories
                SET
                    CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryID = @CategoryID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một loại hàng khỏi CSDL
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Categories
                WHERE CategoryID = @CategoryID
            ";

            int rows = await connection.ExecuteAsync(sql, new { CategoryID = id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra loại hàng có đang được sử dụng
        /// trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM Products
                WHERE CategoryID = @CategoryID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryID = id });

            return count > 0;
        }
    }
}
