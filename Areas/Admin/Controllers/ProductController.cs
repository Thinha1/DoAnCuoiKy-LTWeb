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
                if(IsAvailable != null)
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
        public ActionResult Edit(ProductDTO dto, HttpPostedFileBase ImageUpload, string IsAvailable)
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
                if(IsAvailable != null)
                {
                    p.IsAvailable = 1;
                }
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

        public ActionResult Delete(string productID)
        {
            Product p = db.Products.FirstOrDefault(product => product.ProductID == productID);
            if (p != null)
            {
                //if (!string.IsNullOrEmpty(p.Image))
                //{
                //    string fullPath = Server.MapPath("~/Content/Images/" + p.Image);
                //    if (System.IO.File.Exists(fullPath))
                //    {
                //        System.IO.File.Delete(fullPath);
                //    }
                //}

                p.IsAvailable = 0;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}