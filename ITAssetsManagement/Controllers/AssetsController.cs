using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.IO;
namespace ITAssetsManagement.Controllers
{
    public class AssetsController : BaseController
    {
        public AssetsController(ITAssetsContext context)
           : base(context)
        {
        }
        public IActionResult Index(int? assetTypeId,
                           string? AssetModel,
                           int? status,
                           int? condition,
                           string? department,
                           int? userId,
                           string? location)
        {
            var assets = _context.Assets.AsNoTracking().AsQueryable();

            // 1️⃣ Asset Type
            if (assetTypeId.HasValue)
                assets = assets.Where(a => a.AssetTypeId == assetTypeId);

            // 2️⃣ Asset Model List
            List<string> models;
            string assetTypeName = null;

            if (assetTypeId.HasValue)
            {
                assetTypeName = _context.AssetTypes
                    .Where(t => t.Id == assetTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault();

                models = _context.AssetModels
                    .Where(m => m.AssetType == assetTypeName)
                    .Select(m => m.Asset_Model)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            }
            else
            {
                models = _context.AssetModels
                    .Select(m => m.Asset_Model)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(AssetModel) && !models.Contains(AssetModel))
                AssetModel = null;

            if (!string.IsNullOrWhiteSpace(AssetModel))
                assets = assets.Where(a => a.AssetModel == AssetModel);

            if (status.HasValue)
                assets = assets.Where(a => a.Status == status);

            if (condition.HasValue)
                assets = assets.Where(a => a.Condition == condition);

            var assetList = assets.ToList();

            // 🔹 Latest Assignment
            var latestAssignments = _context.AssetAssignments
                .Where(g => g.ReturnDate == null)
                .AsNoTracking()
                .ToList()
                .GroupBy(x => x.AssetId)
                .Select(g => g.OrderByDescending(x => x.AssignedDate).First())
                .ToList();

            var users = _context.AssetUsers
                .AsNoTracking()
                .ToList();

            //new
            var statusMap = _context.AssetStatuses.ToDictionary(s => s.Id, s => s.Name);

            var result =
                (from a in assetList
                 join la in latestAssignments on a.AssetId equals la.AssetId into laj
                 from assign in laj.DefaultIfEmpty()
                 join u in users on assign?.AssignedTo equals u.UserId into uj
                 from user in uj.DefaultIfEmpty()
                 select new AssetIndexVM
                 {
                     Asset = a,
                     IsAssigned = a.Status == 2 && assign != null,

                     //                 UserName =
                     //a.Status == 2 && user != null
                     //    ? user.UserName + " (" + user.UserId + ")"
                     //    : a.Status == 1
                     //    ? "In Stock"
                     //    : a.Status == 3
                     //    ? "Disposed"
                     //    : a.Status == 4
                     //    ? "Donated"
                     //    : a.Status == 5
                     //    ? "Lost / Stolen"
                     //    : a.Status == 6
                     //    ? "In Repair"

                     //: "Not Assigned",

                     //new
                     UserName = a.Status == 2 && user != null ? user.UserName + " (" + user.UserId + ")" : statusMap.ContainsKey(a.Status) ? statusMap[a.Status] : "Unknown",

                     Department = (a.Status == 2 && user != null)
                         ? user.DeptName
                         : "-",

                     //AssetLocation = assign != null
                     //    ? assign.AssetLocation
                     //    : "In Stock"
                     //new
                     AssetLocation = a.Status == 2 && assign != null ? assign.AssetLocation : statusMap.ContainsKey(a.Status) ? statusMap[a.Status] : "In Stock"
                 })
                .AsQueryable();

            // 🔥 APPLY LOCATION FILTER HERE (Correct Place)
            if (!string.IsNullOrWhiteSpace(location))
                result = result.Where(x => x.AssetLocation == location);

            if (userId.HasValue)
                result = result.Where(x => x.UserId == userId);

            if (!string.IsNullOrWhiteSpace(department))
                result = result.Where(x => x.Department == department);

            // 🔹 Dropdowns
            ViewBag.AssetTypeList = new SelectList(
                _context.AssetTypes.ToList(), "Id", "Name", assetTypeId);

            ViewBag.AssetModelList = new SelectList(models, AssetModel);

            ViewBag.StatusList = new SelectList(
                _context.AssetStatuses.ToList(), "Id", "Name", status);

            ViewBag.ConditionList = new SelectList(
                _context.AssetConditions.ToList(), "CondId", "CondName", condition);

            ViewBag.LocationList = new SelectList(
                _context.AssetLocations
                    .OrderBy(x => x.Location_Name)
                    .Select(x => x.Location_Name)
                    .ToList(),
                location   // 🔥 THIS IS IMPORTANT
            );

            ViewBag.DepartmentList = _context.AssetUsers
                .Select(u => u.DeptName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View(result.ToList());
        }



        public IActionResult Create()
        {
            if (!IsFullAccess)
                return NoAccess();
            LoadDropDowns();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Asset asset)
        {
            if (!IsFullAccess)
                return Unauthorized();
            if (!ModelState.IsValid)
            {
                LoadDropDowns();
                return View(asset);
            }
            if (_context.Assets.Any(a => a.AssetId == asset.AssetId))
            {
                ModelState.AddModelError("AssetId", "Asset ID already exists");
                LoadDropDowns();
                return View(asset);
            }
            asset.LastModified = DateTime.Now;
            try
            {
                _context.Assets.Add(asset);
                AddAuditLog("CREATE", "Asset", asset.AssetId);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content(ex.InnerException?.Message ?? ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(string id)
        {
            if (id == null)
                return NotFound();
            var asset = _context.Assets.Find(id);
            if (asset == null)
                return NotFound();
            LoadDropDowns();
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Asset asset)
        {

            if (!IsFullAccess)
                return NoAccess();
            if (!ModelState.IsValid)
            {
                LoadDropDowns();
                return View(asset);
            }

            var dbAsset = _context.Assets.Find(asset.AssetId);
            if (dbAsset == null)
                return NotFound();

            // ✅ CAPTURE OLD STATUS FIRST
            var oldStatus = dbAsset.Status;

            // 🔽 UPDATE FIELDS
            dbAsset.AssetTypeId = asset.AssetTypeId;
            dbAsset.AssetModel = asset.AssetModel;
            dbAsset.SerialNumber = asset.SerialNumber;
            dbAsset.InvoiceNumber = asset.InvoiceNumber;
            dbAsset.PurchasedDate = asset.PurchasedDate;
            dbAsset.PurchasedFrom = asset.PurchasedFrom;
            dbAsset.PurchasedAs = asset.PurchasedAs;
            dbAsset.Status = asset.Status;          // <-- new status
            dbAsset.Condition = asset.Condition;
            dbAsset.DisposedDate = asset.DisposedDate;
            dbAsset.Comments = asset.Comments;
            dbAsset.LastModified = DateTime.Now;

            // 🔁 AUTO RETURN LOGIC
            if (oldStatus == 2 && asset.Status == 1)
            {
                var latestAssignment = _context.AssetAssignments
                    .Where(x => x.AssetId == asset.AssetId && x.ReturnDate == null)
                    .OrderByDescending(x => x.AssignedDate)
                    .FirstOrDefault();

                if (latestAssignment != null)
                {
                    latestAssignment.ReturnDate = DateTime.Now;

                    AddAuditLog(
                        "AUTO_RETURN",
                        "AssetAssignment",
                        asset.AssetId
                    );
                }
            }

            AddAuditLog("EDIT", "Asset", asset.AssetId);
            _context.SaveChanges();
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content(
                    ex.InnerException?.Message
                    ?? ex.Message
                );
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(string id)
        {
            if (id == null)
                return NotFound();
            var asset = _context.Assets.Find(id);
            if (asset == null)
                return NotFound();
            return View(asset);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var asset = _context.Assets.Find(id);
            if (asset == null)
                return NotFound();
            _context.Assets.Remove(asset);
            AddAuditLog("DELETE", "Asset", asset.AssetId);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        private void LoadDropDowns()
        {
            ViewBag.AssetTypes = _context.AssetTypes.ToList();
            ViewBag.AssetStatuses = _context.AssetStatuses.ToList();
            ViewBag.AssetConditions = _context.AssetConditions.ToList();
            ViewBag.PurchasedAsList = _context.PurchasedAs.ToList();
        }
        public IActionResult ExportToExcel(int? assetTypeId, int? status, int? condition)
        {
            var assets = _context.Assets.AsQueryable();
            if (assetTypeId.HasValue)
                assets = assets.Where(a => a.AssetTypeId == assetTypeId);
            if (status.HasValue)
                assets = assets.Where(a => a.Status == status);
            if (condition.HasValue)
                assets = assets.Where(a => a.Condition == condition);
            var assetList = assets.ToList();
            var assetTypeMap = _context.AssetTypes
                .ToDictionary(x => x.Id, x => x.Name ?? "");
            var statusMap = _context.AssetStatuses
                .ToDictionary(x => x.Id, x => x.Name ?? "");
            var conditionMap = _context.AssetConditions
                .ToDictionary(x => x.CondId, x => x.CondName ?? "");
            var assetTypeList = assetTypeMap.Values.ToList();
            var assetModelList = _context.AssetModels
                .Select(x => x.Asset_Model ?? "")
                .Distinct()
                .ToList();
            var statusList = statusMap.Values.ToList();
            var conditionList = conditionMap.Values.ToList();
            var purchasedAsList = _context.PurchasedAs.Where(x => !string.IsNullOrWhiteSpace(x.PurchasedAs)).Select(x => x.PurchasedAs!).Distinct().ToList();
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Assets");
            string[] headers = {
        "Asset ID","Asset Type","Asset Model","Serial Number",
        "Invoice Number","Purchased Date","Purchased From",
        "Purchased As","Status","Condition","Disposed Date", "Last Modified", "Comments"
    };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Row(1).Style.Font.Bold = true;
            int row = 2;
            foreach (var a in assetList)
            {
                ws.Cell(row, 1).Value = a.AssetId;
                ws.Cell(row, 2).Value =
                    assetTypeMap.ContainsKey(a.AssetTypeId)
                        ? assetTypeMap[a.AssetTypeId]
                        : "";
                ws.Cell(row, 3).Value = a.AssetModel;
                ws.Cell(row, 4).Value = a.SerialNumber;
                ws.Cell(row, 5).Value = a.InvoiceNumber;
                ws.Cell(row, 6).Value = a.PurchasedDate?.ToString("dd-MM-yyyy");
                ws.Cell(row, 7).Value = a.PurchasedFrom;
                ws.Cell(row, 8).Value = a.PurchasedAs;
                ws.Cell(row, 9).Value =
                    statusMap.ContainsKey(a.Status)
                        ? statusMap[a.Status]
                        : "";
                ws.Cell(row, 10).Value =
                    conditionMap.ContainsKey(a.Condition)
                        ? conditionMap[a.Condition]
                        : "";
                ws.Cell(row, 11).Value = a.DisposedDate?.ToString("dd-MM-yyyy");
                ws.Cell(row, 12).Value = a.LastModified.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cell(row, 13).Value = a.Comments;
                row++;
            }
            AddHiddenSheet(wb, "AssetTypes", assetTypeList);
            AddAssetModelSheet(wb);
            AddHiddenSheet(wb, "Statuses", statusList);
            AddHiddenSheet(wb, "Conditions", conditionList);
            AddHiddenSheet(wb, "PurchasedAs", purchasedAsList);
            AddDropdown(ws, "AssetTypes", 2, row - 1, 2);
            for (int r = 2; r <= row - 1; r++)
            {
                var cell = ws.Cell(r, 3);
                var dv = cell.CreateDataValidation();
                dv.List($"=INDIRECT(" + $"SUBSTITUTE(" + $"SUBSTITUTE(" + $"SUBSTITUTE($B{r},\" \",\"_\")," + "\"&\",\"\")," + "\"/\",\"\"))", true);
            }
            AddDropdown(ws, "PurchasedAs", 2, row - 1, 8);
            AddDropdown(ws, "Statuses", 2, row - 1, 9);
            AddDropdown(ws, "Conditions", 2, row - 1, 10);
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            AddAuditLog("EXPORT", "Asset", "MULTIPLE");
            _context.SaveChanges();
            wb.SaveAs(stream);
            stream.Position = 0;
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Assets.xlsx"
            );
        }
        private void AddAssetModelSheet(XLWorkbook wb)
        {
            var ws = wb.Worksheets.Add("AssetModels");
            var grouped = _context.AssetModels
                .AsEnumerable()
                .GroupBy(x => x.AssetType);
            int col = 1;
            foreach (var group in grouped)
            {
                string safeName = ToExcelSafe(group.Key);
                ws.Cell(1, col).Value = safeName;
                int row = 2;
                foreach (var model in group)
                {
                    ws.Cell(row, col).Value = model.Asset_Model;
                    row++;
                }
                wb.NamedRanges.Add(
                    safeName,
                    ws.Range(2, col, row - 1, col)
                );
                col++;
            }
            ws.Visibility = XLWorksheetVisibility.VeryHidden;
        }
        private void AddHiddenSheet(XLWorkbook wb, string sheetName, List<string> values)
        {
            var ws = wb.Worksheets.Add(sheetName);
            for (int i = 0; i < values.Count; i++)
                ws.Cell(i + 1, 1).Value = values[i];
            ws.Visibility = XLWorksheetVisibility.VeryHidden;
        }
        private void AddDropdown(IXLWorksheet ws, string sourceSheet, int fromRow, int toRow, int column)
        {
            var range = ws.Range(fromRow, column, toRow, column);
            var dv = range.CreateDataValidation();
            dv.List($"={sourceSheet}!$A$1:$A$100", true);
        }
        public IActionResult Import()
        {
            if (!IsFullAccess)
                return NoAccess();
            return View();
        }
        //[HttpPost]
        //public IActionResult ImportExcel(IFormFile file)
        //{
        //    if (!IsFullAccess)
        //        return NoAccess();
        //    if (!_context.AssetTypes.Any())
        //    {
        //        TempData["MSG"] = "Asset Types master data missing. Import aborted.";
        //        return RedirectToAction("Index");
        //    }
        //    if (file == null || file.Length == 0)
        //    {
        //        TempData["MSG"] = "No file selected";
        //        return RedirectToAction("Index");
        //    }
        //    if (_context.AssetTypes == null)
        //    {
        //        throw new Exception("AssetTypes DbSet is NULL");
        //    }
        //    var assetTypes = _context.AssetTypes?.ToList() ?? new List<AssetType>();
        //    var assetTypeMap = assetTypes
        //        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
        //        .GroupBy(x => x.Name!.Trim().ToLower())
        //        .ToDictionary(g => g.Key, g => g.First().Id);
        //    var statusMap = _context.AssetStatuses.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToDictionary(x => x.Name!.Trim().ToLower(), x => x.Id);
        //    var conditionMap = _context.AssetConditions.Where(x => !string.IsNullOrWhiteSpace(x.CondName)).ToDictionary(x => x.CondName!.Trim().ToLower(), x => x.CondId);
        //    int inserted = 0;
        //    int skipped = 0;
        //    using var stream = new MemoryStream();
        //    file.CopyTo(stream);
        //    using var wb = new XLWorkbook(stream);
        //    var ws = wb.Worksheet("Assets");
        //    int row = 2;
        //    while (!ws.Cell(row, 1).IsEmpty())
        //    {
        //        try
        //        {
        //            string assetId = ws.Cell(row, 1).GetString().Trim();
        //            string assetTypeName = ws.Cell(row, 2).GetString()?.Trim().ToLower() ?? "";
        //            string assetModel = ws.Cell(row, 3).GetString().Trim();
        //            string serialNo = ws.Cell(row, 4).GetString().Trim();
        //            string invoiceNo = ws.Cell(row, 5).GetString().Trim();
        //            if (string.IsNullOrWhiteSpace(invoiceNo)) invoiceNo = "NA";
        //            DateTime purchasedDate = GetDateSafe(ws.Cell(row, 6));
        //            string purchasedFrom = ws.Cell(row, 7).GetString().Trim();
        //            if (string.IsNullOrWhiteSpace(purchasedFrom)) purchasedFrom = "NA";
        //            string purchasedAs = ws.Cell(row, 8).GetString().Trim();
        //            //string statusName = ws.Cell(row, 9).GetString().Trim().ToLower();
        //            //string conditionName = ws.Cell(row, 10).GetString().Trim().ToLower();

        //            string statusName = GetCellValueOrDefault(ws.Cell(row, 9).GetString(), "In Stock");
        //            string conditionName = GetCellValueOrDefault(ws.Cell(row, 10).GetString(), "Working");

        //            DateTime? disposedDate = ws.Cell(row, 11).IsEmpty()
        //                ? null
        //                : GetDateSafe(ws.Cell(row, 11));
        //            DateTime lastModified = ws.Cell(row, 12).IsEmpty() ? DateTime.Now : GetDateSafe(ws.Cell(row, 13));
        //            string comments = ws.Cell(row, 13).GetString().Trim();
        //            if (string.IsNullOrWhiteSpace(assetId) ||
        //                !assetTypeMap.ContainsKey(assetTypeName) ||
        //                !statusMap.ContainsKey(statusName) ||
        //                !conditionMap.ContainsKey(conditionName) ||
        //                _context.Assets.Any(a => a.AssetId == assetId))
        //            {
        //                skipped++;
        //                row++;
        //                continue;
        //            }
        //            var asset = new Asset
        //            {
        //                AssetId = assetId,
        //                AssetTypeId = assetTypeMap[assetTypeName],
        //                AssetModel = assetModel,
        //                SerialNumber = serialNo,
        //                InvoiceNumber = invoiceNo,
        //                PurchasedDate = purchasedDate,
        //                PurchasedFrom = purchasedFrom,
        //                PurchasedAs = purchasedAs,
        //                Status = statusMap[statusName],
        //                Condition = conditionMap[conditionName],
        //                DisposedDate = disposedDate,
        //                Comments = comments,
        //                LastModified = DateTime.Now
        //            };
        //            _context.Assets.Add(asset);
        //            inserted++;
        //        }
        //        catch
        //        {
        //            skipped++;
        //        }
        //        row++;
        //    }
        //    AddAuditLog("IMPORT", "Asset", "MULTIPLE");
        //    _context.SaveChanges();
        //    TempData["MSG"] = $"Import completed successfully. Inserted: {inserted}, Skipped: {skipped}";
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public IActionResult ImportExcel(IFormFile file)
        {
            if (!IsFullAccess)
                return NoAccess();

            if (file == null || file.Length == 0)
            {
                TempData["MSG"] = "No file selected";
                return RedirectToAction("Index");
            }

            if (!_context.AssetTypes.Any())
            {
                TempData["MSG"] = "Asset Types master data missing. Import aborted.";
                return RedirectToAction("Index");
            }

            // 🔹 Normalize function (IMPORTANT)
            string Normalize(string value)
            {
                return value?.Trim().ToLower().Replace(" ", "") ?? "";
            }

            // 🔹 Master maps (Normalized keys)
            var assetTypeMap = _context.AssetTypes
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToDictionary(x => Normalize(x.Name), x => x.Id);

            var statusMap = _context.AssetStatuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToDictionary(x => Normalize(x.Name), x => x.Id);

            var conditionMap = _context.AssetConditions
                .Where(x => !string.IsNullOrWhiteSpace(x.CondName))
                .ToDictionary(x => Normalize(x.CondName), x => x.CondId);

            int inserted = 0;
            int skipped = 0;

            using var stream = new MemoryStream();
            file.CopyTo(stream);

            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet("Assets");

            int row = 2;

            while (!ws.Cell(row, 1).IsEmpty())
            {
                try
                {
                    string assetId = ws.Cell(row, 1).GetString().Trim();

                    string assetTypeName = Normalize(ws.Cell(row, 2).GetString());
                    string assetModel = ws.Cell(row, 3).GetString().Trim();
                    string serialNo = ws.Cell(row, 4).GetString().Trim();

                    string invoiceNo = ws.Cell(row, 5).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(invoiceNo)) invoiceNo = "NA";

                    DateTime purchasedDate = GetDateSafe(ws.Cell(row, 6));

                    string purchasedFrom = ws.Cell(row, 7).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(purchasedFrom)) purchasedFrom = "NA";

                    string purchasedAs = ws.Cell(row, 8).GetString().Trim();

                    // 🔥 FIXED NORMALIZATION
                    string statusName = Normalize(
                        GetCellValueOrDefault(ws.Cell(row, 9).GetString(), "In Stock")
                    );

                    string conditionName = Normalize(
                        GetCellValueOrDefault(ws.Cell(row, 10).GetString(), "Working")
                    );

                    DateTime? disposedDate = ws.Cell(row, 11).IsEmpty()
                        ? null
                        : GetDateSafe(ws.Cell(row, 11));

                    // 🔥 FIXED COLUMN BUG
                    DateTime lastModified = ws.Cell(row, 12).IsEmpty()
                        ? DateTime.Now
                        : GetDateSafe(ws.Cell(row, 12));

                    string comments = ws.Cell(row, 13).GetString().Trim();

                    // 🔴 VALIDATION
                    if (string.IsNullOrWhiteSpace(assetId) ||
                        !assetTypeMap.ContainsKey(assetTypeName) ||
                        !statusMap.ContainsKey(statusName) ||
                        !conditionMap.ContainsKey(conditionName) ||
                        _context.Assets.Any(a => a.AssetId == assetId))
                    {
                        skipped++;
                        row++;
                        continue;
                    }

                    // ✅ INSERT
                    var asset = new Asset
                    {
                        AssetId = assetId,
                        AssetTypeId = assetTypeMap[assetTypeName],
                        AssetModel = assetModel,
                        SerialNumber = serialNo,
                        InvoiceNumber = invoiceNo,
                        PurchasedDate = purchasedDate,
                        PurchasedFrom = purchasedFrom,
                        PurchasedAs = purchasedAs,
                        Status = statusMap[statusName],
                        Condition = conditionMap[conditionName],
                        DisposedDate = disposedDate,
                        Comments = comments,
                        LastModified = lastModified
                    };

                    _context.Assets.Add(asset);
                    inserted++;
                }
                catch
                {
                    skipped++;
                }

                row++;
            }

            AddAuditLog("IMPORT", "Asset", "MULTIPLE");
            _context.SaveChanges();

            TempData["MSG"] = $"Import completed successfully. Inserted: {inserted}, Skipped: {skipped}";

            return RedirectToAction("Index");
        }

        private string GetCellValueOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value.Trim();
        }
        private DateTime GetDateSafe(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return DateTime.Now;
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            DateTime dt;
            if (DateTime.TryParse(cell.GetString(), out dt))
                return dt;
            return DateTime.Now;
        }
        public IActionResult History(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();
            var asset = _context.Assets.FirstOrDefault(a => a.AssetId == id);
            if (asset == null)
                return NotFound();
            var assetTypeName = _context.AssetTypes
                .Where(t => t.Id == asset.AssetTypeId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? "";
            var statusName = _context.AssetStatuses
                .Where(s => s.Id == asset.Status)
                .Select(s => s.Name)
                .FirstOrDefault() ?? "";
            var model = new AssetHistoryVM
            {
                Asset = asset,
                AssetTypeName = assetTypeName,
                StatusName = statusName,
                AuditLogs = _context.AuditLogs
                    .Where(a => a.AssetId == id)
                    .OrderByDescending(a => a.PerformedOn)
                    .ToList(),
                Assignments = _context.AssetAssignments
                    .Where(a => a.AssetId == id)
                    .OrderByDescending(a => a.AssignedDate)
                    .ToList()
            };
            return View(model);
        }
        public IActionResult GetAssetModels(string assetType)
        {
            var models = _context.AssetModels
                .Where(m => m.AssetType == assetType)
                .Select(m => m.Asset_Model)
                .Distinct()
                .ToList();
            return Json(models);
        }
        private string ToExcelSafe(string text)
        {
            return text
                .Trim()
                .Replace(" ", "_")
                .Replace("&", "")
                .Replace("/", "")
                .Replace("-", "_");
        }
        public IActionResult ActiveAssetHistory(
    string assetId,
    string assignedTo,
    string location)
        {
            var query = _context.AssetAssignments
                .AsNoTracking()
                .Where(x => x.ReturnDate == null)
                .AsQueryable();

            // 🔹 Filters
            if (!string.IsNullOrWhiteSpace(assetId))
                query = query.Where(x => x.AssetId == assetId);

            if (!string.IsNullOrWhiteSpace(assignedTo))
                query = query.Where(x => x.AssignedTo == assignedTo);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(x => x.AssetLocation == location);

            // 🔹 Dropdown data
            ViewBag.AssetIdList = _context.AssetAssignments
                .Where(x => x.ReturnDate == null)
                .Select(x => x.AssetId)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.LocationList = new SelectList(
                _context.AssetLocations
                    .OrderBy(x => x.Location_Name)
                    .Select(x => x.Location_Name)
                    .ToList(),
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

            // 🔹 JOIN for Name (ID)
            var list =
                (from a in query
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
                     Comments = a.Comments
                 }).ToList();

            return View(list);
        }


        public IActionResult AssetHistory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var list = (
                from a in _context.AssetAssignments
                    .AsNoTracking()
                where a.AssetId == id
                      && a.ReturnDate == null
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
                    Comments = a.Comments
                }
            ).ToList();

            ViewBag.AssetId = id;
            return View(list);
        }


    }
}
