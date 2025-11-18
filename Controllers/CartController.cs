using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class CartController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Cart
        public ActionResult Index()
        {
            string userID = Session["UserID"].ToString();
            if (userID != null)
            {
                //List<ShoppingCartItem> spiList = db.ShoppingCarts.Where(sp => sp.UserID == )
            }
            return View();
        }
    }
}