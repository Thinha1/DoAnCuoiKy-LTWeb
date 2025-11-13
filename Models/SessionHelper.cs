using System.Web;

namespace WebBanHoa.Models
{
    public static class SessionHelper
    {
        public static void SetUserSession(UserDTO user)
        {
            HttpContext.Current.Session["UserID"] = user.UserID;
            HttpContext.Current.Session["UserName"] = user.Name;
            HttpContext.Current.Session["UserEmail"] = user.Email;
            HttpContext.Current.Session["UserRole"] = user.RoleID;
            HttpContext.Current.Session["User"] = user;
        }

        public static UserDTO GetUserSession()
        {
            return HttpContext.Current.Session["User"] as UserDTO;
        }

        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}