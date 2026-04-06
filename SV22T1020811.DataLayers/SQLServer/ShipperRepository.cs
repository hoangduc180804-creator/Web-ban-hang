using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Shippers
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IGenericRepository cho đối tượng Shipper.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách người giao hàng theo điều kiện tìm kiếm
        /// và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm phân trang</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
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
                FROM Shippers
                WHERE ShipperName LIKE @SearchValue
                   OR Phone LIKE @SearchValue
            ";

            string dataSql = @"
                SELECT *
                FROM Shippers
                WHERE ShipperName LIKE @SearchValue
                   OR Phone LIKE @SearchValue
                ORDER BY ShipperName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Shipper> data;

            if (input.PageSize == 0)
            {
                string sql = @"
                    SELECT *
                    FROM Shippers
                    WHERE ShipperName LIKE @SearchValue
                       OR Phone LIKE @SearchValue
                    ORDER BY ShipperName
                ";
                data = await connection.QueryAsync<Shipper>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<Shipper>(dataSql, parameters);
            }

            return new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một người giao hàng dựa theo ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>Đối tượng Shipper hoặc null nếu không tồn tại</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Shippers
                WHERE ShipperID = @ShipperID
            ";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { ShipperID = id });
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần thêm</param>
        /// <returns>Mã ShipperID vừa được tạo</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Shippers
                (
                    ShipperName,
                    Phone
                )
                VALUES
                (
                    @ShipperName,
                    @Phone
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)id;
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công, ngược lại false</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Shippers
                SET
                    ShipperName = @ShipperName,
                    Phone = @Phone
                WHERE ShipperID = @ShipperID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một người giao hàng khỏi CSDL
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM Shippers
                WHERE ShipperID = @ShipperID
            ";

            int rows = await connection.ExecuteAsync(sql, new { ShipperID = id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra người giao hàng có đang được sử dụng
        /// trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE ShipperID = @ShipperID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { ShipperID = id });

            return count > 0;
        }
    }
}
