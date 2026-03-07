using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

public class HomeController(OccupancyService occ) : Controller
{
    public async Task<IActionResult> Index()
    {
        var data = await occ.GetLiveHeatmapAsync();
        return View(data);
    }
}
