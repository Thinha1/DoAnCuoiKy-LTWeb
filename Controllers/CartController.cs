using QLBANHOA.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using WebBanHoa.Models;

namespace QLBANHOA.Controllers
{
    public class CartController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();
        private const string GioHangSession = "Cart";

        // GET: GioHang - Trang chính giỏ hàng
        public ActionResult Index()
        {
            var gioHang = LayGioHang();

            // Cập nhật discount rate cho từng item
            foreach (var item in gioHang.Items)
            {
                var discountRate = LayTyLeGiamGia(item.SanPham.ProductID);
                gioHang.CapNhatGiamGia(item.SanPham.ProductID, discountRate);
            }

            return View(gioHang);
        }
        // Action: Thêm vào giỏ hàng và chuyển thẳng đến thanh toán
        public ActionResult ThemVaoGioHangVaThanhToan(string id, int soLuong = 1)
        {
            try
            {
                var sanPham = db.Products.Find(id);
                if (sanPham == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Details", "Home", new { ProductID = id });
                }

                // Kiểm tra số lượng tồn
                if (sanPham.Quantity < soLuong)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{sanPham.ProductName}' chỉ còn {sanPham.Quantity} sản phẩm";
                    return RedirectToAction("Details", "Home", new { ProductID = id });
                }

                // 1. Thêm vào giỏ hàng
                var gioHang = LayGioHang();
                var discountRate = LayTyLeGiamGia(id);
                gioHang.ThemSanPham(sanPham, discountRate, soLuong);

                // 2. Chuyển thẳng đến trang thanh toán
                return RedirectToAction("ThanhToan", "Orders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đặt hàng: " + ex.Message;
                return RedirectToAction("Details", "Home", new { ProductID = id });
            }
        }

        // Thêm sản phẩm vào giỏ hàng
        public ActionResult ThemVaoGioHang(string id, int soLuong = 1)
        {
            var sanPham = db.Products.Find(id);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Index", "SanPham");
            }

            // Kiểm tra số lượng tồn
            if (sanPham.Quantity < soLuong)
            {
                TempData["ErrorMessage"] = $"Sản phẩm '{sanPham.ProductName}' chỉ còn {sanPham.Quantity} sản phẩm";
                return RedirectToAction("Index", "SanPham");
            }

            var gioHang = LayGioHang();
            var discountRate = LayTyLeGiamGia(id);

            gioHang.ThemSanPham(sanPham, discountRate, soLuong);

            TempData["SuccessMessage"] = $"Đã thêm '{sanPham.ProductName}' vào giỏ hàng";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]  
        public ActionResult XoaKhoiGioHang(string id)
        {
            var gioHang = LayGioHang();
            gioHang.XoaSanPham(id);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        public ActionResult CapNhatSoLuong(string id, int soLuong)
        {
            var gioHang = LayGioHang();
            var sanPham = db.Products.Find(id);

            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Index");
            }

            // Kiểm tra số lượng tồn
            if (sanPham.Quantity < soLuong)
            {
                TempData["ErrorMessage"] = $"Số lượng sản phẩm '{sanPham.ProductName}' chỉ còn {sanPham.Quantity}";
                return RedirectToAction("Index");
            }

            gioHang.CapNhatSoLuong(id, soLuong);
            return RedirectToAction("Index");
        }

       
       
        // Phương thức lấy discount rate
        private decimal LayTyLeGiamGia(string productId)
        {
            var now = DateTime.Now;

            var discount = db.Discounts
                .FirstOrDefault(d => d.ProductID == productId
                                   && d.StartDate <= now
                                   && d.EndDate >= now);

            if (discount != null && discount.DiscountRate.HasValue)
            {
                return (decimal)discount.DiscountRate.Value;
            }

            return 0;
        }

        // Lấy giỏ hàng từ Session
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
    }
}