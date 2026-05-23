using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace ITAssetsManagement.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ITAssetsContext _context;
        public BaseController(ITAssetsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        protected string Dept =>
            HttpContext.Session.GetString("DeptName") ?? "";
        protected bool IsFullAccess =>
            Dept.Equals("Software", StringComparison.OrdinalIgnoreCase) || Dept.Equals("Management", StringComparison.OrdinalIgnoreCase) ||
            Dept.Equals("Systems", StringComparison.OrdinalIgnoreCase);
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.IsFullAccess = IsFullAccess;
            base.OnActionExecuting(context);
        }
        protected IActionResult NoAccess()
        {
            return StatusCode(403, "Access Denied");
        }
        protected string CurrentUser => HttpContext.Session.GetString("UserId") ?? "System";
        protected void AddAuditLog(string action, string entity, string assetId)
        {
            var log = new AuditLog
            {
                Action = action,
                Entity = entity,
                AssetId = assetId,
                PerformedBy = CurrentUser,
                PerformedOn = DateTime.Now
            };
            _context.AuditLogs.Add(log);
        }
    }
}
