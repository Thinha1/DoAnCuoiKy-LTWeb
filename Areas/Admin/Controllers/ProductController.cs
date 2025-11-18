using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Product
        public ActionResult Index()
        {
            List<ProductDTO> products = db.Products.Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Price = p.Price,
                Image = p.Image,
                CategoryName = (p.Category == null ? "Không có" : p.Category.CategoryName),
                Quantity = p.Quantity,
                ThemeName = (p.Theme == null ? "Không có" : p.Theme.ThemeName),
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderByDescending(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
            }).ToList();
            return View(products);
        }
    }
}