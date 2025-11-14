using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
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
            var parentTypes = db.ProductTypes
                    .Where(t => t.ProductTypeParentID == null)
                    .ToList();
            ViewBag.ParentTypes = parentTypes;
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

        public ActionResult _SPTheoTungLoai(string productTypeID)
        {
            var childTypeIds = db.ProductTypes
                     .Where(t => t.ProductTypeParentID == productTypeID)
                     .Select(t => t.ProductTypeID)
                     .ToList();

            List<ProductDTO> lst = db.Products.Where(p => childTypeIds.Contains(p.ProductTypeID))
            .Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate,
            }).OrderByDescending(p => p.ProductID).Take(8).ToList();
            ViewBag.ParentProductType = db.ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == productTypeID).ProductTypeName;
            return PartialView(lst);
        }

        public ActionResult Details(string productID)
        {
            ProductDTO product = db.Products.Where(p => p.ProductID == productID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            }).FirstOrDefault();
            return View(product);
        }

        public ActionResult _RelevantProducts(string productTypeID, string productID)
        {
            List<ProductDTO> products = db.Products.Where(p => p.ProductTypeID == productTypeID && p.ProductID != productID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            })
            .OrderBy(p => p.ProductID)
            .Take(4).ToList();
            return PartialView(products);
        }

        public ActionResult SPTheoLoai(string productTypeID, string sort, string size)
        {
            //Mặc định nếu ko chọn thì là 12
            List<ProductDTO> products = db.Products.Where(p => p.ProductTypeID == productTypeID).Select(p => new ProductDTO
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                Description = p.Description,
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
            })
            .OrderBy(p => p.ProductName).Take(12)
            .ToList();

            if (string.IsNullOrWhiteSpace(sort))
            {
                sort = "1"; // Mặc định sắp xếp theo Tên A-Z
            }

            //Truy vấn xuống cơ sở dữ liệu lấy toàn bộ products thoả điều kiện
            var productsBase = db.Products
                .Where(p => p.CategoryID == categoryID)
                .Select(p => new ProductDTO
                {
                    products = db.Products.Where(p => p.ProductTypeID == productTypeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        ProductTypeID = p.ProductTypeID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderBy(p => p.ProductName).Take(iSize)
                    .ToList();
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
                    products = db.Products.Where(p => p.ProductTypeID == productTypeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        ProductTypeID = p.ProductTypeID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .OrderByDescending(p => p.ProductName).Take(iSize)
                    .ToList();
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
                    products = db.Products.Where(p => p.ProductTypeID == productTypeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        ProductTypeID = p.ProductTypeID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .ToList()
                    .OrderBy(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
                    productsBase = productsBase.OrderByDescending(p => p.ProductName);
                }
                else // sort == "1" or "0" (Tên A-Z)
                {
                    products = db.Products.Where(p => p.ProductTypeID == productTypeID).Select(p => new ProductDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        ProductTypeID = p.ProductTypeID,
                        Price = p.Price,
                        Image = p.Image,
                        Description = p.Description,
                        DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderBy(d => d.DiscountRate).FirstOrDefault().DiscountRate
                    })
                    .ToList()
                    .OrderByDescending(p => p.Price - (p.Price * (decimal)(p.DiscountRate ?? 0) / 100))
                    .Take(iSize)
                    .ToList();
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
                .Where(p => p.ThemeID == themeID)
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
            ViewBag.ProductTypeName = db.ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == productTypeID).ProductTypeName;

            ViewBag.Page = page;
            ViewBag.NoOfPages = noOfPages; 

            ViewBag.ThemeName = db.Themes.FirstOrDefault(pt => pt.ThemeID == themeID).ThemeName;

            return View(products);
        }

        //public ActionResult DatHang(string productID, string currentPrice)
        //{

        //}
    }
}