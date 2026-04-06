using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using SV22T1020811.Models.Partner;
using SV22T1020811.Shop.Models;
using System.Security.Claims;

namespace SV22T1020811.Shop.Controllers
{
    public class AccountController : Controller
    {
        #region Đăng nhập & Đăng ký

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Mã hóa MD5 mật khẩu người dùng nhập
            string hashedPassword = CryptographyUtils.ToMD5(model.Password);

            // 2. Gọi hàm AuthorizeAsync từ PartnerDataService mà Đức vừa gửi
            var customer = await PartnerDataService.AuthorizeAsync(model.Username, hashedPassword);

            if (customer == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (customer.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đang bị khóa.");
                return View(model);
            }

            // 3. Thiết lập Cookies
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("CustomerID", customer.CustomerID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

       

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer data, string ConfirmPassword)
        {
            // 1. Kiểm tra mật khẩu nhập lại
            if (data.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu nhập lại không khớp.");
                return View(data);
            }

            try
            {
                // 2. Kiểm tra email đã tồn tại chưa (Sử dụng hàm ValidatelCustomerEmailAsync của Đức)
                bool isEmailValid = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, 0);
                if (!isEmailValid)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký sử dụng.");
                    return View(data);
                }

                // 3. Chuẩn bị dữ liệu (Mã hóa mật khẩu MD5 giống hàm Login)
                data.Password = CryptographyUtils.ToMD5(data.Password);
                data.IsLocked = false;
                if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = data.CustomerName;

                // 4. Gọi hàm Add từ Service của Đức
                int customerId = await PartnerDataService.AddCustomerAsync(data);

                if (customerId > 0)
                {
                    TempData["Success"] = "Đăng ký thành công! Chào mừng Đức đến với hệ thống.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(data);
        }

        #endregion

        #region Quản lý tài khoản (Profile & Password)

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");
            var model = await PartnerDataService.GetCustomerAsync(customerId);

            if (model == null) return RedirectToAction("Logout");

            // Lấy danh sách tỉnh thành và bỏ vào ViewBag
            ViewBag.Provinces = await CommonDataService.ListProvincesAsync();

            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer data)
        {
            try
            {
                // Lấy lại thông tin gốc để đảm bảo không mất Email/Password
                var current = await PartnerDataService.GetCustomerAsync(data.CustomerID);
                if (current == null) return RedirectToAction("Logout");

                current.CustomerName = data.CustomerName;
                current.ContactName = data.ContactName;
                current.Phone = data.Phone;
                current.Address = data.Address;
                current.Province = data.Province;

                // Dùng hàm Update của PartnerDataService
                await PartnerDataService.UpdateCustomerAsync(current);
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Đừng để View() trống, hãy khởi tạo object mới
            var model = new ChangePasswordInput();
            return View(model);
        }

        /// <summary>
        /// Xử lý đổi mật khẩu dùng PartnerDataService
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordInput model)
        {
            if (!ModelState.IsValid) return View(model);

            string email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            int customerId = int.Parse(User.FindFirst("CustomerID")?.Value ?? "0");

            // 1. Kiểm tra mật khẩu cũ (Dùng lại hàm Authorize)
            string hashedOld = CryptographyUtils.ToMD5(model.OldPassword);
            var check = await PartnerDataService.AuthorizeAsync(email, hashedOld);

            if (check == null)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            // 2. Cập nhật mật khẩu mới (Mã hóa trước khi gửi xuống Service)
            string hashedNew = CryptographyUtils.ToMD5(model.NewPassword);
            bool result = await PartnerDataService.ChangePasswordAsync(customerId, hashedNew);

            if (result)
            {
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Không thể đổi mật khẩu. Vui lòng thử lại.");
            return View(model);
        }

        #endregion
    }
}