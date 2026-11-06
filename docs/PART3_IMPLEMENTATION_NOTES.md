# PROG6212 Part 3 implementation notes

## Changes added while preserving Part 2

- Added `RaceDay.Mvc` and `RaceDay.Contracts` to the existing `RaceDay.sln`; existing `RaceDay.API` and `RaceDay.Tests` project files were retained.
- Added API-backed MVC pages for public events/search/details, login/register/logout, organiser event/category/enrolment/result management, participant event entry, My Events, My Results and profile updates.
- Added an API client that forwards the Part 2 `.AspNetCore.Session` cookie from login to subsequent protected API requests.
- Added `MediaController` upload endpoints for event banners and profile pictures. Uploads validate image MIME type and a 5 MB limit, use a private Azure Blob container, save an API media URL on the relevant database row, and stream media through the API.
- Added nullable `BannerImageUrl` and `ProfileImageUrl` model/DTO properties and the safe additive SQL script `Part3_Add_Image_Urls.sql`.
- Added a multi-stage .NET 8 Dockerfile, `.dockerignore`, a Part 3 GitHub Actions workflow, and Part 3 README/run instructions.

## Required local verification before submission

This workspace did not have the .NET SDK or Docker CLI installed. Therefore no claim is made that `dotnet build`, the existing tests, the Azure upload, GitHub Actions, or the Docker image has been successfully run. Verify each on the submission computer.

1. Back up the correct RaceDay database and run `Part3_Add_Image_Urls.sql` against it.
2. Start the API HTTP profile and verify Swagger before running MVC.
3. Run `dotnet restore RaceDay.sln`, `dotnet build RaceDay.sln -c Release`, and `dotnet test RaceDay.Tests/RaceDay.Tests.csproj -c Release`.
4. Set Azure Storage environment variables on the API process and test both upload types.
5. Build/run Docker using the README instructions and verify an authenticated page while MVC is containerised.
6. Push to GitHub, confirm the latest `RaceDay Part 3 CI` workflow is green, and capture real evidence.

## Known limitation / honest status

The Azure account has not yet been created, so image uploads cannot succeed until the user provisions storage and configures its connection string. The SQL upgrade is additive and does not create or drop the database. No credentials are included in this source tree.
