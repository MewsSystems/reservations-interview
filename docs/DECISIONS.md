# DECISIONS

This file records the main technical and product decisions for the reservations app, plus known follow-ups and production hardening tasks.

---

## Initial polish

- Scoped DI for SQL connections and repositories is used, 
- DB seeding is awaited on startup.
- and a simple global exception-handling middleware is run.

Follow-ups:
- CORS will be restricted to the UI origin before production (no `AllowAnyOrigin`)
- exception-based control flow may be replaced with result types
- API namespaces/controllers will be cleaned up (e.g., `Api.*`, plural controller names)

---

## RE-001 Guest booking

- Guest booking is implemented end-to-end: server-side validation (dates, email, room), room existence check, guest upsert, and reservation insert, with consistent `{ errors: [...] }` 400 responses.
- The UI calls the API via `ky.post`, parses responses with Zod, and surfaces validation errors via a shared `ErrorToast`.
- DB Schema fixes are in place: `Start`/`End` stored as text dates, `Guest.Surname` added, dates sent as `YYYY-MM-DD` (local) to avoid UTC drift, past dates checked against server-local `DateTime.Today`.

Follow-ups:

- FluentValidation may be migrated to for declarative, testable validation.

---

## RE-002 Prevent double bookings

- Double-booking prevention is implemented inside a single DB transaction in `CreateReservation`, wrapping the overlap check and insert to avoid TOCTOU issues.
- Overlaps are detected via `SELECT EXISTS` with `[Start] < @End AND [End] > @Start`, allowing same-day checkout/check-in, and conflicts return `409 Conflict`.

---

## Auth framework refactor

- Manual cookie handling has been replaced with ASP.NET Core cookie authentication (`AddAuthentication().AddCookie()`, `SignInAsync`/`SignOutAsync` with `ClaimsPrincipal`).
- Login is `POST /staff/login` (401 on wrong codes); logout is POST; cookies are HttpOnly with `Secure` in production only and 30-minute sliding expiration.
- `[AllowAnonymous]` is applied to public GETs and guest booking; `[Authorize]` is used for staff-only actions (room create/delete, reservation delete, guest list).

Follow-ups:

- Rate limiting will be added for `POST /staff/login` to mitigate brute force.
- `staffAccessCode` will be moved from `appsettings.json` into vaults or a secrets manager.
- A proper `Staff` user model (per-user credentials, claims, audit logging) will be added long-term.

---

## RE-003 Staff login and dashboard

- A single paginated endpoint `GET /reservation?from=&page=&pageSize=` (authorized) returns upcoming reservations with optional `from` filtering and clamped `page`/`pageSize` (1–100). DB indexes added for performance.
- Pagination metadata is exposed via headers (`X-Total-Count`, `X-Page`, `X-Page-Size`); CORS exposes these to the UI. 
- Staff UI (`staff/`) includes `AuthContext` for state management, login page with auto-redirects, dashboard with paginated table, logout button in layout, and reservations hook reading headers; landing page links to `/staff/login`; 401 triggers server-side logout.

Follow-ups:

- Cursor-based pagination will be considered for better scaling.
- Some count and data queries will be combined using for better performance.

---

## RE-004 Check-in flow

- Check-in with Email confirmation is a two-step staff-only flow: `POST /reservation/{id}/checkin` generates a 6-char code; `PUT` validates it, sets `CheckedIn = true` and room `State = Occupied`.
- `VerificationCodeService` is an in-memory TTL store (10 min); check-in is transactional with atomic guards; validation requires today’s start date; `GET /reservation` supports `to` param for “today only”.
- UI includes “Today only” filter, Check-In button/dialog/toasts, and table refresh.

Follow-ups:

- Verification codes will be moved to durable storage (Redis/DB TTL).
- Email integration (SendGrid/SES) will deliver codes to guests.
- Rate limiting will be added to `PUT /checkin` for brute-force protection.

---

## RE-005 Room CSV import

- `POST /room/import` parses CSV (`Number,State,IsDirty`), validates rows, checks duplicates, and batch-inserts in a transaction with configurable limits (`IOptions`).
- Response includes `{ imported, errors: [{ row, message }] }`; `CancellationToken` is supported.
- UI provides drag-and-drop dialog, size warnings, error list, and paginated rooms table.

Follow-ups:

- Binary sniffing will harden file type checks.
- UI limits will be fetched from server config.
- Undo/rollback will be considered for imports.

---

## RE-006 Housekeeping and DB migrations

- DB migrations use `PRAGMA user_version` (V1 tables, V2 indexes, V3 `IsDirty` column); `IsDirty` boolean replaces old enum.
- `PATCH /room/{roomNumber}` supports RFC 6902 JSON Patch (whitelisted paths); check-in sets `IsDirty = 1` transactionally.
- Staff dashboard shows room badges/toggles; guest UI shows “Dirty” badge; errors normalized via `handleApiError`.

Follow-ups:

- A dedicated Housekeeping page with filtering (dirty-only, floors) will be added.
- Audit trail for cleanliness changes will be introduced.

---

## Cross-cutting and operational concerns

- DTOs will be extracted to a folder; down-migration added.
- CORS will be locked to frontend origins before production.
- Global request validation middleware will unify invalid-body responses to `{ errors: [...] }`.
- A health check endpoint will be added for load balancers.
- SQLite will be migrated to PostgreSQL for production concurrency.