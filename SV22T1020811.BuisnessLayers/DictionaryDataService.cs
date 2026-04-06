using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer;
using SV22T1020811.Models.DataDictionary;

namespace SV22T1020811.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;

        /// <summary>
        /// Ctor
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }
        /// <summary>
        /// Lấy danh sách tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}