using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AuthController:Controller {
 private readonly RaceDayApiClient _api; public AuthController(RaceDayApiClient api)=>_api=api;
 [HttpGet] public IActionResult Login()=>View();
 [HttpPost] public async Task<IActionResult> Login(string email,string password){var d=await _api.Post("api/auth/login",new{email,password}); if(d==null)return View(); var x=d.RootElement; HttpContext.Session.SetInt32("UserId",x.GetProperty("userId").GetInt32()); HttpContext.Session.SetString("FullName",x.GetProperty("fullName").GetString()??""); HttpContext.Session.SetString("Role",x.GetProperty("role").GetString()??""); return RedirectToAction("Index", "Home");}
 [HttpGet] public IActionResult Register()=>View();
 [HttpPost] public async Task<IActionResult> Register(string fullName,string email,string password,string role,string? phoneNumber){var d=await _api.Post("api/auth/register",new{fullName,email,password,role,phoneNumber}); if(d==null)return View(); TempData["Success"]="Account created. Please sign in."; return RedirectToAction(nameof(Login));}
 [HttpPost] public async Task<IActionResult> Logout(){await _api.Post("api/auth/logout",new{}); HttpContext.Session.Clear(); return RedirectToAction("Index","Home");}
}
