using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class HomeController : Controller
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

        public ActionResult Search(string TuKhoa)
        {
            List<ProductDTO> lst = db.Products.Where(pd => pd.ProductName.Contains(TuKhoa)).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Price = p.Price,
                Image = p.Image,
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderByDescending(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
            }).ToList();
            return View(lst);
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
            List<ProductDTO> lst = db.Products.Where(sp => sp.IsAvailable == 1).Select(p => new ProductDTO
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
            List<ProductDTO> lst = db.Products.Where(sp => sp.IsAvailable == 1).Select(p => new ProductDTO
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

            List<ProductDTO> lst = db.Products.Where(sp => sp.IsAvailable == 1).Where(p => childTypeIds.Contains(p.CategoryID))
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
            List<ProductDTO> products = db.Products.Where(p => p.CategoryID == categoryID && p.ProductID != productID && p.IsAvailable == 1).Select(p => new ProductDTO
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

        public ActionResult SPTheoLoai(string categoryID, string sort, string size, int page = 1)
        {
            int numberOfRecordPerPage = 8;
            if (!string.IsNullOrWhiteSpace(size))
            {
                numberOfRecordPerPage = Convert.ToInt32(size);
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                sort = "1"; // Mặc định sắp xếp theo Tên A-Z
            }

            //Truy vấn xuống cơ sở dữ liệu lấy toàn bộ products thoả điều kiện
            var productsBase = db.Products
                .Where(p => p.CategoryID == categoryID && p.IsAvailable == 1)
                .Select(p => new ProductDTO
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryID = categoryID,
                    ThemeID = p.ThemeID,
                    Price = p.Price,
                    Image = p.Image,
                    Description = p.Description,
                    DiscountRate = p.Discounts
                        .Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now)
                        .OrderBy(d => d.DiscountRate)
                        .FirstOrDefault().DiscountRate
                });


            int totalRecords = productsBase.Count();
            int noOfPages = (int)Math.Ceiling((double)totalRecords / numberOfRecordPerPage);
            int noOfRecordToSkip = (page - 1) * numberOfRecordPerPage;

            //Tạo list products để đẩy lên view
            List<ProductDTO> products;

            if (sort == "3" || sort == "4")
            {

                var allProducts = productsBase.ToList();

                if (sort == "3") // Giá tăng
                {
                    products = allProducts
                        .OrderBy(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                        .Skip(noOfRecordToSkip)
                        .Take(numberOfRecordPerPage)
                        .ToList();
                }
                else // sort == "4" (Giá giảm)
                {
                    products = allProducts
                        .OrderByDescending(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                        .Skip(noOfRecordToSkip)
                        .Take(numberOfRecordPerPage)
                        .ToList();
                }
            }
            else
            {
                // Sắp xếp tại DATABASE (Nhanh hơn nhiều)
                if (sort == "2") // Tên Z-A
                {
                    productsBase = productsBase.OrderByDescending(p => p.ProductName);
                }
                else // sort == "1" or "0" (Tên A-Z)
                {
                    productsBase = productsBase.OrderBy(p => p.ProductName);
                }

                products = productsBase
                    .Skip(noOfRecordToSkip)
                    .Take(numberOfRecordPerPage)
                    .ToList();
            }

            ViewBag.Page = page;
            ViewBag.NoOfPages = noOfPages;

            ViewBag.CategoryName = db.Categories.FirstOrDefault(pt => pt.CategoryID == categoryID).CategoryName;

            return View(products);
        }

        //Sản phẩm theo chủ đề
        public ActionResult SPTheoChuDe(string themeID, string sort, string size, int page = 1)
        {

            int numberOfRecordPerPage = 8;
            if (!string.IsNullOrWhiteSpace(size))
            {
                numberOfRecordPerPage = Convert.ToInt32(size);
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                sort = "1"; // Mặc định sắp xếp theo Tên A-Z
            }

            //Truy vấn xuống cơ sở dữ liệu lấy toàn bộ products thoả điều kiện
            var productsBase = db.Products
                .Where(p => p.ThemeID == themeID && p.IsAvailable == 1)
                .Select(p => new ProductDTO
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryID = p.CategoryID,
                    ThemeID = themeID,
                    Price = p.Price,
                    Image = p.Image,
                    Description = p.Description,
                    DiscountRate = p.Discounts
                        .Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now)
                        .OrderBy(d => d.DiscountRate)
                        .FirstOrDefault().DiscountRate
                });


            int totalRecords = productsBase.Count();
            int noOfPages = (int)Math.Ceiling((double)totalRecords / numberOfRecordPerPage);
            int noOfRecordToSkip = (page - 1) * numberOfRecordPerPage;

            //Tạo list products để đẩy lên view
            List<ProductDTO> products;

            if (sort == "3" || sort == "4")
            {

                var allProducts = productsBase.ToList();

                if (sort == "3") // Giá tăng
                {
                    products = allProducts
                        .OrderBy(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                        .Skip(noOfRecordToSkip)
                        .Take(numberOfRecordPerPage)
                        .ToList();
                }
                else // sort == "4" (Giá giảm)
                {
                    products = allProducts
                        .OrderByDescending(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                        .Skip(noOfRecordToSkip)
                        .Take(numberOfRecordPerPage)
                        .ToList();
                }
            }
            else
            {
                // Sắp xếp tại DATABASE (Nhanh hơn nhiều)
                if (sort == "2") // Tên Z-A
                {
                    productsBase = productsBase.OrderByDescending(p => p.ProductName);
                }
                else // sort == "1" or "0" (Tên A-Z)
                {
                    productsBase = productsBase.OrderBy(p => p.ProductName);
                }

                products = productsBase
                    .Skip(noOfRecordToSkip)
                    .Take(numberOfRecordPerPage)
                    .ToList();
            }

            ViewBag.Page = page;
            ViewBag.NoOfPages = noOfPages;

            ViewBag.ThemeName = db.Themes.FirstOrDefault(pt => pt.ThemeID == themeID).ThemeName;

            return View(products);
        }
    }
}