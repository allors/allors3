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

- A `Merge.Tests` project for the resource `Merger` (`Core/Database/Merge`), driven black-box through its
  public `Input`/`Output` API. It covers structure preservation for overlapping keys (the regression that
  motivated the resx-merge fix — `<value>`/`<comment>` children and `xml:space`/`type` attributes survive),
  last-writer-wins precedence across input directories, key union, `<xsd:schema>`/`<resheader>` survival, and
  `Input` robustness (missing directories skipped, non-`.resx` files ignored, case-insensitive extension). The
  project is wired into the `DotnetCoreDatabaseTest` target (CI `CiDotnetCoreDatabaseTest`) so it actually gates.

### Changed

- The SqlClient adapter's `LIKE` filter now follows ANSI semantics like the Npgsql and in-memory adapters:
  `%` and `_` are the only wildcards and `[` is matched literally. Previously T-SQL character classes
  (e.g. `[abc]`, `[a-z]`, `[^…]`) were active only on SqlClient, so the same `LIKE` pattern could match
  differently across adapters. Patterns that relied on SqlClient char-classes no longer match as classes
  (that behaviour was never portable to Npgsql/Memory).

### Fixed

- The Json API no longer auto-retries `Invoke` and `Push` on a `DbException`. Both are non-idempotent writes,
  but `PolicyService` retried them with the same policy as the idempotent `Pull`/`Sync` reads — so a
  `DbException` surfacing after the write's commit (e.g. an ambiguous / lost-ack commit, or post-commit work)
  made Polly re-run the controller delegate and re-apply the already-committed invocations/push (double
  execution). `Invoke`/`Push` now run once; `Pull`/`Sync` still auto-retry. Clients that need to retry a
  failed write must do so explicitly.
- The Json `Token` (login) endpoint now counts a failed attempt toward Identity lockout
  (`CheckPasswordSignInAsync(…, lockoutOnFailure: true)`); it previously passed `false`, so a wrong password
  never incremented the lockout counter and an account could be brute-forced indefinitely. With the default
  Identity options (5 attempts / 5-minute lockout) and the lockout-aware `AllorsUserStore`, repeated failures
  now lock the account. (Security.)
- The Json API's `Pull` no longer crashes (`NullReferenceException` → HTTP 500) when a request dependency
  carries an unknown or wrong-kind meta tag. `Api.ToDependencies` cast each client-supplied tag
  (`FindByTag(...)`) to `IComposite`/`IRelationType` and dereferenced it unchecked, so a bogus `o`/`a`/`r`
  tag null-crashed the whole pull. Unresolvable dependencies — which are only prefetch hints — are now
  skipped (and logged as a warning, since they indicate a faulty client), so the pull proceeds normally.
- The workspace UML diagram template (`Workspace/Templates/uml.cs.stg`) now renders a many-valued role as
  an array type (`ElementType[]`), matching the database diagram template; its many-valued branch previously
  emitted the element type without the `[]`, so a collection role looked like a single-valued one.
- `commands.sh` now forwards its arguments with `"$@"` instead of the unquoted `$*`, so an argument
  containing spaces (or shell glob characters) reaches `Database/Commands` as a single token instead of
  being word-split/globbed. Previously e.g. a file path with a space was split into several arguments.
- The resource `Merger` no longer corrupts a resx `<data>` entry when the same key appears in more than one
  input directory. For an existing key it ran `data.Value = mergeData.Value`, whose setter replaces the
  entry's child elements (the `<value>`, and any `<comment>`) with a single raw-text node — emitting
  `<data name="…">text</data>` instead of `<data name="…"><value>text</value></data>`, which is not valid
  resx. It now replaces the whole element with a clone of the incoming one (matching the new-key branch),
  preserving the `<value>`/`<comment>` children and the incoming attributes.
- `PrefetchPolicyBuilder.WithNodes` now nests a tree node's child prefetch rules under their parent role
  instead of flattening them onto the root policy. The `WithNode` helper created a nested builder for the
  child nodes but recursed on the outer builder (`@this`), so the nested policy was always empty (deeper
  tree levels were never prefetched) and the child rules — and their security rules — leaked onto the
  outer policy. It now recurses on the nested builder.
- The remote workspace adapter's `IPullResult.GetValue<T>` now converts a pulled value to `T` instead of
  hard-casting the raw deserialized JSON. `PullResult.Values` exposes values as received over the wire (a
  `JsonElement` for System.Text.Json, a boxed/`JToken` value for Newtonsoft), so `GetValue<T>` threw
  `InvalidCastException` (e.g. casting `JsonElement` to `int`/`byte[]`). It now routes the value through the
  adapter's `IUnitConvert`, mapping the requested CLR type to its unit tag, so values round-trip to the correct
  type. (Pull values are sent untagged, so the conversion is keyed by `T`.)
- The workspace adapters' session-origin `SetCompositeRoleMany2One` no longer leaves a stale inverse when an
  object's many-to-one composite role is reassigned. When changing `A`'s role from `PR` to `R` it detached the
  *new* role `R` (a no-op, since `A` was not yet associated with `R`) instead of the *previous* role `PR`, so
  `PR`'s inverse association still listed `A` while `A`'s role was already `R`. It now detaches the previous
  role, matching the one-to-one sibling. Affects the Local and Remote workspace adapters.
- The workspace `ContainedIn` predicate with an explicit object list now round-trips over the JSON protocol.
  Both `ToJsonVisitor`s (the workspace and the database one) serialized the objects to the `vs` (values) field,
  but the database `FromJsonVisitor` reads them from `obs` (the object-id field), so the object list was lost in
  transit — a `ContainedIn { Objects = … }` pull reached the server with neither objects nor extent and failed
  with HTTP 500. The writers now use `obs`, matching the reader and the `ob`/`obs` convention. (The `Extent`
  form of `ContainedIn` was unaffected.)
- The Local workspace adapter's `Push` now releases (disposes) the database transaction it opens, instead of
  leaving it open. `Session.PushAsync` previously returned without disposing on both the error path (the early
  return when the push has errors — the transaction was then neither committed nor rolled back) and the success
  path (committed but never disposed), leaking a database connection on every push — benign on the in-memory
  adapter but a real connection leak on the SQL adapters. The transaction is now released on every path via
  `try`/`finally`; on the error path disposal rolls back the uncommitted failed push.
- The Local workspace adapter's `Pull` now releases (disposes) the database transaction it opens, once the
  pull has been executed and synced, instead of leaving it open. Previously `Session.PullAsync` / `CallAsync`
  created a transaction per pull that was never committed or disposed, leaking a database connection on every
  pull — benign on the in-memory adapter (which reuses a single transaction) but a real connection leak on the
  SQL adapters. The transaction is now released on every path via `try`/`finally`.
- The Local workspace adapter's `Invoke` now releases (disposes) the database transaction it opens, once the
  method invocations have executed and been committed/rolled back, instead of leaving it open. `Session.InvokeAsync`
  previously created a transaction per invoke that was committed/rolled back but never disposed — the same
  connection leak as `Pull` above (benign on the in-memory adapter, a real connection leak on the SQL adapters).
  The transaction is now released on every path via `try`/`finally`.
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
