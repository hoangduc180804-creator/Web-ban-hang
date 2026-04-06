using SV22T1020811.Datalayers.SQLServer;
using SV22T1020811.DataLayers;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer;
using SV22T1020811.Models.Catalog;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.DataDictionary;
using SV22T1020811.Models.HR;
using SV22T1020811.Models.Partner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Cần thiết cho Task

namespace SV22T1020811.BusinessLayers
{
    public static class CommonDataService
    {
        private static readonly IEmployeeRepository employeeDB;
        private static readonly IGenericRepository<Supplier> supplierDB;
        private static readonly IDataDictionaryRepository<Province> provinceDB;
        private static readonly IGenericRepository<Shipper> shipperDB;// Sửa lại Interface cho Province

        static CommonDataService()
        {
            string connectionString = Configuration.ConnectionString;

            employeeDB = new EmployeeRepository(connectionString);
            supplierDB = new SupplierRepository(connectionString);
            provinceDB = new ProvinceRepository(connectionString);
            shipperDB = new ShipperRepository(connectionString);
        }

        #region Province
        /// <summary>
        /// Lấy danh sách toàn bộ tỉnh thành (Phải dùng Async để khớp với Repository bạn gửi)
        /// </summary>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
        #endregion

        #region Supplier
        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin một nhà cung cấp theo ID
        /// </summary>
        public static async Task<Supplier?> GetSupplierAsync(int id)
        {
            return await supplierDB.GetAsync(id);
        }

        /// <summary>
        /// Bổ sung nhà cung cấp mới
        /// </summary>
        public static async Task<int> AddSupplierAsync(Supplier data)
        {
            return await supplierDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public static async Task<bool> UpdateSupplierAsync(Supplier data)
        {
            return await supplierDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa nhà cung cấp (nếu không có dữ liệu liên quan)
        /// </summary>
        public static async Task<bool> DeleteSupplierAsync(int id)
        {
            if (await supplierDB.IsUsedAsync(id))
                return false;

            return await supplierDB.DeleteAsync(id);
        }

        /// <summary>
        /// Kiểm tra xem nhà cung cấp có dữ liệu liên quan hay không
        /// </summary>
        public static async Task<bool> IsUsedSupplierAsync(int id)
        {
            return await supplierDB.IsUsedAsync(id);
        }
        #endregion

        #region Shipper
        /// <summary>
        /// Danh sách người giao hàng có phân trang và tìm kiếm
        /// </summary>
        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        /// <summary>
        /// Thêm mới một người giao hàng
        /// </summary>
        public static async Task<int> AddShipperAsync(Shipper data)
        {
            return await shipperDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin một người giao hàng
        /// </summary>
        public static async Task<bool> UpdateShipperAsync(Shipper data)
        {
            return await shipperDB.UpdateAsync(data);
        }
        /// <summary>
        /// Chỉnh sửa thông tin một người giao hàng
        /// </summary>
        public static async Task<Shipper?> GetShipperAsync(int id)
        {
            return await shipperDB.GetAsync(id);
        }
        /// <summary>
        /// Xóa thông tin một người giao hàng
        /// </summary>
        public static async Task<bool> DeleteShipperAsync(int id)
        {
            return await shipperDB.DeleteAsync(id);
        }
        #endregion

        #region Employee

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            return await employeeDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên dựa vào mã nhân viên
        /// </summary>
        public static async Task<Employee?> GetEmployeeAsync(int id)
        {
            return await employeeDB.GetAsync(id);
        }

        /// <summary>
        /// Bổ sung một nhân viên mới
        /// </summary>
        public static async Task<int> AddEmployeeAsync(Employee data)
        {
            return await employeeDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public static async Task<bool> UpdateEmployeeAsync(Employee data)
        {
            return await employeeDB.UpdateAsync(data);
        }

        public static async Task<bool> UpdateEmployeeRoleAsync(int id, string roleNames)
        {
            // Gọi xuống Repository để thực hiện Update cột RoleNames
            return await employeeDB.UpdateRoleAsync(id, roleNames);
        }

        /// <summary>
        /// Xóa một nhân viên (nếu không có dữ liệu liên quan trong bảng đơn hàng)
        /// </summary>
        public static async Task<bool> DeleteEmployeeAsync(int id)
        {
            if (await employeeDB.IsUsedAsync(id))
                return false;

            return await employeeDB.DeleteAsync(id);
        }

        /// <summary>
        /// Kiểm tra xem nhân viên có dữ liệu liên quan hay không
        /// </summary>
        public static async Task<bool> IsUsedEmployeeAsync(int id)
        {
            return await employeeDB.IsUsedAsync(id);
        }

        /// <summary>
        /// Kiểm tra email nhân viên có hợp lệ (không trùng) hay không
        /// </summary>
        public static async Task<bool> ValidateEmployeeEmailAsync(string email, int id = 0)
        {
            return await employeeDB.ValidateEmailAsync(email, id);
        }

        public static async Task<bool> ChangePasswordEmployeeAsync(int id, string newPassword)
        {
            // Mã hóa mật khẩu MD5
            string encryptedPassword = CryptographyUtils.ToMD5(newPassword);

            // Gửi xuống Repository để UPDATE DUY NHẤT cột Password
            return await employeeDB.ChangePasswordAsync(id, encryptedPassword);
        }



        #endregion
    }
}