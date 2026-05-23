using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace ITAssetsManagement.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
 
        public HomeController(
            ILogger<HomeController> logger,
            ITAssetsContext context
        ) : base(context)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            var assets = _context.Assets.AsNoTracking();
            var model = new DashboardVM
            {
                TotalAssets = assets.Count(),
                InStock = assets.Count(a => a.Status == 1),
                Assigned = assets.Count(a => a.Status == 2),
                Disposed = assets.Count(a => a.Status == 3),
                Donated = assets.Count(a => a.Status == 4),
                Lost = assets.Count(a => a.Status == 5),
                InRepair = assets.Count(a => a.Status == 6),
                Working = assets.Count(a => a.Status == 1 || a.Status == 2),
                NotWorking = assets.Count(a => a.Status == 3 || a.Status == 4 || a.Status == 5),
                Desktop = assets.Count(a => a.AssetTypeId == 1),
                Laptop = assets.Count(a => a.AssetTypeId == 2),
                Mac = assets.Count(a => a.AssetTypeId == 3),
                Tablet = assets.Count(a => a.AssetTypeId == 4),
                Server = assets.Count(a => a.AssetTypeId == 5),
                Firewall = assets.Count(a => a.AssetTypeId == 6),
                InternalHardDisk = assets.Count(a => a.AssetTypeId == 7),
                ExternalHardDisk = assets.Count(a => a.AssetTypeId == 8),
                Monitor = assets.Count(a => a.AssetTypeId == 9),
                Keyboard = assets.Count(a => a.AssetTypeId == 10),
                Mouse = assets.Count(a => a.AssetTypeId == 11),
                Printer = assets.Count(a => a.AssetTypeId == 12),
                Scanner = assets.Count(a => a.AssetTypeId == 13),
                Router = assets.Count(a => a.AssetTypeId == 14),
                Switch = assets.Count(a => a.AssetTypeId == 15),
                Storage = assets.Count(a => a.AssetTypeId == 16),
                Workstation = _context.Assets.Count(a => a.AssetTypeId == 17),
                OtherAccessories = assets.Count(a => a.AssetTypeId == 18)
            };
            return View(model);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
