using ITAssetsManagement.Models;
using ITAssetsManagement.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace ITAssetsManagement.Controllers
{
    public class UsersController : BaseController
    {
        public UsersController(ITAssetsContext context)
            : base(context)
        {
        }
        public IActionResult Create()
        {
            if (!IsFullAccess)
                return NoAccess();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AssetUser model)
        {
            if (!IsFullAccess)
                return NoAccess();
            if (!ModelState.IsValid)
                return View(model);
            if (_context.AssetUsers.Any(u => u.UserId == model.UserId))
            {
                ModelState.AddModelError("UserId", "User ID already exists");
                return View(model);
            }
            model.UserPassword = DataSecurity.Encrypt(model.UserPassword);
            if (string.IsNullOrWhiteSpace(model.DeptName))
                model.DeptName = "General";
            _context.AssetUsers.Add(model);
            AddAuditLog("CREATE", "USER", model.UserId);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Index(string? department, string? status, string? userId)
        {
            var users = _context.AssetUsers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(department))
                users = users.Where(u => u.DeptName == department);
            if (!string.IsNullOrWhiteSpace(status))
                users = users.Where(u => u.User_Status == status);
            if (!string.IsNullOrWhiteSpace(userId))
                users = users.Where(u => u.UserId == userId);
            var result = users
                .OrderBy(u => u.User_Status)
                .ThenBy(u => u.UserName)
                .ToList();
            ViewBag.DepartmentList = new SelectList(
                _context.AssetUsers
                    .Select(u => u.DeptName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList(),
                department
            );
            ViewBag.StatusList = new SelectList(
                new List<string> { "A", "I" },
                status
            );
            ViewBag.UserList = new SelectList(
                _context.AssetUsers
                    .Select(u => new
                    {
                        u.UserId,
                        Display = u.UserName + " (" + u.UserId + ")"
                    })
                    .OrderBy(x => x.Display)
                    .ToList(),
                "UserId",
                "Display",
                userId
            );
            return View(result);
        }
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();
            var user = _context.AssetUsers.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();
            
            ViewBag.DepartmentList = new SelectList(
                _context.AssetUsers
                    .Select(u => u.DeptName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList(),
                user.DeptName
            );
            
            ViewBag.StatusList = new SelectList(
                new List<string> { "A", "I" },
                user.User_Status
            );
            
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AssetUser model)
        {
            var user = _context.AssetUsers.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
                return NotFound();
            
            user.DeptName = model.DeptName;
            user.User_Status = model.User_Status;
            _context.SaveChanges();
            AddAuditLog("EDIT", "USER", model.UserId);
            return RedirectToAction("Index");
        }
    }
}
