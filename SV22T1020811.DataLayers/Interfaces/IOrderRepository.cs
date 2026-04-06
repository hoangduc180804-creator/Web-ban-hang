using SV22T1020811.Models.Common;
using SV22T1020811.Models.Sales;

namespace SV22T1020811.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các chức năng xử lý dữ liệu cho đơn hàng
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input);
        /// <summary>
        /// Lấy thông tin 1 đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<OrderViewInfo?> GetAsync(int orderID);
        /// <summary>
        /// Bổ sung đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Mã đơn hàng được bổ sung</returns>
        Task<int> AddAsync(Order data);
        /// <summary>
        /// Cập nhật đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Order data);
        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int orderID);


        /// <summary>
        /// Lấy danh sách mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <returns></returns>
        Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID);
        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng trong một đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID);
        /// <summary>
        /// Bổ sung mặt hàng vào đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> AddDetailAsync(OrderDetail data);
        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateDetailAsync(OrderDetail data);
        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> DeleteDetailAsync(int orderID, int productID);
    }
}
