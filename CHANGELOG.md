# Changelog

All notable changes to Allors3 are documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses
[Semantic Versioning](https://semver.org/). The changelog of the previous line of `main` is on
the archive branch `archive/main-2026-10`; the changelog up to v3.1 remains on the `v3.1` branch.

## [Unreleased]

### Added

- The Snapshot adapter, `Allors.Database.Adapters.Snapshot`: an in-memory database that many
  transactions share, also on different threads, for test runs without a database server (#307).
  A transaction reads the immutable snapshot that was current when it started or when it last
  committed or rolled back, writes to a private overlay and publishes a new snapshot when it
  commits; commits are serialized by one lock. A commit fails with `ConcurrencyException`, a
  `DbException`, when another transaction changed or deleted an object that this transaction
  changed or deleted after it read it, as a database server does under snapshot isolation; a
  change to the other end of a relation is reconciled instead (#311). The adapter saves and loads
  the population format of the other adapters, and `TakeRestorePoint` and `Restore` put a
  population back without reloading it, so a test suite resets its database in memory.
  `DatabaseBuilder` accepts the adapter name `snapshot` without a connection string, and
  `config/snapshot` is a configuration root template for it. The shared adapter tests gain
  `ConcurrencyTest`, which the Snapshot, SqlClient and Npgsql adapters pass alike.
- The Nuke build takes `--provider` with `snapshot`, `sqlclient`, `npgsql` or `localdb`: the
  template under `config/` that is the configuration root of the tests, the command-line tool and
  the server that its targets start. It wins over `ALLORS_CONFIG_ROOT` (#314).

### Changed

- `main` restarts from `0e907fea68`, the platform state of 8 May 2026 that a downstream
  application inlined and runs in production, and takes over that application's changes to
  System and Core: configuration variants per environment and platform, with environment
  variables winning over every file; a password policy of 12 characters with 4 distinct ones; a
  self-service password change that requires the current password and rotates the security
  stamp; SQL Server test provisioning from the admin connection in `ALLORS_SQLCLIENT`; the
  adapters-json fix for the permissions a concurrent pull left pending; and dependency updates.
  The previous line of `main`, up to 27 October 2026, is kept on `archive/main-2026-10`. The
  fixes released on `v3.1` after 8 May 2026 and the archived work are to be ported in later pull
  requests.
- The Core and TypeScript suites default to the Snapshot adapter and need no database server: the
  test projects and the Nuke build use the `config/snapshot` template when `ALLORS_CONFIG_ROOT` is
  not set (#314). The local workspace tests build their database from the configuration root instead
  of constructing a Memory database (#317), the remote API tests create their users through the
  test-only `Test/CreateUser` of the Core test server instead of a database connection of their own
  (#316), and the shared pull tests compare the objects of a pull without sorting regardless of
  their order (#318). Every Core and TypeScript suite now runs on the Snapshot adapter, on SQL
  Server and on PostgreSQL. The Core domain tests run on the Memory adapter, the adapter for a
  single transaction over a population, unless the configuration root names SQL Server or PostgreSQL
  (#320).
- The SqlClient adapter suite skips its provisioning theory, which creates a SQL Server login, when
  the admin connection names LocalDB, which accepts Windows authentication only (#320).
- CI runs every suite as a job of its own, in tiers (#320). A pull request, whatever its base branch
  (#321), and a push to `main` run on Linux the adapter suites, those of SQL Server and PostgreSQL
  against service containers, and the Core database, Core workspace and TypeScript suites on the
  Snapshot adapter. A push to `main` runs those Core and TypeScript suites on SQL Server and
  PostgreSQL as well. A nightly workflow, which also starts on demand, runs every suite on Windows:
  the adapter suites, and the Core and TypeScript suites on the Snapshot adapter, on SQL Server
  LocalDB and on PostgreSQL. The aggregate check `CI result` stands for the suites of a pull request
  and of a push; a change to the four Markdown files at the root skips them. A push to a `feature/`
  or `issue/` branch no longer starts a run of its own: a branch runs through its pull request, or
  on demand.
- Nerdbank.GitVersioning versions the line as `3.2.0-alpha.{height}`, with public-release
  versions on `main` and the `v*` branches.
- The TypeScript workspace runs on nx 23, jest 30 and TypeScript 6 and depends on cross-fetch and
  tslib only.
- Dependabot keeps the GitHub Actions of the workflows current, in one grouped pull request a week.
- The PostgreSQL adapter tests take their admin connection from `ALLORS_NPGSQL`, as the SQL Server
  tests take theirs from `ALLORS_SQLCLIENT`, and create their databases through `Provisioning` in
  `Allors.Database.Adapters.Sql.Npgsql` (#80, released on `v3.1` in June 2026);
  `ALLORS_TEST_NPGSQL_CONNECTION` is accepted as an alias. The account `postgres` with the password
  `root` on `localhost` is no longer built in, and CI's PostgreSQL service runs as the user `allors`.

### Fixed

- The Local workspace adapter releases its database transaction after Pull, Push and Invoke, and
  Sync selects the roles of the connection's workspace only (#44, #45, #46, #259).
- Loading a population in the Memory, SqlClient and Npgsql adapters keeps a unit value that
  precedes a self-closing relation element; the loaders replaced it with an empty value (#306).
- Loading a population reports every relation of a relation type that the meta population no
  longer knows; the reporters stepped into the next relation, so the Memory adapter failed such a
  load and the SQL adapters reported every second relation only (#308).
- Loading a population reads an object type element without objects, self-closing or with an end
  tag, as an element that holds no objects. The form with an end tag failed the load on every
  adapter, and on the SqlClient and Npgsql adapters a self-closing element made the loader skip the
  object type element that followed it directly, so its objects were not loaded (#325).
- On PostgreSQL, `Init` starts the numbering of the objects again and `Load` continues it after the
  highest loaded id, as the Memory, Snapshot and SqlClient adapters do. The Npgsql adapter left the
  sequence of the object ids where it was, so an object created after a load could get the id of a
  loaded object and fail its transaction with a duplicate key: after a load into a new database,
  and after `Init` had created the tables again for a changed schema (#326).
- On the SqlClient and Npgsql adapters a failure while a load reads or writes relations stops the
  load. The loaders rolled back the relation type that failed and went on, so the load returned
  normally without any relation of that relation type. That happened for a composite relation that
  the schema does not allow when no `RelationNotLoaded` handler is attached, for an exception
  thrown by such a handler, and for an error of the database (#329).
- Loading a population reports a value that its role type cannot hold as it is, as it reports a
  value that the unit type cannot read: a string that is longer than the size, and a decimal that
  the precision and the scale cannot hold exactly. The adapters cut, rounded or kept such a value,
  or the database failed the load (#330).
- Setting a decimal on a role gives the same result on every adapter: the decimal is rounded to the
  scale of its role type, half away from zero, and refused with an `ArgumentException` when it is
  then too large for the precision. The Memory and Snapshot adapters kept the decimal as it was
  set; the SqlClient and Npgsql adapters rounded it at the commit, and failed the commit for a
  decimal that was too large (#331).
- Loading a population on the SqlClient and Npgsql adapters stops at an element that the loader
  does not know, as it does on the Memory and Snapshot adapters, with the same error:
  `Unknown child element <…> in parent element <…>`. The SQL loaders skipped such an element with
  everything in it and returned normally, so a file with `objects`, `relations`, a `database`
  element, an object type, a relation type or a relation under another name or in another place
  loaded as an empty database, or without those objects or relations. A self-closing element under
  an unknown name among the relation types, which every adapter skipped, stops the load too (#335).
- Loading a file without a `population` element fails with an `ArgumentException`. Every adapter
  returned normally and left an empty database, for `<allors/>` as for any other XML document. A
  population without content still loads as a population without objects (#336).
- Loading a population refuses an object with an id of 0 or below, with an `ArgumentException`: the
  id of a database object is 1 or higher. Every adapter loaded such an object, under an id that the
  platform reads as no object or as a workspace object that is not saved yet (#337).
- Loading a population stops at a relation of an object that the file does not hold, as its
  association or as a role, with an `ArgumentException` that names the relation type and the
  objects. With a `RelationNotLoaded` handler attached every adapter reported such a relation and
  went on, as if a schema change explained it, and a file with its relations before its objects
  loaded without any relation. A relation that a schema change explains is still reported, but only
  when the file holds all its objects; a role that is not a number in such a relation stops the
  load, where it was reported with its text. A handler can no longer let a relation of a missing
  object pass: such a file has to be repaired before it loads (#338).
- The Memory adapter's extent sort treats two objects without a value for a sort order as equal
  and lets the next sort order decide; the comparer contradicted itself, so short lists kept an
  arbitrary order and longer ones could fail the sort (#309).
- Initializing a database again in the same process forgets the objects of the previous
  population: Core's database services kept their caches of unique ids, classes, permissions,
  security and versioned ids across `Init`, `Load` and `Restore`. On PostgreSQL, where the object
  ids continued after `Init` until #326, the second `Test/Setup` of the Core test server failed;
  the Snapshot adapter and SQL Server reuse the ids, which hid the stale entries (#319).
- The System adapter fixes released on `v3.1` in June 2026 (#29, #32 to #36, #38, #39, #41 to #43,
  #47 to #49): the `Union` of ranges keeps an element equal to `default(T)`; the Memory adapter's
  `Like` follows SQL semantics and escapes metacharacters, and the SQL Server adapter's `Like`
  follows ANSI semantics with `[` as a literal; a one2one change set reports the displaced
  association; `NotAssociationContainedInEnumerable` emits balanced parentheses; `Instantiate`
  tolerates duplicate object ids; prefetching uses an object's modified composites role; a converted
  `Extent.CopyTo` honours the destination index; `Load` honours the configured schema name; the
  shared `Database` lazy caches are thread-safe; `CreateObjects` holds object ids as `bigint`;
  `ContainedIn` by objects round-trips over JSON; the Remote adapter's `GetValue<T>` converts pulled
  JSON values; and `SetCompositeRoleMany2One` detaches the previous role.
- A procedure's decimal output values travel as the protocol's invariant strings, like decimal
  roles, so the Remote adapter's `GetValue<decimal>` keeps their precision on both JSON libraries;
  untyped consumers of a pull response receive strings for decimal values.
- The Core server's default SQL Server connection string disables the connect timeout, as the
  command and test configurations already did.
- The Core fixes released on `v3.1` in June 2026 (#50 to #57, #61 to #64): a nested prefetch policy
  is built on the nested builder; the resource `Merger` preserves the structure of a duplicate key's
  data element and gets its own test project, `Merge.Tests`, in the Core database test suite;
  `commands.sh` quotes the arguments it forwards; the UML template renders a many-valued role as an
  array; a pull dependency with an unknown or mismatched meta tag is logged and ignored instead of
  failing the pull; `Invoke` and `Push` are no longer retried on a database exception, since a
  retry after an ambiguous commit would apply the unit twice, while `Pull` and `Sync` still are; the
  test share holders controller resolves the current user through `IUserService`; and the Core test
  suites drop a denied-permissions stub, a `SelectRef` stub and a vacuous sign-out test, and run the
  push test of a workspace object without a workspace.
- The TypeScript workspace fixes released on `v3.1` in June 2026 (#84, #85): `nodeLeafs` returns the
  leaf nodes of a tree, covered by the new `system-workspace-domain-tests` project in the TypeScript
  workspace suite, and an object's `toJSON` serializes it through its strategy. The pull retry is
  covered by the regression test of #276, which fails a pull's first SQL command.
- The fixes of the archived line of `main` from August and September 2026 (#270 to #272, and the
  platform part of #261): the .NET workspace sorts the collection a composites role is set from, so
  lookups in the role no longer miss objects given in creation order; `ITransaction.Instantiate(IObject)`
  returns null for null in the SQL adapters, as it does in the memory adapter; the generated lazy meta
  of the .NET workspace compiles for a workspace without inheritance, covered by the new
  `Generate.Tests` project in the Core database test suite, which generates and compiles the
  workspace meta of every workspace of the test domain; `AllorsUserStore.CreateAsync` no longer fails
  for a user created with a password hash, the store logs the exceptions it turns into failed
  results, and the generated builder guard spells its message "One multiplicity".
- `AllorsUserStore.HasPasswordAsync` answers whether the user has a password hash, a fix from the
  security hardening of the archived line of `main` (#255). It returned the inverse, so the account
  pages of ASP.NET Core Identity sent a user with a password to "set password" and a user without
  one to "change password".
- The Npgsql adapter fixes of #80, released on `v3.1` in June 2026. The adapter sets
  `Npgsql.EnableLegacyTimestampBehavior` and `Npgsql.EnableStoredProcedureCompatMode` itself, in a
  module initializer; only the fixture of its tests set them, and without them its calls fail with
  `42809: … is not a procedure`. The adapter connects to the lower-cased database name, the name
  PostgreSQL gives a database created without quotes, so `Database=AllorsCore` reaches `allorscore`;
  a database created under a quoted name with capitals has to be renamed.
- The PostgreSQL adapter tests pool their connections, a fix of the archived line of `main` from
  September 2026 (#273). Their connection string set `Pooling=false`, so every transaction opened
  and closed a connection, and every close left a port in `TIME_WAIT`. Against a PostgreSQL in a
  local container, whose port forwarder holds a second port per connection, the suite in parallel
  used up the ephemeral ports of the host within seconds (`Can't assign requested address`). It now
  passes in parallel.

### Security

- A failed sign-in counts towards Identity's lockout, so repeated wrong passwords lock the account
  for the configured period (#58).
- An Identity update is rejected when the user's security stamp changed since the user was loaded,
  so recording a failed sign-in cannot undo an intervening password change.
- A production error response carries a generic message instead of the exception's message; the
  development environment keeps the message and the stack trace (#60).

### Removed

- Base and Apps: the domains `dotnet/Base` and `dotnet/Apps`, the Angular applications and
  libraries under `typescript/modules/apps` and `typescript/modules/libs`, the end-to-end tests
  under `typescript/e2e`, the Angular workspace script, their configuration templates, build
  targets, solution folders and CI jobs, and the Angular toolchain of the TypeScript workspace.
  Base and Apps continue on the `v3.1` branch.
