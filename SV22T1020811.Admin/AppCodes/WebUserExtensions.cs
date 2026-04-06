using System.Security.Claims;
using System.Text.Json;
using SV22T1020811.Models; // Đảm bảo namespace này chứa class UserData của bạn

namespace SV22T1020811.Admin
{
    public static class WebUserExtensions
    {
        /// <summary>
        /// Lấy thông tin người dùng được lưu trong Claim của Principal
        /// </summary>
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                // Tìm claim có tên là "UserData"
                var userDataClaim = principal.FindFirstValue("UserData");
                if (string.IsNullOrEmpty(userDataClaim))
                    return null;

                // Giải mã chuỗi JSON thành object
                return JsonSerializer.Deserialize<WebUserData>(userDataClaim);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Cấu trúc dữ liệu người dùng cần lưu trong phiên làm việc
    /// </summary>
    public class WebUserData
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Photo { get; set; } = "";
        public string RoleName { get; set; } = "";
    }
}