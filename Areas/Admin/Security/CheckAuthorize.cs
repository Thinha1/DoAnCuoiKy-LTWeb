using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBanHoa.Areas.Security
{
    public class CheckAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var userID = filterContext.HttpContext.Session["UserID"];

            if (userID == null)
            {
                // Nếu chưa đăng nhập -> Đá về trang Login
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                        area = ""
                    })
                );
                return;
            }
            var userRole = filterContext.HttpContext.Session["UserRole"];
            if (userRole == null || userRole.ToString().Trim() != "R001")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index",
                        area = ""
                    })
                );
                return;
            }
        }
    }
}