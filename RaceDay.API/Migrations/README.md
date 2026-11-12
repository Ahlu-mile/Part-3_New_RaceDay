# EF Core migrations — important setup note

This project ZIP did not contain EF Core migration files. A correct migration must be scaffolded against the
actual `ApplicationDbContext` and checked against the database schema on the machine that will run the project.
A fabricated migration or an unconditional `InitialCreate` could try to recreate tables already present in
`RaceDayDB` and damage or conflict with the existing Part 2 database.

After confirming that `localhost\SQLEXPRESS` is reachable and identifying whether `RaceDayDB` already
contains the six Part 1 tables, use the appropriate path:

## If RaceDayDB already contains the Part 1 tables

Do not run `InitialCreate` or execute the destructive full database creation script. First back up the database.
The Part 3 additive script `docs/Part3_Add_Image_Urls.sql` adds the optional image URL columns safely.
For future EF migration tracking, baseline the existing schema only after comparing the live database with
`ApplicationDbContext`; this is a deliberate database-specific step and should not be guessed in a ZIP.

## If RaceDayDB is genuinely empty / newly created

From the solution root in Visual Studio Terminal:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
dotnet ef migrations add InitialCreate --project RaceDay.API --startup-project RaceDay.API --output-dir Migrations
dotnet ef database update --project RaceDay.API --startup-project RaceDay.API
```

If `dotnet-ef` is already installed, skip the install command. Confirm the generated migration before
applying it. Do not run these commands against a database containing existing tables unless you have
deliberately baselined and verified the schema.

## Verify migration status

```powershell
dotnet ef migrations list --project RaceDay.API --startup-project RaceDay.API
```

This folder is intentionally accompanied by this note rather than an unverified migration file. The project
must be scaffolded against the actual database state before a safe migration can be supplied.

