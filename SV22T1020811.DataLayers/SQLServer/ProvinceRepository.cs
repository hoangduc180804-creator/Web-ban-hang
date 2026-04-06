using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.Models.DataDictionary;

namespace SV22T1020811.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu Tỉnh/Thành phố
    /// trong SQL Server sử dụng thư viện Dapper.
    /// 
    /// Cài đặt interface IDataDictionaryRepository cho đối tượng Province.
    /// </summary>
    public class ProvinceRepository : IDataDictionaryRepository<Province>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor khởi tạo repository với chuỗi kết nối đến CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public ProvinceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy danh sách tất cả tỉnh/thành phố
        /// </summary>
        /// <returns>Danh sách Province</returns>
        public async Task<List<Province>> ListAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Provinces
                ORDER BY ProvinceName
            ";

            var data = await connection.QueryAsync<Province>(sql);
            return data.ToList();
        }
    }
}
