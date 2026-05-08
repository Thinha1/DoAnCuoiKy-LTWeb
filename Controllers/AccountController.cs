using System;
using System.Web.Mvc;
using System.Linq;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    public class AccountController : Controller
    {
        private QLBANHOAEntities db = new QLBANHOAEntities();

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (user != null && PasswordHelper.VerifyPassword(model.Password, user.Password) && user.IsEnabled == 1)
                    {
                        var role = db.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);
                        var userDTO = new UserDTO
                        {
                            UserID = user.UserID,
                            Name = user.Name,
                            Email = user.Email,
                            RoleID = user.RoleID,
                            RoleName = role?.RoleName ?? "Khách hàng"
                        };

                        SessionHelper.SetUserSession(userDTO);
                        TempData["SuccessMessage"] = "Đăng nhập thành công! Chúc mừng bạn.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Message = "Đăng nhập thất bại!";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi đăng nhập: " + ex.Message);
                }
            }
            return View(model);
        }

        //// AJAX LOGIN
        //[HttpPost]
        //public JsonResult AjaxLogin(LoginModel model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
        //            if (user != null && PasswordHelper.VerifyPassword(model.Password, user.Password))
        //            {
        //                var role = db.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);
        //                var userDTO = new UserDTO
        //                {
        //                    UserID = user.UserID,
        //                    Name = user.Name,
        //                    Email = user.Email,
        //                    RoleID = user.RoleID,
        //                    RoleName = role?.RoleName ?? "Khách hàng"
        //                };

        //                SessionHelper.SetUserSession(userDTO);
        //                return Json(new { success = true, message = "Đăng nhập thành công" });
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });
        //            }
        //        }
        //        return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi đăng nhập: " + ex.Message });
        //    }
        //}

        //GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ViewBag.Message = "Lỗi email đã tồn tại!";
                    return View(model);
                }
                if (model.Password.Length < 8 || model.Password.Length > 50)
                {
                    ViewBag.Message = "Mật khẩu phải từ 8 đến 50 ký tự!";
                    return View(model);
                }
                if (!model.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    ViewBag.Message = "Mật khẩu phải có ít nhất một ký tự đặc biệt!";
                    return View(model);
                }

                string userID = IDGenerator.GenerateUserID();
                string cartID = IDGenerator.GenerateShoppingCartID();

                var user = new User
                {
                    UserID = userID,
                    RoleID = "R002",
                    Name = model.Name,
                    Email = model.Email,
                    Password = PasswordHelper.HashPassword(model.Password),
                    Gender = model.Gender,
                    Address = model.Address,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    IsEnabled = 1
                };

                var shoppingCart = new ShoppingCart
                {
                    ShoppingCartID = cartID,
                    UserID = userID
                };

                db.Users.Add(user);
                db.ShoppingCarts.Add(shoppingCart);
                db.SaveChanges();

                var userDTO = new UserDTO
                {
                    UserID = user.UserID,
                    Name = user.Name,
                    Email = user.Email,
                    RoleID = user.RoleID,
                    RoleName = "Khách hàng"
                };

                SessionHelper.SetUserSession(userDTO);
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return View(model);
            }
        }

        // //AJAX REGISTER
        //[HttpPost]
        // public JsonResult AjaxRegister(RegisterModel model)
        // {
        //     try
        //     {
        //         if (ModelState.IsValid)
        //         {
        //             if (db.Users.Any(u => u.Email == model.Email))
        //             {
        //                 return Json(new { success = false, message = "Email đã tồn tại" });
        //             }

        //             string userID = IDGenerator.GenerateUserID();
        //             string cartID = IDGenerator.GenerateShoppingCartID();

        //             var user = new User
        //             {
        //                 UserID = userID,
        //                 RoleID = "R002",
        //                 Name = model.Name,
        //                 Email = model.Email,
        //                 Password = PasswordHelper.HashPassword(model.Password),
        //                 Gender = model.Gender,
        //                 Address = model.Address,
        //                 CreatedAt = DateTime.Now,
        //                 CreatedBy = "System"
        //             };

        //             var shoppingCart = new ShoppingCart
        //             {
        //                 ShoppingCartID = cartID,
        //                 UserID = userID
        //             };

        //             db.Users.Add(user);
        //             db.ShoppingCarts.Add(shoppingCart);
        //             db.SaveChanges();

        //             var userDTO = new UserDTO
        //             {
        //                 UserID = user.UserID,
        //                 Name = user.Name,
        //                 Email = user.Email,
        //                 RoleID = user.RoleID,
        //                 RoleName = "Khách hàng"
        //             };

        //             return Json(new { success = true, message = "Đăng ký thành công" });
        //         }
        //         return Json(new { success = false, message = "Vui lòng kiểm tra lại thông tin" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new { success = false, message = "Lỗi đăng ký: " + ex.Message });
        //     }
        // }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            SessionHelper.ClearSession();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // AJAX LOGOUT
        [HttpPost]
        public JsonResult AjaxLogout()
        {
            try
            {
                SessionHelper.ClearSession();
                return Json(new { success = true, message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi đăng xuất: " + ex.Message });
            }
        }

        // GET: /Account/UpdateProfile
        public ActionResult UpdateProfile()
        {
            var user = SessionHelper.GetUserSession();
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}