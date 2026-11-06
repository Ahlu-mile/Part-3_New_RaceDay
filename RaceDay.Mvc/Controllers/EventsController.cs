using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class EventsController:Controller {
 private readonly RaceDayApiClient _api; public EventsController(RaceDayApiClient api)=>_api=api;
 private bool LoggedIn=>HttpContext.Session.GetInt32("UserId")!=null; private bool Organiser=>HttpContext.Session.GetString("Role")=="Organiser";
 public async Task<IActionResult> Index(string? eventType,string? location){var q="api/events"; var p=new List<string>();if(!string.IsNullOrWhiteSpace(eventType))p.Add("eventType="+Uri.EscapeDataString(eventType));if(!string.IsNullOrWhiteSpace(location))p.Add("location="+Uri.EscapeDataString(location));if(p.Count>0)q+="?"+string.Join("&",p);ViewBag.Events=RaceDayApiClient.CloneRoot(await _api.Get(q));ViewBag.EventType=eventType;ViewBag.Location=location;return View();}
 public async Task<IActionResult> Details(int id){ViewBag.Event=RaceDayApiClient.CloneRoot(await _api.Get($"api/events/{id}"));ViewBag.Categories=RaceDayApiClient.CloneRoot(await _api.Get($"api/events/{id}/categories"));ViewBag.Results=RaceDayApiClient.CloneRoot(await _api.Get($"api/events/{id}/results"));return View();}
 [HttpPost] public async Task<IActionResult> Enter(int categoryId){if(!LoggedIn||Organiser)return RedirectToAction("Login","Auth");await _api.Post("api/enrolments",new{categoryId});return RedirectToAction("MyEvents","Participant");}
}
