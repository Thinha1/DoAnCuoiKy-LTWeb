using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Order
        public ActionResult Index()
        {
            List<OrderDTO> odLists = db.Orders.Select( o =>
                new OrderDTO
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    CustomerName = o.User.Name,
                    Address = o.Address,
                    Status = o.Status,
                }).ToList();
            return View(odLists);
        }

        public ActionResult Details(string orderID)
        {
            List<OrderDetailsDTO> orderDetails = db.OrderDetails.Select(
                od => new OrderDetailsDTO
                {
                    ProductID = od.ProductID,
                    OrderID = od.OrderID,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                }).ToList();
            return View(orderDetails);
        }
    }
}