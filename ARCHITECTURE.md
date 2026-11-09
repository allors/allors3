# Allors3 architecture

Allors3 is a platform for applications built around **domain inheritance**. The 3.2 line narrows
the repository to the platform and continues from the state that a downstream application runs
in production.

## Scope

| Area | Responsibility |
| --- | --- |
| System | Database and workspace engines, adapters, protocols, metadata, and generation machinery. |
| Core | Foundational domain behaviour, authorization, the JSON API, authentication, and hosting building blocks, reusable through domain inheritance. |
| Platform tests | Core's test domain, the runnable Core test server, and the suites that prove platform behaviour on every adapter. |
| Downstream applications | Business domains inheriting from Core, screens, forms, tables, navigation, and component libraries. |

**Base and Apps are not part of the platform.** Their business domains, applications,
Angular/Material and Blazor component libraries, configuration and build targets remain available
on the `v3.1` branch.

## Domain inheritance

Core is an abstract domain. A production application declares its own domain, which extends Core
directly or through other domains of its own; a domain extends one domain. The repository
declarations describe the domain hierarchy, and generated metadata and dispatch code support the
inherited model and behaviour.

The .NET projects include the layer-named source folders of their ancestors through compile
globs: a downstream server project includes the `Core*/**/*.cs` sources from
`dotnet/Core/Database/Server` alongside its own implementation, and its meta, domain,
configuration and commands projects do the same with the matching Core folders.
**Inheritable implementation code belongs in folders named after its domain.** Project globs must
select those folders explicitly.

Objects delegate operations to their strategies. Object creation and deletion, and all relation
reads and writes, use the Allors APIs. Relations are bidirectional: roles are the forward,
writable endpoints, and associations are the inverse, read-only endpoints.

## Test scaffolding is separate from inherited code

The `Custom` folders of Core hold internal scaffolding: the concrete test domain that extends
Core, its setup and population, database-reset endpoints and test-only helpers for the Core tests
and the Core test server. The `Core*` globs exclude them, so downstream projects do not compile
the scaffolding. The runnable test server is a test harness, not a production deployment.

`Custom` is also the conventional name of a downstream application's own extension domain. The
application compiles its own `Custom` folders and never Core's, so the two do not meet.

Test coverage must continue to exercise inheritance, relations, permissions, generation, adapters
and workspace synchronization. Reusable libraries must not carry test routes, test hooks or other
scaffolding; dedicated test projects are the exception.

The Core and TypeScript suites run by default on the Snapshot adapter, an in-memory database that
transactions share. It reports a write conflict as a database server does, so a suite that passes
on it does not hide one. The SQL Server and PostgreSQL adapters remain the production path, and CI
runs the suites on them as well. The Memory adapter remains for a single transaction over a
population, which is how the domain tests use it.

## Configuration and hosting

The Core server, the command-line tool and the API tests take their settings from the
configuration root that `ALLORS_CONFIG_ROOT` names and from environment variables, as the
[README](README.md#configuration) describes; environment variables override every file, and no
`appsettings` file is read from the folder of a project. A program stops without a setting that it
needs; the test projects and the Nuke build default to the `config/snapshot` template instead, so a
test run needs no database server. The admin connections in `ALLORS_SQLCLIENT` and `ALLORS_NPGSQL`
serve database management only.

Authentication lives in Core's server folder in this line: ASP.NET Core Identity user and role
stores over Allors users, a JWT bearer scheme and the sign-in endpoint. Moving authentication into
plug-ins, with cookie sessions and Microsoft Entra ID, is part of the archived previous line and
returns when an application needs it.
