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
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
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
                    TotalSold = p.OrderDetails.Where(o => o.Order.Status != "Đã huỷ").Sum(od => od.Quantity),
                    TotalRevenue = p.OrderDetails.Where(o => o.Order.Status != "Đã huỷ").Sum(o => o.Quantity * o.UnitPrice)
                })
                .ToList();
            ViewBag.Top5Products = top5Products;

            var orders = db.Orders.Where(o => o.Status != "Đã huỷ"); // Lấy tất cả đơn thành công

            if (fromDate == null && toDate == null)
            {
                // Mặc định lấy 6 tháng đổ lại
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);
                orders = orders.Where(o => o.OrderDate >= sixMonthsAgo);
            }
            else
            {
                // Nếu CÓ chọn ngày thì lọc như bình thường
                if (fromDate.HasValue)
                    orders = orders.Where(o => o.OrderDate >= fromDate.Value);

                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.AddDays(1);
                    orders = orders.Where(o => o.OrderDate < endDate);
                }
            }

            ViewBag.MonthlyOrders = orders.ToList();

            return View();
        }
    }
}