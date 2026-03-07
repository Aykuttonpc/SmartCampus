using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class AdminController(AdminService admin) : Controller
{
    private bool IsAdmin() =>
        (HttpContext.Session.GetString("Roles") ?? "").Contains("Admin");

    // GET /Admin
    public async Task<IActionResult> Index()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var report = await admin.GetNoShowReportAsync();
        return View(report);
    }

    // GET /Admin/Facilities
    public async Task<IActionResult> Facilities()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var list = await admin.GetAllFacilitiesAsync();
        return View(list);
    }

    // POST /Admin/DeleteFacility/3
    [HttpPost]
    public async Task<IActionResult> DeleteFacility(int id)
    {
        if (!IsAdmin()) return Forbid();
        await admin.DeleteFacilityAsync(id);
        return RedirectToAction(nameof(Facilities));
    }
}
