using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu của bảng Products,
    /// ProductAttributes và ProductPhotos trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IProductRepository.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Product

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách Product</returns>
        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            // 1. Thêm IsSelling vào parameters
            var parameters = new
            {
                SearchValue = string.IsNullOrEmpty(input.SearchValue) ? "%%" : "%" + input.SearchValue + "%",
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice,
                IsSelling = input.IsSelling, // hiễn thị danh sách hàng đang bán
                Offset = input.Offset,
                PageSize = input.PageSize
            };

            // 2. Cập nhật câu lệnh đếm (countSql) để lọc IsSelling
            string countSql = @"
        SELECT COUNT(*)
        FROM Products
        WHERE (ProductName LIKE @SearchValue)
          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
          AND (@SupplierID = 0 OR SupplierID = @SupplierID)
          AND (@MinPrice = 0 OR Price >= @MinPrice)
          AND (@MaxPrice = 0 OR Price <= @MaxPrice)
          AND (@IsSelling IS NULL OR IsSelling = @IsSelling) -- <--- THÊM DÒNG NÀY
    ";

            // 3. Cập nhật câu lệnh lấy dữ liệu phân trang (dataSql)
            string dataSql = @"
        SELECT *
        FROM Products
        WHERE (ProductName LIKE @SearchValue)
          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
          AND (@SupplierID = 0 OR SupplierID = @SupplierID)
          AND (@MinPrice = 0 OR Price >= @MinPrice)
          AND (@MaxPrice = 0 OR Price <= @MaxPrice)
          AND (@IsSelling IS NULL OR IsSelling = @IsSelling) -- <--- THÊM DÒNG NÀY
        ORDER BY ProductName
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY
    ";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Product> data;

            if (input.PageSize == 0)
            {
                // 4. Cập nhật câu lệnh lấy toàn bộ dữ liệu (nếu PageSize == 0)
                string sql = @"
            SELECT *
            FROM Products
            WHERE (ProductName LIKE @SearchValue)
              AND (@CategoryID = 0 OR CategoryID = @CategoryID)
              AND (@SupplierID = 0 OR SupplierID = @SupplierID)
              AND (@MinPrice = 0 OR Price >= @MinPrice)
              AND (@MaxPrice = 0 OR Price <= @MaxPrice)
              AND (@IsSelling IS NULL OR IsSelling = @IsSelling) -- <--- THÊM DÒNG NÀY
            ORDER BY ProductName
        ";
                data = await connection.QueryAsync<Product>(sql, parameters);
            }
            else
            {
                data = await connection.QueryAsync<Product>(dataSql, parameters);
            }

            return new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng theo ProductID
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>Đối tượng Product hoặc null nếu không tìm thấy</returns>
        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Products
                WHERE ProductID = @ProductID
            ";

            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductID = productID });
        }

        /// <summary>
        /// Bổ sung mặt hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần thêm</param>
        /// <returns>Mã ProductID vừa được tạo</returns>
        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Products
                (
                    ProductName,
                    ProductDescription,
                    SupplierID,
                    CategoryID,
                    Unit,
                    Price,
                    Photo,
                    IsSelling
                )
                VALUES
                (
                    @ProductName,
                    @ProductDescription,
                    @SupplierID,
                    @CategoryID,
                    @Unit,
                    @Price,
                    @Photo,
                    @IsSelling
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (int)id;
        }

        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Products
                SET
                    ProductName = @ProductName,
                    ProductDescription = @ProductDescription,
                    SupplierID = @SupplierID,
                    CategoryID = @CategoryID,
                    Unit = @Unit,
                    Price = @Price,
                    Photo = @Photo,
                    IsSelling = @IsSelling
                WHERE ProductID = @ProductID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa mặt hàng (bao gồm cả thuộc tính và ảnh liên quan)
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            // Xóa các ảnh và thuộc tính liên quan trước
            string deleteAttributesSql = "DELETE FROM ProductAttributes WHERE ProductID = @ProductID";
            string deletePhotosSql = "DELETE FROM ProductPhotos WHERE ProductID = @ProductID";
            string deleteProductSql = "DELETE FROM Products WHERE ProductID = @ProductID";

            await connection.ExecuteAsync(deleteAttributesSql, new { ProductID = productID });
            await connection.ExecuteAsync(deletePhotosSql, new { ProductID = productID });

            int rows = await connection.ExecuteAsync(deleteProductSql, new { ProductID = productID });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra mặt hàng có đang được sử dụng trong đơn hàng hay không
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>true nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT COUNT(*)
                FROM OrderDetails
                WHERE ProductID = @ProductID
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { ProductID = productID });
            return count > 0;
        }

        #endregion

        #region ProductAttribute

        /// <summary>
        /// Lấy danh sách thuộc tính của một mặt hàng
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>Danh sách ProductAttribute</returns>
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM ProductAttributes
                WHERE ProductID = @ProductID
                ORDER BY DisplayOrder, AttributeName
            ";

            var data = await connection.QueryAsync<ProductAttribute>(sql, new { ProductID = productID });
            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin một thuộc tính mặt hàng theo AttributeID
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính</param>
        /// <returns>Đối tượng ProductAttribute hoặc null</returns>
        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM ProductAttributes
                WHERE AttributeID = @AttributeID
            ";

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { AttributeID = attributeID });
        }

        /// <summary>
        /// Bổ sung thuộc tính cho mặt hàng
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần thêm</param>
        /// <returns>Mã AttributeID vừa được tạo</returns>
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO ProductAttributes
                (
                    ProductID,
                    AttributeName,
                    AttributeValue,
                    DisplayOrder
                )
                VALUES
                (
                    @ProductID,
                    @AttributeName,
                    @AttributeValue,
                    @DisplayOrder
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (long)id;
        }

        /// <summary>
        /// Cập nhật thuộc tính của mặt hàng
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE ProductAttributes
                SET
                    AttributeName = @AttributeName,
                    AttributeValue = @AttributeValue,
                    DisplayOrder = @DisplayOrder
                WHERE AttributeID = @AttributeID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM ProductAttributes
                WHERE AttributeID = @AttributeID
            ";

            int rows = await connection.ExecuteAsync(sql, new { AttributeID = attributeID });
            return rows > 0;
        }

        #endregion

        #region ProductPhoto

        /// <summary>
        /// Lấy danh sách ảnh của một mặt hàng
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>Danh sách ProductPhoto</returns>
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM ProductPhotos
                WHERE ProductID = @ProductID
                ORDER BY DisplayOrder, PhotoID
            ";

            var data = await connection.QueryAsync<ProductPhoto>(sql, new { ProductID = productID });
            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin một ảnh của mặt hàng theo PhotoID
        /// </summary>
        /// <param name="photoID">Mã ảnh</param>
        /// <returns>Đối tượng ProductPhoto hoặc null</returns>
        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM ProductPhotos
                WHERE PhotoID = @PhotoID
            ";

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { PhotoID = photoID });
        }

        /// <summary>
        /// Bổ sung ảnh cho mặt hàng
        /// </summary>
        /// <param name="data">Thông tin ảnh cần thêm</param>
        /// <returns>Mã PhotoID vừa được tạo</returns>
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO ProductPhotos
                (
                    ProductID,
                    Photo,
                    Description,
                    DisplayOrder,
                    IsHidden
                )
                VALUES
                (
                    @ProductID,
                    @Photo,
                    @Description,
                    @DisplayOrder,
                    @IsHidden
                );

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return (long)id;
        }

        /// <summary>
        /// Cập nhật thông tin ảnh của mặt hàng
        /// </summary>
        /// <param name="data">Thông tin ảnh cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE ProductPhotos
                SET
                    Photo = @Photo,
                    Description = @Description,
                    DisplayOrder = @DisplayOrder,
                    IsHidden = @IsHidden
                WHERE PhotoID = @PhotoID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa ảnh của mặt hàng
        /// </summary>
        /// <param name="photoID">Mã ảnh cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                DELETE FROM ProductPhotos
                WHERE PhotoID = @PhotoID
            ";

            int rows = await connection.ExecuteAsync(sql, new { PhotoID = photoID });
            return rows > 0;
        }

        #endregion
    }
}
