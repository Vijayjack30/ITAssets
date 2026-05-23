using ITAssetsManagement.Controllers;
using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class AuditLogsController : BaseController
{
    public AuditLogsController(ITAssetsContext context)
            : base(context)
    {
    }
    public IActionResult Index(string logAction, string entity, string assetId, string performedBy)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(logAction))
            query = query.Where(x => x.Action == logAction);
        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(x => x.Entity.Contains(entity));
        if (!string.IsNullOrWhiteSpace(assetId))
            query = query.Where(x => x.AssetId.Contains(assetId));
        if (!string.IsNullOrWhiteSpace(performedBy))
            query = query.Where(x => x.PerformedBy.Contains(performedBy));
        ViewBag.ActionList = _context.AuditLogs
                     .Select(x => x.Action)
                     .Distinct()
                     .OrderBy(x => x)
                     .ToList();
        ViewBag.EntityList = _context.AuditLogs
                     .Select(x => x.Entity)
                     .Distinct()
                     .OrderBy(x => x)
                     .ToList();
        ViewBag.AssetIdList = _context.AuditLogs
                      .Select(x => x.AssetId)
                      .Distinct()
                      .OrderBy(x => x)
                      .ToList();
        ViewBag.PerformedByList = _context.AuditLogs
                      .Select(x => x.PerformedBy)
                      .Distinct()
                      .OrderBy(x => x)
                      .ToList();
        return View(query
                .OrderByDescending(x => x.PerformedOn)
                .ToList());
    }
}
