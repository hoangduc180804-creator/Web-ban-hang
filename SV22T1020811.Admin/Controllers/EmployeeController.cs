using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.HR;

namespace SV22T1020811.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý nhân viên (chỉ dành cho Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    public class EmployeeController : Controller
    {
        private const int PAGE_SIZE = 20;

        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? ""
            };
            var data = await CommonDataService.ListEmployeesAsync(input);
            return View(data);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            var data = await CommonDataService.ListEmployeesAsync(input);
            return PartialView("Search", data);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhân viên";
            var data = new Employee()
            {
                EmployeeID = 0,
                BirthDate = new DateTime(2000, 1, 1),
                Photo = "nophoto.png",
                IsWorking = true
            };
            return View("Edit", data);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var data = await CommonDataService.GetEmployeeAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Employee data, string birthDateInput, IFormFile? uploadPhoto)
        {
            // 1. Xử lý ngày sinh từ chuỗi nhập vào (dd/MM/yyyy)
            DateTime? d = birthDateInput.ToDateTime(); // Giả sử bạn có Extension ToDateTime
            if (d.HasValue) data.BirthDate = d.Value;

            // 2. Kiểm tra dữ liệu đầu vào (Validation)
            if (string.IsNullOrWhiteSpace(data.FullName))
                ModelState.AddModelError(nameof(data.FullName), "Họ tên không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Thêm nhân viên" : "Cập nhật nhân viên";
                return View("Edit", data);
            }

            // 3. Xử lý ảnh tải lên (nếu có)
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/employees");
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            if (data.EmployeeID == 0) // Trường hợp thêm mới
            {
                // 1. Gán quyền mặc định (ví dụ là nhân viên)
                data.RoleNames = "employee";

                // 2. Gán mật khẩu mặc định (ví dụ: 123456) và PHẢI MÃ HÓA nó
                data.Password = CryptographyUtils.ToMD5("123456");

                await CommonDataService.AddEmployeeAsync(data);
            }
            else
            {
                await CommonDataService.UpdateEmployeeAsync(data);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }
            var data = await CommonDataService.GetEmployeeAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        /// <summary>
        /// Hiển thị form đổi mật khẩu cho nhân viên theo ID.
        /// </summary>
        /// <param name="id">ID của nhân viên cần đổi mật khẩu.</param>
        /// <returns>View đổi mật khẩu với tiêu đề "Đổi mật khẩu nhân viên".</returns>
        public async Task<IActionResult> ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu nhân viên";

            // 1. Lấy thông tin nhân viên từ Service dựa vào ID
            var data = await CommonDataService.GetEmployeeAsync(id);

            // 2. Nếu không tìm thấy nhân viên, quay lại trang danh sách
            if (data == null)
            {
                return RedirectToAction("Index");
            }

            // 3. Truyền 'data' vào View để hiển thị tên, email... lên form
            return View(data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            // 1. Kiểm tra mật khẩu khớp nhau
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp hoặc đang để trống.");
                var employee = await CommonDataService.GetEmployeeAsync(id);
                return View(employee);
            }

            // 2. Gọi hàm đổi mật khẩu chuyên biệt (Chỉ update cột Password)
            bool result = await CommonDataService.ChangePasswordEmployeeAsync(id, newPassword);

            if (result)
            {
                return RedirectToAction("Index"); // Thành công quay về danh sách
            }
            else
            {
                ModelState.AddModelError("", "Không thể cập nhật mật khẩu. Vui lòng thử lại.");
                var employee = await CommonDataService.GetEmployeeAsync(id);
                return View(employee);
            }
        }
        // 1. Hiển thị form phân quyền
        public async Task<IActionResult> ChangeRole(int id)
        {
            var data = await CommonDataService.GetEmployeeAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        // 2. Xử lý lưu phân quyền
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string[] roles)
        {
            // Chuyển mảng checkbox thành chuỗi cách nhau bởi dấu phẩy
            string roleNames = (roles != null) ? string.Join(",", roles) : "";

            // Gạch đỏ thường ở dòng này nếu CommonDataService chưa có hàm tương ứng
            bool result = await CommonDataService.UpdateEmployeeRoleAsync(id, roleNames);

            if (result)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không thể cập nhật phân quyền.");
            var data = await CommonDataService.GetEmployeeAsync(id);
            return View(data);
        }
    }
}
