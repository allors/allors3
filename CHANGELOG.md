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

### Fixed

- The Local workspace adapter releases its database transaction after Pull, Push and Invoke, and
  Sync selects the roles of the connection's workspace only (#44, #45, #46, #259).

### Removed

- Base and Apps: the domains `dotnet/Base` and `dotnet/Apps`, the Angular applications and
  libraries under `typescript/modules/apps` and `typescript/modules/libs`, the end-to-end tests
  under `typescript/e2e`, the Angular workspace script, their configuration templates, build
  targets, solution folders and CI jobs, and the Angular toolchain of the TypeScript workspace.
  Base and Apps continue on the `v3.1` branch.
