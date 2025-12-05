using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    public class DiscountController : Controller
    {
        // GET: Admin/Promotion
        public ActionResult Index()
        {
            List<Discount> discountList = new List<Discount>();
            return View();
        }
    }
}