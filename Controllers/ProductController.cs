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
                ProductTypeID = p.ProductTypeID,
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
                ProductTypeID = p.ProductTypeID,
                Price = p.Price,
                Image = p.Image,
                //Lấy giảm giá sâu nhất còn hạn sử dụng
                DiscountRate = p.Discounts.Where(d => d.EndDate > DateTime.Now && d.StartDate <= DateTime.Now).OrderByDescending(d => d.DiscountRate).FirstOrDefault().DiscountRate,
                Description = p.Description,
                TotalSold = p.OrderDetails.Sum(od => od.Quantity).Value
            }).OrderByDescending(p => p.TotalSold).Take(8).ToList();

            return PartialView(lst);
        }

        public ActionResult _SPTheoTungLoai(string productTypeId)
        {
            var childTypeIds = db.ProductTypes
                     .Where(t => t.ProductTypeParentID == productTypeId)
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
            ViewBag.ParentProductType = db.ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == productTypeId).ProductTypeName;
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
            if (!string.IsNullOrWhiteSpace(sort) && !string.IsNullOrWhiteSpace(size))
            {
                if (size == "0")
                {
                    if (sort == "0" || sort == "1")
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
                        .OrderByDescending(p => p.ProductName).Take(12)
                        .ToList();
                    }
                    else if (sort == "2")
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
                        .OrderBy(p => p.ProductName).Take(12)
                        .ToList();
                    }
                    else if (sort == "3")
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
                        .OrderBy(p => p.Price).Take(12)
                        .ToList();
                    }
                    else if(sort == "4")
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
                        .OrderByDescending(p => p.Price).Take(12)
                        .ToList();
                    }
                }
                else if (size == "1")
                {
                    if (sort == "0" || sort == "1")
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
                        .OrderBy(p => p.ProductName).Take(24)
                        .ToList();
                    }
                    else if (sort == "2")
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
                        .OrderByDescending(p => p.ProductName).Take(24)
                        .ToList();
                    }
                    else if (sort == "3")
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
                        .OrderBy(p => p.Price).Take(24)
                        .ToList();
                    }
                    else if (sort == "4")
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
                        .OrderByDescending(p => p.Price).Take(24)
                        .ToList();
                    }
                }
            }
            ViewBag.ProductTypeName = db.ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == productTypeID).ProductTypeName;
            return View(products);
        }
    }
}