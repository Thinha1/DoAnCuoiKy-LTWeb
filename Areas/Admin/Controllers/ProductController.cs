using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Areas.Security;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    [CheckAuthorize]
    public class ProductController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Product
        public ActionResult Index(string TuKhoa)
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
                IsAvailable = p.IsAvailable,
            }).ToList();

            if (!string.IsNullOrWhiteSpace(TuKhoa))
            {
                products = products.Where(od => od.ProductName.Contains(TuKhoa)).ToList();
                return View(products);
            }
            return View(products);
        }

        public ActionResult Create()
        {
            ProductDTO p = new ProductDTO();
            p.ProductID = IDGenerator.GenerateProductID();
            ViewBag.Theme = db.Themes.Where(t => t.ParentThemeID != null).ToList();
            ViewBag.Category = db.Categories.Where(ct => ct.ParentCategoryID != null).ToList();
            return View(p);
        }

        [HttpPost]
        public ActionResult Create(ProductDTO dto, HttpPostedFileBase ImageUpload, string IsAvailable)
        {
            if (ModelState.IsValid)
            {
                Product p = new Product();
                p.ProductID = dto.ProductID;
                p.ProductName = dto.ProductName;
                p.CategoryID = dto.CategoryID;
                p.ThemeID = dto.ThemeID;
                p.Price = dto.Price;
                p.Quantity = dto.Quantity;
                p.Description = dto.Description;
                if (IsAvailable != null)
                {
                    p.IsAvailable = 1;
                }

                if (ImageUpload != null)
                {
                    string extension = Path.GetExtension(ImageUpload.FileName);

                    string productFileName = "img_" + p.ProductID + extension;

                    string path = Path.Combine(Server.MapPath("~/Content/Images/"), productFileName);

                    ImageUpload.SaveAs(path);

                    p.Image = productFileName;
                }
                else
                {
                    p.Image = "no-image.jpg";
                }
                p.CreatedAt = DateTime.Now;
                p.CreatedBy = Session["UserName"].ToString();
                db.Products.Add(p);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Theme = db.Themes.Where(t => t.ParentThemeID != null).ToList();
                ViewBag.Category = db.Categories.Where(ct => ct.ParentCategoryID != null).ToList();
                return View("Create", dto);
            }
        }

        public ActionResult Edit(string productID)
        {
            Product product = db.Products.SingleOrDefault(p => p.ProductID == productID);
            if (product != null)
            {
                ProductDTO productDTO = new ProductDTO
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Image = product.Image,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    ThemeID = (product.Theme == null ? "Không có" : product.Theme.ThemeID),
                    CategoryID = (product.Category == null ? "Không có" : product.Category.CategoryID),
                    IsAvailable = product.IsAvailable,
                };
                ViewBag.Theme = db.Themes.Where(t => t.ParentThemeID != null).ToList();
                ViewBag.Category = db.Categories.Where(ct => ct.ParentCategoryID != null).ToList();
                return View(productDTO);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(ProductDTO dto, HttpPostedFileBase ImageUpload, string isAvailable)
        {
            if (ModelState.IsValid)
            {
                Product p = db.Products.FirstOrDefault(product => product.ProductID == dto.ProductID);
                p.ProductID = dto.ProductID;
                p.ProductName = dto.ProductName;
                p.CategoryID = dto.CategoryID;
                p.ThemeID = dto.ThemeID;
                p.Price = dto.Price;
                p.Quantity = dto.Quantity;
                p.Description = dto.Description;
                p.IsAvailable = Convert.ToInt16(isAvailable);
                if (ImageUpload != null)
                {
                    string extension = Path.GetExtension(ImageUpload.FileName);

                    string productFileName = "img_" + p.ProductID + extension;

                    string path = Path.Combine(Server.MapPath("~/Content/Images/"), productFileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    ImageUpload.SaveAs(path);

                    p.Image = productFileName;
                }
                p.UpdatedAt = DateTime.Now;
                p.UpdatedBy = Session["UserName"].ToString();
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Theme = db.Themes.Where(t => t.ParentThemeID != null).ToList();
                ViewBag.Category = db.Categories.Where(ct => ct.ParentCategoryID != null).ToList();
                return View("Edit", dto);
            }
        }

        public ActionResult DisableProduct(string productID)
        {
            Product p = db.Products.FirstOrDefault(product => product.ProductID == productID);
            if (p != null)
            {
                p.IsAvailable = 0;
                CancelOrder(productID);
                db.SaveChanges();
            }
            TempData["Success"] = "Khoá sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult EnableProduct(string productID)
        {
            Product p = db.Products.FirstOrDefault(product => product.ProductID == productID);
            if (p != null)
            {
                p.IsAvailable = 1;
                db.SaveChanges();
            }
            TempData["Success"] = "Mở khoá sản phẩm thành công!";
            return RedirectToAction("Index");
        }
        public void CancelOrder(string productID)
        {
            //huỷ đơn hàng sau khi disable sản phẩm
            List<Order> orders = db.Orders.Where(o => o.Status == "Chờ xử lý"
            && o.OrderDetails.Any(sp => sp.ProductID == productID)).ToList();
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    order.Status = "Đã huỷ";
                }
            }
        }
    }
}