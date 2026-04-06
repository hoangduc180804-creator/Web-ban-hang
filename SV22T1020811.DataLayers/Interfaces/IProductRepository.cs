using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;

namespace SV22T1020811.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu cho mặt hàng
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResult<Product>> ListAsync(ProductSearchInput input);
        /// <summary>
        /// Lấy thông tin 1 mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<Product?> GetAsync(int productID);
        /// <summary>
        /// Bổ sung mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Mã mặt hàng được bổ sung</returns>
        Task<int> AddAsync(Product data);
        /// <summary>
        /// Cập nhật mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Product data);
        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int productID);
        /// <summary>
        /// Kiểm tra mặt hàng có dữ liệu liên quan không
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> IsUsedAsync(int productID);

        /// <summary>
        /// Lấy danh sách thuộc tính của mặt hàng
        /// </summary>
        /// <param name="productID">Mã của mặt hàng</param>
        /// <returns></returns>
        Task<List<ProductAttribute>> ListAttributesAsync(int productID);
        /// <summary>
        /// Lấy thông tin của một thuộc tính
        /// </summary>
        /// <param name="attributeID">Mã của thuộc tính</param>
        /// <returns></returns>
        Task<ProductAttribute?> GetAttributeAsync(long attributeID);
        /// <summary>
        /// Bổ sung thuộc tính
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<long> AddAttributeAsync(ProductAttribute data);
        /// <summary>
        /// Cập nhật thuộc tính
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAttributeAsync(ProductAttribute data);
        /// <summary>
        /// Xóa thuộc tính
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        Task<bool> DeleteAttributeAsync(long attributeID);

        /// <summary>
        /// Lấy danh sách ảnh của mặt hàng
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns></returns>
        Task<List<ProductPhoto>> ListPhotosAsync(int productID);
        /// <summary>
        /// Lấy thông tin 1 ảnh của mặt hàng
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        Task<ProductPhoto?> GetPhotoAsync(long photoID);
        /// <summary>
        /// Bổ sung ảnh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<long> AddPhotoAsync(ProductPhoto data);
        /// <summary>
        /// Cập nhật ảnh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdatePhotoAsync(ProductPhoto data);
        /// <summary>
        /// Xóa ảnh
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        Task<bool> DeletePhotoAsync(long photoID);
    }
}
