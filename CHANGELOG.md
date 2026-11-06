# Changelog

All notable changes to Allors3 are documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses
[Semantic Versioning](https://semver.org/). The changelog of the previous line of `main` is on
the archive branch `archive/main-2026-10`; the changelog up to v3.1 remains on the `v3.1` branch.

## [Unreleased]

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
- CI runs on Linux: the in-memory adapter suite without a database, the Npgsql adapter suite
  against a PostgreSQL service container, and the SQL Server adapter, Core database, Core
  workspace and TypeScript suites against a SQL Server service container. The aggregate check
  `CI result` stands for the suites; a change to the four Markdown files at the root skips them.
- Nerdbank.GitVersioning versions the line as `3.2.0-alpha.{height}`, with public-release
  versions on `main` and the `v*` branches.
- The TypeScript workspace runs on nx 23, jest 30 and TypeScript 6 and depends on cross-fetch and
  tslib only.
- Dependabot keeps the GitHub Actions of the workflow current, in one grouped pull request a week.
- The PostgreSQL adapter tests take their admin connection from `ALLORS_NPGSQL`, as the SQL Server
  tests take theirs from `ALLORS_SQLCLIENT`, and create their databases through `Provisioning` in
  `Allors.Database.Adapters.Sql.Npgsql` (#80, released on `v3.1` in June 2026);
  `ALLORS_TEST_NPGSQL_CONNECTION` is accepted as an alias. The account `postgres` with the password
  `root` on `localhost` is no longer built in, and CI's PostgreSQL service runs as the user `allors`.

### Fixed

- The Local workspace adapter releases its database transaction after Pull, Push and Invoke, and
  Sync selects the roles of the connection's workspace only (#44, #45, #46, #259).
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
  workspace suite, and an object's `toJSON` serialises it through its strategy. The pull retry is
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
