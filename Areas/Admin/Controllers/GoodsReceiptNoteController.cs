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
    public class GoodsReceiptNoteController : Controller
    {
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/GoodsReceiptNote
        public ActionResult Index(string tuKhoa)
        {
            List<GoodsReceiptNoteDTO> goodsReceiptNoteDTOs =
                db.GoodsReceiptNotes
                .OrderByDescending(g => g.ReceiptDate)
                .Select(g => new GoodsReceiptNoteDTO
                {
                    GoodsReceiptNoteID = g.GoodsReceiptNoteID,
                    SupplierID = g.SupplierID,
                    SupplierName = g.Supplier.Name,
                    ReceiptDate = (DateTime)g.ReceiptDate,
                    IsDeleted = (int)g.IsDeleted,
                    TotalPrice = (decimal)g.GoodsReceiptNoteDetails.Sum(gd => gd.UnitPrice * gd.Quantity)
                }).ToList();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                goodsReceiptNoteDTOs = db.GoodsReceiptNotes
                .Where(n => n.GoodsReceiptNoteID.Contains(tuKhoa) || n.Supplier.Name.ToLower().Contains(tuKhoa.ToLower())).OrderByDescending(g => g.ReceiptDate)
                .Select(g => new GoodsReceiptNoteDTO
                {
                    GoodsReceiptNoteID = g.GoodsReceiptNoteID,
                    SupplierID = g.SupplierID,
                    SupplierName = g.Supplier.Name,
                    ReceiptDate = (DateTime)g.ReceiptDate,
                    IsDeleted = (int)g.IsDeleted,
                    TotalPrice = (decimal)g.GoodsReceiptNoteDetails.Sum(gd => gd.UnitPrice * gd.Quantity)
                }).ToList();
            }
            return View(goodsReceiptNoteDTOs);
        }

        public ActionResult Create()
        {
            GoodsReceiptNoteDTO goodsReceiptNote = new GoodsReceiptNoteDTO();
            goodsReceiptNote.GoodsReceiptNoteID = IDGenerator.GenerateGoodsReceiptNoteID();
            goodsReceiptNote.IsDeleted = 0;
            var suppliers = db.Suppliers.Where(s => s.IsDeleted != 1).OrderBy(s => s.Name).ToList();
            ViewBag.SupplierID = new SelectList(suppliers, "SupplierID", "Name");
            var products = db.Products.Where(p => p.IsAvailable == 1).OrderBy(p => p.ProductName).ToList();
            ViewBag.ProductID = new SelectList(products, "ProductID", "ProductName");
            return View(goodsReceiptNote);
        }

        private decimal RoundUpToThousand(decimal price)
        {
            return Math.Ceiling(price / 1000) * 1000;
        }

        [HttpPost]
        public ActionResult Create(GoodsReceiptNoteDTO dto)
        {
            // 1. VALIDATION THỦ CÔNG (Dùng TempData và Redirect ngay nếu lỗi)
            if (dto.GoodsReceiptNoteDetails == null || !dto.GoodsReceiptNoteDetails.Any())
            {
                TempData["Error"] = "Vui lòng nhập ít nhất một dòng sản phẩm.";
                return RedirectToAction("Create");
            }

            if (dto.ReceiptDate > DateTime.Now)
            {
                TempData["Error"] = "Ngày nhập kho không được lớn hơn thời điểm hiện tại.";
                return RedirectToAction("Create");
            }

            if (!ModelState.IsValid)
            {
                // Gom tất cả lỗi lại thành 1 chuỗi HTML để hiển thị
                string allErrors = string.Join("<br/>", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage));

                TempData["Error"] = "Dữ liệu không hợp lệ:<br/>" + allErrors;
                return RedirectToAction("Create");
            }

            // 3. XỬ LÝ LƯU DATABASE
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<string> updatedProducts = new List<string>();

                    // A. LƯU PHIẾU NHẬP (MASTER)
                    GoodsReceiptNote note = new GoodsReceiptNote();
                    note.GoodsReceiptNoteID = dto.GoodsReceiptNoteID;
                    note.IsDeleted = 0;
                    note.SupplierID = dto.SupplierID;
                    note.ReceiptDate = dto.ReceiptDate;
                    note.CreatedBy = Session["UserID"]?.ToString() ?? "Admin";
                    note.CreatedAt = DateTime.Now;
                    note.IsDeleted = 0;

                    db.GoodsReceiptNotes.Add(note);

                    // B. LƯU CHI TIẾT & CỘNG KHO (DETAIL)
                    foreach (var item in dto.GoodsReceiptNoteDetails)
                    {
                        // B1. Tạo chi tiết
                        GoodsReceiptNoteDetail detail = new GoodsReceiptNoteDetail();
                        detail.GoodsReceiptNoteID = note.GoodsReceiptNoteID;
                        detail.ProductID = item.ProductID;
                        detail.Quantity = item.Quantity;
                        detail.UnitPrice = item.UnitPrice;
                        db.GoodsReceiptNoteDetails.Add(detail);

                        // B2. Cập nhật Kho & Giá
                        var product = db.Products.Find(item.ProductID);
                        if (product != null)
                        {
                            // Cộng kho
                            product.Quantity = (product.Quantity ?? 0) + item.Quantity; // Lưu ý: dùng Stock hay Quantity tùy tên cột trong DB của bạn
                            product.UpdatedAt = DateTime.Now;
                            product.UpdatedBy = Session["UserID"].ToString();

                            // Logic Tăng Giá Bán
                            decimal currentSellingPrice = product.Price ?? 0;
                            decimal importPrice = item.UnitPrice;

                            //Nếu giá nhập lớn hơn 70% giá bán
                            if (currentSellingPrice == 0 || importPrice >= (currentSellingPrice * 0.7m))
                            {
                                decimal newSellingPrice = importPrice / 0.7m; // Biên lợi nhuận 30%
                                product.Price = RoundUpToThousand(newSellingPrice);
                                updatedProducts.Add(product.ProductName);
                            }
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // 4. THÔNG BÁO THÀNH CÔNG (TempData)
                    if (updatedProducts.Any())
                    {
                        TempData["Success"] = $"Nhập kho thành công! Đã tự động tăng giá bán cho: {string.Join(", ", updatedProducts)}.";
                    }
                    else
                    {
                        TempData["Success"] = "Đã nhập kho thành công!";
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Lỗi hệ thống -> Báo lỗi và quay lại trang tạo
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                    return RedirectToAction("Create");
                }
            }
        }

        public ActionResult Detail(string noteID)
        {
            var note = db.GoodsReceiptNotes.Find(noteID);
            ViewBag.ReceiptID = note.GoodsReceiptNoteID;
            ViewBag.SupplierName = note.Supplier.Name;
            ViewBag.ReceiptDate = note.ReceiptDate;

            List<GoodsReceiptNoteDetailDTO> goodsReceiptNoteDetailDTOs =
                db.GoodsReceiptNotes.Find(noteID)
                .GoodsReceiptNoteDetails.Select(gd => new GoodsReceiptNoteDetailDTO
                {
                    GoodsReceiptNoteID = gd.GoodsReceiptNoteID,
                    ProductID = gd.ProductID,
                    ProductName = gd.Product.ProductName,
                    Image = gd.Product.Image,
                    UnitPrice = (decimal)gd.UnitPrice,
                    Quantity = (int)gd.Quantity,
                }).ToList();
            return View(goodsReceiptNoteDetailDTOs);
        }

        public ActionResult Delete(string noteId)
        {
            // Dùng Transaction để đảm bảo: Hoặc là trừ kho thành công hết, hoặc là không làm gì cả
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tìm phiếu nhập
                    var note = db.GoodsReceiptNotes.FirstOrDefault(n => n.GoodsReceiptNoteID == noteId);

                    if (note == null || note.IsDeleted == 1)
                    {
                        TempData["Error"] = "Phiếu nhập không tồn tại hoặc đã bị hủy trước đó!";
                        return RedirectToAction("Index");
                    }

                    // 2. CHECK VÀ HOÀN KHO (QUAN TRỌNG NHẤT)
                    // Lặp qua từng sản phẩm trong chi tiết phiếu nhập đó
                    foreach (var detail in note.GoodsReceiptNoteDetails)
                    {
                        var product = db.Products.Find(detail.ProductID);

                        if (product != null)
                        {
                            // Logic: Tồn kho hiện tại - Lượng hàng đã nhập trong phiếu này
                            // Nếu kết quả < 0 => Nghĩa là hàng nhập về đã bị bán đi mất rồi -> KHÔNG ĐƯỢC HỦY
                            if ((product.Quantity ?? 0) - detail.Quantity < 0)
                            {
                                // Báo lỗi ngay lập tức và Rollback
                                transaction.Rollback();
                                TempData["Error"] = $"Không thể hủy! Sản phẩm '{product.ProductName}' đã được bán ra. Tồn kho hiện tại ({product.Quantity}) nhỏ hơn lượng cần hủy ({detail.Quantity}).";
                                return RedirectToAction("Index");
                            }

                            // Nếu đủ điều kiện thì Trừ kho
                            product.Quantity -= detail.Quantity;
                        }
                    }

                    // 3. Cập nhật trạng thái phiếu (Xóa mềm)
                    note.IsDeleted = 1;
                    note.UpdatedAt = DateTime.Now;

                    // Xử lý Session null để tránh lỗi
                    note.UpdatedBy = Session["UserID"] != null ? Session["UserID"].ToString() : "Admin";

                    // 4. Lưu DB và Chốt Transaction
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Đã hủy phiếu nhập và hoàn kho (trừ tồn kho) thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Có bất kỳ lỗi gì xảy ra thì hoàn tác lại hết
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }
    }
}