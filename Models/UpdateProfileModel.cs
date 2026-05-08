using System.ComponentModel.DataAnnotations;

namespace WebBanHoa.Models
{
    public class UpdateProfileModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}
