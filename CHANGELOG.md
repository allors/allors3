# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(`3.1.0-alpha.{height}`), so the version auto-increments with each commit on `main`.
Changes accumulate under **[Unreleased]** until a version is cut, at which point they move
under a dated version heading.

## [Unreleased]

<!-- Add entries under one of: Added, Changed, Deprecated, Removed, Fixed, Security -->

### Fixed

- The in-memory adapter's change set now reports the association that is displaced when a one-to-one
  composite role is reassigned. `SetCompositeRoleOne2One` recorded the displaced association's original
  role as the wrong value, so when the new association had no prior role its role change (now → null) was
  trimmed out and omitted from the change set.
- The in-memory adapter's `LIKE` filter now follows SQL `LIKE` semantics: `%` (any sequence) and `_`
  (any single character) are wildcards and every other character — including regex metacharacters such
  as `.`, `(`, `[`, `\` — is matched literally. Previously the pattern was compiled to a regex without
  escaping, so metacharacters were misinterpreted and `_` was treated literally.
- `DefaultStructRanges.Union` no longer drops a leading element equal to `default(T)` (e.g. `0`).
  Both merge branches relied on a sentinel that never fired (`Equals(previous, default)` resolves
  `default` to `null`, so it is always false for a value type); they now use a nullable `T? previous`
  sentinel (`previous == null`), matching the `DefaultClassRanges` sibling. Object-id ranges never contain `0`
  (`0` denotes null), so this is a generic data-structure correctness/consistency fix.
- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
- SqlClient adapter tests now run with a 300s command timeout and `Connection Timeout=0` against
  SQL Server LocalDB, matching the Npgsql adapter tests. This stops sporadic CI failures
  (`SqlException: Execution Timeout Expired`) caused by LocalDB slowness on hosted runners.
