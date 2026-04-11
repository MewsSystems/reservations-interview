# Nikita Lebedenko

This PR covers the three interview tasks and delivers the required reservation functionality across backend, tests,
and a minimal frontend flow. It includes guest booking, reservation conflict validation, and staff access to view
current and upcoming reservations.

## Task Status

RE-001: Completed - implemented guest booking flow
RE-002: Completed - implemented reservation overlap validation
RE-003: Completed - implemented staff access-code login and staff reservation list

## Task Report

The commit history for this branch reflects the delivery order of the work:

- `0369309` RE-001 guest booking flow
- `84ed5eb` RE-002 reservation overlap validation
- `83a4a98` RE-003 staff reservation access flow

The first task took 1 hour and 40 minutes. The other two tasks together took 1 hour and 20 minutes.

Most of the time was spent building the backend behavior and covering it with tests. The frontend
was kept intentionally small and pragmatic so the main focus stayed on the backend interview tasks
and API behavior.

## Notes

I tried to keep the implementation pragmatic and aligned with the existing structure, with the main focus on
delivering the required behavior within the interview time limit. If I had more time, I would spend it on improving
the backend architecture, tightening some of the API boundaries, and doing a bit more cleanup and refinement around
the overall design.
