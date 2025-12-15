
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class OrdersController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        // GET: Hiển thị trang thanh toán
        public ActionResult ThanhToan()
        {
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account");
            }

            string userId = Session["UserID"].ToString();

            // Lấy giỏ hàng từ database
            var shoppingCart = db.ShoppingCarts
                .FirstOrDefault(c => c.UserID == userId);

            if (shoppingCart == null)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy items từ database
            var cartItems = db.ShoppingCartItems
                .Where(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID)
                .ToList();

            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Tạo CartDTO để hiển thị
            var cartDTO = new CartDTO
            {
                ShoppingCartID = shoppingCart.ShoppingCartID,
                UserID = userId,
                Items = new List<CartItemDTO>()
            };

            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var product = db.Products.Find(item.ProductID);
                if (product != null)
                {
                    var discountRate = GetCurrentDiscountRate(product.ProductID);
                    decimal price = (decimal)product.Price;
                    decimal discountedPrice = price - (price * discountRate / 100);


                    int itemQuantity = item.Quantity ?? 0;

                    cartDTO.Items.Add(new CartItemDTO
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Price = price,
                        DiscountRate = discountRate,
                        Quantity = item.Quantity,
                        Image = product.Image
                    });

                    totalAmount += discountedPrice * itemQuantity;
                }
            }

            ViewBag.TotalAmount = totalAmount;

            // Lấy thông tin user từ bảng Users để hiển thị
            var user = db.Users.Find(userId);
            if (user != null)
            {
                ViewBag.UserName = user.Name;
                ViewBag.UserEmail = user.Email;
                ViewBag.UserAddress = user.Address;
            }

            return View(cartDTO);
        }

        // POST: Xử lý thanh toán
        [HttpPost]
        public ActionResult ThanhToan(string Address, string PaymentMethod)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                string userId = Session["UserID"].ToString();

                // 1. Lấy giỏ hàng từ database
                var shoppingCart = db.ShoppingCarts
                    .FirstOrDefault(c => c.UserID == userId);

                if (shoppingCart == null)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index", "Cart");
                }

                // 2. Lấy items từ database
                var cartItems = db.ShoppingCartItems
                    .Where(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID)
                    .ToList();

                if (cartItems.Count == 0)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index", "Cart");
                }

                // 3. Kiểm tra số lượng tồn kho
                foreach (var item in cartItems)
                {
                    var product = db.Products.Find(item.ProductID);
                    if (product == null)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm không tồn tại";
                        return RedirectToAction("ThanhToan");
                    }

                    int productQuantity = product.Quantity ?? 0;
                    int itemQuantity = item.Quantity ?? 0;

                    if (productQuantity < itemQuantity)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm '{product.ProductName}' chỉ còn {productQuantity} sản phẩm";
                        return RedirectToAction("ThanhToan");
                    }
                }
                // 4. Tạo mã đơn hàng mới
                string orderId = GenerateOrderID();

                // 5. Tạo đơn hàng mới
                var order = new Order
                {
                    OrderID = orderId,
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    Address = Address,
                    Status = "Chờ xác nhận",
                    UserPaymentMethod = PaymentMethod,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId

                };

                db.Orders.Add(order);

                // 6. Thêm chi tiết đơn hàng và cập nhật số lượng
                decimal totalOrderAmount = 0;

                foreach (var item in cartItems)
                {
                    var product = db.Products.Find(item.ProductID);
                    var discountRate = GetCurrentDiscountRate(item.ProductID);

                    decimal unitPrice = (decimal)product.Price;
                    decimal discountedPrice = unitPrice - (unitPrice * discountRate / 100);

                    int itemQuantity = item.Quantity ?? 0;

                    // Tạo chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderID = orderId,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = discountedPrice
                    };

                    db.OrderDetails.Add(orderDetail);

                    // Cập nhật số lượng tồn kho
                    if (product.Quantity.HasValue)
                    {
                        product.Quantity = product.Quantity.Value - itemQuantity;
                    }
                    else
                    {
                        product.Quantity = -itemQuantity;
                    }

                    totalOrderAmount += itemQuantity * discountedPrice;
                }

                // 7. Xóa giỏ hàng sau khi thanh toán
                db.ShoppingCartItems.RemoveRange(cartItems);

                // 8. Lưu tất cả thay đổi vào database
                db.SaveChanges();
                if (PaymentMethod.Contains("COD"))
                {
                    return RedirectToAction("ThanhCong", new { id = orderId });
                }
                else
                {
                    return RedirectToAction("XacThucThanhToan", new { orderID = order.OrderID });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi đặt hàng: {ex.Message}";
                return RedirectToAction("ThanhToan");
            }
        }

        public ActionResult XacThucThanhToan(string orderID)
        {
            var order = db.Orders.Find(orderID);
            if (order == null) return HttpNotFound();

            // Tính toán số tiền và nội dung
            decimal amount = (decimal)order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice); // Lấy tổng tiền
            string content = "ThanhToan" + order.OrderID; // Nội dung: DH1005

            string bank = "MB"; // Ngân hàng MB
            string account = "0919114642"; // Số tài khoản của bạn

            // Tạo link VietQR (dùng SePay hoặc VietQR API đều được)
            // Format SePay: https://qr.sepay.vn/img?acc={Acc}&bank={Bank}&amount={Amount}&des={Content}
            string qrCodeUrl = $"https://qr.sepay.vn/img?acc={account}&bank={bank}&amount={amount.ToString("0")}&des={content}";

            ViewBag.QrCodeUrl = qrCodeUrl;
            ViewBag.OrderId = orderID;
            ViewBag.Amount = amount;
            ViewBag.Content = content;

            return View();
        }

        public JsonResult CheckOrderStatus(string orderId)
        {
            // Tìm đơn hàng trong DB
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);

            if (order != null)
            {
                // Trả về trạng thái hiện tại 
                return Json(new { status = order.Status }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "Error" }, JsonRequestBehavior.AllowGet);
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

            if (Session["UserID"]?.ToString() != order.UserID)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin user để hiển thị tên, địa chỉ
            var user = db.Users.Find(order.UserID);

            // Lấy chi tiết đơn hàng
            var orderDetails = db.OrderDetails
                .Where(od => od.OrderID == id)
                .Include("Product")
                .ToList();

            // Tính tổng tiền
            decimal totalAmount = 0;
            foreach (var od in orderDetails)
            {
                int quantity = od.Quantity ?? 0;
                decimal unitPrice = od.UnitPrice ?? 0;
                totalAmount += quantity * unitPrice;
            }

            // Tạo OrderDTO
            var orderDTO = new OrderDTO
            {
                OrderID = order.OrderID,
                CustomerName = user?.Name,
                Address = order.Address,
                UserPaymentMethod = order.UserPaymentMethod,
                OrderDate = order.OrderDate ?? DateTime.Now,
                TotalAmount = totalAmount,
                Status = order.Status
            };

            // Tạo danh sách OrderDetailsDTO
            var orderDetailsDTOs = orderDetails.Select(od => new OrderDetailsDTO
            {
                OrderID = od.OrderID,
                ProductID = od.ProductID,
                ProductName = od.Product?.ProductName,
                Image = od.Product?.Image,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice
            }).ToList();

            ViewBag.OrderDetails = orderDetailsDTOs;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.User = user;

            return View(orderDTO);
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

            var orderDTOs = new List<OrderDTO>();

            foreach (var order in orders)
            {
                decimal totalAmount = 0;
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == order.OrderID).ToList();
                foreach (var od in orderDetails)
                {
                    int quantity = od.Quantity ?? 0;
                    decimal unitPrice = od.UnitPrice ?? 0;
                    totalAmount += quantity * unitPrice;
                }

                // Lấy thông tin user để hiển thị tên
                var user = db.Users.Find(order.UserID);

                orderDTOs.Add(new OrderDTO
                {
                    OrderID = order.OrderID,
                    CustomerName = user?.Name,
                    Address = order.Address,
                    UserPaymentMethod = order.UserPaymentMethod,
                    OrderDate = order.OrderDate ?? DateTime.Now,
                    TotalAmount = totalAmount,
                    Status = order.Status
                });
            }

            return View(orderDTOs);
        }
        // GET: Chi tiết đơn hàng
        public ActionResult Details(string id)
        {
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

            id = id.Replace(" ", "").Trim();

            System.Diagnostics.Debug.WriteLine($"ID sau khi xử lý: '{id}'");

            string userId = Session["UserID"].ToString();

            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {

                var orderFromDb = db.Orders
                    .Include("OrderDetails.Product")
                    .Where(o => o.UserID == userId)
                    .ToList()
                    .FirstOrDefault(o => o.OrderID.Replace(" ", "") == id);

                if (orderFromDb != null)
                {

                    return RedirectToAction("Details", new { id = orderFromDb.OrderID });
                }

                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            // Lấy thông tin user
            var user = db.Users.Find(order.UserID);

            // Tính tổng tiền
            decimal totalAmount = 0;
            if (order.OrderDetails != null)
            {
                foreach (var od in order.OrderDetails)
                {
                    int quantity = od.Quantity ?? 0;
                    decimal unitPrice = od.UnitPrice ?? 0;
                    totalAmount += quantity * unitPrice;
                }
            }

            // Tạo OrderDTO chính
            var orderDTO = new OrderDTO
            {
                OrderID = order.OrderID,
                CustomerName = user?.Name,
                Address = order.Address,
                UserPaymentMethod = order.UserPaymentMethod,
                OrderDate = order.OrderDate ?? DateTime.Now,
                TotalAmount = totalAmount,
                Status = order.Status
            };

            // Tạo danh sách OrderDetailsDTO cho chi tiết sản phẩm
            var orderDetailsDTOs = new List<OrderDetailsDTO>();
            if (order.OrderDetails != null)
            {
                orderDetailsDTOs = order.OrderDetails.Select(od => new OrderDetailsDTO
                {
                    OrderID = od.OrderID,
                    ProductID = od.ProductID,
                    ProductName = od.Product?.ProductName,
                    Image = od.Product?.Image,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList();
            }

            ViewBag.OrderDetails = orderDetailsDTOs;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.User = user;

            return View(orderDTO);
        }

        // POST: Hủy đơn hàng
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

            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã huỷ";
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = userId;

                // Hoàn trả số lượng sản phẩm
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                foreach (var detail in orderDetails)
                {
                    var product = db.Products.Find(detail.ProductID);
                    if (product != null && detail.Quantity.HasValue)
                    {
                        int currentQty = product.Quantity ?? 0;
                        product.Quantity = currentQty + detail.Quantity.Value;
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


        private decimal GetCurrentDiscountRate(string productId)
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

        private string GenerateOrderID()
        {
            var lastOrder = db.Orders
                .OrderByDescending(o => o.OrderID)
                .FirstOrDefault();

            if (lastOrder == null)
            {
                return "OD001";
            }

            var number = int.Parse(lastOrder.OrderID.Substring(2)) + 1;
            return $"OD{number:D3}";
        }


    }
}