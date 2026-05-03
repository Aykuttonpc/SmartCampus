using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class AdminController(AdminService admin) : Controller
{
    private bool IsAdmin() =>
        (HttpContext.Session.GetString("Roles") ?? "").Contains("Admin");

    // GET /Admin
    public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, string? riskFilter = null)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var reportList = await admin.GetNoShowReportAsync();
        
        IEnumerable<SmartCampus.Models.NoShowReportVm> filteredList = reportList;

        if (!string.IsNullOrEmpty(searchString))
        {
            filteredList = filteredList.Where(r => r.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(riskFilter))
        {
            filteredList = filteredList.Where(r => r.RiskLabel.Equals(riskFilter, StringComparison.OrdinalIgnoreCase));
        }

        var report = SmartCampus.Models.PaginatedList<SmartCampus.Models.NoShowReportVm>.Create(filteredList, pageNumber, 10);
        
        ViewBag.SearchString = searchString;
        ViewBag.RiskFilter = riskFilter;
        return View(report);
    }

    // GET /Admin/Facilities
    public async Task<IActionResult> Facilities(int pageNumber = 1, string? searchString = null)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var list = await admin.GetAllFacilitiesAsync(pageNumber, 10, searchString);
        ViewBag.SearchString = searchString;
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

    // GET /Admin/Users
    public async Task<IActionResult> Users(int pageNumber = 1, string? searchString = null, string? roleFilter = null)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var list = await admin.GetAllUsersWithRolesAsync(pageNumber, 10, searchString, roleFilter);
        ViewBag.SearchString = searchString;
        ViewBag.RoleFilter = roleFilter;
        return View(list);
    }

    // POST /Admin/ChangeRole
    [HttpPost]
    public async Task<IActionResult> ChangeRole(int userId, string newRole)
    {
        if (!IsAdmin()) return Forbid();
        var validRoles = new[] { "Student", "Staff", "Admin" };
        if (validRoles.Contains(newRole))
        {
            await admin.UpdateUserRoleAsync(userId, newRole);
        }
        return RedirectToAction(nameof(Users));
    }
}
