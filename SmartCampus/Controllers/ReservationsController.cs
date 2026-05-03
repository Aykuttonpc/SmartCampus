using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class ReservationsController(ReservationService res) : Controller
{
    private int? UserId => HttpContext.Session.GetInt32("UserID");

    // GET /Reservations  – Bugünün takvimi
    public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, string? statusFilter = null)
    {
        if (UserId == null) return RedirectToAction("Login", "Account");
        var list = await res.GetTodayReservationsAsync(pageNumber, 10, searchString, statusFilter);
        ViewBag.SearchString = searchString;
        ViewBag.StatusFilter = statusFilter;
        return View(list);
    }

    // GET /Reservations/Book?facilityId=3&date=2026-03-07
    public async Task<IActionResult> Book(int facilityId, DateOnly? date)
    {
        if (UserId == null) return RedirectToAction("Login", "Account");
        date ??= DateOnly.FromDateTime(DateTime.Today);
        if (date < DateOnly.FromDateTime(DateTime.Today)) date = DateOnly.FromDateTime(DateTime.Today); // EC4: geçmiş tarih
        var facilityName = await res.GetFacilityNameAsync(facilityId);
        if (facilityName == null) return NotFound();  // EC9: geçersiz facilityId
        var slots = await res.GetFreeSlotsAsync(facilityId, date.Value);
        ViewBag.FacilityId = facilityId;
        ViewBag.FacilityName = facilityName;
        ViewBag.Date = date.Value.ToString("yyyy-MM-dd");
        return View(slots);
    }

    // POST /Reservations/Confirm
    [HttpPost]
    public async Task<IActionResult> Confirm(BookingFormVm form)
    {
        var userId = UserId;
        if (userId == null) return RedirectToAction("Login", "Account");

        var (ok, error) = await res.BookAsync(form.FacilityID, userId.Value, form.SlotStart, form.SlotEnd);
        if (!ok) { TempData["Error"] = error; return RedirectToAction(nameof(Book), new { facilityId = form.FacilityID }); }

        return RedirectToAction(nameof(Index));
    }

    // POST /Reservations/Cancel/5
    [HttpPost]
    public async Task<IActionResult> Cancel(long id, string? returnUrl)
    {
        var userId = UserId;
        if (userId == null) return RedirectToAction("Login", "Account");

        // Admin/Staff her rezervasyonu iptal edebilir; normal kullanıcı yalnızca kendi rezervasyonunu
        var roles = HttpContext.Session.GetString("Roles") ?? "";
        int? ownerFilter = (roles.Contains("Admin") || roles.Contains("Staff")) ? null : userId;

        await res.CancelAsync(id, ownerFilter);

        // BUG-3: Gelinen sayfaya geri dön
        return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : LocalRedirect(returnUrl);
    }

    // POST /Reservations/Approve/5  (Admin/Staff yetkisi)
    [HttpPost]
    public async Task<IActionResult> Approve(long id)
    {
        var roles = HttpContext.Session.GetString("Roles") ?? "";
        if (!roles.Contains("Admin") && !roles.Contains("Staff"))
            return RedirectToAction(nameof(Index));
        await res.ApproveAsync(id);
        TempData["Success"] = "Rezervasyon onaylandı.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Reservations/Alternatives?typeName=Library
    public async Task<IActionResult> Alternatives(string typeName)
    {
        if (UserId == null) return RedirectToAction("Login", "Account");
        var alts = await res.GetAlternativesAsync(typeName, DateTime.Now, DateTime.Now.AddHours(1));
        return View(alts);
    }
}
