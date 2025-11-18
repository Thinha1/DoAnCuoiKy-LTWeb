using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    public class DashBoardController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/DashBoard
        public ActionResult Index()
        {
            ViewBag.NoOfProducts = db.Products.Count();
            ViewBag.NoOfOrders = db.Orders.Count();
            ViewBag.NoOfCustomers = db.Users.Where(u => u.RoleID == "R002").Count();
            return View();
        }
    }
}