# Allors3

[![CI](https://github.com/allors/allors3/actions/workflows/ci.yml/badge.svg)](https://github.com/allors/allors3/actions/workflows/ci.yml)

Allors3 is an actively developed platform for applications built with **domain inheritance**.
An application declares its own domain that extends Core and inherits its model and behaviour.

## The 3.2 line

The 3.2 line starts from `0e907fea68`, the platform state of 8 May 2026 that a downstream
application inlined and runs in production, and takes over that application's changes to System
and Core. The scope is the platform: **System**, the database and workspace engines, adapters,
protocols, metadata and generators; **Core**, the foundational domain behaviour, authorization,
the JSON API and the server building blocks; the TypeScript system and core libraries; and the
generator templates.

**Base and Apps** are not part of the platform. Their code remains on the `v3.1` branch.
Downstream applications own their business domains, screens, forms, tables, navigation and
component libraries.

The previous line of `main`, up to 27 October 2026, is kept on the archive branch
`archive/main-2026-10`. It holds work that this line does not have yet: the fixes of June 2026
that `v3.1` also carries, the security hardening of the server, the rename of Core's test domain
to Test, the Identity and Entra authentication plug-ins, a domain that extends several domains,
the workspace API changes and the documentation website. Fixes return first, from `v3.1`, one
pull request at a time; features return when an application needs them.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the platform boundary and the inheritance rules.

## Development and releases

Pull requests are the default contribution workflow. The retained version branches are `main`,
`v3.0` and `v3.1`.

[version.json](version.json) specifies `3.2.0-alpha.{height}`: the development version of the
line, not a completed release. Changes are recorded in [CHANGELOG.md](CHANGELOG.md). Agent
instructions are in `AGENTS.md`.

## Configuration

A server, the command-line tool and the API tests take their settings from the configuration
root, the folder that `ALLORS_CONFIG_ROOT` names, and from environment variables. No `appsettings`
file is read from the folder of a project, and no program defaults the root. The test projects and
the Nuke build do: when `ALLORS_CONFIG_ROOT` is not set they use the `config/snapshot` template, so
a test run needs no database server unless the variable, or `--provider` for the build, names
another template. The remote API tests are the exception: they change the population through a
database connection of their own, which the server must share, so they need a SQL template, and so
does `DotnetCoreDatabaseTest`, which includes them.

Below the root every domain has its folder; Core's is `core`. A server also reads
`core/<application>`, and the command-line tool `core/commands`. At each of these locations the
variants are read in this order, each one optional:

```
appsettings.json
appsettings.{environment}.json              # Development, Staging, Production; servers only
appsettings.{platform}.json                 # windows, linux, osx
appsettings.{environment}.{platform}.json
```

Environment variables override every file: `adapter=snapshot`, `adapter=sqlclient` or
`adapter=npgsql`, `ConnectionStrings__DefaultConnection=…`, `JwtToken__Key=…`. Without
`ALLORS_CONFIG_ROOT` they are the only source. A program stops when a setting that it needs has no
value; the message names the setting, its environment variable and the folders that were read.

`config/sqlclient`, `config/npgsql`, `config/localdb` and `config/snapshot` are templates for a
root: SQL Server and PostgreSQL on `localhost` with the login `allors`, SQL Server LocalDB on
Windows, and the Snapshot adapter, which keeps the population in memory and needs no connection
string and no database server. Point `ALLORS_CONFIG_ROOT` at one of them, or at a copy that holds
your own values. With the Snapshot adapter the `init` command has nothing to drop or create, and a
population that the command-line tool builds ends with the process; the Core test server builds its
own through `Test/Setup`.

`ALLORS_SQLCLIENT` and `ALLORS_NPGSQL` hold an admin connection to the same server and serve
database management only; no program runs on them. The `init` command of the command-line tool
drops and creates the database of the configured connection and makes the login of that
connection its owner, creating the login when the server does not know it. The adapter suites
create and open their test databases with the admin connection. On Windows the admin connection
for SQL Server defaults to LocalDB.

```
./build.sh DotnetCoreDatabaseTestServerLocal             # Snapshot adapter, no database server
export ALLORS_SQLCLIENT='Server=localhost;User Id=sa;Password=…;TrustServerCertificate=true'
./build.sh DotnetCoreDatabaseTest --provider sqlclient   # SQL Server through config/sqlclient
```

## Building and testing

The build is [Nuke](https://nuke.build): `./build.sh <target>` on macOS and Linux,
`build.cmd <target>` on Windows. A fresh clone does not compile until `./build.sh Generate` has
run; generated files are not committed and are never edited by hand.

The `Ci*` targets in `build/Build.ci.cs` are the suites that CI runs on Linux:

| Suite | Needs |
| --- | --- |
| `CiDotnetSystemAdaptersTestMemory`, `CiDotnetSystemAdaptersTestSnapshot` | nothing |
| `CiDotnetSystemAdaptersTestNpgsql` | PostgreSQL: the admin connection in `ALLORS_NPGSQL` |
| `CiDotnetSystemAdaptersTestSqlClient` | SQL Server: the admin connection in `ALLORS_SQLCLIENT`, or LocalDB on Windows |
| `CiDotnetCore*Test`, `CiTypescriptWorkspace*Test` | a [configuration root](#configuration), `config/snapshot` unless `ALLORS_CONFIG_ROOT` or `--provider` names another template; with a SQL template also the admin connection of its adapter. CI runs them on SQL Server with `config/sqlclient`, and in memory with `config/snapshot` except the remote API tests, which change the population through a database connection of their own that the server must share |

The TypeScript workspace in `typescript/modules` is an nx workspace. After `npm install` there,
`npm run system-workspace-meta:test`, `npm run system-workspace-meta-json:test` and
`npm run system-workspace-adapters:test` run without a server;
`system-workspace-adapters-json:test` needs the Core test server, which the Nuke target starts.
