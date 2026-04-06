namespace SV22T1020811.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu sử dụng cho từ điển dữ liệu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataDictionaryRepository<T> where T : class
    {
        /// <summary>
        /// Lấy danh sách dữ liệu
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ListAsync();
    }
}
