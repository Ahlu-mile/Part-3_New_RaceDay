using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController : ControllerBase
{
    private readonly ApplicationDbContext _db; private readonly IConfiguration _configuration; private readonly ILogger<MediaController> _logger;
    public MediaController(ApplicationDbContext db, IConfiguration configuration, ILogger<MediaController> logger) { _db=db; _configuration=configuration; _logger=logger; }

    [HttpPost("events/{eventId}/banner")]
    [RequireRole("Organiser")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadEventBanner(int eventId, IFormFile file)
    {
        if (!TryBlobService(out var service)) return StatusCode(503, new { message="Azure Blob Storage is not configured. Set AzureStorage:ConnectionString on the API and configure its private image containers." });
        if (!ValidImage(file)) return BadRequest(new { message="Upload a PNG, JPEG, or WebP image up to 5 MB." });
        var ev=await _db.Events.FirstOrDefaultAsync(e=>e.EventId==eventId); if(ev is null)return NotFound(new {message="Event not found."});
        var uid=HttpContext.Session.GetInt32(SessionKeys.UserId); if(uid is null || ev.OrganiserId!=uid.Value)return Forbid();
        var url=await SaveBlob(service!,"event-banners",file); if(url is null)return StatusCode(503,new {message="Blob upload failed. Check the Azure Storage configuration."});
        ev.BannerImageUrl=url; await _db.SaveChangesAsync(); return Ok(new { bannerImageUrl=url });
    }

    [HttpPost("profile-picture")]
    [RequireAuth]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (!TryBlobService(out var service)) return StatusCode(503, new { message="Azure Blob Storage is not configured. Set AzureStorage:ConnectionString on the API and configure its private image containers." });
        if (!ValidImage(file)) return BadRequest(new { message="Upload a PNG, JPEG, or WebP image up to 5 MB." });
        var uid=HttpContext.Session.GetInt32(SessionKeys.UserId); if(uid is null)return Unauthorized();
        var user=await _db.Users.FindAsync(uid.Value); if(user is null)return NotFound();
        var url=await SaveBlob(service!,"profile-images",file); if(url is null)return StatusCode(503,new {message="Blob upload failed. Check the Azure Storage configuration."});
        user.ProfileImageUrl=url; await _db.SaveChangesAsync(); return Ok(new { profileImageUrl=url });
    }

    [HttpGet("files/{**blobName}")]
    public async Task<IActionResult> GetFile(string blobName)
    {
        if (!TryBlobService(out var service)) return StatusCode(503);
        if (string.IsNullOrWhiteSpace(blobName) || blobName.Contains("..", StringComparison.Ordinal)) return BadRequest();
        try
        {
            var parts = blobName.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return BadRequest();
            var containerName = parts[0] switch
            {
                "event-banners" => _configuration["AzureStorage:EventImagesContainer"] ?? "event-images",
                "profile-images" => _configuration["AzureStorage:ProfileImagesContainer"] ?? "profile-images",
                _ => null
            };
            if (containerName is null) return BadRequest();
            var blob=service!.GetBlobContainerClient(containerName).GetBlobClient(parts[1]);
            if(!await blob.ExistsAsync())return NotFound();
            var download=await blob.DownloadStreamingAsync();
            return File(download.Value.Content, download.Value.Details.ContentType ?? "application/octet-stream");
        }
        catch (Exception ex) { _logger.LogError(ex, "Unable to read media blob {BlobName}", blobName); return StatusCode(503, new { message = "Image storage is temporarily unavailable." }); }
    }

    private bool TryBlobService(out BlobServiceClient? client)
    {
        var connection=_configuration["AzureStorage:ConnectionString"]; if(string.IsNullOrWhiteSpace(connection)){client=null;return false;}
        try { client=new BlobServiceClient(connection); return true; } catch (Exception ex) { _logger.LogError(ex, "Azure Blob Storage connection configuration is invalid"); client=null; return false; }
    }
    private static bool ValidImage(IFormFile? file) => file is { Length: > 0 and <= 5_000_000 } && new[]{"image/png","image/jpeg","image/webp"}.Contains(file.ContentType,StringComparer.OrdinalIgnoreCase);
    private async Task<string?> SaveBlob(BlobServiceClient service,string folder,IFormFile file)
    {
        try
        {
            var containerName = folder == "event-banners"
                ? _configuration["AzureStorage:EventImagesContainer"] ?? "event-images"
                : _configuration["AzureStorage:ProfileImagesContainer"] ?? "profile-images";
            var container=service.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);
            var ext=Path.GetExtension(file.FileName).ToLowerInvariant();
            if(!new[]{".png",".jpg",".jpeg",".webp"}.Contains(ext))ext=file.ContentType=="image/png"?".png":file.ContentType=="image/webp"?".webp":".jpg";
            var blob=container.GetBlobClient($"{Guid.NewGuid():N}{ext}");
            await using var stream=file.OpenReadStream();
            await blob.UploadAsync(stream,new BlobHttpHeaders{ContentType=file.ContentType});
            var publicBase=(_configuration["ApiSettings:PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
            return $"{publicBase}/api/media/files/{folder}/{Path.GetFileName(blob.Name)}";
        }
        catch (Exception ex) { _logger.LogError(ex, "Unable to upload media blob for folder {Folder}", folder); return null; }
    }
}

