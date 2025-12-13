using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;
using WebBanHoa.Models.Payment;

namespace WebBanHoa.Controllers
{
    public class PaymentController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(string userID, string CustomerName, string Phone, string Address, string Email, string PaymentMethod)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "User", new { area = "" });
            }
            string userIdString = Session["UserID"].ToString();
            var cartItems = db.ShoppingCartItems.Where(c => c.ShoppingCart.UserID == userIdString).ToList();

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 2. Tính tổng tiền
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                // Tìm thông tin sản phẩm mới nhất để lấy giá
                var product = db.Products.Find(item.ProductID);
                if (product != null)
                {
                    decimal price = product.Price ?? 0;
                    decimal discountRate = 0; // Mặc định là không giảm

                    // 2. Tìm mã giảm giá áp dụng cho sản phẩm này
                    // Lưu ý: Cần kiểm tra xem mã còn hạn sử dụng không (StartDate <= Now <= EndDate)
                    var discountObj = db.Discounts.FirstOrDefault(d =>
                        d.ProductID == item.ProductID &&
                        d.StartDate <= DateTime.Now &&
                        d.EndDate >= DateTime.Now);

                    // 3. Nếu tìm thấy mã giảm giá hợp lệ, lấy % ra
                    if (discountObj != null)
                    {
                        discountRate = (decimal)discountObj.DiscountRate;
                    }

                    // 4. Tính giá cuối cùng
                    decimal finalPrice = price - (price * discountRate / 100);

                    totalAmount += finalPrice * (item.Quantity ?? 1);
                }
            }

            Order newOrder = new Order();
            newOrder.OrderID = IDGenerator.GenerateOrderID();
            newOrder.UserID = userIdString;
            newOrder.OrderDate = DateTime.Now;
            newOrder.Status = "Chờ xử lý"; 
            newOrder.Address = Address;
            foreach(var ci in cartItems)
            {
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderID = newOrder.OrderID;
                orderDetail.ProductID = ci.ProductID;
                orderDetail.Quantity = ci.Quantity;
                db.OrderDetails.Add(orderDetail);
            }
            db.Orders.Add(newOrder);
            db.SaveChanges();

            // 4. ĐIỀU HƯỚNG DỰA TRÊN PHƯƠNG THỨC THANH TOÁN
            switch (PaymentMethod)
            {
                case "COD":
                default:
                    // Xóa giỏ hàng và báo thành công
                    ShoppingCart cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userIdString);
                    foreach (var item in cart.ShoppingCartItems)
                    {
                        db.ShoppingCartItems.Remove(item);
                    }
                    return View("ThanhCong", "Orders", new { id = newOrder.OrderID });
            }
        }
    }
}