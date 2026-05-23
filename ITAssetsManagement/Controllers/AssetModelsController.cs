using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
public class AssetModelsController : Controller
{
    private readonly ITAssetsContext _context;
    public AssetModelsController(ITAssetsContext context)
    {
        _context = context;
    }
    public IActionResult Create()
    {
        ViewBag.AssetTypes = _context.AssetTypes
            .OrderBy(x => x.Name)
            .ToList();
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(string assetType, string assetModel)
    {
        if (string.IsNullOrWhiteSpace(assetType) ||
            string.IsNullOrWhiteSpace(assetModel))
        {
            ModelState.AddModelError("", "Asset Type and Asset Model are required");
        }
        bool exists = _context.AssetModels.Any(x =>
            x.AssetType == assetType &&
            x.Asset_Model == assetModel);
        if (exists)
        {
            ModelState.AddModelError("", "This Asset Model already exists");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.AssetTypes = _context.AssetTypes
                .OrderBy(x => x.Name)
                .ToList();
            return View();
        }
        _context.AssetModels.Add(new AssetModel
        {
            AssetType = assetType,
            Asset_Model = assetModel.Trim()
        });
        _context.SaveChanges();
        TempData["MSG"] = "Asset Model added successfully";
        return RedirectToAction("Create");
    }
}
