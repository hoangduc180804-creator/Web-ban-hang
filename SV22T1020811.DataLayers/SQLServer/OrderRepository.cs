using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Orders
    /// và OrderDetails trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IOrderRepository.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách OrderViewInfo</returns>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                SearchValue = string.IsNullOrEmpty(input.SearchValue) ? "%%" : "%" + input.SearchValue + "%",
                Status = (int)input.Status,
                DateFrom = input.DateFrom,
                DateTo = input.DateTo,
                CustomerID = input.CustomerID ?? 0, 
                Offset = input.Offset,
                PageSize = input.PageSize
            };

            string countSql = @"
    SELECT COUNT(*)
    FROM Orders o
    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
    WHERE (@Status = 0 OR o.Status = @Status)
      AND (@DateFrom IS NULL OR o.OrderTime >= @DateFrom)
      AND (@DateTo IS NULL OR o.OrderTime <= @DateTo)
      AND (@CustomerID = 0 OR o.CustomerID = @CustomerID) 
      AND (c.CustomerName LIKE @SearchValue OR e.FullName LIKE @SearchValue)";

            // CHỈNH SỬA: Thêm Subquery tính SumOrderTotal vào dataSql
            string dataSql = @"
    SELECT o.*,
           c.CustomerName, c.ContactName AS CustomerContactName,
           c.Email AS CustomerEmail, c.Phone AS CustomerPhone,
           c.Address AS CustomerAddress,
           e.FullName AS EmployeeName,
           s.ShipperName, s.Phone AS ShipperPhone,
           ISNULL((SELECT SUM(Quantity * SalePrice) 
                   FROM OrderDetails 
                   WHERE OrderID = o.OrderID), 0) AS SumOrderTotal
    FROM Orders o
    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
    LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
    WHERE (@Status = 0 OR o.Status = @Status)
      AND (@DateFrom IS NULL OR o.OrderTime >= @DateFrom)
      AND (@DateTo IS NULL OR o.OrderTime <= @DateTo)
      AND (@CustomerID = 0 OR o.CustomerID = @CustomerID) 
      AND (c.CustomerName LIKE @SearchValue OR c.ContactName LIKE @SearchValue OR e.FullName LIKE @SearchValue)
    ORDER BY o.OrderTime DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            IEnumerable<OrderViewInfo> data;

            if (input.PageSize == 0)
            {
                // CHỈNH SỬA: Thêm Subquery vào đây cho trường hợp không phân trang
                string sql = @"
            SELECT o.*, c.CustomerName, e.FullName AS EmployeeName, s.ShipperName,
                   ISNULL((SELECT SUM(Quantity * SalePrice) FROM OrderDetails WHERE OrderID = o.OrderID), 0) AS SumOrderTotal
            FROM Orders o
            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
            LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
            LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
            WHERE (@Status = 0 OR o.Status = @Status)
              AND (@DateFrom IS NULL OR o.OrderTime >= @DateFrom)
              AND (@DateTo IS NULL OR o.OrderTime <= @DateTo)
              AND (c.CustomerName LIKE @SearchValue OR e.FullName LIKE @SearchValue)
            ORDER BY o.OrderTime DESC";
                data = await connection.QueryAsync<OrderViewInfo>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<OrderViewInfo>(dataSql, parameters);
            }

            return new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng theo OrderID
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <returns>Đối tượng OrderViewInfo hoặc null nếu không tìm thấy</returns>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
        SELECT o.*,
               c.CustomerName, c.ContactName AS CustomerContactName,
               c.Email AS CustomerEmail, c.Phone AS CustomerPhone,
               c.Address AS CustomerAddress,
               e.FullName AS EmployeeName,
               s.ShipperName, s.Phone AS ShipperPhone,
               -- CHỈNH SỬA: Thêm Subquery tính tổng tiền
               ISNULL((SELECT SUM(Quantity * SalePrice) FROM OrderDetails WHERE OrderID = o.OrderID), 0) AS SumOrderTotal
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
        LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
        LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
        WHERE o.OrderID = @OrderID";

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { OrderID = orderID });
        }

        /// <summary>
        /// Bổ sung một đơn hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin đơn hàng cần thêm</param>
        /// <returns>Mã OrderID vừa được tạo</returns>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Orders
                (
                    CustomerID,
                    OrderTime,
                    DeliveryProvince,
                    DeliveryAddress,
                    EmployeeID,
                    AcceptTime,
                    ShipperID,
                    ShippedTime,
                    FinishedTime,
                    Status
                )
                VALUES
                (
                    @CustomerID,
                    @OrderTime,
                    @DeliveryProvince,
                    @DeliveryAddress,
                    @EmployeeID,
                    @AcceptTime,
                    @ShipperID,
                    @ShippedTime,
                    @FinishedTime,
                    @Status
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)id;
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        /// <param name="data">Thông tin đơn hàng cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Orders
                SET
                    CustomerID = @CustomerID,
                    OrderTime = @OrderTime,
                    DeliveryProvince = @DeliveryProvince,
                    DeliveryAddress = @DeliveryAddress,
                    EmployeeID = @EmployeeID,
                    AcceptTime = @AcceptTime,
                    ShipperID = @ShipperID,
                    ShippedTime = @ShippedTime,
                    FinishedTime = @FinishedTime,
                    Status = @Status
                WHERE OrderID = @OrderID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa đơn hàng (bao gồm cả chi tiết đơn hàng liên quan)
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            // Xóa chi tiết đơn hàng trước
            string deleteDetailsSql = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
            string deleteOrderSql = "DELETE FROM Orders WHERE OrderID = @OrderID";

            await connection.ExecuteAsync(deleteDetailsSql, new { OrderID = orderID });

            int rows = await connection.ExecuteAsync(deleteOrderSql, new { OrderID = orderID });
            return rows > 0;
        }

        #endregion

        #region OrderDetail

        /// <summary>
        /// Lấy danh sách mặt hàng trong một đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <returns>Danh sách OrderDetailViewInfo</returns>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT od.*, p.ProductName, p.Unit, p.Photo
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID
                ORDER BY p.ProductName
            ";

            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { OrderID = orderID });
            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng trong một đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>Đối tượng OrderDetailViewInfo hoặc null</returns>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT od.*, p.ProductName, p.Unit, p.Photo
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID
                  AND od.ProductID = @ProductID
            ";

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql,
                new { OrderID = orderID, ProductID = productID });
        }

        /// <summary>
        /// Bổ sung mặt hàng vào đơn hàng
        /// </summary>
        /// <param name="data">Thông tin chi tiết đơn hàng cần thêm</param>
        /// <returns>true nếu thêm thành công</returns>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                IF NOT EXISTS (
                    SELECT 1 FROM OrderDetails 
                    WHERE OrderID = @OrderID AND ProductID = @ProductID
                )
                BEGIN
                    INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                    VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)
                END
                ELSE
                BEGIN
                    UPDATE OrderDetails
                    SET Quantity = Quantity + @Quantity,
                        SalePrice = @SalePrice
                    WHERE OrderID = @OrderID AND ProductID = @ProductID
                END
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="data">Thông tin chi tiết cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE OrderDetails
                SET
                    Quantity = @Quantity,
                    SalePrice = @SalePrice
                WHERE OrderID = @OrderID
                  AND ProductID = @ProductID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng cần xóa khỏi đơn hàng</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM OrderDetails
                WHERE OrderID = @OrderID
                  AND ProductID = @ProductID
            ";

            int rows = await connection.ExecuteAsync(sql, new { OrderID = orderID, ProductID = productID });
            return rows > 0;
        }

        #endregion
    }
}
