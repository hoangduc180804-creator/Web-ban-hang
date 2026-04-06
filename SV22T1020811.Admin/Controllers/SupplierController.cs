using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;

namespace SV22T1020811.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý nhà cung cấp (danh sách, thêm, sửa, xóa).
    /// </summary>
    [Authorize]
    public class SupplierController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách tất cả nhà cung cấp.
        /// </summary>
        /// <returns>View danh sách nhà cung cấp.</returns>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            // Đặt pageSize lớn hơn số lượng bản ghi bạn có (ví dụ 20) 
            // để nó không bị ngắt trang khi bạn chỉ có vài nhà cung cấp.
            int pageSize = 20;

            // Quan trọng: Nếu searchValue là null thì gán thành chuỗi rỗng
            searchValue = searchValue ?? "";

            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };

            ViewBag.SearchValue = searchValue;
            var data = await CommonDataService.ListSuppliersAsync(input);

            return View(data);
        }

        /// <summary>
        /// Hiển thị form thêm mới nhà cung cấp (dùng lại view Edit).
        /// </summary>
        /// <returns>View Edit với tiêu đề "Thêm nhà cung cấp".</returns>
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";

            // Khởi tạo đối tượng trống
            var data = new Supplier()
            {
                SupplierID = 0
            };

            // Lấy danh sách tỉnh thành cho dropdown
            ViewBag.Provinces = await CommonDataService.ListProvincesAsync();

            return View("Edit", data);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Supplier data)
        {
            // 1. Kiểm tra dữ liệu đầu vào (Validation)
            if (string.IsNullOrWhiteSpace(data.SupplierName))
            {
                ViewBag.Provinces = await CommonDataService.ListProvincesAsync();
                return View("Edit", data);
            }

            // 2. Thực hiện Lưu (Thêm hoặc Sửa)
            if (data.SupplierID == 0)
            {
                await CommonDataService.AddSupplierAsync(data);
            }
            else
            {
                await CommonDataService.UpdateSupplierAsync(data);
            }

            // 3. QUAN TRỌNG: Chuyển hướng về Index và truyền kèm tên vừa lưu vào ô tìm kiếm
            // Việc này giúp người dùng thấy ngay kết quả vừa thao tác.
            return RedirectToAction("Index", new { searchValue = data.SupplierName });
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin nhà cung cấp theo ID.
        /// </summary>
        /// <param name="id">ID của nhà cung cấp cần chỉnh sửa.</param>
        /// <returns>View Edit với tiêu đề "Cập nhật nhà cung cấp".</returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật nhà cung cấp";

            // 1. Lấy dữ liệu nhà cung cấp từ database dựa trên ID
            var data = await CommonDataService.GetSupplierAsync(id);

            // Nếu không tìm thấy nhà cung cấp (id không tồn tại), quay về trang danh sách
            if (data == null)
            {
                return RedirectToAction("Index");
            }

            // 2. Lấy danh sách tỉnh thành để hiển thị trong SelectBox
            ViewBag.Provinces = await CommonDataService.ListProvincesAsync();

            // 3. Truyền đối tượng 'data' vào View "Edit"
            // (Vì Create và Edit dùng chung View "Edit.cshtml" nên ta gọi đích danh View này)
            return View("Edit", data);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa nhà cung cấp theo ID.
        /// </summary>
        /// <param name="id">ID của nhà cung cấp cần xóa.</param>
        /// <returns>View xác nhận xóa.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa nhà cung cấp";
            var data = await CommonDataService.GetSupplierAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        // Thực thi xóa khi nhấn nút Xóa trên Form
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string dummy = "") // Thêm tham số dummy để khác biệt chữ ký hàm
        {
            await CommonDataService.DeleteSupplierAsync(id);
            return RedirectToAction("Index");
        }
    }
}
