using System.ComponentModel.DataAnnotations;

namespace WebBanHoa.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}