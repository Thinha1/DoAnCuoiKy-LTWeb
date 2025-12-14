using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;
using WebBanHoa.Areas.Security;

namespace WebBanHoa.Areas.Admin.Controllers
{
    [CheckAuthorize]
    public class DashBoardController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/DashBoard
        public ActionResult Index()
        {
            ViewBag.NoOfProducts = db.Products.Count();
            ViewBag.NoOfOrders = db.Orders.Count();
            ViewBag.NoOfCustomers = db.Users.Where(u => u.RoleID == "R002").Count();
            ViewBag.Revenue = db.OrderDetails.Where(o => o.Order.Status != "Đã huỷ").Sum(od => od.UnitPrice * od.Quantity);

            List<ProductDTO> top5Products = db.Products
                .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity)) // Sắp xếp theo tổng bán
                .Take(5)
                .Select(p => new ProductDTO
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Image = p.Image,
                    Price = p.Price,
                    TotalSold = p.OrderDetails.Sum(od => od.Quantity),
                    TotalRevenue = p.OrderDetails.Sum(o => o.Quantity * o.UnitPrice)
                })
                .ToList();
            ViewBag.Top5Products = top5Products;
            return View();
        }
    }
}