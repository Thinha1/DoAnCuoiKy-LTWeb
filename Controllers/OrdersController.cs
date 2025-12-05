using QLBANHOA.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class OrdersController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        private const string GioHangSession = "Cart";

        // GET: Hiển thị trang thanh toán (GET)
        public ActionResult ThanhToan()
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account");
            }

            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            return View(gioHang);
        }

        // POST: Xử lý thanh toán (POST)
        [HttpPost]
        public ActionResult ThanhToan(
            string CustomerName,
            string Phone,
            string Address,
            string Email,
            string Note,
            string DeliveryMethod,
            string PaymentMethod)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                var gioHang = LayGioHang();
                if (gioHang.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index", "Cart");
                }

                // 1. Tạo mã đơn hàng mới
                string orderId = "OD" + (db.Orders.Count() + 1).ToString("D4");

                // 2. Tạo đơn hàng mới
                var order = new Order
                {
                    OrderID = orderId,
                    UserID = Session["UserID"].ToString(),
                    OrderDate = DateTime.Now,
                    Address = Address,
                    Status = "Chờ xác nhận",
                    UserPaymentMethod = PaymentMethod,
                    CreatedAt = DateTime.Now,
                    CreatedBy = Session["UserID"].ToString()
                };

                db.Orders.Add(order);

                // 3. Thêm chi tiết đơn hàng và cập nhật số lượng
                foreach (var item in gioHang.Items)
                {
                    // Lấy giá sản phẩm (có tính discount)
                    var discountRate = LayTyLeGiamGia(item.SanPham.ProductID);
                    decimal giaBan = item.GiaSauGiam;

                    // Tạo chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderID = orderId,
                        ProductID = item.SanPham.ProductID,
                        Quantity = item.SoLuong,
                        UnitPrice = giaBan
                    };

                    // Cập nhật số lượng tồn kho
                    var product = db.Products.Find(item.SanPham.ProductID);
                    if (product != null)
                    {
                        if (product.Quantity < item.SoLuong)
                        {
                            TempData["ErrorMessage"] = $"Sản phẩm '{product.ProductName}' chỉ còn {product.Quantity} sản phẩm";
                            return RedirectToAction("ThanhToan");
                        }
                        product.Quantity -= item.SoLuong;
                    }

                    db.OrderDetails.Add(orderDetail);
                }

                // 4. Lưu tất cả thay đổi
                db.SaveChanges();

                // 5. Xóa giỏ hàng
                Session.Remove(GioHangSession);

                // 6. Chuyển đến trang thành công
                return RedirectToAction("ThanhCong", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đặt hàng: " + ex.Message;
                return RedirectToAction("ThanhToan");
            }
        }

        // GET: Hiển thị trang thanh toán thành công
        public ActionResult ThanhCong(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Home");
            }

            var order = db.Orders.Find(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                return RedirectToAction("Index", "Home");
            }

            // Tính tổng tiền từ OrderDetails
            var totalAmount = db.OrderDetails
                .Where(od => od.OrderID == id)
                .Sum(od => od.Quantity * od.UnitPrice);

            ViewBag.TotalAmount = totalAmount; // Truyền qua ViewBag

            return View(order);
        }

        // GET: Danh sách đơn hàng của user
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            string userId = Session["UserID"].ToString();
            var orders = db.Orders
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Tính tổng tiền cho mỗi order
            foreach (var order in orders)
            {
                order.OrderDetails = db.OrderDetails
                    .Where(od => od.OrderID == order.OrderID)
                    .ToList();

                // Tính tổng và lưu tạm vào ViewBag
                ViewData[$"Total_{order.OrderID}"] = order.OrderDetails
                    .Sum(od => od.Quantity * od.UnitPrice);
            }

            return View(orders);
        }

        // GET: Chi tiết đơn hàng
       
        public ActionResult Details(string id)
        {
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

           
            id = (id ?? "").Trim();

            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            string userId = Session["UserID"].ToString();

            // TRIM trong query
            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(o => (o.OrderID ?? "").Trim() == id && o.UserID == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            // Tính tổng tiền
            ViewBag.TotalAmount = order.OrderDetails?.Sum(od => od.Quantity * od.UnitPrice) ?? 0;

            return View(order);
        }

        // GET: Hủy đơn hàng
        [HttpPost]
        public ActionResult HuyDon(string id)
        {
            id = (id ?? "").Trim();
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            string userId = Session["UserID"].ToString();
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            // Chỉ cho phép hủy nếu đơn hàng chưa được xác nhận
            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = userId;

                // Hoàn trả số lượng sản phẩm
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                foreach (var detail in orderDetails)
                {
                    var product = db.Products.Find(detail.ProductID);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                    }
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này";
            }

            return RedirectToAction("Index");
        }
        // Phương thức hỗ trợ
        private GioHang LayGioHang()
        {
            var gioHang = Session[GioHangSession] as GioHang;
            if (gioHang == null)
            {
                gioHang = new GioHang();
                Session[GioHangSession] = gioHang;
            }
            return gioHang;
        }

        private decimal LayTyLeGiamGia(string productId)
        {
            var now = DateTime.Now;
            var discount = db.Discounts
                .FirstOrDefault(d => d.ProductID == productId
                                   && d.StartDate <= now
                                   && d.EndDate >= now);

            return discount != null && discount.DiscountRate.HasValue
                ? (decimal)discount.DiscountRate.Value
                : 0;
        }
    }
}