using ITAssetsManagement.Utility;
using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
namespace ITAssetsManagement.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(ITAssetsContext context)
            : base(context)
        {
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string userId, string password)
        {
            var user = _context.AssetUsers
                .FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                ViewBag.Error = "Invalid User ID";
                return View();
            }
            var decryptedPwd = DataSecurity.Decrypt(user.UserPassword);
            if (decryptedPwd != password)
            {
                ViewBag.Error = "Invalid Password";
                return View();
            }
            
            HttpContext.Session.SetString("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("DeptName", user.DeptName);
            AddAuditLog("LOGIN", "USER", user.UserId);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}