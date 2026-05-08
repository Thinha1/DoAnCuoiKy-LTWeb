using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountAPIController : ApiController
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok("API đã hoạt động bình thường!");
        }

        [HttpPut]
        [Route("update-profile/{id}")]
        public IHttpActionResult UpdateProfile(string id, [FromBody] UpdateProfileModel model)
        {
            // 1. Kiểm tra tính hợp lệ của dữ liệu đầu vào (Data Annotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 2. Tìm người dùng trong cơ sở dữ liệu
                var user = db.Users.FirstOrDefault(u => u.UserID == id);

                if (user == null)
                {
                    return NotFound(); // Trả về mã lỗi 404 nếu không tìm thấy User
                }

                // 3. Kiểm tra trùng Email: Chỉ báo lỗi nếu email này ĐÃ BỊ LẤY bởi một ID KHÁC.
                // Nếu bạn giữ nguyên email cũ, hệ thống sẽ bỏ qua và không báo lỗi.
                bool isEmailTaken = db.Users.Any(u => u.Email == model.Email && u.UserID != id);
                if (isEmailTaken)
                {
                    return BadRequest("Email này đã được sử dụng bởi một tài khoản khác.");
                }

                // 4. Cập nhật thông tin
                user.Name = model.Name;
                user.Email = model.Email;

                // ---> ĐÃ XÓA TOÀN BỘ CODE CẬP NHẬT SESSION Ở ĐÂY <---

                // 5. Lưu thay đổi vào DB
                db.SaveChanges();

                // 6. Trả về kết quả thành công
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thông tin thành công!",
                    data = new
                    {
                        user.UserID,
                        user.Name,
                        user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 nếu có lỗi server (Ví dụ: lỗi kết nối database)
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("change-password/{id}")]
        public IHttpActionResult ChangePassword(string id, [FromBody] ChangePasswordModel model)
        {
            // 1. Kiểm tra tính hợp lệ của model (Data Annotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 2. Tìm user trong cơ sở dữ liệu
                var user = db.Users.FirstOrDefault(u => u.UserID == id);

                if (user == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy
                }

                // 3. Kiểm tra mật khẩu cũ có khớp với database không
                if (!PasswordHelper.VerifyPassword(model.OldPassword, user.Password))
                {
                    return BadRequest("Mật khẩu cũ không chính xác!");
                }

                // (Tùy chọn) Kiểm tra mật khẩu mới không được trùng mật khẩu cũ
                if (PasswordHelper.VerifyPassword(model.NewPassword, user.Password))
                {
                    return BadRequest("Mật khẩu mới không được giống mật khẩu cũ!");
                }

                // 4. Băm mật khẩu mới và cập nhật
                user.Password = PasswordHelper.HashPassword(model.NewPassword);

                // 5. Lưu vào cơ sở dữ liệu
                db.SaveChanges();

                // 6. Trả về kết quả
                return Ok(new
                {
                    success = true,
                    message = "Đổi mật khẩu thành công!"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
