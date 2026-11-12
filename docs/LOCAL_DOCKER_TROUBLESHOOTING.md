# Local Docker and API troubleshooting

## MVC website shows an API 500 error

The MVC container reached the API, but the API request failed. Check the PowerShell window running `RaceDay.API` for the underlying exception. The browser only shows the HTTP status.

Check that the API process responds:

```powershell
Invoke-RestMethod http://localhost:5080/health
```

Then check the event endpoint that populates the home page:

```powershell
Invoke-RestMethod http://localhost:5080/api/events
```

If `/health` responds but `/api/events` fails with a SQL connection error, set the connection string for the API process and restart it:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Use the server and instance that actually host `RaceDayDB`. If the error names an invalid `BannerImageUrl` or `ProfileImageUrl` column, follow [the additive image-column upgrade](Part3_Add_Image_Urls.sql) against the correct database.

## MVC container cannot reach the API

Start the API on the host at `http://0.0.0.0:5080` and confirm `http://localhost:5080/health` responds. The Compose default uses `http://host.docker.internal:5080/` from MVC. Check the container output with:

```powershell
docker compose ps
docker compose logs --tail 100 mvc
```

## Image upload fails

Uploads are sent to the API. Set `AzureStorage__ConnectionString`, `AzureStorage__EventImagesContainer`, and `AzureStorage__ProfileImagesContainer` in the API process. Keep the Blob containers private and do not commit the connection string.
