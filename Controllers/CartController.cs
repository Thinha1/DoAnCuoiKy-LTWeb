using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class GioHangController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        // GET: /GioHang/ThemVaoGioHang
        public ActionResult ThemVaoGioHang(string productId, int quantity = 1, string returnUrl = null)
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                Session["ReturnUrl"] = returnUrl ?? Request.Url?.PathAndQuery;
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng";
                return RedirectToAction("Login", "Account");
            }

            // Tìm sản phẩm
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId && p.IsAvailable == 1);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra số lượng tồn kho
            if (product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm";
                return RedirectToAction("Details", "Home", new { productID = productId });
            }

            // Thêm vào giỏ hàng session
            var gioHang = LayGioHang();
            gioHang.ThemSanPham(product, quantity);
            Session["GioHang"] = gioHang;

            // Đồng bộ với database ShoppingCart
            DongBoVoiDatabase(productId, quantity);

            TempData["SuccessMessage"] = $"Đã thêm '{product.ProductName}' vào giỏ hàng";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Home", new { productID = productId });
        }

        // GET: /GioHang/Index
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng";
                return RedirectToAction("Login", "Account");
            }

            var gioHang = LayGioHang();
            return View(gioHang);
        }

        // POST: /GioHang/XoaKhoiGioHang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaKhoiGioHang(string productId)
        {
            var gioHang = LayGioHang();
            gioHang.XoaSanPham(productId);
            Session["GioHang"] = gioHang;

            // Đồng bộ với database
            XoaKhoiDatabase(productId);

            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return RedirectToAction("Index");
        }

        // POST: /GioHang/CapNhatSoLuong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatSoLuong(string productId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "Số lượng không thể nhỏ hơn 1";
                return RedirectToAction("Index");
            }

            // Kiểm tra tồn kho
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product != null && product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Chỉ còn {product.Quantity} sản phẩm trong kho";
                return RedirectToAction("Index");
            }

            var gioHang = LayGioHang();
            gioHang.CapNhatSoLuong(productId, quantity);
            Session["GioHang"] = gioHang;

            // Đồng bộ với database
            CapNhatSoLuongDatabase(productId, quantity);

            TempData["SuccessMessage"] = "Đã cập nhật số lượng sản phẩm";
            return RedirectToAction("Index");
        }

        // GET: /GioHang/ThanhToan
        public ActionResult ThanhToan()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var gioHang = LayGioHang();
            if (gioHang.TongSoLuong == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            // Kiểm tra tồn kho trước khi thanh toán
            foreach (var item in gioHang.Items)
            {
                var product = db.Products.FirstOrDefault(p => p.ProductID == item.SanPham.ProductID);
                if (product == null || product.Quantity < item.SoLuong)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{item.SanPham.ProductName}' không đủ số lượng tồn kho";
                    return RedirectToAction("Index");
                }
            }

            return View(gioHang);
        }

        // POST: /GioHang/ThanhToan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToan(FormCollection form)
        {
            var gioHang = LayGioHang();
            if (gioHang.TongSoLuong == 0)
            {
                return RedirectToAction("Index");
            }

            // Tạo đơn hàng
            var order = new Order
            {
                OrderID = GenerateOrderId(),
                UserID = Session["UserID"].ToString(),
                OrderDate = DateTime.Now,
                Address = form["Address"],
                Status = "Đang xử lý",
                UserPaymentMethod = form["PaymentMethod"]
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in gioHang.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.SanPham.ProductID,
                    Quantity = item.SoLuong,
                    UnitPrice = item.SanPham.Price
                };
                db.OrderDetails.Add(orderDetail);

                // Cập nhật số lượng tồn kho
                var product = db.Products.Find(item.SanPham.ProductID);
                if (product != null)
                {
                    product.Quantity -= item.SoLuong;
                }
            }

            db.SaveChanges();

            // Xóa giỏ hàng
            gioHang.XoaTatCa();
            Session["GioHang"] = gioHang;

            // Xóa giỏ hàng trong database
            XoaGioHangDatabase();

            return RedirectToAction("ThanhToanThanhCong", new { id = order.OrderID });
        }

        public ActionResult ThanhToanThanhCong(string id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }

        // GET: /GioHang/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            var gioHang = LayGioHang();
            if (gioHang.TongSoLuong > 0)
            {
                gioHang.XoaTatCa();
                Session["GioHang"] = gioHang;

                // Xóa giỏ hàng trong database
                XoaGioHangDatabase();

                TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng";
            }

            return RedirectToAction("Index");
        }

        // ========== HELPER METHODS ==========
        private GioHang LayGioHang()
        {
            var gioHang = Session["GioHang"] as GioHang;
            if (gioHang == null)
            {
                gioHang = new GioHang();
                Session["GioHang"] = gioHang;

                // Đồng bộ từ database nếu user đã đăng nhập
                if (Session["UserID"] != null)
                {
                    DongBoTuDatabase();
                }
            }
            return gioHang;
        }

        private void DongBoTuDatabase()
        {
            var userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
            if (cart != null)
            {
                var cartItems = db.ShoppingCartItems
                    .Where(ci => ci.ShoppingCartID == cart.ShoppingCartID)
                    .ToList();

                var gioHang = LayGioHang();

                foreach (var cartItem in cartItems)
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductID == cartItem.ProductID);
                    if (product != null)
                    {
                        gioHang.ThemSanPham(product, cartItem.Quantity ?? 0);
                    }
                }

                Session["GioHang"] = gioHang;
            }
        }

        private void DongBoVoiDatabase(string productId, int quantity)
        {
            var userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            var shoppingCart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    ShoppingCartID = GenerateCartId(),
                    UserID = userId
                };
                db.ShoppingCarts.Add(shoppingCart);
                db.SaveChanges();
            }

            var cartItem = db.ShoppingCartItems.FirstOrDefault(
                ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID
                   && ci.ProductID == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ShoppingCartID = shoppingCart.ShoppingCartID,
                    ProductID = productId,
                    Quantity = quantity
                };
                db.ShoppingCartItems.Add(cartItem);
            }

            db.SaveChanges();
        }

        private void XoaKhoiDatabase(string productId)
        {
            var userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
            if (cart != null)
            {
                var cartItem = db.ShoppingCartItems.FirstOrDefault(
                    ci => ci.ShoppingCartID == cart.ShoppingCartID
                       && ci.ProductID == productId);

                if (cartItem != null)
                {
                    db.ShoppingCartItems.Remove(cartItem);
                    db.SaveChanges();
                }
            }
        }

        private void CapNhatSoLuongDatabase(string productId, int quantity)
        {
            var userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
            if (cart != null)
            {
                var cartItem = db.ShoppingCartItems.FirstOrDefault(
                    ci => ci.ShoppingCartID == cart.ShoppingCartID
                       && ci.ProductID == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    db.SaveChanges();
                }
            }
        }

        private void XoaGioHangDatabase()
        {
            var userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
            if (cart != null)
            {
                var cartItems = db.ShoppingCartItems
                    .Where(ci => ci.ShoppingCartID == cart.ShoppingCartID)
                    .ToList();

                db.ShoppingCartItems.RemoveRange(cartItems);
                db.SaveChanges();
            }
        }

        private string GenerateOrderId()
        {
            var lastOrder = db.Orders
                .OrderByDescending(o => o.OrderID)
                .FirstOrDefault();

            if (lastOrder == null)
                return "DH001";

            var lastId = lastOrder.OrderID;
            var number = int.Parse(lastId.Substring(2)) + 1;
            return "DH" + number.ToString("D3");
        }

        private string GenerateCartId()
        {
            var lastCart = db.ShoppingCarts
                .OrderByDescending(c => c.ShoppingCartID)
                .FirstOrDefault();

            if (lastCart == null)
                return "GH001";

            var lastId = lastCart.ShoppingCartID;
            var number = int.Parse(lastId.Substring(2)) + 1;
            return "GH" + number.ToString("D3");
        }
    }
}