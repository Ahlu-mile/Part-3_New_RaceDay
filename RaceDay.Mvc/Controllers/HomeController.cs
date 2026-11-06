using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class HomeController : Controller
{
 private readonly RaceDayApiClient _api; public HomeController(RaceDayApiClient api)=>_api=api;
 public async Task<IActionResult> Index(){ var data=await _api.Get("api/events"); ViewBag.Events=RaceDayApiClient.CloneRoot(data); return View(); }
 public IActionResult Error()=>View();
}
