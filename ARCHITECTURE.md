# Allors3 architecture

Allors3 remains an actively maintained and developed platform built around **domain inheritance**.
The direction for v3.2 narrows the repository to the platform, moves authentication into an
Identity domain, and introduces a reactive workspace.

## Scope and implementation status

The agreed platform boundary is:

| Area | Responsibility |
| --- | --- |
| System | Database and workspace engines, adapters, protocols, metadata, and generation machinery. |
| Core | Foundational domain behavior, authorization, security defaults, API, and hosting, reusable through domain inheritance. |
| Identity | Authentication with ASP.NET Core Identity, as a domain of its own. |
| Reactive workspace | Signals for workspace values and state in the .NET and TypeScript workspaces, with thin integrations for UI frameworks. |
| Platform tests | Test domains, runnable test servers, and small applications that prove platform and integration behavior. |
| Downstream applications | Business domains inheriting from Core, screens, forms, tables, navigation, and component libraries. |

**Base and Apps were removed without a separate continuation.** Their business domains,
applications, Angular/Material and Blazor component libraries, configuration, and build targets
remain available on the `v3.1` branch.

**Current implementation:** the tree holds System and Core with their build and test
infrastructure. The Identity domain and the signals workspace API described below are planned.
These are the agreed design requirements for the transition, not a claim that the Identity domain
or signals support has already shipped.

The platform-only baseline must build and pass its retained tests before the workspace API is
changed. The Base and Apps tests retired with their domains; platform behavior that only they
covered is to be represented in platform test domains and test applications where it remains
relevant.

## Domain inheritance

Core is an abstract domain. A production application declares its own domain extending Core and
may build further layers through domain inheritance. Allors3 will continue to use domain
inheritance as it develops.

The repository declarations describe the domain hierarchy. Generated metadata and dispatch code
support the inherited model and behavior. The .NET projects also include the layer-named source
folders of their ancestors through compile globs: for example, downstream server projects include
the `Core*/**/*.cs` sources from `dotnet/Core/Database/Server` alongside their own implementation.

**Inheritable implementation code belongs in folders named after its domain.** Project globs must
select those folders explicitly.

Objects delegate operations to their strategies. Object creation and deletion, and all relation
reads and writes, use the Allors APIs. Relations are bidirectional: roles are the forward,
writable endpoints, and associations are the inverse, read-only endpoints.

## Identity domain

Authentication with ASP.NET Core Identity will move out of Core into Identity, a plug-in that
Core hosts. Authorization stays in Core. [docs/domains.md](docs/domains.md) describes the kinds
of domains and how an application selects a plug-in.

**Current implementation:** the Identity domain does not exist yet. The ASP.NET Core Identity
integration is part of Core, in `dotnet/Core/Database/Server/Core/Identity` and in the hosting
configuration described under
[Security defaults for inheritors](#security-defaults-for-inheritors).

## Reactive workspace direction

Signals will be the default public API of every workspace. That covers both the .NET and the
TypeScript workspace, and any workspace added from here on. Breaking changes to the generated
workspace API are allowed for this transition; preserving the old property API alongside signals
is not a requirement.

The reactive surface should cover:

- Role values and read-only associations, including inherited members.
- Read and write permissions, and method execution permissions.
- Existence and change state for members, objects, and sessions.
- Changes caused by local edits, pull/synchronization, push, reset, and permission updates.

Signals observe the workspace's own state. Writes continue through the Allors strategies so that
relation consistency and change tracking remain managed by the workspace. UI integrations should
not duplicate domain state in a separate store.

Thin integrations adapt workspace signals to a UI framework's reactive and lifecycle mechanisms,
including subscription cleanup. The platform does not supply production form/table components,
application shells, or a view engine. Small test applications verify the integrations; downstream
applications own their presentation.

The concrete signal implementations and generated API details will be established during the
workspace work. Domain inheritance, role/association semantics, and controlled writes remain the
constraints on that design.

## Test scaffolding is separate from inherited code

The `Test/` folders hold internal scaffolding: concrete setup and population for automated tests,
database-reset endpoints such as `Test/Init` and `Test/Setup`, and test-only authentication
helpers.

`Test` is a domain in its own right. Core's repository declares a `Test` struct extending Core,
and Allors binds hook implementations by domain name, such as `TestOnPostDerive` and `TestSetup`.
The `Core*` globs exclude `Test/`, so downstream projects using those globs do not compile the
scaffolding. The repository's runnable test servers are test harnesses, not production deployments.

`Custom` is conventionally the name of a downstream application's own production extension
domain. Internal test scaffolding uses `Test` to keep those responsibilities distinct.

Test coverage must continue to exercise inheritance across domain levels, as well as relations,
permissions, generation, adapters, and workspace synchronization. Browser tests for thin UI
integrations belong in isolated test applications. Reusable production libraries must not carry
test routes, test hooks, or other scaffolding; dedicated test projects are the exception.

## Security defaults for inheritors

Foundational security behavior belongs in inheritable Core folders. In particular,
`dotnet/Core/Database/Server/Core/Hosting` contains the shared `AddAllorsServer`/`UseAllorsServer`
configuration for DataProtection, forwarded headers, security headers, rate limiting, Identity
policy, and production-secret validation. Authentication is to move to the Identity domain, so
the authentication parts of this configuration will move with it. Authorization stays in Core.

Inheritors override the defaults through configuration sections such as `Identity`, `Security`,
`DataProtection`, `ForwardedHeaders`, and `Logging:JSNLog`. Runtime configuration follows the
[README](README.md#configuration).

Test-only endpoints stay in `Test/`. The existing `InheritableSurfaceTests` check the inheritable
server folder for test/bypass controllers. That boundary must remain enforced.
