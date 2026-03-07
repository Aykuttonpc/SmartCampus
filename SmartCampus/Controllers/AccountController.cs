using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class AccountController(AuthService auth, ReservationService res, OccupancyService occ) : Controller
{
    // GET /Account/Login
    public IActionResult Login() => View();

    // POST /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await auth.ValidateAsync(email, password);
        if (user == null) { ViewBag.Error = "E-posta veya şifre hatalı."; return View(); }

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("UserName", user.FullName ?? user.FirstName);
        var roles = await auth.GetRolesAsync(user.UserID);
        HttpContext.Session.SetString("Roles", string.Join(",", roles));

        return RedirectToAction("Index", "Home");
    }

    // GET /Account/Profile
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null) return RedirectToAction(nameof(Login));
        var vm = await res.GetUserDashboardAsync(userId.Value);
        return View(vm);
    }

    // POST /Account/Logout
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
