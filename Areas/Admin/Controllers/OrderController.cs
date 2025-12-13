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
    public class OrderController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/Order
        public ActionResult Index(string TuKhoa, string trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = db.Orders.AsQueryable();

            // 2. LỌC THEO TỪ KHÓA (Logic cũ)
            if (!string.IsNullOrEmpty(TuKhoa))
            {
                TuKhoa = TuKhoa.Trim().ToLower();
                query = query.Where(o => o.OrderID.ToString().Contains(TuKhoa) ||
                                         o.User.Name.ToLower().Contains(TuKhoa));
            }

            // 3. LỌC THEO TRẠNG THÁI 
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(o => o.Status == trangThai);
            }

            // 4. LỌC THEO NGÀY 
            if (tuNgay.HasValue)
            {
                // Lấy từ đầu ngày (00:00:00)
                DateTime startDate = tuNgay.Value.Date;
                query = query.Where(o => o.OrderDate >= startDate);
            }

            // 2. Lọc Đến ngày (End Date)
            if (denNgay.HasValue)
            {
                // Quan trọng: Phải cộng thêm 1 ngày rồi dùng dấu nhỏ hơn (<)
                DateTime endDate = denNgay.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate < endDate);
            }

            // 5. Sắp xếp (Mới nhất lên đầu)
            query = query.OrderByDescending(o => o.OrderDate);

            // 6. Lưu lại giá trị filter để hiển thị lại trên View
            ViewBag.CurrentFilter = TuKhoa;
            ViewBag.CurrentStatus = trangThai; // Để selected dropdown
            ViewBag.FromDate = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = denNgay?.ToString("yyyy-MM-dd");

            // 7. Map sang DTO 
            var result = query.Select(o => new OrderDTO
            {
                OrderID = o.OrderID,
                OrderDate = (DateTime)o.OrderDate,
                CustomerName = o.User.Name,
                Address = o.Address,
                Status = o.Status,
                UserPaymentMethod = o.UserPaymentMethod,
            }).ToList();
            return View(result);
        }

        public ActionResult Edit(string orderID)
        {
            Order order = db.Orders.FirstOrDefault(o => o.OrderID == orderID);
            if (order != null)
            {
                OrderDTO dto = new OrderDTO
                {
                    OrderID = order.OrderID,
                    OrderDate = order.OrderDate ?? DateTime.Now,
                    CustomerName = order.User.Name,
                    Address = order.Address,
                    Status = order.Status,
                    UserPaymentMethod = order.UserPaymentMethod,
                };
                return View(dto);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(OrderDTO dto)
        {
            Order order = db.Orders.FirstOrDefault(o => o.OrderID == dto.OrderID);
            if (order != null)
            {
                order.Address = dto.Address;
                if (dto.Status == "Đã huỷ")
                {
                    //Không được huỷ đơn hàng đã giao hoặc đã huỷ rồi tránh hoàn kho 2 lần
                    if (order.Status != "Đã huỷ" && order.Status != "Đã giao")
                    {
                        List<OrderDetail> odList = db.OrderDetails.Where(od => order.OrderID == dto.OrderID).ToList();
                        //Hoàn hàng về kho
                        foreach (var o in odList)
                        {
                            //Lấy ra từng sản phẩm
                            Product p = db.Products.FirstOrDefault(sp => sp.ProductID == o.ProductID);
                            if (p != null)
                            {
                                p.Quantity += o.Quantity;
                                p.UpdatedAt = DateTime.Now;
                                p.UpdatedBy = Session["UserID"].ToString();
                            }
                        }
                        order.UpdatedAt = DateTime.Now;
                        order.UpdatedBy = Session["UserID"].ToString();
                    }
                    order.Status = "Đã huỷ";
                }
                else
                {
                    if (order.Status == "Đã huỷ")
                    {
                        TempData["Error"] = "Đơn hàng đã huỷ không thể khôi phục trạng thái khác!";
                        return RedirectToAction("Index");
                    }
                    //Không được thay đổi đơn hàng đã giao
                    else if (order.Status == "Đã giao")
                    {
                        TempData["Error"] = "Đơn hàng đã giao không thể khôi phục trạng thái khác!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //TRƯỜNG HỢP KHÁC
                        order.Status = dto.Status;
                        order.UpdatedAt = DateTime.Now;
                        order.UpdatedBy = Session["UserID"].ToString();
                    }
                }
            }
            db.SaveChanges();
            TempData["Success"] = "Cập nhật đơn hàng thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Details(string orderID)
        {
            List<OrderDetailsDTO> orderDetails = db.OrderDetails.Where(o => o.OrderID == orderID).Select(
                od => new OrderDetailsDTO
                {
                    ProductID = od.ProductID,
                    OrderID = od.OrderID,
                    ProductName = od.Product.ProductName,
                    Image = od.Product.Image,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                }).ToList();
            return View(orderDetails);
        }

        //HUỶ ĐƠN HÀNG
        public ActionResult CancelOrder(string orderID)
        {
            Order od = db.Orders.FirstOrDefault(o => o.OrderID == orderID);
            //Tránh huỷ đơn hàng hai lần dẫn đến hoàn về kho hai lần cũng như không huỷ đơn hàng đã giao
            if (od != null && od.Status != "Đã huỷ" && od.Status != "Đã giao")
            {
                od.UpdatedAt = DateTime.Now;
                od.UpdatedBy = Session["UserID"].ToString();
                List<OrderDetail> odList = db.OrderDetails.Where(order => order.OrderID == orderID).ToList();
                //Hoàn hàng về kho
                foreach (var o in odList)
                {
                    //Lấy ra từng sản phẩm
                    Product p = db.Products.FirstOrDefault(sp => sp.ProductID == o.ProductID);
                    if (p != null)
                    {
                        p.Quantity += o.Quantity;
                        p.UpdatedAt = DateTime.Now;
                        p.UpdatedBy = Session["UserID"].ToString();
                    }
                }
                od.Status = "Đã huỷ";
                db.SaveChanges();
            }
            else
            {
                if (od.Status == "Đã huỷ")
                {
                    TempData["Error"] = "Đơn hàng đã huỷ không thể huỷ được nữa!";
                    return RedirectToAction("Index");
                }
                //Không được thay đổi đơn hàng đã giao
                else
                {
                    TempData["Error"] = "Đơn hàng đã giao không thể huỷ!";
                    return RedirectToAction("Index");
                }
            }
            TempData["Success"] = "Huỷ đơn hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}