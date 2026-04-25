# Changelog

All notable changes to this project will be documented in this file.

## [2026-04-25]

### Added
- **Staff Authentication System:** Implemented a secure authentication flow using ASP.NET Core Identity/Cookie middleware, replacing basic `HttpOnly` flags with signed, encrypted authentication tickets.
- **Staff Dashboard (RE-003):** Created a protected frontend view that displays upcoming reservations, including guest email visibility as required for staff operations.
- **One-Click Check-In (RE-004):** Added a "Check In" feature on the staff dashboard that triggers an atomic database transaction to update guest status and room state.
- **Room Status Management (RE-006):** Implemented logic to automatically mark a room as "Dirty" upon guest check-in.
- **Email Confirmation Workflow:** Added a mandatory email verification prompt during the check-in process to ensure data accuracy.
- **Check-In Service Layer:** Introduced `CheckInService` to handle domain logic, decoupling business rules from the `StaffController` for better maintainability.

### Changed
- **Frontend Routing:** Integrated `useNavigate` within the `LandingPage` to provide a seamless transition between the guest landing area and the staff dashboard.
- **Error Feedback:** Enhanced error handling to display specific server-side validation messages (e.g., "Room is dirty") via existing toast components.
- **Test Coverage:** Added unit tests for the `StaffController` and `CheckInService` using Moq to verify authentication logic and transaction integrity.

### Fixed
- **Dapper Type Mapping:** Resolved a `System.InvalidCastException` by implementing custom `TypeHandler`s for `Guid` and `Boolean` types to bridge the gap between SQLite storage formats and C# models.
- **Security Vulnerabilities:** Patched a "TODO" regarding weak authentication by enforcing the `[Authorize]` attribute on staff endpoints and ensuring proper middleware ordering in `Program.cs`.
- **Casing Mismatches:** Aligned Zod frontend schemas with backend JSON serialization to correctly parse camelCase properties returned by the API.
- **Connection Lifecycle:** Fixed a bug where the singleton database connection was being prematurely disposed of during transactions, causing subsequent query failures.

## [Previous]

### Added
- **Guest Booking Validation (RE-001):**
  - Implemented requirement for Start Date to be before End Date.
  - Enforced a minimum booking duration of 1 day and a maximum of 30 days.
  - Added email validation requiring the presence of a domain.
  - Integrated 3-digit room number validation ("###") ensuring floor levels 0–9 and prohibiting door number "00".
  - Added check to ensure room numbers correspond to existing rooms.
- **Conflict Prevention (RE-002):**
  - Developed a conflict detection system to prevent double bookings for the same room.
  - Implemented overlap logic that identifies conflicts if any part of a reservation's duration overlaps with an existing booking.
- **Infrastructure & Testing:**
  - Extracted interfaces (`IReservationRepository`, `IRoomRepository`, `IGuestRepository`, `IReservationValidator`) to support SOLID principles and mockability.
  - Added an xUnit test project (`api.tests`) featuring unit tests for the controller and validator layers.
  - Implemented repository integration tests using an in-memory SQLite provider.
  - Switched repository service lifetimes to **Scoped** in `Program.cs` to ensure thread safety and proper connection disposal.

### Changed
- Updated the frontend `BookingForm` to include email domain requirement hints.

### Fixed
- Configured `.gitignore` to prevent tracking of build artifacts (`.dll`, `.exe`, `.pdb`).