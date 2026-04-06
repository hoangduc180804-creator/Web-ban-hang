namespace SV22T1020811.Models.Sales
{
    /// <summary>
    /// Mở rộng các phương thức cho enum OrderStatusEnum
    /// </summary>
    public static class OrderStatusExtensions
    {
        /// <summary>
        /// Lấy chuỗi mô tả cho từng trạng thái của đơn hàng
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetDescription(this OrderStatusEnum status)
        {
            return status switch
            {
                OrderStatusEnum.Rejected => " bị từ chối",
                OrderStatusEnum.Cancelled => " đã bị hủy",
                OrderStatusEnum.New => " vừa tạo",
                OrderStatusEnum.Accepted => " đang duyệt",
                OrderStatusEnum.Shipping => " đang vận chuyển",
                OrderStatusEnum.Completed => " đã hoàn tất",
                _ => "Không xác định"
            };
        }
    }
}
