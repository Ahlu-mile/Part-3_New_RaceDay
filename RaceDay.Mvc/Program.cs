using RaceDay.Mvc.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; o.Cookie.SameSite = SameSiteMode.Lax; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<RaceDayApiClient>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseStaticFiles(); app.UseRouting(); app.UseSession(); app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })); app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}"); app.Run();
