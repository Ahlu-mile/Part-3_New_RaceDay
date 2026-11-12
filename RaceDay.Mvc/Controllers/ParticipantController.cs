using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController:Controller {
 private readonly RaceDayApiClient _api; public ParticipantController(RaceDayApiClient api)=>_api=api;
 private bool Guard(){if(HttpContext.Session.GetString("Role")!="Participant"){TempData["Error"]="Please sign in as a Participant to access this page.";return false;}return true;}
 public async Task<IActionResult> Index(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Events=RaceDayApiClient.CloneRoot(await _api.Get("api/events"));return View();}
 public async Task<IActionResult> MyEvents(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Events=RaceDayApiClient.CloneRoot(await _api.Get("api/enrolments/me"));return View();}
 public async Task<IActionResult> MyResults(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Results=RaceDayApiClient.CloneRoot(await _api.Get("api/results/me"));return View();}
 public async Task<IActionResult> Profile(){if(!Guard())return RedirectToAction("Login","Auth");ViewBag.Profile=RaceDayApiClient.CloneRoot(await _api.Get("api/users/me"));return View();}
 [HttpPost] public async Task<IActionResult> Profile(string fullName,string? phoneNumber,IFormFile? picture){if(!Guard())return RedirectToAction("Login","Auth");var updated=await _api.Put("api/users/me",new{fullName,phoneNumber});if(updated==null)return RedirectToAction(nameof(Profile));var pictureSaved=true;if(picture is { Length: > 0 })pictureSaved=await _api.Upload("api/media/profile-picture",picture)!=null;if(pictureSaved)TempData["Success"]="Profile saved.";else TempData["Error"]="Profile details were saved, but the profile picture upload failed.";return RedirectToAction(nameof(Profile));}
}
