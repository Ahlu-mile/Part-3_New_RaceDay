# RaceDay — PROG6212 Part 2 and Part 3

RaceDay is an event-management system for running, walking and cycling events. Part 2 supplies the ASP.NET Core REST API and automated tests. Part 3 adds an ASP.NET Core MVC website which calls that API; the website does **not** connect directly to SQL Server.

## Solution structure

- `RaceDay.API/` — existing Part 2 API, EF Core models, session-based authentication, role checks and endpoints.
- `RaceDay.Tests/` — existing Part 2 automated tests.
- `RaceDay.Contracts/` — shared response contract types for API/MVC integration.
- `RaceDay.Mvc/` — Part 3 website, controllers, Razor views, API client and Dockerfile.
- `docker-compose.yml` — MVC container connected to the API running on the host computer.
- `docs/Part3_Add_Image_Urls.sql` — safe additive SQL upgrade for event-banner and profile-picture URL fields.
- `.github/workflows/ci-part3.yml` — restore, build whole solution, run API tests and upload test results.

## Prerequisites

- Visual Studio 2022 with the .NET 8 SDK/workload, or .NET 8 SDK installed.
- SQL Server instance `LabVM2049939\\SQLEXPRESS` and a compatible `RaceDayDB` database.
- Docker Desktop for the container requirement.
- An Azure subscription/storage account is needed to demonstrate successful Blob uploads. The MVC and API code can be developed before Azure is provisioned, but uploads will return a clear configuration error until credentials are set.

## Database setup

Keep the database and connection string that already work with your Part 2 API. The API configuration now uses `Server=LabVM2049939\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;...` in both `appsettings.json` and `appsettings.Development.json`. The API launches at `http://localhost:5080` in the included HTTP launch profile. Confirm that this server is reachable and that `RaceDayDB` exists before running migrations.

Part 3 stores uploaded image URLs on the existing `Events` and `Users` records. After backing up the database, open `docs/Part3_Add_Image_Urls.sql` in SSMS, select the correct RaceDay database, and execute it once. It only adds nullable columns if they do not exist; it does not drop tables. Do not run it against a different database by accident.

## Run locally

1. Open `RaceDay.sln` in Visual Studio.
2. Confirm `RaceDay.API` has a valid `ConnectionStrings:DefaultConnection` and can run independently. Start its **HTTP** profile (`http://localhost:5080`) and confirm Swagger opens at `/swagger`.
3. Run `RaceDay.Mvc` as the startup project. Its `appsettings.json` uses `ApiSettings:BaseUrl = http://localhost:5080/`. Change this setting if your API is on a different address.
4. Register an Organiser and a Participant through the website. Log in with each role to check the different navigation and protected features.
5. Use the website to create events, manage categories/enrolments/results, enter events as a Participant, and edit the profile.

### Implemented API-backed pages

- Public event list, type/location filters, event details, categories and leaderboard.
- Registration/login/logout; MVC session stores user ID/name/role and retains the Part 2 API session cookie for subsequent protected calls.
- Organiser dashboard, event create/edit/delete, category create/delete, enrolment listing/status updates and result capture.
- Participant dashboard, enter event, My Events, My Results and profile update.
- Image upload requests travel from MVC to the Part 2 API. The API validates image type/size, uploads to Azure Blob Storage and saves the returned URL on the corresponding record.

## Azure Blob Storage setup

Azure has not been configured by default and no secret is committed in this repository. To enable uploads:

1. In the Azure portal, create a Storage Account.
2. Create two private Blob containers: `event-images` and `profile-images`. Do not enable anonymous public access just to make the demo work.
3. Obtain the storage connection string from the account's access keys. Keep it private.
4. Set it as an environment variable for the API process, not in a public commit. In PowerShell for the current terminal:

```powershell
$env:AzureStorage__ConnectionString = "<your-storage-connection-string>"
$env:AzureStorage__EventImagesContainer = "event-images"
$env:AzureStorage__ProfileImagesContainer = "profile-images"
dotnet run --project RaceDay.API --launch-profile http
```

5. Run the SQL upgrade above before uploading images.
6. In the MVC website, edit an event and choose a banner, or open Profile and choose a profile picture. A successful upload response is saved to the database and the URL is displayed in the UI.

**Security note:** the container remains private. The database stores an API media URL, and the API streams the image from Blob Storage, so the browser does not need public blob access or an Azure account key. Do not expose account keys or commit connection strings.

## Docker

The MVC website runs in Docker and connects to the API running on your Windows computer. This keeps the existing API connection to SQL Server and Azure settings intact. Start the API the same way you normally do, but bind it to an address Docker can reach. In PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5080"
dotnet run --project RaceDay.API --no-launch-profile
```

This uses the API's Development settings and .NET User Secrets, if already configured. Keep that terminal open. Then, from the solution root, start the website container:

```powershell
docker compose up --build -d
docker compose ps
docker compose logs -f mvc
```

Open `http://localhost:8080`. The MVC container calls the API at `http://host.docker.internal:5080/`; the API continues to use its existing SQL Server and Azure configuration. Stop the website with `docker compose down`. To use a different API address, set `RACEDAY_API_BASE_URL` before starting Compose.

An API Dockerfile is included for environments that use SQL authentication and can provide the database connection string to the container. The default Compose setup keeps the API on the host so the existing Windows/SQL Express and Azure configuration can continue to work.
## CI/CD

GitHub Actions workflow: `.github/workflows/ci-part3.yml`. It installs .NET 8, restores and builds `RaceDay.sln`, builds both Docker images, runs the existing API tests, and uploads test results. It validates image builds; publishing to Azure Container Registry still requires a registry and credentials configured in the repository's Actions secrets. Push to the repository to trigger it and add a screenshot of a genuinely green run here before final submission. Do not claim CI passed until the Actions run has completed successfully.

## Video presentation evidence checklist

1. Explain architecture: browser → MVC → Part 2 API → SQL Server; image files go MVC → API → Azure Blob Storage.
2. Show the solution structure and existing Part 2 API/tests.
3. Demonstrate registration/login and both role menus.
4. As Organiser, create/edit an event, manage categories, view enrolments and capture results.
5. As Participant, filter events, view details/categories, enter an event, show My Events, My Results and profile.
6. Demonstrate a real Azure image upload only after setting up Azure and upgrading the database.
7. Build and run the Docker container; show `docker images`, `docker ps`, `docker logs`, and use the site through `http://localhost:8080`.
8. Show the actual successful GitHub Actions run and repository commit history.

## Important verification status

This repository includes Docker build and Compose wiring. Azure storage, the SQL Server database, and any Azure Container Registry are external services and must be provisioned and configured with private environment variables before they can be used. Source files alone do not prove external credentials, database connectivity, Azure uploads, or a cloud deployment have been verified.

## CI Flow Green Tick
<img width="1917" height="1018" alt="image" src="https://github.com/user-attachments/assets/963c9f6f-fddb-498e-984e-dce22e57cddc" />






## SQL Server and EF Core migrations

The requested SQL Server instance is configured in both API settings files: `LabVM2049939\\SQLEXPRESS`. The database name remains `RaceDayDB`. See `docs/SQL_Server_and_Migrations_Guide.md` and `RaceDay.API/Migrations/README.md` before applying migrations. No `InitialCreate` migration is included because the existing Part 2 database may already contain the tables. Verify the actual database state before creating any migration so existing tables are not recreated.
