# Part 2 Compliance Audit and Change Log

**Project:** RaceDay / Marathon Part 2  
**Student number:** ST10488271  
**Module:** PROG6212  
**Audit basis:** `PROG6212_Part_2_Detailed_Student_Guide.pdf` and the two Part 2 rubric screenshots supplied inside the original ZIP.

## Summary

The source ZIP contains a substantial ASP.NET Core .NET 8 API, SQL Server/EF Core configuration, session-based authentication, role filters, controllers for users/events/categories/enrolments/results, Swagger setup, a GitHub Actions workflow, and 15 xUnit tests. I made focused source/documentation updates and retained the original rubric screenshots.

**Important verification limit:** this environment does not have the `dotnet` CLI installed, so I could not compile the solution, run the tests, start SQL Server, verify Swagger live, or confirm the GitHub Actions run. Static inspection is not a substitute for those checks. The ZIP does not contain a `.git` directory, so commit count/history cannot be verified or changed here.

## Changes made in this update

1. **Added the guide's `GET /api/events/mine` endpoint** for an authenticated Organiser to list only their own events. It uses the session user ID and filters by `OrganiserId`.
2. **Added profile route aliases** `GET /api/profile/me` and `PUT /api/profile/me`, while preserving the Part 1-planned `GET/PUT /api/users/me` routes. This supports the route names in the student guide without removing the original plan's routes.
3. **Added event-type validation to event updates.** `PUT /api/events/{id}` now rejects values other than `Run`, `Walk`, or `Cycle`, consistent with create validation and the database constraint.
4. **Enabled XML documentation generation** for the API project and configured Swagger to include the generated XML comments when present, so endpoint summaries can appear in Swagger.
5. **Added this audit/change log** to make the remaining rubric evidence gaps visible instead of claiming they have been completed.
6. **Retained both original rubric screenshots** from the supplied ZIP.

## Requirement-by-requirement status

Status meanings: **Implemented in source** = found by static inspection; **Partial / verify** = some source exists but the full rubric criterion needs a live run or additional evidence; **Outstanding** = not present in the ZIP or cannot be honestly completed without the user's repository/account/recording.

| Guide / rubric criterion | Status | Notes |
|---|---|---|
| Clear project structure and README | Partial / verify | Solution, API, tests, docs, workflow and README exist. README still has placeholders for the real green-build screenshot and unlisted video link. |
| .NET 8 API project | Implemented in source | `RaceDay.API` targets `net8.0`. Build still needs to be run locally/CI. |
| SQL Server and EF Core Code First | Implemented in source / verify | SQL Server provider and `ApplicationDbContext` are present. No EF migration files were included in the ZIP; create/apply a migration and verify the database locally. |
| Part 1 ERD and SQL retained | Implemented in source | ERD PDF/PNG, SQL script, endpoint plan and deviation notes are included. |
| Registration and login | Implemented in source / verify | Auth controller exists; duplicate email and invalid credentials are handled. Live API tests remain necessary. |
| Password hashing | Implemented in source | PBKDF2-SHA256 with per-password random salt; plaintext passwords are not stored. |
| Session management | Implemented in source / verify | Login writes UserId and Role into session; logout clears session. Cookie/session behavior needs live verification. |
| Role enforcement and ownership | Implemented in source / verify | Role filters and ownership checks exist for event, category, enrolment and result actions. Negative-role scenarios must pass in local tests. |
| Profiles | Implemented in source | Own-profile GET/PUT exists. This update also adds the guide's `/api/profile/me` aliases. |
| Events | Implemented in source | Browse, detail, route, create, update and delete exist; this update adds organiser-owned `/api/events/mine`. The planned external weather endpoint remains deferred and is documented. |
| Categories | Implemented in source / verify | Read/create/update/delete routes exist with organiser ownership checks. |
| Enrolments | Implemented in source / verify | Participant enrolment, own enrolments, organiser event enrolments, status update and participant cancellation exist. |
| Results | Implemented in source / verify | Create, participant's own results, public event results and owner-only correction exist. |
| Correct HTTP status codes | Partial / verify | Appropriate 201/400/401/403/404/409/204 responses appear in source; test each failure and success path. |
| Swagger | Partial / verify | Swagger is configured for Development. XML comment output is now enabled, but Swagger must be launched and every endpoint checked in a browser. |
| Unit tests | Partial / verify | 15 xUnit tests are present across authentication, events and enrolments. Tests cover several success/failure paths, but the full suite could not be run here and broader category/result/profile/role cases may be needed for the highest band. |
| GitHub Actions CI/CD | Partial / verify | Workflow restores, builds, tests and uploads test results. It must run successfully in the actual GitHub repository. |
| At least 20 meaningful commits | Outstanding / cannot verify | The supplied ZIP has no `.git` history. Do not manufacture commits; submit the real repository history. |
| Green CI screenshot in README | Outstanding | README placeholder remains until a real successful GitHub Actions run is captured and inserted. |
| Video presentation (15 marks) | Outstanding | README placeholder remains until the real unlisted YouTube video is recorded, uploaded and linked. |
| GitHub repository link / ARC submission | Outstanding | Must be completed by the student through the assigned repository and ARC. |

## Rubric-aligned next steps before submission

1. Open `RaceDay.sln` in Visual Studio 2022 and build the solution.
2. Run `dotnet test RaceDay.sln` and fix every failure.
3. Create/apply the EF Core migration and confirm the SQL Server database/tables match the Part 1 ERD and SQL script.
4. Run the API in Development and verify all routes in Swagger, including unauthenticated `401`, wrong-role `403`, organiser ownership rejection, and successful workflows.
5. Push meaningful work to the assigned GitHub repository until the real history shows at least 20 meaningful Part 2 commits.
6. Confirm the GitHub Actions workflow is green; add a genuine screenshot to the README.
7. Record the guide's video walkthrough, upload it as **Unlisted**, and replace the README placeholder with the real link.
8. Submit the GitHub repository link on ARC as instructed by the guide.

## Rubric screenshots

The two screenshots from the supplied ZIP are retained at the project root:
- `Screenshot 2026-10-12 082651.png` — Correct Submission, GitHub and CI/CD, Authentication and Role-Based Access.
- `Screenshot 2026-10-12 082703.png` — API Endpoints and Database, Swagger, Unit Testing, Video Presentation, and GitHub penalty note.
