# FOUNDATION_DEBT_LEGACY_TEST_DATABASE

## Recorded during Task 7C-S

- Offending legacy test: `Backend.ApiTests.DeduplicateKhoaHocsTest.DeduplicateAllKhoaHocs`.
- Root cause: the test embedded a connection string targeting `LMS` and directly removed duplicate `KhoaHoc` records before `SaveChangesAsync`.
- Safety remediation: all 36 literal `Database=LMS` test connections were replaced with the shared fail-closed `TestDatabaseSafetyGuard`. The guard accepts only `LMS_TEST_CONNECTION_STRING`, connects, and verifies `SELECT DB_NAME()` begins with `LMS_TEST_`.
- Fallback remediation: direct EF test helpers no longer fall back to `ConnectionStrings__DefaultConnection` or application settings.
- Legacy assertions and business behavior were not changed.
- Full legacy-suite execution is deferred to a project-wide stabilization phase. It must not resume until every mutating test has been audited for the shared guard.

No credentials, connection strings, or tokens are recorded in this document.
