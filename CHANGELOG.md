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

- The `PersonEdit` Blazor page no longer crashes when its `{id}` route parameter is not a number. It parsed
  the segment with `long.Parse(id)` in `OnInitializedAsync`, throwing `FormatException` for a non-numeric id
  (e.g. `/person/edit/abc`); it now uses `long.TryParse` and skips the pull when the id is invalid (the page
  renders nothing instead of throwing).
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
- The Blazor.Bootstrap.Site server's Identity logout page (`Areas/Identity/Pages/Account/LogOut.cshtml`) no
  longer carries `@attribute [IgnoreAntiforgeryToken]`, so the logout POST is antiforgery-protected again.
  Without it, a cross-site request could log a signed-in user out without consent (logout CSRF). The
  scaffolded logout form (`_LoginPartial`) already posts the antiforgery token, so normal logout is
  unaffected. (Security.)
- The production error handler no longer returns raw exception detail to clients. `ExceptionHandler`'s
  middleware wrote `error.Message` to the response in non-development environments (the full error is already
  logged server-side), leaking internal details (e.g. SQL errors, paths). Production responses are now a
  generic message (`"An internal server error has occurred."`, or `"Authentication token expired."` for an
  expired token); Development still returns the message and stack trace. (Security.)
- The image content endpoint's stale-revision redirect now targets `/allors/image/{id}/{revision}` instead of
  `/image/{id}/{revision}`. `BaseImageController.Get` is routed at `/allors/image/...`, but on a revision
  mismatch it issued a permanent redirect to a path missing the `/allors` prefix — which matches no route, so
  the redirect 404'd instead of serving the current revision. The prefix now matches the route (and the
  image URL builders, which already emit `/allors/image/...`).
- The Json API's `Pull` no longer crashes (`NullReferenceException` → HTTP 500) when a request dependency
  carries an unknown or wrong-kind meta tag. `Api.ToDependencies` cast each client-supplied tag
  (`FindByTag(...)`) to `IComposite`/`IRelationType` and dereferenced it unchecked, so a bogus `o`/`a`/`r`
  tag null-crashed the whole pull. Unresolvable dependencies — which are only prefetch hints — are now
  skipped (and logged as a warning, since they indicate a faulty client), so the pull proceeds normally.
- The workspace UML diagram template (`Workspace/Templates/uml.cs.stg`) now renders a many-valued role as
  an array type (`ElementType[]`), matching the database diagram template; its many-valued branch previously
  emitted the element type without the `[]`, so a collection role looked like a single-valued one.
- `Core/commands.sh` now forwards its arguments with `"$@"` instead of the unquoted `$*`, so an argument
  containing spaces (or shell glob characters) reaches `Database/Commands` as a single token instead of
  being word-split/globbed. Previously e.g. a file path with a space was split into several arguments.
- `Base/commands.sh` had the identical unquoted `$*` defect; it now also forwards its arguments with
  `"$@"`, so an argument containing spaces (or shell glob characters) reaches `Database/Commands` as a
  single token instead of being word-split/globbed.
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
- Object versioning no longer creates a redundant `*Version` on every derive of a versioned object that has a
  non-empty many-role (and no longer throws when comparing one). `VersionedExtensions.CoreOnPostDerive`'s
  many-role change check led with `!(!versionedRole.Any() && !versionRole.Any())`, which is `true` whenever
  either side is non-empty — so the role always looked "changed" (a new version every derive) and the real
  `Count()`/`SequenceEqual` comparison was short-circuited. Removing that clause exposed a second defect: the
  comparison ordered the composites with `OrderBy(s => s)` on `IObject` (which is not `IComparable`), throwing
  `ArgumentException`. The clause is removed and the composites are ordered by `.Id`, so a versioned many-role
  now creates a new version only when its contents actually change.
- The remote workspace adapter's `IPullResult.GetValue<T>` now converts a pulled value to `T` instead of
  hard-casting the raw deserialized JSON. `PullResult.Values` exposes values as received over the wire (a
  `JsonElement` for System.Text.Json, a boxed/`JToken` value for Newtonsoft), so `GetValue<T>` threw
  `InvalidCastException` (e.g. casting `JsonElement` to `int`/`byte[]`). It now routes the value through the
  adapter's `IUnitConvert`, mapping the requested CLR type to its unit tag, so values round-trip to the correct
  type. (Pull values are sent untagged, so the conversion is keyed by `T`.)
- The image content endpoint no longer returns HTTP 500 for an overlay-only request (an overlay with no
  width). `BaseImageController.Get` enters its image-processing branch when a width *or* an overlay is
  supplied, but always passed `w.Value` to `Process`, throwing `InvalidOperationException` when only the
  overlay was set (width null). `Process` now takes a nullable width and resizes only when one is given; an
  overlay-only request keeps the original dimensions and just draws the overlay.
- The workspace adapters' session-origin `SetCompositeRoleMany2One` no longer leaves a stale inverse when an
  object's many-to-one composite role is reassigned. When changing `A`'s role from `PR` to `R` it detached the
  *new* role `R` (a no-op, since `A` was not yet associated with `R`) instead of the *previous* role `PR`, so
  `PR`'s inverse association still listed `A` while `A`'s role was already `R`. It now detaches the previous
  role, matching the one-to-one sibling. Affects the Local and Remote workspace adapters.
- The Blazor server workspace configuration now parses the Allors user id from the `NameIdentifier` claim as a
  `long` instead of an `int`. Object ids are `long` (`DefaultStructRanges<long>`, and
  `DatabaseConnection.UserId` is `long`), so `int.Parse` threw `OverflowException` once a user's id exceeded
  `int.MaxValue` (~2.1 billion), preventing the workspace from being created for that user.
- The workspace `ContainedIn` predicate with an explicit object list now round-trips over the JSON protocol.
  Both `ToJsonVisitor`s (the workspace and the database one) serialized the objects to the `vs` (values) field,
  but the database `FromJsonVisitor` reads them from `obs` (the object-id field), so the object list was lost in
  transit — a `ContainedIn { Objects = … }` pull reached the server with neither objects nor extent and failed
  with HTTP 500. The writers now use `obs`, matching the reader and the `ob`/`obs` convention. (The `Extent`
  form of `ContainedIn` was unaffected.)
- A required many-valued role is now flagged as missing when its collection is empty. `RoleField.Validate`
  tested `Model == null`, but for a many-valued role `Model` is a non-null (possibly empty) composites
  collection, so an empty required many-role was never reported as required. It now treats an empty collection
  as missing for many-valued roles; unit and single-composite roles keep the existing null check.
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
- `BarcodeTest.Default` now asserts the generated barcode image instead of only writing it to disk. The test
  produced a barcode via `IBarcodeGenerator` and wrote the bytes to `barcode.png` without any assertion, so it
  passed even if `Generate` returned `null`/empty/non-image data (the generator returns `null` when PNG
  encoding fails). It now asserts the result is non-null, non-empty, and begins with the PNG file signature.
- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
- `MediaTest.ModifyMediaContent` now re-derives after changing the media content and asserts the outcome,
  instead of asserting the pre-modification state. It set `MediaContent.Data` to an empty array but never
  called `Derive()` again, so its assertions still reflected the original (valid) derivation and the test
  passed even though emptying the data should be rejected. It now re-derives and asserts the derivation
  reports an error (`MediaContent`'s post-derivation rejects empty data), matching `BuilderWithEmptyData`.
- SqlClient adapter tests now run with a 300s command timeout and `Connection Timeout=0` against
  SQL Server LocalDB, matching the Npgsql adapter tests. This stops sporadic CI failures
  (`SqlException: Execution Timeout Expired`) caused by LocalDB slowness on hosted runners.
- The CI `Upload TRX artifacts` step (test-result diagnostics) now runs only on failure and is
  non-gating (`continue-on-error`), so a transient GitHub artifact-service flake on the success path
  no longer fails the whole job. The test report is still published on every run — the reporter reads
  the `.trx` from disk, not from the uploaded artifact.
- `IImageService.Source`'s `background` parameter now defaults to `"FFF"` on the interface, matching the
  `LocalImageService` / `WeservImageService` implementations (which already defaulted to `"FFF"`). C# binds
  optional-argument defaults from the static type of the receiver, and the service is consumed through the
  `IImageService` interface (DI-registered, injected into `Image.razor`), so callers that omitted `background`
  picked up the interface's `null` default — not the impls' `"FFF"` — and PNG image URLs were built with an
  empty `b=` / `bg=` (background) query parameter. The interface and both implementations now agree.
