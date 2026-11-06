# RaceDay - API Endpoint Plan (Part 1)

**Student:** ST10488271
**Module:** PROG6212
**Purpose:** This document plans every endpoint the RaceDay API will expose in Part 2, before any API code is written. Roles: **None** (public), **Any** (any logged-in user), **Organiser**, **Participant**.

---

## 1. Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user as either an Organiser or a Participant. | None | `{ fullName, email, password, role, phoneNumber }` | 201 Created - user object (no password) <br> 400 Bad Request - validation failed <br> 409 Conflict - email already registered |
| POST | /api/auth/login | Authenticates a user and returns a JWT access token. | None | `{ email, password }` | 200 OK - `{ token, userId, role }` <br> 401 Unauthorized - invalid credentials |

## 2. User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the profile of the currently logged-in user. | Any | None | 200 OK - user object <br> 401 Unauthorized |
| PUT | /api/users/me | Updates the logged-in user's own profile details. | Any | `{ fullName, phoneNumber }` | 200 OK - updated user object <br> 400 Bad Request |
| GET | /api/users/{id} | Returns a specific user's public profile (e.g. organiser contact info). | Any | None | 200 OK - user object <br> 404 Not Found |

## 3. Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all upcoming events, with optional filters (type, location, date). | None | None | 200 OK - array of events |
| GET | /api/events/{id} | Returns full details for a single event, including its categories and route info. | None | None | 200 OK - event object <br> 404 Not Found |
| POST | /api/events | Creates a new event. | Organiser | `{ eventName, description, eventType, eventDate, location, startTime }` | 201 Created - event object <br> 400 Bad Request |
| PUT | /api/events/{id} | Updates an existing event owned by the logged-in organiser. | Organiser | `{ eventName, description, eventType, eventDate, location, startTime }` | 200 OK - updated event <br> 403 Forbidden - not the owner <br> 404 Not Found |
| DELETE | /api/events/{id} | Deletes an event owned by the logged-in organiser. | Organiser | None | 204 No Content <br> 403 Forbidden <br> 404 Not Found |
| GET | /api/events/{id}/route | Returns route/course info for an event (for race-day prep). | None | None | 200 OK - route object(s) <br> 404 Not Found |
| GET | /api/events/{id}/weather | Returns live weather forecast for the event location/date. | None | None | 200 OK - weather object <br> 502 Bad Gateway - external weather service unavailable |

## 4. Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Lists all categories for a specific event. | None | None | 200 OK - array of categories |
| POST | /api/events/{eventId}/categories | Adds a new category to an event. | Organiser | `{ categoryName, distanceKM, maxParticipants, entryFee }` | 201 Created - category object <br> 403 Forbidden <br> 404 Not Found |
| PUT | /api/categories/{id} | Updates a category's details. | Organiser | `{ categoryName, distanceKM, maxParticipants, entryFee }` | 200 OK - updated category <br> 403 Forbidden <br> 404 Not Found |
| DELETE | /api/categories/{id} | Removes a category from an event. | Organiser | None | 204 No Content <br> 403 Forbidden <br> 404 Not Found |

## 5. Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enters the logged-in participant into a chosen category. | Participant | `{ categoryId }` | 201 Created - enrolment object <br> 400 Bad Request - category full <br> 409 Conflict - already enrolled |
| GET | /api/enrolments/me | Lists all enrolments (past and upcoming) for the logged-in participant. | Participant | None | 200 OK - array of enrolments |
| GET | /api/events/{eventId}/enrolments | Lists all enrolments for an event, for the organiser managing it. | Organiser | None | 200 OK - array of enrolments <br> 403 Forbidden - not the owner |
| PUT | /api/enrolments/{id}/status | Updates an enrolment's status (e.g. confirm or cancel). | Organiser | `{ status }` | 200 OK - updated enrolment <br> 403 Forbidden <br> 404 Not Found |
| DELETE | /api/enrolments/{id} | Cancels the logged-in participant's own enrolment. | Participant | None | 204 No Content <br> 403 Forbidden <br> 404 Not Found |

## 6. Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/results | Captures a finish result against a participant's enrolment. | Organiser | `{ enrolmentId, finishTime, overallPosition, categoryPosition }` | 201 Created - result object <br> 400 Bad Request <br> 409 Conflict - result already captured |
| GET | /api/results/me | Returns the logged-in participant's personal performance history. | Participant | None | 200 OK - array of results |
| GET | /api/events/{eventId}/results | Returns the full results list for an event (leaderboard). | None | None | 200 OK - array of results <br> 404 Not Found |
| PUT | /api/results/{id} | Corrects a previously captured result. | Organiser | `{ finishTime, overallPosition, categoryPosition }` | 200 OK - updated result <br> 403 Forbidden <br> 404 Not Found |

---

### Notes
- All routes except `POST /api/auth/register`, `POST /api/auth/login`, and public `GET` browsing routes require a valid JWT bearer token.
- `Organiser` routes are further restricted at the API level in Part 2 so that an organiser can only modify events/categories/results they own.
- This plan will be revisited at the start of Part 2; any deviations between this plan and the implemented API will be explained in the README.
