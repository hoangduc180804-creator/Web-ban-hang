using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020811.BusinessLayers;
using System.Security.Claims;

namespace SV22T1020811.Admin.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            // MÃ HÓA mật khẩu người dùng nhập vào trước khi so khớp với DB
            string hashedPassword = CryptographyUtils.ToMD5(password);

            // Gọi Service với mật khẩu đã mã hóa
            var userAccount = await SecurityDataService.AuthorizeAsync(username, hashedPassword);

            if (userAccount != null)
            {
                // 1. Khởi tạo danh sách claims với các thông tin cơ bản
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userAccount.UserName),
                    new Claim("UserId", userAccount.UserId),
                    new Claim("DisplayName", userAccount.DisplayName),
                    new Claim("Email", userAccount.Email),
                    new Claim("Photo", userAccount.Photo ?? "")
                };

                // 2. TÁCH CHUỖI QUYỀN: Biến "employee,admin" thành mảng ["employee", "admin"]
                if (!string.IsNullOrEmpty(userAccount.RoleNames))
                {
                    var roles = userAccount.RoleNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var roleName in roles)
                    {
                        // Nạp từng quyền riêng lẻ vào danh sách Claims
                        claims.Add(new Claim(ClaimTypes.Role, roleName.Trim()));
                    }
                }

                // 4. Tạo cấu hình xác thực
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                // 5. Lưu Cookie và tiến hành đăng nhập
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            // Báo lỗi nếu sai Email hoặc Mật khẩu
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Xóa phiên đăng nhập (Cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Login");
        }

        // GET: Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string fullName, string oldPassword, string newPassword, string confirmPassword)
        {
            string userName = User.Identity?.Name ?? "";

            // 1. Kiểm tra mật khẩu cũ (Bắt buộc để xác thực chủ sở hữu)
            string hashedOldPassword = CryptographyUtils.ToMD5(oldPassword);
            var user = await SecurityDataService.AuthorizeAsync(userName, hashedOldPassword);
            if (user == null)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View();
            }

            // 2. Cập nhật Họ tên (Nếu có thay đổi)
            if (!string.IsNullOrEmpty(fullName))
            {
                // Gọi hàm vừa tạo
                bool isUpdated = await SecurityDataService.UpdateDisplayNameAsync(userName, fullName);

                if (isUpdated)
                {
                    // Mẹo: Sau khi đổi tên trong DB xong, Đức nên Logout 
                    // để khi Login lại Header sẽ cập nhật tên mới từ Claims
                    ViewBag.Message = "Cập nhật họ tên thành công!";
                }
            }

            // 3. Cập nhật mật khẩu (Nếu người dùng có nhập mật khẩu mới)
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Xác nhận mật khẩu mới không khớp.");
                    return View();
                }
                string hashedNewPassword = CryptographyUtils.ToMD5(newPassword);
                await SecurityDataService.ChangePasswordAsync(userName, hashedNewPassword);
            }
           
            ViewBag.Message = "Cập nhật thông tin thành công! Vui lòng đăng nhập lại để làm mới hiển thị.";
            return View();
        }
    }
}