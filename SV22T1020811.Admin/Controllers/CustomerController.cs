using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Common;
using SV22T1020811.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020811.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý khách hàng (danh sách, thêm, sửa, xóa).
    /// </summary>
    [Authorize]
    public class CustomerController : Controller
    {
        
        private const string CUSTOMER_SEARCH="CustomerSearchInput"; // Khóa lưu trữ thông tin tìm kiếm trong session
        /// <summary>
        /// nhập đầu vào tìm kiếm  -> hiển thị kết quả tìm kiếm
        /// Hiển thị danh sách tất cả khách hàng.
        /// </summary>
        /// <returns>View danh sách khách hàng.</returns>
        public  IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
            if(input == null)
                 input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = ""
            };
            return View(input);
        }
        /// <summary>
        /// tìm kiếm trả về kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var results = await PartnerDataService.ListCustomersAsync(input);
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
            return View(results);
        }
        /// <summary>
        /// Hiển thị form thêm khách hàng mới (dùng lại view Edit).
        /// </summary>
        /// <returns>View Edit với tiêu đề "Thêm khách hàng".</returns>
        // GET: Hiển thị form thêm mới
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm khách hàng";
            var model = new Customer()
            {
                CustomerID = 0,
                IsActive = true // Mặc định khách hàng mới là đang hoạt động
            };
            return View("Edit", model); // Vẫn dùng chung giao diện Edit.cshtml
        }

        // POST: Thực hiện lưu khách hàng mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Thêm khách hàng";
                return View("Edit", model);
            }

            try
            {
                int id = await PartnerDataService.AddCustomerAsync(model);

                if (id > 0)
                {
                    TempData["SuccessMessage"] = "Thêm mới khách hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Không thể thêm khách hàng. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                // Bắt lỗi email trùng ở đây
                ModelState.AddModelError("Email", ex.Message);
            }

            ViewBag.Title = "Thêm khách hàng";
            return View("Edit", model);
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin khách hàng theo ID.
        /// </summary>
        /// <param name="id">ID của khách hàng cần chỉnh sửa.</param>
        /// <returns>View Edit với tiêu đề "Cập nhật khách hàng".</returns>
        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật khách hàng";

            // Sử dụng await thay vì .Result để tránh chặn thread
            var customer = await PartnerDataService.GetCustomerAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm để bảo mật chống tấn công CSRF
        public async Task<IActionResult> Edit(Customer model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Cập nhật khách hàng";
                return View(model);
            }

            // Cập nhật dữ liệu bất đồng bộ
            var result = await PartnerDataService.UpdateCustomerAsync(model);

            if (result)
            {
                // Thông báo thành công (có thể dùng TempData)
                TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Cập nhật khách hàng thất bại! Vui lòng thử lại.");
            ViewBag.Title = "Cập nhật khách hàng";
            return View(model);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa khách hàng theo ID.
        /// </summary>
        /// <param name="id">ID của khách hàng cần xóa.</param>
        /// <returns>View xác nhận xóa.</returns>
        // GET: Hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return RedirectToAction("Index"); 

            return View(customer); // Truyền thông tin khách hàng để hiển thị trên view
        }

        // POST: Thực hiện xóa khách hàng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra khách hàng có đang được sử dụng (ví dụ: trong Orders)
            if (await PartnerDataService.IsUsedCustomerAsync(id))
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng đang được sử dụng!";
                return RedirectToAction("Index");
            }

            bool result = await PartnerDataService.DeleteCustomerAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Xóa khách hàng thất bại!";
            }

            return RedirectToAction("Index"); 
        }

        /// <summary>
        /// Hiển thị form đổi mật khẩu cho khách hàng theo ID.
        /// </summary>
        /// <param name="id">ID của khách hàng cần đổi mật khẩu.</param>
        /// <returns>View ChangePassword.</returns>

        public async Task<IActionResult> ChangePassword(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return RedirectToAction("Index");

            ViewBag.Customer = customer;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp hoặc để trống.");
                ViewBag.Customer = await PartnerDataService.GetCustomerAsync(id);
                return View();
            }

            // --- BƯỚC MÃ HÓA Ở ĐÂY ---
            // Ví dụ: Sử dụng một hàm MD5 hoặc SHA (tuy nhiên nên dùng BCrypt nếu có thể)
            // Ở đây tôi giả định bạn gọi một hàm Hash từ tiện ích của mình
            string hashedPassword = EncodePassword(newPassword);

            bool result = await PartnerDataService.ChangePasswordAsync(id, hashedPassword);

            if (result)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu.");
            ViewBag.Customer = await PartnerDataService.GetCustomerAsync(id);
            return View();
        }

        // Hàm mã hóa ví dụ (Bạn nên để hàm này trong một lớp Common/Utility)
        private string EncodePassword(string password)
        {
            // Bạn có thể dùng MD5 hoặc SHA256 để demo nhanh:
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = provider.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }
    }
}
