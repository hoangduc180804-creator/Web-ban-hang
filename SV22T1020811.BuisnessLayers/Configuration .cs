namespace SV22T1020811.BusinessLayers
{
    /// <summary>
    /// lớp lưu giữ thông tin cấu hình cần sử dụng trong Business Layer.
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString="";
        /// <summary>
        ///  khởi tạo cau hình cần thiết cho Business Layer, hiện tại chỉ có chuỗi kết nối đến CSDL.
        ///  (hàm này sẽ được gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        ///  thuộc tính chỉ đọc trả về chuỗi kết nối đến CSDL đã được khởi tạo trước đó.
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
