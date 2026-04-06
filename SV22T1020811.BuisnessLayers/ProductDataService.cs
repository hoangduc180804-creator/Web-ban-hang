using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly IProductRepository productDB;
        private static readonly IGenericRepository<Category> categoryDB;

        static ProductDataService()
        {
            string connectionString = Configuration.ConnectionString;
            productDB = new ProductRepository(connectionString);
            categoryDB = new CategoryRepository(connectionString); // Khởi tạo CategoryRepository
        }

        /// <summary>
        /// Lấy toàn bộ danh sách danh mục (không phân trang) để hiển thị Sidebar
        /// </summary>
        public static async Task<List<Category>> ListCategoriesAsync()
        {
            // Tận dụng logic "PageSize = 0" trong Repository của Đức để lấy toàn bộ dữ liệu
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 0,
                SearchValue = ""
            };
            var result = await categoryDB.ListAsync(input);
            return result.DataItems;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng (Async)
        /// </summary>
        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            return await productDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một mặt hàng
        /// </summary>
        public static async Task<Product?> GetProductAsync(int productID)
        {
            return await productDB.GetAsync(productID);
        }

        /// <summary>
        /// Lấy danh sách ảnh phụ của mặt hàng
        /// </summary>
        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return await productDB.ListPhotosAsync(productID);
        }

        /// <summary>
        /// Lấy danh sách thuộc tính của mặt hàng
        /// </summary>
        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return await productDB.ListAttributesAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin 1 ảnh phụ cụ thể
        /// </summary>
        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return await productDB.GetPhotoAsync(photoID);
        }

        /// <summary>
        /// Lấy thông tin 1 thuộc tính cụ thể
        /// </summary>
        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return await productDB.GetAttributeAsync(attributeID);
        }
    }
}