using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Areas.Security;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers
{
    [CheckAuthorize]
    public class DiscountController : Controller
    {
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Discount
        public ActionResult Index(string TuKhoa)
        {
            List<DiscountDTO> discounts = db.Discounts.Select(d => new DiscountDTO
            {
                DiscountID = d.DiscountID,
                ProductID = d.ProductID,
                ProductName = d.Product.ProductName,
                DiscountName = d.DiscountName,
                DiscountRate = (double)d.DiscountRate,
            }).ToList();
            if(!string.IsNullOrWhiteSpace(TuKhoa))
            {
                discounts = discounts.Where(d => d.ProductName.Contains(TuKhoa)).ToList();
            }
            return View(discounts);
        }

        public ActionResult Create()
        {
            DiscountDTO discount = new DiscountDTO();
            discount.DiscountID = IDGenerator.GenerateDiscountID();
            ViewBag.ProductID = new SelectList(db.Products.ToList(), "ProductID", "ProductName");
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DiscountDTO discount)
        {
            
            if (ModelState.IsValid)
            {
                Discount d = new Discount();
                d.DiscountID = discount.DiscountID;
                d.DiscountName = discount.DiscountName;
                d.ProductID = discount.ProductID;
                d.DiscountRate = discount.DiscountRate;
                d.StartDate = discount.StartDate;
                d.EndDate = discount.EndDate;
                d.CreatedAt = DateTime.Now;
                d.CreatedBy = Session["UserID"].ToString();
                db.Discounts.Add(d);
            }
            db.SaveChanges();
            TempData["Success"] = "Thêm mã giảm giá thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string discountID)
        {
            Discount discount = db.Discounts.Where(d => d.DiscountID == discountID).FirstOrDefault();
            if (discount != null)
            {
                DiscountDTO discountDTO = new DiscountDTO()
                {
                    DiscountID = discount.DiscountID,
                    DiscountName = discount.DiscountName,
                    ProductID = discount.ProductID,
                    ProductName = discount.Product.ProductName,
                    DiscountRate = (double)discount.DiscountRate,
                    StartDate = (DateTime)discount.StartDate,
                    EndDate = (DateTime)discount.EndDate
                };
                ViewBag.ProductList = new SelectList(db.Products.ToList(), "ProductID", "ProductName", discountDTO.ProductID);
                return View(discountDTO);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy mã giảm giá!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DiscountDTO dto)
        {
            if (ModelState.IsValid)
            {
                Discount discount = db.Discounts.Where(d => d.DiscountID == dto.DiscountID).FirstOrDefault();
                discount.DiscountName = dto.DiscountName;
                discount.ProductID = dto.ProductID;
                discount.DiscountRate = dto.DiscountRate;
                discount.StartDate = dto.StartDate;
                discount.EndDate = dto.EndDate;
                discount.UpdatedAt = DateTime.Now;
                discount.UpdatedBy = Session["UserID"].ToString();
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Mã giảm giá không tồn tại!";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string discountID)
        {
            Discount discount =  db.Discounts.FirstOrDefault(d => d.DiscountID ==  discountID);
            if (discount != null)
            {
                db.Discounts.Remove(discount);
                db.SaveChanges();
                TempData["Success"] = "Xoá thành công mã giảm giá!";
            }
            else
            {
                TempData["Error"] = "Xoá thất bại!";   
            }
            return RedirectToAction("Index");
        }
    }
}