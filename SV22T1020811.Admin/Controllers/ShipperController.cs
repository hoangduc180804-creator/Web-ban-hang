using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;

namespace SV22T1020811.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý đơn vị vận chuyển (danh sách, thêm, sửa, xóa).
    /// </summary>
    [Authorize]
    public class ShipperController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách tất cả đơn vị vận chuyển.
        /// </summary>
        /// <returns>View danh sách đơn vị vận chuyển.</returns>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            int pageSize = 10;
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue ?? ""
            };

            ViewBag.SearchValue = searchValue;

            // Gọi Business Layer để lấy dữ liệu (đảm bảo hàm này đã có trong CommonDataService)
            var data = await CommonDataService.ListShippersAsync(input);

            return View(data);
        }

        /// <summary>
        /// Mở giao diện để bổ sung người giao hàng mới
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung người giao hàng";
            var data = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", data); // Dùng chung View Edit với hàm Edit sau này
        }

        /// <summary>
        /// Lưu dữ liệu (Thêm mới hoặc Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save(Shipper data)
        {
            // Kiểm tra dữ liệu đầu vào đơn giản
            if (string.IsNullOrWhiteSpace(data.ShipperName))
            {
                // Nếu lỗi, trả về lại View và báo lỗi (có thể bổ sung ModelState sau)
                ViewBag.Title = data.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật người giao hàng";
                return View("Edit", data);
            }

            // Thực hiện gọi Business Layer để lưu
            if (data.ShipperID == 0)
            {
                await CommonDataService.AddShipperAsync(data);
            }
            else
            {
                await CommonDataService.UpdateShipperAsync(data);
            }

            // Sau khi lưu xong, quay về trang danh sách và tìm đúng tên vừa lưu
            return RedirectToAction("Index", new { searchValue = data.ShipperName });
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin đơn vị vận chuyển.
        /// </summary>
        /// <returns>View Edit với tiêu đề "Cập nhật đơn vị vận chuyển".</returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật người giao hàng";

            // 1. Lấy dữ liệu người giao hàng từ database dựa trên ID
            var data = await CommonDataService.GetShipperAsync(id);

            // Nếu không tìm thấy (id không tồn tại), quay về trang danh sách
            if (data == null)
            {
                return RedirectToAction("Index");
            }

            // 2. Truyền đối tượng 'data' vào View "Edit"
            return View("Edit", data);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa đơn vị vận chuyển theo ID.
        /// </summary>
        /// <param name="id">ID của đơn vị vận chuyển cần xóa.</param>
        /// <returns>View xác nhận xóa.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa người giao hàng";
            var data = await CommonDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        // Thực thi xóa khi nhấn nút
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string dummy = "")
        {
            await CommonDataService.DeleteShipperAsync(id);
            return RedirectToAction("Index");
        }
    }
}
