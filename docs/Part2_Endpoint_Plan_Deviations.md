# Part 2 - Deviations from the Part 1 Endpoint Plan

The Part 1 brief requires that unexplained deviations between the approved endpoint plan and the implemented API be noted here.

## Implemented exactly as planned
All endpoints for Authentication, User Profile, Events, Categories, Event Enrolments, and Results were implemented exactly as specified in `API_Endpoint_Plan.md`, including HTTP methods, routes, role restrictions, and status codes.

## Deliberate deviations

1. **`GET /api/events/{id}/weather` was deferred.**
   The Part 1 plan included a weather endpoint that would call an external weather service. Part 2's functional requirements (as specified in the Part 2 brief) do not list weather as a minimum requirement, so this endpoint was not implemented in Part 2 to keep the API's scope focused on the graded functional requirements: Authentication, Profile, Events, Categories, Enrolments, and Results. It remains in the Part 1 plan as a candidate for Part 3 if time allows.

2. **`POST /api/auth/logout` was added.**
   This was not in the original Part 1 plan but was added in Part 2 as a natural complement to session-based login - it simply clears the session server-side. It requires no role and carries no request body.

3. **Guide-compatible route aliases and organiser event listing added.**
   The guide names `GET /api/events/mine` and `GET/PUT /api/profile/me`, while the Part 1 plan uses `/api/users/me` for the profile. Part 2 now implements `/api/events/mine` (Organiser only) and adds `/api/profile/me` aliases while retaining `/api/users/me`, preserving backward consistency with Part 1.

4. **Session-based authentication, not JWT.**
   The Part 1 plan did not commit to a specific authentication mechanism. Per the Part 2 brief's explicit requirement for "session management to maintain the user's authenticated state and role for all subsequent requests," the API uses ASP.NET Core's server-side session (`HttpContext.Session`) rather than JWT bearer tokens. The session stores `UserId` and `Role` after a successful login, and every protected endpoint reads that session via `RequireAuthAttribute` / `RequireRoleAttribute` rather than validating a token.

Event updates also validate `EventType` against `Run`, `Walk`, and `Cycle`, matching the create endpoint and SQL constraint. No other deliberate endpoint deviations are recorded. The implemented database schema (via EF Core Code-First in `ApplicationDbContext`) matches the Part 1 ERD and `RaceDay_Database.sql` exactly - same six entities, same primary/foreign keys, same constraints.
