using WebBanHoa.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;


namespace WebBanHoa.Controllers
{
    public class CartController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        // GET: Giỏ hàng - Lấy từ database
        public ActionResult Index()
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng";
                    return RedirectToAction("Login", "Account");
                }

                // 1. Lấy giỏ hàng từ database
                var shoppingCart = GetOrCreateShoppingCart(userId);

                // 2. Lấy items từ database
                var cartItems = db.ShoppingCartItems
                    .Where(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID)
                    .ToList();

                // 3. Tạo CartDTO từ database
                var cartDTO = new CartDTO
                {
                    ShoppingCartID = shoppingCart.ShoppingCartID,
                    UserID = userId,
                    Items = new System.Collections.Generic.List<CartItemDTO>()
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
                        int productQuantity = product.Quantity ?? 0;

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
                return View(cartDTO);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // Thêm vào giỏ hàng và chuyển thẳng đến thanh toán
        public ActionResult ThemVaoGioHangVaThanhToan(string id, int soLuong = 1)
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                var product = db.Products.Find(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Details", "Home", new { id = id });
                }

                // Kiểm tra số lượng tồn - SỬA: xử lý int?
                int productQuantity = product.Quantity ?? 0;
                if (productQuantity < soLuong)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{product.ProductName}' chỉ còn {productQuantity} sản phẩm";
                    return RedirectToAction("Details", "Home", new { id = id });
                }

                // 1. Xóa giỏ hàng cũ (nếu có)
                var oldCart = db.ShoppingCarts.FirstOrDefault(sc => sc.UserID == userId);
                if (oldCart != null)
                {
                    var oldItems = db.ShoppingCartItems
                        .Where(ci => ci.ShoppingCartID == oldCart.ShoppingCartID)
                        .ToList();
                    db.ShoppingCartItems.RemoveRange(oldItems);
                    db.ShoppingCarts.Remove(oldCart);
                }

                // 2. Tạo giỏ hàng mới chỉ với sản phẩm này
                var shoppingCart = new ShoppingCart
                {
                    ShoppingCartID = GenerateCartID(),
                    UserID = userId
                };
                db.ShoppingCarts.Add(shoppingCart);

                var cartItem = new ShoppingCartItem
                {
                    ShoppingCartID = shoppingCart.ShoppingCartID,
                    ProductID = id,
                    Quantity = soLuong
                };
                db.ShoppingCartItems.Add(cartItem);

                db.SaveChanges();

                // 3. Chuyển thẳng đến trang thanh toán
                return RedirectToAction("ThanhToan", "Orders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Details", "Home", new { id = id });
            }
        }


        // Thêm sản phẩm vào giỏ hàng
        public ActionResult ThemVaoGioHang(string id, int soLuong)
        {
            var sanPham = db.Products.Find(id);
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string user = Session["UserID"].ToString();

            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra số lượng tồn
            if (sanPham.Quantity < soLuong)
            {
                TempData["ErrorMessage"] = $"Sản phẩm '{sanPham.ProductName}' chỉ còn {sanPham.Quantity} sản phẩm";
                return RedirectToAction("Details", "Home", new { productID = id });
            }

            var gioHang = db.ShoppingCarts.FirstOrDefault(sc => sc.UserID == user);

            var itemInCart = db.ShoppingCartItems.FirstOrDefault(
                        i => i.ShoppingCartID == gioHang.ShoppingCartID && i.ProductID == id);
            if (itemInCart != null)
            {
                // Đã có -> Cộng dồn số lượng
                itemInCart.Quantity += soLuong;
            }
            else
            {
                //Chưa có -> Tạo mới Item
                var newItem = new ShoppingCartItem();
                newItem.ShoppingCartID = gioHang.ShoppingCartID;
                newItem.ProductID = id;
                newItem.Quantity = soLuong;

                db.ShoppingCartItems.Add(newItem);
            }
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã thêm '{sanPham.ProductName}' vào giỏ hàng";
            return RedirectToAction("Details", "Home", new { productID = id });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult XoaKhoiGioHang(string id)
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy giỏ hàng của user
                var shoppingCart = db.ShoppingCarts
                    .FirstOrDefault(sc => sc.UserID == userId);

                if (shoppingCart == null)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                // Tìm và xóa item từ database
                var item = db.ShoppingCartItems
                    .FirstOrDefault(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID
                                       && ci.ProductID == id);

                if (item != null)
                {
                    db.ShoppingCartItems.Remove(item);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Cập nhật số lượng (lưu database)
        [HttpPost]
        public ActionResult CapNhatSoLuong(string id, int soLuong)
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy giỏ hàng của user
                var shoppingCart = db.ShoppingCarts
                    .FirstOrDefault(sc => sc.UserID == userId);

                if (shoppingCart == null)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                var product = db.Products.Find(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }

                // Kiểm tra số lượng tồn - SỬA: xử lý int?
                int productQuantity = product.Quantity ?? 0;
                if (productQuantity < soLuong)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm chỉ còn {productQuantity} sản phẩm";
                    return RedirectToAction("Index");
                }

                if (soLuong <= 0)
                {
                    // Nếu số lượng <= 0 thì xóa sản phẩm
                    return XoaKhoiGioHang(id);
                }

                // Tìm và cập nhật số lượng trong database
                var item = db.ShoppingCartItems
                    .FirstOrDefault(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID
                                       && ci.ProductID == id);

                if (item != null)
                {
                    item.Quantity = soLuong;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đã cập nhật số lượng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Xóa toàn bộ giỏ hàng (lưu database)
        [HttpPost]
        public ActionResult XoaToanBoGioHang()
        {
            try
            {
                string userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy giỏ hàng của user
                var shoppingCart = db.ShoppingCarts
                    .FirstOrDefault(sc => sc.UserID == userId);

                if (shoppingCart == null)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                // Xóa tất cả items từ database
                var items = db.ShoppingCartItems
                    .Where(ci => ci.ShoppingCartID == shoppingCart.ShoppingCartID)
                    .ToList();

                db.ShoppingCartItems.RemoveRange(items);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }



        // Phương thức lấy discount rate
        //private decimal LayTyLeGiamGia(string productId)
        //{
        //    return Session["UserID"]?.ToString();
        //}

        // Phương thức lấy user id
        private string GetCurrentUserId()
        {
            return Session["UserID"]?.ToString();
        }

        // Lấy hoặc tạo giỏ hàng trong database
        private ShoppingCart GetOrCreateShoppingCart(string userId)
        {
            var shoppingCart = db.ShoppingCarts
                .FirstOrDefault(sc => sc.UserID == userId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    ShoppingCartID = GenerateCartID(),
                    UserID = userId
                };
                db.ShoppingCarts.Add(shoppingCart);
                db.SaveChanges();
            }

            return shoppingCart;
        }

        
        private string GenerateCartID()
        {
            var lastCart = db.ShoppingCarts
                .OrderByDescending(sc => sc.ShoppingCartID)
                .FirstOrDefault();

            if (lastCart == null)
            {
                return "CART001";
            }

            var number = int.Parse(lastCart.ShoppingCartID.Substring(4)) + 1;
            return $"CART{number:D3}";
        }

        // Lấy tỉ lệ giảm giá
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

       
    }
}