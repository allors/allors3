# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(`3.1.0-alpha.{height}`), so the version auto-increments with each commit on `main`.
Changes accumulate under **[Unreleased]** until a version is cut, at which point they move
under a dated version heading.

## [Unreleased]

<!-- Add entries under one of: Added, Changed, Deprecated, Removed, Fixed, Security -->

### Added

- `MediaContent` is now an interface with two strategy implementations: `EmbeddedMediaContent`
  (bytes in the database, the previous behaviour) and `ExternalMediaContent` (bytes in external
  storage; the filesystem backend names them by the object id).
- `IMediaContentFactory` service selects which implementation new media use; its build strategy is a
  lambda configured in code at startup (`DefaultDatabaseServices`, defaults to embedded), shared by
  every host so the server and command-line processes agree.
- `IMediaContentStorage` service (filesystem implementation `FileMediaContentStorage`) for
  reading/writing/deleting file-backed content and for enumerating and sizing it (`Enumerate`,
  `Length`). External (file-backed) media storage is **opt-in and optional**: it is configured by the
  `Media:Directory` setting, and `Get<IMediaContentStorage>()` returns `null` when that setting is unset,
  so embedded-only installs need no media directory. There is no fallback; when configured, the directory
  is never created automatically and **must already exist** — a missing directory fails hard at construction.
- `MediaContent.HasData`, a cheap presence/non-empty probe (no full read for external storage), used
  by validation and the media controller.
- `ExternalizeMedia` command (in `Base` Commands, shared into `Apps`) converts every `Media`'s
  `EmbeddedMediaContent` to an `ExternalMediaContent` (moving bytes from the database to external
  storage). Whether new media is stored externally is a separate, code-configured choice
  (`IMediaContentFactory`). Idempotent; backed by `Medias.ConvertEmbeddedMediaContentToExternal`.

### Changed

- Media content is now **write-once**: changing a `Media`'s data builds a fresh `MediaContent` and
  cascade-deletes the previous one instead of mutating it in place. `ExternalMediaContent.Data`
  enforces this — reassigning it on an already-persisted content throws (an overwrite is not
  rollback-safe). An empty data update is rejected as a validation error without destroying the
  existing content.
- Orphaned file-backed media (left by a rolled-back write or a deferred delete) is reclaimed
  automatically during a Load/Upgrade via `ExternalMediaContents.ReconcileFiles` (invoked from
  `Upgrade.Execute()`), where the load process is the only connection so a file with no live owner is
  always a true orphan. This replaces the earlier ceiling-based `PruneMediaFiles` command (removed),
  which could not reclaim the highest-id orphans and was unsafe to run against a live database.
- File-backed media now **fails fast on a lost file**: `FileMediaContentStorage.Read` throws when an
  `ExternalMediaContent`'s backing file is missing, instead of returning `null` bytes, so a
  data-integrity loss surfaces at the read site (e.g. printing) rather than degrading silently. The
  presence probes (`Exists`/`Length`/`HasData`) stay tolerant. The HTTP media/image endpoints (`BaseMediaController`,
  `BaseImageController`) now return `500` for a live `MediaContent` whose bytes are missing/empty (a
  lost external file); only genuinely-absent content (no `MediaContent`) returns `204`, and the error
  is `no-store` so it is never cached.
- The SqlClient adapter's `LIKE` filter now follows ANSI semantics like the Npgsql and in-memory adapters:
  `%` and `_` are the only wildcards and `[` is matched literally. Previously T-SQL character classes
  (e.g. `[abc]`, `[a-z]`, `[^…]`) were active only on SqlClient, so the same `LIKE` pattern could match
  differently across adapters. Patterns that relied on SqlClient char-classes no longer match as classes
  (that behaviour was never portable to Npgsql/Memory).

### Migration

- `EmbeddedMediaContent` reuses the former `MediaContent` class id, so existing rows are re-typed
  to `EmbeddedMediaContent` automatically by the standard save/load upgrade — no data migration
  code is required. Because the table is renamed (`mediacontent` → `embeddedmediacontent`), run the
  Upgrade (save then load) rather than opening an old database in place.

### Fixed

- File-backed media (`ExternalMediaContent`) no longer unlinks its file during derivation, which was
  not rollback-safe: a rolled-back delete (or content replacement) permanently lost the file even
  though the database object was restored. Deletion is now deferred and reclaimed at the next
  Load/Upgrade.
- All hosts now resolve the same external-media directory. The `Apps` server and command-line tools
  and the Blazor host did not read `Media:Directory` (they fell back to a stray, working-directory
  relative `media/` folder); they now resolve it through the shared service registration like the
  `Base` server. Set an absolute `Media:Directory` in production for hosts that share a database.
- The media controller now returns `204 No Content` (not `304 Not Modified`) for a conditional
  request when an external file is missing, so a client is not told its stale cached copy is valid.
- Print and document-template rendering no longer throw a `NullReferenceException` when no logo/photo
  is configured — the image is simply omitted. (A configured media whose external file is missing now
  fails fast at the read path; see Changed.)
- The SQL adapters' `CreateObjects` stored procedure now holds the generated object id in a `bigint` variable
  instead of `INT`/`integer`, so creating objects no longer overflows once ids pass `int.MaxValue` (≈2.1 billion):
  SqlClient's `@IDS` table variable and Npgsql's `ID` variable. (The unused `@O INT` declaration on SqlClient was
  also removed.)
- The SQL adapters' shared `Database` caches (`concreteClassesByObjectType`, `sortedUnitRolesByObjectType`) are
  now `ConcurrentDictionary`s populated via `GetOrAdd`, so concurrent transactions no longer race while lazily
  computing concrete classes / sorted unit roles — an unsynchronized `Dictionary` write could previously corrupt
  the cache or throw. The cached values are immutable once built.
- The SQL adapters' serialization `Load` now reads the staging objects table using the configured schema
  (`{SchemaName}._o`) instead of a hardcoded `allors._o`, so loading into a database configured with a
  non-default `SchemaName` no longer fails with "Invalid object name 'allors._o'". Fixes the SqlClient and
  Npgsql adapters.
- `Extent.CopyTo(array, index)` for a converted extent (the `IObject[] → Allors.Database.Extent` cast) now
  begins copying at the requested destination `index` instead of always at `0`. The override hardcoded `0` as
  the `Array.Copy` destination, ignoring its `index` parameter and overwriting earlier elements of the target.
- Prefetching (SQL adapters) now uses an object's modified (uncommitted) composites role.
  `PrefetchTryGetCompositesRole` set its out-parameter from the modified role but always returned `false`, so
  the prefetcher ignored it and prefetched the committed value instead — leaving targets added in the
  transaction out of the transitive prefetch (extra database round-trips).
- `Instantiate(IEnumerable<long>)` (SQL adapters) no longer throws `ArgumentException` ("An item with the
  same key has already been added") when the ids contain a duplicate of an already-cached object alongside
  an uncached id; the per-id reference lookup now tolerates duplicate keys.
- The SQL adapters no longer emit invalid SQL for a `NOT (association ContainedIn enumerable)` filter on a
  many-to-many association (or a relation without exclusive database classes): that branch opened three
  parentheses but closed only two, causing a syntax error (`Incorrect syntax near ')'`) at query execution.
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
- The CI `Upload TRX artifacts` step (test-result diagnostics) now runs only on failure and is
  non-gating (`continue-on-error`), so a transient GitHub artifact-service flake on the success path
  no longer fails the whole job. The test report is still published on every run — the reporter reads
  the `.trx` from disk, not from the uploaded artifact.
