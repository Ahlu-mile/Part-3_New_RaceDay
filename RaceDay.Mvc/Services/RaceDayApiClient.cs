using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace RaceDay.Mvc.Services;
public sealed class RaceDayApiClient
{
    private readonly HttpClient _http; private readonly IHttpContextAccessor _accessor; private readonly IConfiguration _config; private readonly ILogger<RaceDayApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor, IConfiguration config, ILogger<RaceDayApiClient> logger) { _http=http; _accessor=accessor; _config=config; _logger=logger; }
    private HttpContext? Context => _accessor.HttpContext;
    private string BaseUrl => _config["ApiSettings:BaseUrl"] ?? "http://localhost:5080/";
    public async Task<JsonDocument?> SendAsync(HttpMethod method, string path, object? body=null, IFormFile? file=null)
    {
        using var req = new HttpRequestMessage(method, new Uri(new Uri(BaseUrl), path.TrimStart('/')));
        var cookie = Context?.Session.GetString("ApiCookie"); if (!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie", cookie);
        if (file != null) { var form=new MultipartFormDataContent(); var stream=file.OpenReadStream(); var content=new StreamContent(stream); content.Headers.ContentType=new MediaTypeHeaderValue(file.ContentType); form.Add(content,"file",file.FileName); req.Content=form; }
        else if (body != null) req.Content=new StringContent(JsonSerializer.Serialize(body,JsonOptions),Encoding.UTF8,"application/json");
        try {
            using var response=await _http.SendAsync(req);
            if (response.Headers.TryGetValues("Set-Cookie",out var setCookies)) { var apiCookie=setCookies.Select(x=>x.Split(';',2)[0]).FirstOrDefault(x=>x.StartsWith(".AspNetCore.Session=")); if(apiCookie!=null) Context?.Session.SetString("ApiCookie",apiCookie); }
            var raw=await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) { _logger.LogWarning("RaceDay API {Method} {Path} returned HTTP {StatusCode}", method, path, (int)response.StatusCode); var message=$"API request failed ({(int)response.StatusCode} {response.ReasonPhrase})."; try { using var e=JsonDocument.Parse(raw); if(e.RootElement.TryGetProperty("message",out var m)) message=m.GetString()??message; else if(e.RootElement.TryGetProperty("title",out var t)) message=t.GetString()??message; } catch {} Context?.Session.SetString("ApiError",message); return null; }
            Context?.Session.Remove("ApiError");
            if(string.IsNullOrWhiteSpace(raw)) return JsonDocument.Parse("{}"); return JsonDocument.Parse(raw);
        } catch(HttpRequestException ex) { _logger.LogWarning(ex, "RaceDay API {Method} {Path} could not be reached", method, path); Context?.Session.SetString("ApiError","RaceDay API could not be reached. Start the API and check ApiSettings:BaseUrl."); return null; }
    }
    public Task<JsonDocument?> Get(string path)=>SendAsync(HttpMethod.Get,path);
    public Task<JsonDocument?> Post(string path,object body)=>SendAsync(HttpMethod.Post,path,body);
    public Task<JsonDocument?> Put(string path,object body)=>SendAsync(HttpMethod.Put,path,body);
    public Task<JsonDocument?> Delete(string path)=>SendAsync(HttpMethod.Delete,path);
    public Task<JsonDocument?> Upload(string path,IFormFile file)=>SendAsync(HttpMethod.Post,path,file:file);
    public static JsonElement CloneRoot(JsonDocument? doc)=>doc?.RootElement.Clone() ?? JsonDocument.Parse("null").RootElement.Clone();
}
