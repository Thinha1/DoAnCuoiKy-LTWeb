using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebBanHoa.Models;
using WebBanHoa.Models.Payment;

namespace WebBanHoa.Controllers
{
    public class SePayController : Controller
    {
        private const string SePayApiKey = "RHMCQQDURZEIPLRUHMA7GBE3SGDHGW9PKKB8S8FE5YCJBNX3F1HDNYLVBVNIO2AI";
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: SePay
        [HttpPost]
        public ActionResult Webhook(SePayModel data)
        {
            try
            {
                // ---------------------------------------------------------
                // 1. BẢO MẬT: Kiểm tra xem request có phải từ SePay không?
                // ---------------------------------------------------------
                string authHeader = Request.Headers["Authorization"];

                // SePay gửi token dạng: "Bearer API_KEY_CUA_BAN"
                if (string.IsNullOrEmpty(authHeader) || !authHeader.Contains(SePayApiKey))
                {
                    // Nếu sai key -> Chặn ngay lập tức
                    return new HttpStatusCodeResult(401, "Unauthorized");
                }

                // ---------------------------------------------------------
                // 2. PHÂN TÍCH DỮ LIỆU: Lấy mã đơn hàng từ nội dung CK
                // ---------------------------------------------------------

                var match = Regex.Match(data.content, @"OD(\d+)");

                if (!match.Success)
                {
                    // Không tìm thấy mã đơn hàng trong nội dung chuyển khoản
                    return Json(new { success = false, message = "Không tìm thấy mã đơn hàng" });
                }

                // Lấy được ID đơn hàng (ví dụ: 1055)
                string orderId = match.Value;

                // ---------------------------------------------------------
                // 3. CẬP NHẬT DATABASE
                // ---------------------------------------------------------
                var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Kiểm tra xem đơn này đã thanh toán chưa (tránh xử lý lặp lại)
                if (order.Status == "Đã thanh toán")
                {
                    return Json(new { success = true, message = "Already Paid" });
                }

                // Kiểm tra số tiền chuyển có đủ không
                if (data.transferAmount >= order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))
                {
                    // Cập nhật trạng thái đơn hàng
                    order.Status = "Đã thanh toán"; // Hoặc trạng thái bạn muốn

                    db.SaveChanges();

                    return Json(new { success = true, message = "Thanh toán thành công" });
                }
                else
                {
                    // Khách chuyển thiếu tiền -> Có thể cập nhật trạng thái "Thanh toán thiếu"
                    return Json(new { success = false, message = "Không đủ" });
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }
    }
}