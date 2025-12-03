namespace WebBanHoa.Models
{
    public class UserDTO
    {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public int IsEnabled { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
    }
}