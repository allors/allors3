# Allors3 architecture

Allors3 remains an actively maintained and developed platform built around **domain inheritance**.
The direction for v3.2 narrows the repository to the platform, moves authentication into
plug-ins, and introduces a reactive workspace.

## Scope and implementation status

The agreed platform boundary is:

| Area | Responsibility |
| --- | --- |
| System | Database and workspace engines, adapters, protocols, metadata, and generation machinery. |
| Core | Foundational domain behavior, authorization, the API, and hosting building blocks, reusable through domain inheritance. |
| Identity | Authentication with ASP.NET Core Identity, as a domain of its own. |
| Entra | Authentication with Microsoft Entra ID, as a domain of its own. |
| Reactive workspace | Signals for workspace values and state in the .NET and TypeScript workspaces, with thin integrations for UI frameworks. |
| Platform tests | Test domains, runnable test servers, and small applications that prove platform and integration behavior. |
| Downstream applications | Business domains inheriting from Core, screens, forms, tables, navigation, and component libraries. |

**Base and Apps were removed without a separate continuation.** Their business domains,
applications, Angular/Material and Blazor component libraries, configuration, and build targets
remain available on the `v3.1` branch.

**Current implementation:** the tree holds System, Core, Identity and Entra with their build and
test infrastructure. The signals workspace API described below is planned: that section states the
agreed design requirements, not a claim that signals support has shipped.

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
A project that selects a plug-in includes its folders the same way: the `Identity*` folders of
`dotnet/Identity`, or the `Entra*` folders of `dotnet/Entra`.

**Inheritable implementation code belongs in folders named after its domain.** Project globs must
select those folders explicitly.

Objects delegate operations to their strategies. Object creation and deletion, and all relation
reads and writes, use the Allors APIs. Relations are bidirectional: roles are the forward,
writable endpoints, and associations are the inverse, read-only endpoints.

## Authentication plug-ins

Authentication lives in plug-ins that Core hosts: Identity, with ASP.NET Core Identity, and
Entra, with Microsoft Entra ID. Authorization stays in Core. [docs/domains.md](docs/domains.md)
describes the kinds of domains and how an application selects a plug-in;
[docs/authentication.md](docs/authentication.md) describes how Core, a plug-in and the
application's domain share a sign-in.

**Current implementation:** the Identity tree, `dotnet/Identity`, is Core ← Identity ← Test, and
the Entra tree, `dotnet/Entra`, is Core ← Entra ← Test. The Identity domain declares the
authentication fields of `User` and the `Login` class, with their rules, the password hasher and
the migration; its inheritable server folder `dotnet/Identity/Database/Server/Identity` holds
`AddAllorsIdentity`, the Allors user and role stores, the Identity `IUserResolver` and the login
page. The Entra domain declares the Entra identity of a `User` and what the directory says about
it, all derived; its inheritable server folder `dotnet/Entra/Database/Server/Entra` holds
`AddAllorsEntra`, the admission of a validated principal, the Entra `IUserResolver` and the
sign-in and sign-out endpoints, over Microsoft.Identity.Web.

Three seams in Core's server folder connect the parties, and each plug-in is a thin layer over
its library: `IUserResolver`, through which a plug-in tells Core which Allors user a signed-in
principal stands for; `IUserFactory`, through which a plug-in has the application's domain
create a user; and `AllorsAuthenticationOptions`, in which a plug-in names its schemes so that
Core owns the browser session and selects the scheme per request. Four guards in
`InheritableSurfaceTests` keep the boundary: `CoreTreeReferencesNoAspNetCoreIdentity` and
`CoreTreeReferencesNoIdentityProviderLibrary`, so that nothing under `dotnet/Core` references
ASP.NET Core Identity, the OpenID Connect or JWT bearer handlers or Microsoft's identity
libraries; `InheritableCoreFoldersNameNoAuthenticationField`, so that Core's `User` keeps no
authentication field; and `PlugInFoldersNameNoClassOfTheConcreteDomain`, so that a plug-in
compiles for any concrete domain. The hosting side is described under
[Hosting building blocks](#hosting-building-blocks).

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

`Test` is a domain in its own right. Each tree's repository declares a `Test` struct, Core's
extending Core, Identity's extending Identity and Entra's extending Entra, and Allors binds hook
implementations by domain name, such as `TestOnPostDerive` and `TestSetup`.
The `Core*` globs exclude `Test/`, so downstream projects using those globs do not compile the
scaffolding. The repository's runnable test servers are test harnesses, not production deployments.

`Custom` is conventionally the name of a downstream application's own production extension
domain. Internal test scaffolding uses `Test` to keep those responsibilities distinct.

Test coverage must continue to exercise inheritance across domain levels, as well as relations,
permissions, generation, adapters, and workspace synchronization. Browser tests for thin UI
integrations belong in isolated test applications. Reusable production libraries must not carry
test routes, test hooks, or other scaffolding; dedicated test projects are the exception.

## Hosting building blocks

Core's server decides only what the Allors API needs. `AddAllorsServer` registers the Allors
services, the rules of the browser session and the scheme that selects between session and
bearer token, both bound to the schemes a plug-in names in `AllorsAuthenticationOptions`, and
the JSON API controllers, which carry `[Authorize]`: the API always requires an authenticated
user, whatever the application decides for its own endpoints. The seams `IUserResolver` and
`IUserFactory` are in Core's server folder; the selected plug-in registers the resolver and the
application its factory.
`InheritableSurfaceTests.InheritableServerControllersRequireAuthorization` checks every
controller in the inheritable server folder. The application's `Startup` builds the database,
writes `UseRouting`, `UseAuthentication` and `UseAuthorization` itself and calls
`UseAllorsServer()` after them. That adds antiforgery for the session scheme, the current user
and the API endpoints, and its callback maps the application's own endpoints, such as Razor
Pages or the Entra sign-in.

The rest are building blocks in Core's inheritable server folder that a `Startup` switches on:
`AddAllorsDefaultDeny`; `AddAllorsRateLimiting`, with the paths of the plug-in and of
`Security:AuthenticationRateLimit`; `AddAllorsDataProtection`, with
`DataProtection:KeysDirectory`; `UseAllorsForwardedHeaders`, with `ForwardedHeaders`;
`UseAllorsSecurityHeaders`, with `Security:ContentSecurityPolicy`; and
`ConfigureExceptionHandler`. `AddAllorsIdentity` registers the Identity plug-in and reads the
`Identity` section; `AddAllorsEntra` registers the Entra plug-in and reads the `Entra` section.
Core's test server switches on every block. `AllorsServerTests` checks that `AddAllorsServer`
adds no fallback policy, no rate limiter and no data protection key store, that `UseAllorsServer`
needs no Razor Pages, and the rules of the session and of the selecting scheme. Runtime
configuration follows the [README](README.md#configuration).

Test-only endpoints stay in `Test/`. `InheritableSurfaceTests` checks the inheritable server
folders for test/bypass controllers. That boundary must remain enforced.
