using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến danh mục hàng hóa của hệ thống, 
    /// bao gồm: Loại hàng (Cagegory), mặt hàng (Product), thuộc tính của mặt hàng (ProductAttribute) và ảnh của mặt hàng (ProductPhoto).
    /// </summary>
    public static class CatalogDataService
    {
        private static readonly IProductRepository productDB;
        private static readonly IGenericRepository<Category> categoryDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static CatalogDataService()
        {
            categoryDB = new CategoryRepository(Configuration.ConnectionString);
            productDB = new ProductRepository(Configuration.ConnectionString);
        }

        #region Category

        /// <summary>
        /// Tìm kiếm và lấy danh sách loại hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách loại hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            return await categoryDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một loại hàng dựa vào mã loại hàng.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Category nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Category?> GetCategoryAsync(int CategoryID)
        {
            return await categoryDB.GetAsync(CategoryID);
        }

        /// <summary>
        /// Bổ sung một loại hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần bổ sung.</param>
        /// <returns>Mã loại hàng được tạo mới.</returns>
        public static async Task<int> AddCategoryAsync(Category data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await categoryDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một loại hàng.
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateCategoryAsync(Category data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await categoryDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa một loại hàng dựa vào mã loại hàng.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu loại hàng đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteCategoryAsync(int CategoryID)
        {
            if (await categoryDB.IsUsedAsync(CategoryID))
                return false;

            return await categoryDB.DeleteAsync(CategoryID);
        }

        /// <summary>
        /// Kiểm tra xem một loại hàng có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần kiểm tra.</param>
        /// <returns>
        /// True nếu loại hàng đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedCategoryAsync(int CategoryID)
        {
            return await categoryDB.IsUsedAsync(CategoryID);
        }

        #endregion

        #region Product

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang mặt hàng.
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách mặt hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            return await productDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Product nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Product?> GetProductAsync(int productID)
        {
            return await productDB.GetAsync(productID);
        }

        /// <summary>
        /// Bổ sung một mặt hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần bổ sung.</param>
        /// <returns>Mã mặt hàng được tạo mới.</returns>
        public static async Task<int> AddProductAsync(Product data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await productDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateProductAsync(Product data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await productDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa một mặt hàng dựa vào mã mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu mặt hàng đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteProductAsync(int productID, bool forceDelete = false)
        {
            // Nếu không phải xóa cưỡng bức thì mới kiểm tra xem đã được dùng chưa
            if (!forceDelete)
            {
                if (await productDB.IsUsedAsync(productID))
                    return false;
            }

            return await productDB.DeleteAsync(productID);
        }

        /// <summary>
        /// Kiểm tra xem một mặt hàng có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần kiểm tra.</param>
        /// <returns>
        /// True nếu mặt hàng đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedProductAsync(int productID)
        {
            return await productDB.IsUsedAsync(productID);
        }

        #endregion

        #region ProductAttribute

        /// <summary>
        /// Lấy danh sách các thuộc tính của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>
        /// Danh sách các thuộc tính của mặt hàng.
        /// </returns>
        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return await productDB.ListAttributesAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính.</param>
        /// <returns>
        /// Đối tượng ProductAttribute nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return await productDB.GetAttributeAsync(attributeID);
        }

        /// <summary>
        /// Bổ sung một thuộc tính mới cho mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần bổ sung.</param>
        /// <returns>Mã thuộc tính được tạo mới.</returns>
        public static async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await productDB.AddAttributeAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một thuộc tính mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            return await productDB.UpdateAttributeAsync(data);
        }

        /// <summary>
        /// Xóa một thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            return await productDB.DeleteAttributeAsync(attributeID);
        }

        #endregion

        #region ProductPhoto

        /// <summary>
        /// Lấy danh sách ảnh của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>
        /// Danh sách ảnh của mặt hàng.
        /// </returns>
        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return await productDB.ListPhotosAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh.</param>
        /// <returns>
        /// Đối tượng ProductPhoto nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return await productDB.GetPhotoAsync(photoID);
        }

        /// <summary>
        /// Bổ sung một ảnh mới cho mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin ảnh cần bổ sung.</param>
        /// <returns>Mã ảnh được tạo mới.</returns>
        public static async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            return await productDB.AddPhotoAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một ảnh mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin ảnh cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            return await productDB.UpdatePhotoAsync(data);
        }

        /// <summary>
        /// Xóa một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeletePhotoAsync(long photoID)
        {
            return await productDB.DeletePhotoAsync(photoID);
        }

        #endregion
    }
}