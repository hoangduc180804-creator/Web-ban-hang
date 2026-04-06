using System.ComponentModel.DataAnnotations;

namespace SV22T1020811.Shop.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}