using System.ComponentModel.DataAnnotations;

namespace WebBanHoa.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 50 ký tự")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}