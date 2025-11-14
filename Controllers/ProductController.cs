using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            //Lấy ra những thằng loại cha để duyệt
            var parentTypes = db.Categories
                    .Where(t => t.ParentCategoryID == null)
                    .ToList();
            ViewBag.ParentTypes = parentTypes;
            return View();
        }

        public ActionResult _NavBar()
        {
            List<Category> lst = db.Categories.Where(pt => pt.ParentCategoryID == null).ToList();
            ViewBag.Themes = db.Themes.Where(t => t.ParentThemeID == null).ToList();
            return PartialView(lst);
        }

        public ActionResult _DiscountingProducts()
        {
            //Lấy ra 8 sản phẩm đang giảm giá sâu nhất
            List<ProductDTO> lst = db.Products.Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Price = p.Price,
                Image = p.Image,
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderByDescending(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
            }).OrderByDescending(p => p.DiscountRate).Take(8).ToList();
            return PartialView(lst);
        }

        public ActionResult _MostOrderedProducts()
        {
            //lấy ra những sản phẩm nhiều người đặt nhất
            List<ProductDTO> lst = db.Products.Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Price = p.Price,
                Image = p.Image,
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderByDescending(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
                TotalSold = p.OrderDetails.Sum(od => od.Quantity).Value
            }).OrderByDescending(p => p.TotalSold).Take(8).ToList();

            return PartialView(lst);
        }

        public ActionResult _SPTheoTungLoai(string categoryID)
        {
            var childTypeIds = db.Categories
                     .Where(t => t.ParentCategoryID == categoryID)
                     .Select(t => t.CategoryID)
                     .ToList();

            List<ProductDTO> lst = db.Products.Where(p => childTypeIds.Contains(p.CategoryID))
            .Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate,
            }).OrderByDescending(p => p.ProductID).Take(8).ToList();
            ViewBag.ParentProductType = db.Categories.FirstOrDefault(pt => pt.CategoryID == categoryID).CategoryName;
            return PartialView(lst);
        }

        public ActionResult Details(string productID)
        {
            ProductDTO product = db.Products.Where(p => p.ProductID == productID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                ThemeID = p.ThemeID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            }).FirstOrDefault();
            return View(product);
        }

        public ActionResult _RelevantProducts(string categoryID, string productID)
        {
            List<ProductDTO> products = db.Products.Where(p => p.CategoryID == categoryID && p.ProductID != productID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            })
            .OrderBy(p => p.ProductID)
            .Take(4).ToList();
            return PartialView(products);
        }

        public ActionResult SPTheoLoai(string categoryID, string sort, string size)
        {
            //Mặc định nếu ko chọn thì là 12
            List<ProductDTO> products = db.Products.Where(p => p.CategoryID == categoryID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            })
            .OrderBy(p => p.ProductName).Take(12)
            .ToList();

            if (!string.IsNullOrWhiteSpace(sort) && !string.IsNullOrWhiteSpace(size))
            {
                int iSize = Convert.ToInt32(size);
                if (sort == "0" || sort == "1")
                {
                    products = db.Products.Where(p => p.CategoryID == categoryID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderBy(p => p.ProductName).Take(iSize)
                    .ToList();
                }
                else if (sort == "2")
                {
                    products = db.Products.Where(p => p.CategoryID == categoryID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderByDescending(p => p.ProductName).Take(iSize)
                    .ToList();
                }
                else if (sort == "3")
                {
                    products = db.Products.Where(p => p.CategoryID == categoryID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                        //FinalPrice = p.Price - (p.Price * (decimal)(p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate) / 100)
                    })
                    .ToList()
                    .OrderBy(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
                }
                else if (sort == "4")
                {
                    products = db.Products.Where(p => p.CategoryID == categoryID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                        //FinalPrice = p.Price - (p.Price * (decimal)(p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate)/100)
                        
                    })
                    .ToList()
                    .OrderByDescending(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
                }
            }
            ViewBag.ProductTypeName = db.Categories.FirstOrDefault(pt => pt.CategoryID == categoryID).CategoryName;
            return View(products);
        }

        //Sản phẩm theo chủ đề
        public ActionResult SPTheoChuDe(string themeID, string sort, string size)
        {
            List<ProductDTO> products = db.Products.Where(p => p.ThemeID == themeID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            })
            .OrderBy(p => p.ProductName).Take(12)
            .ToList();

            if (!string.IsNullOrWhiteSpace(sort) && !string.IsNullOrWhiteSpace(size))
            {
                int iSize = Convert.ToInt32(size);
                if (sort == "0" || sort == "1")
                {
                    //Sort theo tên tăng dần
                    products = db.Products.Where(p => p.ThemeID == themeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderBy(p => p.ProductName).Take(iSize)
                    .ToList();
                }
                //Sort theo tên
                else if (sort == "2")
                {
                    products = db.Products.Where(p => p.ThemeID == themeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderByDescending(p => p.ProductName).Take(iSize)
                    .ToList();
                }
                //Sort giá đã giảm
                else if (sort == "3")
                {
                    products = db.Products.Where(p => p.ThemeID == themeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                        //FinalPrice = p.Price - (p.Price * (decimal)(p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate) / 100)
                    })
                    .ToList()
                    .OrderBy(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
                }
                //Sort theo giá 
                else if (sort == "4")
                {
                    products = db.Products.Where(p => p.ThemeID == themeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryID = p.CategoryID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                        //FinalPrice = p.Price - (p.Price * (decimal)(p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate)/100)

                    })
                    .ToList()
                    .OrderByDescending(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
                }
            }
            ViewBag.ThemeName = db.Themes.FirstOrDefault(pt => pt.ThemeID == themeID).ThemeName;
            return View(products);
        }
    }
}