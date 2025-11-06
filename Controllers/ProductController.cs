using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class ProductController : Controller
    {
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _NavBar()
        {
            List<ProductType> lst = db.ProductTypes.Where(pt => pt.ProductTypeParentID == null).ToList();
            return PartialView(lst);
        }

        public ActionResult _DiscountingProducts()
        {
            //Lấy ra 8 sản phẩm đang giảm giá sâu nhất
            List<Product> lst = db.Products.Include("Discounts")
                .Where(p => p.Discounts.Any(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now))
                //Lấy ra mã giảm giá để so sánh
                .OrderBy(p => p.Discounts
                .Where(d => d.StartDate <= DateTime.Now && d.EndDate > DateTime.Now)
                .Max(d => d.DiscountRate)).ToList();
            return PartialView(lst);
        }

        public ActionResult _MostOrderedProducts()
        {
            List<ProductDTO> lst = db.Products.Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
                TotalSold = p.OrderDetails.Sum(od => od.Quantity).Value
            }).OrderByDescending(p => p.TotalSold).Take(8).ToList();

            return PartialView(lst);
        }
    }
}