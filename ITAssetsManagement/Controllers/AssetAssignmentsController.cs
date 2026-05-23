using ITAssetsManagement.Controllers;
using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
public class AssetAssignmentsController : BaseController
{
    public AssetAssignmentsController(ITAssetsContext context)
          : base(context)
    {
    }
    public IActionResult Index(
     string mode,
     string assetId,
     string assignedTo,
     string location)
    {
        if (!IsFullAccess)
            return NoAccess();
        var query = _context.AssetAssignments
            .AsNoTracking()
            .AsQueryable();
        //if (mode == "InRepair")
        //    query = query.Where(x => x.ReturnDate != null);
        if (mode == "Assigned")
            query = query.Where(x => x.ReturnDate == null);
        if (!string.IsNullOrWhiteSpace(assetId))
            query = query.Where(x => x.AssetId == assetId);
        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(x => x.AssignedTo == assignedTo);
        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(x =>
                x.AssetLocation != null &&
                x.AssetLocation.Trim().ToLower() == location.Trim().ToLower());
        }
        ViewBag.AssetIdList = _context.AssetAssignments
            .Select(x => x.AssetId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var locations = _context.AssetLocations
    .OrderBy(x => x.Location_Name)
    .Select(x => x.Location_Name)
    .ToList();

        locations.Insert(0, "-- All --");

        ViewBag.LocationList = new SelectList(
            locations,
            location
        );

        ViewBag.Users = new SelectList(
    _context.AssetUsers
        .OrderBy(u => u.UserName)
        .Select(u => new
        {
            Value = u.UserId,   
            Text = u.UserName + " (" + u.UserId + ")" 
        })
        .ToList(),
    "Value",
    "Text"
);
        var list = (
    from a in query
    join u in _context.AssetUsers
        on a.AssignedTo equals u.UserId into uj
    from u in uj.DefaultIfEmpty()
    orderby a.AssignedDate descending
    select new AssetAssignmentVM
    {
        AssetId = a.AssetId,
        AssignedTo = a.AssignedTo,
        AssignedToDisplay = u != null
            ? u.UserName + " (" + u.UserId + ")"
            : a.AssignedTo,
        AssetLocation = a.AssetLocation,
        AssignedDate = a.AssignedDate,
        ReturnDate = a.ReturnDate,
        RepairCompletedDate = a.RepairCompletedDate,
        Comments = a.Comments
    }
).ToList();
        return View(list);
    }
    public IActionResult Create(string assetId = null)
    {
        if (!IsFullAccess)
            return NoAccess();
        ViewBag.Assets = new SelectList(
            _context.Assets.Where(a => a.Status == 1),
            "AssetId",
            "AssetId",
            assetId
        );
        ViewBag.Users = new SelectList(
    _context.AssetUsers
        .OrderBy(u => u.UserName)
        .Select(u => new
        {
            Value = u.UserId,
            Text = u.UserName + " (" + u.UserId + ")"
        })
        .ToList(),
    "Value",
    "Text"
);
        ViewBag.LocationList = new SelectList(
    _context.AssetLocations
        .OrderBy(x => x.Location_Name)
        .Select(x => x.Location_Name)
        .ToList()
);
        return View();
    }
    
    
    
    [HttpPost]
    public IActionResult Create(AssetAssignment model)
    {
        if (!IsFullAccess)
            return NoAccess();
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.AssignedTo))
        {
            ViewBag.Assets = new SelectList(
                _context.Assets.Where(a => a.Status == 1),
                "AssetId",
                "AssetId"
            );
            ViewBag.Users = new SelectList(
    _context.AssetUsers
        .OrderBy(u => u.UserName)
        .Select(u => new
        {
            Value = u.UserId,   
	    Text = u.UserName + " (" + u.UserId + ")" 
		   
        })
        .ToList(),
    "Value",
    "Text"
);
            ViewBag.LocationList = new SelectList(
    _context.AssetLocations
        .OrderBy(x => x.Location_Name)
        .Select(x => x.Location_Name)
        .ToList()
);
            return View(model);
        }
        var now = DateTime.Now;
        model.AssignedDate = new DateTime(
            now.Year, now.Month, now.Day,
            now.Hour, now.Minute, now.Second
        );
        if (string.IsNullOrWhiteSpace(model.AssetLocation))
            model.AssetLocation = "Office";
        _context.AssetAssignments.Add(model);
        AddAuditLog("ASSIGN", "AssetAssignment", model.AssetId);
        var asset = _context.Assets.FirstOrDefault(a => a.AssetId == model.AssetId);
        if (asset != null)
        {
            asset.Status = 2; 
            asset.LastModified = DateTime.Now;
        }
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    public IActionResult Return(string assetId, DateTime assignedDate)
    {
        if (!IsFullAccess)
            return NoAccess();
        var assignment = _context.AssetAssignments
            .FirstOrDefault(x =>
                x.AssetId == assetId &&
                x.AssignedDate == assignedDate &&
                x.ReturnDate == null);
        if (assignment == null)
            return NotFound();
        assignment.ReturnDate = DateTime.Now;
        AddAuditLog("IN_REPAIR", "AssetAssignment", assetId);
        
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    public IActionResult RepairCompleted(string assetId, DateTime assignedDate)
    {
        if (!IsFullAccess)
            return NoAccess();
        var assignment = _context.AssetAssignments.FirstOrDefault(x =>
            x.AssetId == assetId &&
            x.AssignedDate == assignedDate &&
            x.ReturnDate != null &&
            x.RepairCompletedDate == null);
        if (assignment == null)
            return NotFound();
        assignment.RepairCompletedDate = DateTime.Now;
        var asset = _context.Assets.FirstOrDefault(a => a.AssetId == assetId);
        if (asset != null)
        {
            asset.Status = 1; 
            asset.LastModified = DateTime.Now;
        }
        AddAuditLog("REPAIR_COMPLETED", "AssetAssignment", assetId);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
