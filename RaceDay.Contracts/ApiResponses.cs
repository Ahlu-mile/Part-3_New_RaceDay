namespace RaceDay.Contracts;

/// <summary>Consistent lightweight response shape for API messages.</summary>
public sealed record ApiMessage(string Message);

/// <summary>Response returned when an image upload is stored.</summary>
public sealed record MediaUploadResponse(string ImageUrl);
