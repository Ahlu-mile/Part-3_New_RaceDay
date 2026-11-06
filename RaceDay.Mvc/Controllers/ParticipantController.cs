using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController:Controller {
 private readonly RaceDayApiClient _api; public ParticipantController(RaceDayApiClient api)=>_api=api;
 private bool Guard(){if(HttpContext.Session.GetString("Role")!="Participant"){TempData["Error"]="Please sign in as a Participant to access this page.";return false;}return true;}
 public async Task<IActionResult> Index(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Events=RaceDayApiClient.CloneRoot(await _api.Get("api/events"));return View();}
 public async Task<IActionResult> MyEvents(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Events=RaceDayApiClient.CloneRoot(await _api.Get("api/enrolments/me"));return View();}
 public async Task<IActionResult> MyResults(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Results=RaceDayApiClient.CloneRoot(await _api.Get("api/results/me"));return View();}
 public async Task<IActionResult> Profile(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Profile=RaceDayApiClient.CloneRoot(await _api.Get("api/users/me"));return View();}
 [HttpPost] public async Task<IActionResult> Profile(string fullName,string? phoneNumber,IFormFile? picture){if(!Guard())return RedirectToAction("Login","Auth");await _api.Put("api/users/me",new{fullName,phoneNumber});if(picture is { Length: > 0 })await _api.Upload("api/media/profile-picture",picture);TempData["Success"]="Profile saved. If you selected a picture, check Azure Storage settings if upload failed.";return RedirectToAction(nameof(Profile));}
}
