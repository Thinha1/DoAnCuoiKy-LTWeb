using System.ComponentModel.DataAnnotations;

namespace WebBanHoa.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 50 ký tự")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        public string Gender { get; set; }
        public string Address { get; set; }
    }
}