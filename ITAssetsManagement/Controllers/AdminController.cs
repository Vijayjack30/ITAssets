using DocumentFormat.OpenXml.InkML;
using ITAssetsManagement.Controllers;
using ITAssetsManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class AdminController : BaseController
{
    public AdminController(ITAssetsContext context)
        : base(context)
    {
    }
    [HttpPost]
    public IActionResult SyncUsers()
    {
        if (!IsFullAccess)
            return NoAccess();
        var sql = @"
        INSERT INTO ITAssets.dbo.Asset_User (User_ID, User_Name, User_Password, Dept_Name, User_Status)
        SELECT
            U.User_ID,
            U.User_Name,
            U.User_Password,
            COALESCE(TD.Team_Name, TT.Team_Name, 'GENERAL'),
			U.User_Status
        FROM QSWFlow.dbo.WF_User_Master U
        LEFT JOIN QSWFlow.dbo.WF_Team_Master TT
            ON U.Team_ID = TT.Team_ID
        LEFT JOIN QSWFlow.dbo.WF_Team_Master TD
            ON TT.Dept_ID = TD.Team_ID
        WHERE U.User_Status = 'A' and NOT EXISTS (
            SELECT 1
            FROM ITAssets.dbo.Asset_User A
            WHERE A.User_ID = U.User_ID
        );";
        _context.Database.ExecuteSqlRaw(sql);
        AddAuditLog("SYNC", "USER", "ALL");
        return RedirectToAction("Index", "Assets");
    }
}
