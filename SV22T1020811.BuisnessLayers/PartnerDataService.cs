using SV22T1020811.BusinessLayers;
using SV22T1020811.Datalayers.SQLServer;
using SV22T1020811.DataLayers.Interfaces;
using SV22T1020811.DataLayers.SQLServer;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;

/// <summary>
/// Cung cấp các chức năng xử lý dữ liệu liên quan đến các đối tác của hệ thống
/// bao gồm: nhà cung cấp (Supplier), khách hàng (Customer) và người giao hàng (Shipper)
/// </summary>
public static class PartnerDataService
{
    private static readonly IGenericRepository<Supplier> supplierDB;
    private static readonly ICustomerRepository customerDB;
    private static readonly IGenericRepository<Shipper> shipperDB;

    /// <summary>
    /// Ctor
    /// </summary>
    static PartnerDataService()
    {
        supplierDB = new SupplierRepository(Configuration.ConnectionString);
        customerDB = new CustomerRepository(Configuration.ConnectionString);
        shipperDB = new ShipperRepository(Configuration.ConnectionString);
    }

    #region Supplier

    /// <summary>
    /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
    /// </param>
    /// <returns>
    /// Kết quả tìm kiếm dưới dạng danh sách nhà cung cấp có phân trang.
    /// </returns>
    public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
    {
        return await supplierDB.ListAsync(input);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một nhà cung cấp dựa vào mã nhà cung cấp.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần tìm.</param>
    /// <returns>
    /// Đối tượng Supplier nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Supplier?> GetSupplierAsync(int supplierID)
    {
        return await supplierDB.GetAsync(supplierID);
    }

    /// <summary>
    /// Bổ sung một nhà cung cấp mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin nhà cung cấp cần bổ sung.</param>
    /// <returns>Mã nhà cung cấp được tạo mới.</returns>
    public static async Task<int> AddSupplierAsync(Supplier data)
    {
        //TODO: Kiểm tra dữ liệu hợp lệ
        return await supplierDB.AddAsync(data);
    }

    /// <summary>
    /// Cập nhật thông tin của một nhà cung cấp.
    /// </summary>
    /// <param name="data">Thông tin nhà cung cấp cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateSupplierAsync(Supplier data)
    {
        //TODO: Kiểm tra dữ liệu hợp lệ
        return await supplierDB.UpdateAsync(data);
    }

    /// <summary>
    /// Xóa một nhà cung cấp dựa vào mã nhà cung cấp.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu nhà cung cấp đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteSupplierAsync(int supplierID)
    {
        if (await supplierDB.IsUsedAsync(supplierID))
            return false;

        return await supplierDB.DeleteAsync(supplierID);
    }

    /// <summary>
    /// Kiểm tra xem một nhà cung cấp có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần kiểm tra.</param>
    /// <returns>
    /// True nếu nhà cung cấp đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedSupplierAsync(int supplierID)
    {
        return await supplierDB.IsUsedAsync(supplierID);
    }

    #endregion

    #region Customer

    /// <summary>
    /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang.
    /// </param>
    /// <returns>
    /// Danh sách khách hàng phù hợp với điều kiện tìm kiếm.
    /// </returns>
    public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
    {
        return await customerDB.ListAsync(input);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một khách hàng dựa vào mã khách hàng.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần tìm.</param>
    /// <returns>
    /// Đối tượng Customer nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Customer?> GetCustomerAsync(int customerID)
    {
        return await customerDB.GetAsync(customerID);
    }

    /// <summary>
    /// Bổ sung một khách hàng mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin khách hàng cần bổ sung.</param>
    /// <returns>Mã khách hàng được tạo mới.</returns>
    public static async Task<int> AddCustomerAsync(Customer data)
    {
        // Kiểm tra email đã tồn tại chưa
        bool isValid = await customerDB.ValidateEmailAsync(data.Email, 0);
        if (!isValid)
            throw new Exception("Email đã tồn tại!");

        return await customerDB.AddAsync(data);
    }

    /// <summary>
    /// Cập nhật thông tin của một khách hàng.
    /// </summary>
    /// <param name="data">Thông tin khách hàng cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateCustomerAsync(Customer data)
    {
        // Kiểm tra email (trừ chính customer hiện tại)
        bool isValid = await customerDB.ValidateEmailAsync(data.Email, data.CustomerID);
        if (!isValid)
            throw new Exception("Email đã tồn tại!");

        return await customerDB.UpdateAsync(data);
    }

    /// <summary>
    /// Xóa một khách hàng dựa vào mã khách hàng.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu khách hàng đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteCustomerAsync(int customerID)
    {
        if (await customerDB.IsUsedAsync(customerID))
            return false;

        return await customerDB.DeleteAsync(customerID);
    }

    /// <summary>
    /// Kiểm tra xem một khách hàng có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần kiểm tra.</param>
    /// <returns>
    /// True nếu khách hàng đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedCustomerAsync(int customerID)
    {
        return await customerDB.IsUsedAsync(customerID);
    }

    /// <summary>
    /// Kiểm tra xem email của khách hàng có hợp lệ không
    /// </summary>
    /// <param name="email">Địa chỉ email cần kiểm tra</param>
    /// <param name="customerID">
    /// Bằng 0 nếu kiểm tra email đối với khách hàng mới.
    /// Khác 0 nếu kiểm tra email của khách hàng có mã là <paramref name="customerID"/>
    /// </param>
    /// <returns></returns>
    public static async Task<bool> ValidatelCustomerEmailAsync(string email, int customerID = 0)
    {
        return await customerDB.ValidateEmailAsync(email, customerID);
    }

    /// <summary>
    /// Xác thực thông tin đăng nhập của khách hàng
    /// </summary>
    /// <param name="email">Email đăng nhập</param>
    /// <param name="password">Mật khẩu</param>
    /// <returns>Đối tượng Customer nếu đúng, null nếu sai</returns>
    public static async Task<Customer?> AuthorizeAsync(string email, string password)
    {
        // Gọi xuống DataLayer để kiểm tra
        return await customerDB.AuthorizeAsync(email, password);
    }

    /// <summary>
    /// thực hiện đổi mật khẩu cho khách hàng.
    /// </summary>
    /// <param name="customerID"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public static async Task<bool> ChangePasswordAsync(int customerID, string newPassword)
    {
        // Giả sử customerRepository là biến đã được khởi tạo để gọi xuống DataLayer
        // Nếu bạn dùng Static, hãy gọi qua instance của Repository tương ứng
        return await customerDB.ChangePasswordAsync(customerID, newPassword);
    }
    #endregion

    #region Shipper

    /// <summary>
    /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang.
    /// </param>
    /// <returns>
    /// Danh sách người giao hàng phù hợp với điều kiện tìm kiếm.
    /// </returns>
    public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
    {
        return await shipperDB.ListAsync(input);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người giao hàng dựa vào mã người giao hàng.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần tìm.</param>
    /// <returns>
    /// Đối tượng Shipper nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Shipper?> GetShipperAsync(int shipperID)
    {
        return await shipperDB.GetAsync(shipperID);
    }

    /// <summary>
    /// Bổ sung một người giao hàng mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin người giao hàng cần bổ sung.</param>
    /// <returns>Mã người giao hàng được tạo mới.</returns>
    public static async Task<int> AddShipperAsync(Shipper data)
    {
        //TODO: Kiểm tra dữ liệu hợp lệ
        return await shipperDB.AddAsync(data);
    }

    /// <summary>
    /// Cập nhật thông tin của một người giao hàng.
    /// </summary>
    /// <param name="data">Thông tin người giao hàng cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateShipperAsync(Shipper data)
    {
        //TODO: Kiểm tra dữ liệu hợp lệ
        return await shipperDB.UpdateAsync(data);
    }

    /// <summary>
    /// Xóa một người giao hàng dựa vào mã người giao hàng.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu người giao hàng đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteShipperAsync(int shipperID)
    {
        if (await shipperDB.IsUsedAsync(shipperID))
            return false;

        return await shipperDB.DeleteAsync(shipperID);
    }

    /// <summary>
    /// Kiểm tra xem một người giao hàng có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần kiểm tra.</param>
    /// <returns>
    /// True nếu người giao hàng đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedShipperAsync(int shipperID)
    {
        return await shipperDB.IsUsedAsync(shipperID);
    }

    #endregion
}