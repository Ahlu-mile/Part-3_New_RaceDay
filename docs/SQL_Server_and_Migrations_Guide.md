# SQL Server connection and EF Core migrations guide

## Configured server

The API now points to:

- Server: `localhost\SQLEXPRESS`
- Database: `RaceDayDB`
- Authentication: Windows Integrated Authentication (`Trusted_Connection=True`)

Both `RaceDay.API/appsettings.json` and `RaceDay.API/appsettings.Development.json` use the same connection string. The MVC website does not connect directly to SQL Server; it calls the API.

If your SQL Server uses a different machine or instance, override the setting for the API process with `ConnectionStrings__DefaultConnection`. For example, in PowerShell: `$env:ConnectionStrings__DefaultConnection = "Server=<server>\<instance>;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"`.

## 1. Check that SQL Server is reachable

Open PowerShell. Run:

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT @@SERVERNAME AS ServerName;"
```

If PowerShell says `sqlcmd` is not recognised, install the Microsoft SQL command-line tools or perform the checks in SSMS.

## 2. Check whether the database exists

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT name FROM sys.databases WHERE name = 'RaceDayDB';"
```

If no row is returned, the database is not present on this instance. Do not apply migrations until the correct database name and setup plan are confirmed.

## 3. Check access to RaceDayDB

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -d "RaceDayDB" -Q "SELECT DB_NAME() AS ConnectedDatabase;"
```

A successful result should show `RaceDayDB`. If login fails, check Windows authentication permissions and that the SQL Server instance is running.

## 4. Check tables without changing anything

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -d "RaceDayDB" -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME;"
```

Look for the existing Part 2 tables: `Users`, `Events`, `Categories`, `Enrolments`, `Results`, and `RouteInfo`.

## 5. Build the solution and run tests

From the directory containing `RaceDay.sln`:

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet test RaceDay.sln
```

These commands verify restore, compilation and automated tests. They do not prove that SQL login or every endpoint works.

## 6. Migration safety — read before running

The supplied ZIP did not contain migration files. The project includes a `RaceDay.API/Migrations/README.md` explaining the safe paths.

**If the database already has the six Part 1 tables:** do not run `InitialCreate`, and do not run `docs/RaceDay_Database.sql` because that full setup script drops and recreates `RaceDayDB`. First back up the database. Use `docs/Part3_Add_Image_Urls.sql` only if the two image URL columns are missing; it is an additive script.

**If the database is genuinely empty:** after confirming it is safe to create the schema, open Visual Studio Terminal at the solution root and run:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
dotnet ef migrations add InitialCreate --project RaceDay.API --startup-project RaceDay.API --output-dir Migrations
dotnet ef migrations list --project RaceDay.API --startup-project RaceDay.API
dotnet ef database update --project RaceDay.API --startup-project RaceDay.API
```

If the tool is already installed, skip the installation command. Review the generated migration before applying it. Never apply an `InitialCreate` migration to a database that already contains those tables.

## 7. Test the API

Start `RaceDay.API` using its HTTP launch profile. Open:

`http://localhost:5080/swagger`

In Swagger, execute `GET /api/events`. A `200 OK` indicates that endpoint returned successfully. A `500` requires checking the API output/exception; a SQL connection failure is usually a server, database-name, or permission issue.

## Expected architecture

Browser → RaceDay MVC → existing RaceDay API → SQL Server

The MVC project must not contain a direct SQL Server connection or its own EF Core DbContext.


