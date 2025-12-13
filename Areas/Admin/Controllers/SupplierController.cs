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
    public class SupplierController : Controller
    {
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Supplier
        public ActionResult Index(string tuKhoa)
        {
            List<Supplier> suppliers = db.Suppliers.ToList();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                suppliers = db.Suppliers.Where(s => s.SupplierID.ToLower().Contains(tuKhoa.ToLower()) || s.Name.ToLower().Contains(tuKhoa.ToLower())).ToList();
            }
            return View(suppliers);
        }

        public ActionResult Create()
        {
            Supplier supplier = new Supplier();
            supplier.SupplierID = IDGenerator.GenerateSupplierID();
            return View(supplier);
        }

        [HttpPost]
        public ActionResult Create(Supplier s)
        {
            if (ModelState.IsValid)
            {
                s.CreatedAt = DateTime.Now;
                s.CreatedBy = Session["UserID"].ToString();
                db.Suppliers.Add(s);
                TempData["Success"] = "Bạn đã thêm nhà cung cấp mới thành công !";
                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Đã xảy ra lỗi trong quá trình thêm!";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string supplierId)
        {
            Supplier s = db.Suppliers.Find(supplierId);
            if (s != null)
            {
                return View(s);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy nhà cung cấp!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(Supplier s)
        {
            if (ModelState.IsValid)
            {
                Supplier supplier = db.Suppliers.Find(s);
                if (supplier != null)
                {
                    supplier.Name = s.Name;
                    supplier.Address = s.Address;
                    supplier.Email = s.Email;
                    supplier.Phone = s.Phone;
                    supplier.UpdatedAt = DateTime.Now;
                    supplier.UpdatedBy = Session["UserID"].ToString();
                }
                db.SaveChanges();
                TempData["Success"] = "Bạn đã cập nhật thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại!";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string supplierId)
        {
            Supplier s = db.Suppliers.Find(supplierId);
            if (s != null)
            {
                s.IsDeleted = 1;
                s.UpdatedAt = DateTime.Now;
                s.UpdatedBy = Session["UserID"].ToString();
                TempData["Success"] = "Đã xoá nhà cung cấp thành công!";
                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Không tìm thấy nhà cung cấp!";
            }
            return RedirectToAction("Index");
        }
    }
}