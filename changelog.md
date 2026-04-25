# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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