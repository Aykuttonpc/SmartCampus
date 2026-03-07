using Microsoft.AspNetCore.Mvc;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class ReservationsController(ReservationService res) : Controller
{
    // GET /Reservations  – Bugünün takvimi
    public async Task<IActionResult> Index()
    {
        var list = await res.GetTodayReservationsAsync();
        return View(list);
    }

    // GET /Reservations/Book?facilityId=3&date=2026-03-07
    public async Task<IActionResult> Book(int facilityId, DateOnly? date)
    {
        date ??= DateOnly.FromDateTime(DateTime.Today);
        var slots = await res.GetFreeSlotsAsync(facilityId, date.Value);
        ViewBag.FacilityId = facilityId;
        ViewBag.Date = date.Value.ToString("yyyy-MM-dd");
        return View(slots);
    }

    // POST /Reservations/Confirm
    [HttpPost]
    public async Task<IActionResult> Confirm(BookingFormVm form)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null) return RedirectToAction("Login", "Account");

        var (ok, error) = await res.BookAsync(form.FacilityID, userId.Value, form.SlotStart, form.SlotEnd);
        if (!ok) { TempData["Error"] = error; return RedirectToAction(nameof(Book), new { facilityId = form.FacilityID }); }

        return RedirectToAction(nameof(Index));
    }

    // POST /Reservations/Cancel/5
    [HttpPost]
    public async Task<IActionResult> Cancel(long id)
    {
        await res.CancelAsync(id);
        return RedirectToAction(nameof(Index));
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
        var alts = await res.GetAlternativesAsync(typeName, DateTime.Now, DateTime.Now.AddHours(1));
        return View(alts);
    }
}
