# RaceDay — PROG6212 Part 2 and Part 3

RaceDay is an event-management system for running, walking and cycling events. Part 2 supplies the ASP.NET Core REST API and automated tests. Part 3 adds an ASP.NET Core MVC website which calls that API; the website does **not** connect directly to SQL Server.

## Solution structure

- `RaceDay.API/` — existing Part 2 API, EF Core models, session-based authentication, role checks and endpoints.
- `RaceDay.Tests/` — existing Part 2 automated tests.
- `RaceDay.Contracts/` — shared response contract types for API/MVC integration.
- `RaceDay.Mvc/` — Part 3 website, controllers, Razor views, API client and Dockerfile.
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
2. Create a private Blob container, for example `raceday-media`. Do not enable anonymous public access just to make the demo work.
3. Obtain the storage connection string from the account's access keys. Keep it private.
4. Set it as an environment variable for the API process, not in a public commit. In PowerShell for the current terminal:

```powershell
$env:AzureStorage__ConnectionString = "<your-storage-connection-string>"
$env:AzureStorage__ContainerName = "raceday-media"
dotnet run --project RaceDay.API --launch-profile http
```

5. Run the SQL upgrade above before uploading images.
6. In the MVC website, edit an event and choose a banner, or open Profile and choose a profile picture. A successful upload response is saved to the database and the URL is displayed in the UI.

**Security note:** the container remains private. The database stores an API media URL, and the API streams the image from Blob Storage, so the browser does not need public blob access or an Azure account key. Do not expose account keys or commit connection strings.

## Docker

Build from the solution root (the directory containing `RaceDay.sln`):

```powershell
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .
```

Start the Part 2 API on the Windows host at `http://localhost:5080`, then start the MVC container in a second terminal:

```powershell
docker rm -f raceday-mvc 2>$null
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5080/ raceday-mvc:part3
docker ps
docker logs raceday-mvc
```

Open `http://localhost:8080`. `host.docker.internal` lets the container reach the API on the host machine. The API must be running and reachable for data pages to work. To pass Azure configuration to a containerised API, configure the same environment variables on the API process; do not put storage secrets in the MVC container.

## CI/CD

GitHub Actions workflow: `.github/workflows/ci-part3.yml`. It installs .NET 8, restores `RaceDay.sln`, builds the full solution (including MVC), runs the existing API tests, and uploads test results. Push to the repository to trigger it and add a screenshot of a genuinely green run here before final submission. Do not claim CI passed until the Actions run has completed successfully.

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

The Part 2 files have been preserved and Part 3 MVC/API source files have been added. This development environment did not have the .NET SDK available, so a local `dotnet build`, test run, Azure upload, or Docker build could not be executed here. Run those checks on your machine and fix any compiler/runtime issues before submitting. A source ZIP is not evidence that the application or container has passed those checks.


## SQL Server and EF Core migrations

The requested SQL Server instance is configured in both API settings files: `LabVM2049939\\SQLEXPRESS`. The database name remains `RaceDayDB`. See `docs/SQL_Server_and_Migrations_Guide.md` and `RaceDay.API/Migrations/README.md` before applying migrations. This ZIP does not invent an `InitialCreate` migration because the supplied Part 2 database may already contain the six tables; first verify the actual database state to avoid attempting to recreate existing tables.
