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
    public class UserController : Controller
    {
        QLBANHOAEntities db = new QLBANHOAEntities();
        // GET: Admin/User
        public ActionResult Index(string TuKhoa)
        {
            List<UserDTO> users = db.Users.Where(r => r.RoleID != "R001")
                .Select(u => new UserDTO
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Email = u.Email,
                    Gender = u.Gender,
                    Address = u.Address,
                    IsEnabled = (int)u.IsEnabled
                }).ToList();
            if (!string.IsNullOrWhiteSpace(TuKhoa))
            {
                users = db.Users.Where(u => u.Name.Contains(TuKhoa) || u.Email.Contains(TuKhoa)).Select(u => new UserDTO
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Email = u.Email,
                    Gender = u.Gender,
                    Address = u.Address,
                    IsEnabled = (int)u.IsEnabled
                }).ToList();
            }
            return View(users);
        }

        public ActionResult Edit(string userID)
        {
            User u = db.Users.FirstOrDefault(us => us.UserID == userID);
            if (u != null)
            {
                UserDTO user = new UserDTO
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Email = u.Email,
                    Gender = u.Gender,
                    Address = u.Address,
                    IsEnabled = (int)u.IsEnabled
                };
                return View(user);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người dùng!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(UserDTO user, string isEnabled)
        {
            if (ModelState.IsValid)
            {
                User u = db.Users.FirstOrDefault(us => us.UserID == user.UserID);
                if (u != null)
                {
                    u.Name = user.Name;
                    u.Email = user.Email;
                    u.Gender = user.Gender;
                    u.Address = user.Address;
                    u.IsEnabled = Convert.ToInt16(isEnabled);
                    db.SaveChanges();
                }
            }
            TempData["Success"] = "Đã sửa thông tin khách hàng thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult LockUser(string userID)
        {
            User u = db.Users.FirstOrDefault(us => us.UserID == userID);
            if (u != null)
            {
                u.IsEnabled = 0;
            }
            TempData["Success"] = "Đã khoá tài khoản người dùng!";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult UnlockUser(string userID)
        {
            User u = db.Users.FirstOrDefault(us => us.UserID == userID);
            if (u != null)
            {
                u.IsEnabled = 1;
            }
            TempData["Success"] = "Đã mở khoá tài khoản người dùng!";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}