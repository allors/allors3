# Changelog

This changelog starts with the v3.2 platform direction. For the changelog up to v3.1, see the
[`v3.1` branch](https://github.com/allors/allors3/blob/v3.1/CHANGELOG.md).

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
[version.json](version.json) specifies `3.2.0-alpha.{height}` for development builds.
Changes accumulate under **[Unreleased]** until a version is released.

## [Unreleased]

### Added

- Protected atomic Invoke on normal .NET Local, both .NET HTTP serializers and
  TypeScript connections. Ordered methods commit once or reject as one batch; affected refresh
  defaults to true and preserves unsaved edits, creations and checkpoints. Shared results keep
  commitment separate from refresh/application failures and expose raw diagnostics without replay.
  Host admission and exact method eligibility use the protected SQL write boundary.

- Ready asynchronous connection factories in .NET Local, both HTTP serializers and TypeScript.
  Initialization uses the connection's read assembler without application callbacks; all normal
  workspaces use complete reads and protected Push/Invoke. Connection cleanup stops admission,
  drains dispatched work, closes every Session and leaves borrowed transports/databases host-owned.
  Common Session cleanup preserves held values and diffs.
- Connection ownership of fresh .NET workspace/Session services, with registration before exactly
  one initialization callback and disposal after drain. Profiles and the TypeScript Fixture capture
  transport identity, drain before login replacement and await cleanup across independent connections.
- A maintainer inventory of workspace capability removals, normal lifecycle/caller migration,
  exact test migrations and the separate server-endpoint removal boundary.
- Scoped PushRead integration tests for .NET Local, both .NET HTTP serializers and TypeScript
  HTTP against SQL providers. They cover immediate refresh, request counts, exact body reuse,
  concurrent membership and permissions, deletion, retirement and response loss after commit.
  Core server Local tests accept `ALLORS_TEST_DATABASE_PREFIX` for isolated test databases;
  the HTTP fixtures accept `ALLORS_TEST_URL` for separate test servers.
- Ordinary Push in .NET and TypeScript with affected refresh enabled by default, explicit
  refresh skip and optional discovery/query completion. Shared results distinguish committed,
  rejected and unknown outcomes from refresh and local application; raw diagnostics and complete
  mappings remain inspectable. Unknown commitment or unsafe acknowledgement requires reconciliation.
  Saving pauses edits, preserves checkpoints and makes one protected request without replay.
- Server `PushRead` for PostgreSQL and SQL Server, through authenticated HTTP and Local execution.
  It validates scope before writing, fences population maintenance through commit, and returns
  optional full affected-object refresh and query discovery from one fresh protected snapshot.
  Mutation and read outcomes stay separate; derivations, deletions, inverse relation ends and
  submitted no-ops are tracked across checkpoints. Strict shared messages cover both runtimes and
  .NET serializers.
- Complete workspace reads for PostgreSQL and SQL Server. One protected snapshot supplies
  selected records, named outputs, explicit unavailable outcomes, and all required security and
  permission data. Local and HTTP transports use strict matching .NET/TypeScript messages and
  make one attempt. Inline declarations are captured and validated under a representation revision.
- Connection-owned internal read caches in .NET and TypeScript. One operation owner covers
  discovery, completion, Session application and notifications. Immutable references replace public
  store/coordinator, pin and completed-result release APIs. Exact historical bodies remain available
  while the connection is live; close clears lookup after drain without invalidating held values.
- Sessions with their own immutable editing baselines and checkpoints and current permissions.
  A connection orders reads through adaptation and one-attempt
  Push/Invoke through response handling. Argument/result names and structured diagnostics are
  preserved. Default save refresh accepts a new editing baseline; explicit skip retains the previous
  baseline and write restriction until an accepted read.
- SQL population identity and maintenance state, with offline enrollment and renewal commands.
  Init/import cannot publish readiness until all phases finish. Protected reads check readiness,
  trusted identity and scope, and reject writes or transaction restarts even after caught errors.
  Legacy endpoints do not enforce readiness; native restores require explicit renewal.
- Shared protocol fixtures and read tests across runtimes, SQL providers and transports. The
  [read reference](docs/workspace-reads.md) covers supported behavior; maintainer pages map code
  and tests. Server-endpoint removal, signals and other consumers remain separate work.

- Workspace session baseline characterization tests across .NET Local, both .NET JSON HTTP
  adapters, and TypeScript HTTP, with a maintainer map in
  `docs/internals/workspace-session-state.md`. They cover remote refresh, disjoint and conflicting
  edits, diff/version retention, Reset/Pull recovery, and DateTime/binary unit equality.

- A domain extends several domains: `[Extends]` takes the names of all of them, and the graph
  may hold diamonds. The order of the domains, for the hooks of domains that do not extend each
  other, is defined: a domain before the domains it extends; branches in the id order of their
  top domain, each branch kept whole. It is the C3 linearization over the superdomains sorted by
  id, after a superdomain that another superdomain already extends is dropped; names and the
  order in `[Extends]` play no part, so the order survives a rename and a reordering of the
  declaration. `DomainLinearization` holds the one implementation, `MetaPopulation.SortedDomains`
  exposes the result and `MethodCompiler` runs the hooks in that order. `DomainOrderTests` pins
  the rule and `RepositoryDomainsTests` the parser, on repository projects built in memory.
- The Diamond tree, `dotnet/Diamond`, with the platform test domains: the functional domains
  `Level1` and `Level2`, the test plug-in `Plugin1`, hosted by Core, and the concrete `Test`
  domain that extends `Level2` and `Plugin1`, so the population is a diamond: Core ← Level1 ←
  Level2 ← Test and Core ← Plugin1 ← Test. It inherits Core as the Identity tree does and has a
  database side with domain tests on the memory adapter; it has no commands, server or
  workspace. Level1 declares a class with a derived role and a rule, and the service
  `ILevel1Log`; Level2 adds a role to Level1's class and declares a class with a rule of its own;
  Plugin1 declares a derived role on `User` with a rule, and the service `IPlugin1Log`. Every
  hook of the four domains records that it ran, in the one hook log that the concrete domain
  provides for both services. `DomainsTests` pins the order of the five domains, which follows
  from their ids: Test, Plugin1, Level2, Level1, Core. `HookOrderTests` checks that the
  `OnPostBuild` hooks run in that order, `SetupOrderTests` that the dispatch shims run the setup
  and security hooks base first, and `DomainIsolationTests` that no folder of Level1, Level2 or
  Plugin1 names a type of a domain it does not extend. Build targets `DotnetDiamondMerge`,
  `DotnetDiamondGenerate`, `DotnetDiamondDatabaseTest` and `DotnetDiamondTest`, CI job
  `CiDotnetDiamondTest` in the `memory` job. `VirtualDispatchTests` checks the hooks of the three
  new domains in the dispatch shims too.
- Documentation for users and maintainers under `docs/`, starting with the domain model:
  functional domains, plug-ins and their hosts, the concrete domain that selects plug-ins, and
  the order of the domains. The first page for maintainers,
  `docs/internals/domain-inheritance.md`, maps domain inheritance from `[Extends]` to the order
  of the hooks: which part owns what, why the ids decide the order, and which test of the
  Diamond tree pins what.
  A page on logging says how the host receives the logs of Allors. A page on authentication
  says how Core, an authentication plug-in and the application's domain share a sign-in, and a
  how-to guide takes an application through signing in with Microsoft Entra ID. `AGENTS.md`
  holds the rules for these pages.
- The build target `DotnetSystemSharedTest` and the CI job `CiDotnetSystemSharedTest` for the
  `Ranges` tests in `dotnet/System/Shared.Tests`, which no target or job ran before.
- A `Generate.Tests` project for the generator and its templates. `WorkspaceTemplateTests`
  generates the workspace meta for each workspace of the Core test domain and for a workspace
  without types, and compiles the result. It runs in the `DotnetCoreDatabaseTest` target.
- The Identity tree, `dotnet/Identity`, where the concrete `Test` domain selects the Identity
  plug-in: Core ← Identity ← Test. It inherits Core through the compile globs on Core's `Core*`
  folders, as an application does, and has a database side, commands and a server that signs
  users in with ASP.NET Core Identity; it has no workspace. The Identity domain holds the ASP.NET
  Core Identity integration that was in Core; see Changed. Build targets `DotnetIdentityMerge`,
  `DotnetIdentityGenerate`, `DotnetIdentityDatabaseTest` and `DotnetIdentityTest`, CI job
  `CiDotnetIdentityDatabaseTest`, configuration templates in `config/<provider>/identity`.
  `VirtualDispatchTests` checks the hooks of the Identity layer in the dispatch shims too.
- The Entra tree, `dotnet/Entra`, where the concrete `Test` domain selects the Entra plug-in for
  signing in with Microsoft Entra ID: Core ← Entra ← Test. It inherits Core as the Identity tree
  does and has a database side, commands and a server; it has no workspace. The Entra domain
  declares the Entra identity of a `User`, the pair `EntraTenantId` and `EntraObjectId`, what
  the directory says about the person: `EntraUserName`, `EntraDisplayName` and `EntraEmail`, and
  for a guest invited from another organization its home and status: `EntraIdentityProvider` and
  `EntraIsGuest`. All seven are derived, so nobody writes them through the API.
  `Users.FindByEntraIdentity` finds the user of an identity. The concrete `Test` domain has two
  `User` classes, `Person` and `Agent`, the second for programs that call the application with a
  token of their own. `UserEntraIdentityRule` refuses half an identity and a second user with an
  identity that the transaction can see; the store cannot keep an identity unique across
  transactions, so of several users with one identity the lookup finds the oldest.
  The server side of the plug-in, in the inheritable `Server/Entra` folder, is a thin layer over
  Microsoft.Identity.Web, which validates every token: `AddAllorsEntra` registers the
  authorization code flow with PKCE for a browser and the JWT bearer scheme for a client, from
  the configuration section `Entra` (`TenantId`, `ClientId`, `ClientSecret`, `Instance`,
  `SessionLifetime`), under the scheme names of `EntraDefaults`, and names them to Core, which
  selects between session and token per request and owns the session. The server refuses to
  start without a tenant id, which must be the GUID of one tenant, a client id and a client
  credential. `EntraAdmission` connects a validated principal to a user: it finds the user of the
  principal's identity, or has the application's `IUserFactory` create one at the first sign-in
  and writes the seven fields; a browser sign-in refreshes the profile fields, a bearer token
  does not. The plug-in never decides who is admitted or of which class a user is, a person's or
  a program's token alike; `EntraClaims` reads the claims an application's factory decides on.
  `EntraUserResolver` looks the user up by its identity on every request. `MapAllorsEntra` maps
  `/entra/sign-in?returnUrl=` for a browser without a session and `POST /entra/sign-out`, which
  validates the antiforgery token. The session cookie keeps only the Entra identity and the user
  name. `AddAllorsEntraUsers` connects the users of an application that registers
  Microsoft.Identity.Web itself. The test server signs in against a fake Entra of its own in its
  non-inherited `Test/FakeEntra` folder, which serves Microsoft's documents under Microsoft's
  issuer with endpoints on the test server and signs tokens of Microsoft's shape; the test server
  routes the requests of the handlers and of Microsoft.Identity.Web's issuer validator for
  Microsoft's host to it, so the plug-in is tested as configured for production, with every
  validation on, and without a tenant. Pointed at a real tenant by configuration
  (`FakeEntra:Enabled` false), the same server signs in against Microsoft. Build targets
  `DotnetEntraMerge`, `DotnetEntraGenerate`, `DotnetEntraDatabaseTest` and `DotnetEntraTest`,
  CI job `CiDotnetEntraDatabaseTest`, configuration templates in `config/<provider>/entra`.
  `VirtualDispatchTests` and the plug-in guards of `InheritableSurfaceTests` cover the Entra
  tree too.
- Core selects the scheme that authenticates a request, for an authentication plug-in with both a
  browser session and bearer tokens. `AddAllorsServer` registers the scheme
  `AllorsAuthenticationDefaults.AuthenticationScheme` (`Allors`), which forwards a request with an
  `Authorization: Bearer` header to the scheme a plug-in names in
  `AllorsAuthenticationOptions.BearerScheme` and every other request to the `SessionScheme`; the
  plug-in makes it the default scheme. `UseAllorsServer` stops with an actionable error when a
  named scheme is not registered, or when the selecting scheme is the default and no scheme is
  named. Two more rules of the browser session, each only when asked for: outside the Allors API
  the session's challenge goes to `AllorsAuthenticationOptions.ChallengeScheme`, the OpenID Connect
  scheme of a plug-in that signs in elsewhere; and a session ends `SessionLifetime` after its
  sign-in, however often its sliding expiration renewed it, for a plug-in whose identity provider
  cannot end the application's session. Nothing changes for an application that names none of
  them. A guard test in `InheritableSurfaceTests` checks that nothing under `dotnet/Core`
  references the OpenID Connect or JWT bearer handlers, Microsoft's Entra libraries or the token
  libraries underneath them: signing in with an identity provider belongs to a plug-in.
- `Agent`, a second `User` class in the Core test domain next to `Person`, so that the platform
  tests do not assume that every user is a `Person`. The test population has an agent in the
  Administrators group, and tests check access lists, pulls, the test sign-in header, the .NET
  workspaces and the TypeScript adapters for it. With two classes, SQL extents over `User` take
  the path for interfaces with more than one class.

### Changed

- .NET and TypeScript coordinated workspace reads always discover, then repeat the original query
  with held cache inventory. The final response handles changed records, permissions and membership
  without a read restart. Remove the optional discovery switch.
- Prefer diagrams with succinct supporting text in documentation. The
  [workspace read reference](docs/workspace-reads.md) describes save, affected-object refresh,
  the Session editing pause and explicit query completion.
- Simplify workspace read capture, storage validation and server bookkeeping, and shorten the
  read documentation. Record correctness, robustness and
  measured performance as the development priorities, with reviewability required throughout.

- SQL Server always uses Snapshot isolation, including XML loading. Its adapter accepts an
  unset `IsolationLevel` or explicit `Snapshot` and rejects other values with an actionable error.
  Breaking for callers that selected another isolation level: remove that setting or use
  `Snapshot`. The Core, Identity, and Entra console programs no longer accept the `-i` isolation
  override. PostgreSQL retains programmatic isolation overrides. XML loading keeps its existing
  bulk-copy batches, import phases, and failure cleanup.

- Remove workspace object masking. Within the workspace's configured classes, Pull and Sync
  retain object identities and readable relations retain their targets even when a target's
  roles are unreadable. Role permissions continue to control which values Sync returns.
  This breaks custom security configuration: `IWorkspaceMask` and the `IsMasked` APIs are
  removed. Remove mask service registrations and the masking arguments from calls to the
  `WorkspaceAclsService` and `WorkspaceAccessControl` constructors.

- Move the conditional Sync, Access, and Permission sequence for workspace Pulls into the
  database connection in .NET and TypeScript. Sessions retain response handling and merge
  refreshed records only after synchronization completes; request conditions, editing baselines,
  and permission-refresh behavior stay the same.

- The .NET local workspace adapter calls the extracted JSON `Api` in process and reuses
  the JSON workspace/session implementation. Requests use one database transaction each,
  with the user set before dispatch; the API builders own commits and rollbacks. Calls
  remain synchronous on the calling thread and must be sequential per connection. Record
  storage stays private to each connection; no HTTP or JSON text round trip is involved.
  Local bootstrap now uses `Remote.Configuration` and constructs `Local.DatabaseConnection`
  with the configuration, workspace-services builder, database, user id, `IdGenerator`,
  and ranges. Its user id is immutable. The duplicate Local operation, workspace, session,
  strategy and record types, `Local.Configuration`, and `Allors.Workspace.Protocol.Direct`
  are removed, along with the unused, always-null `UserId` on the Remote connections.
  Local pull results now follow the JSON API's ordering and case-insensitive result-name
  lookup.
- The JSON protocol's `Api` and tracing events move from the Core server into
  `Allors.Database.Workspace.Json`, so hosts can use the same entry point from System.
  Object creation, derivation and metadata use System interfaces; the namespace,
  constructor and protocol messages stay the same. `Api.M` and `Api.MetaPopulation`
  now expose `IMetaPopulation`; consumers of the moved types must rebuild.
- Domain inheritance, breaking for code that reads the repository model or builds a meta
  population by hand: `ExtendsAttribute.Value` is `Values`, the repository `Domain.Base` is
  `DirectSuperdomains`, and `Repository.SortedDomains`, which no code used, is gone. The
  generator stops with an actionable error on a `[Extends]` it used to swallow: a name that is no
  domain, a domain listed twice, a domain that extends itself, or two domains with one name.
  `MetaPopulation.StructuralDerive` stops on a cycle, on a conflicting order and on a second
  domain that no domain extends, naming the domains, where a cycle was cut silently before. The
  generated `MetaBuilder` calls the `Build<Domain>` methods base first, in the order of
  `SortedDomains`, instead of in the order of the repository files. `Domain.Superdomains` lists
  the superdomains nearest first.
- Entra admission no longer serializes user creation, so a slow factory call does not block
  admission of another principal. Concurrent first requests for the same identity can create
  duplicate users or be refused by derivation; lookups continue to select the oldest user
  when duplicates exist.
- Document the v3.2 platform scope: System, Core and the authentication plug-ins Identity and
  Entra, continued domain inheritance, and a planned signals-based API for the .NET and
  TypeScript workspaces with thin UI integrations. Base and Apps are removed without a separate
  continuation; the reactive workspace changes have not landed yet.
- Update development guidance for the platform scope, a default pull-request workflow in which
  creating a pull request requires approval, one pull request per agreed body of work with a
  correct title and description, Purpose Prefixes for branch names, and changelog handling for
  changes ported between branches.
- Start a new changelog from this point; the changelog up to v3.1 remains on the `v3.1` branch.
- Core's server decides only what the Allors API needs, and offers the rest as building blocks
  that an application switches on in its `Startup`. Breaking for every server `Startup`:
  - The JSON API controllers carry `[Authorize]`, so the API always requires an authenticated
    user, whatever the application decides for its own endpoints. A guard test in
    `InheritableSurfaceTests` checks every controller in Core's inheritable server folder.
  - An authentication plug-in tells Core who the signed-in user is through `IUserResolver`;
    `UseAllorsServer` stops with an actionable error unless exactly one is registered.
    `TransactionService` no longer reads the `NameIdentifier` claim itself.
  - The application writes `UseRouting`, `UseAuthentication` and `UseAuthorization` itself and
    calls `UseAllorsServer()` after them. `UseAllorsServer` adds antiforgery for cookie
    sign-ins, the current user and the API endpoints. It no longer builds the database: the
    `Startup` assigns `IDatabaseService.Database` with its own rules, services and command
    timeout.
  - Building blocks: `AddAllorsDefaultDeny` (the authorization fallback policy, before always
    on), `AddAllorsRateLimiting`, `AddAllorsDataProtection`, `UseAllorsForwardedHeaders`,
    `UseAllorsSecurityHeaders` and `ConfigureExceptionHandler`. Response caching, HSTS, HTTPS
    redirection and static files are ASP.NET Core's own calls.
  - ASP.NET Core Identity is registered by its own `AddAllorsIdentity`, which also registers the
    Identity `IUserResolver` and names the Identity application cookie as the browser session
    (`AllorsAuthenticationOptions.SessionScheme`). `IdentityPaths.Authentication` lists the
    Identity pages to rate-limit; rate limiting has no default paths any more.
  - `UserInfo` returns the user name of the signed-in identity.
- Allors logs through `Microsoft.Extensions.Logging` instead of NLog's static `LogManager`, and
  the host of an application decides where the logs go. Before, an application without NLog
  configuration lost these messages, among them the warnings for ignored pull dependencies.
  - `Api` takes an optional `ILogger`; the JSON API controllers pass an `ILogger<Api>`.
  - The commands `Load` and `Save` and the Test commands take an `ILogger<T>` in their
    constructor. Breaking for every commands `Program`: it registers logging and lets McMaster
    inject it with `UseConstructorInjection`.
  - The code generator takes an `ILoggerFactory` in `Generate.Execute`; its command line tool
    logs to the console.
  - Messages are declared once, with `[LoggerMessage]`.
  - The servers and commands of Core and Identity log to the console, one line per message. The
    server configuration templates log `Microsoft.Hosting.Lifetime` at `Information`, so the
    startup lines show; everything else stays at `Warning`.
- The test sign-in header `X-Allors-TestUser` of Core's test server carries the `UniqueId` of a
  user of the test population instead of a user name, and the server finds that user in the
  Allors database instead of through ASP.NET Core Identity. The users the tests sign in as have
  fixed ids, `Users.JaneId` and the others in the Core test domain; the .NET and TypeScript test
  clients keep naming them as before.
- The server side of ASP.NET Core Identity lives in the Identity tree, in the plug-in's
  inheritable folder `dotnet/Identity/Database/Server/Identity`: `AddAllorsIdentity`, the Allors
  user and role stores, the Identity `IUserResolver`, `IdentityPaths` and the login page that
  signs in by user name. Core's server projects no longer reference ASP.NET Core Identity, which
  a guard test in `InheritableSurfaceTests` checks; Core's test server signs in through the test
  header only, with a resolver of its own. The tests of the plug-in moved with it: the cookie
  sign-in, lockout, disabled-user, rate-limit, Identity UI and cookie antiforgery tests run
  against the Identity test server, which pulls an `Organisation` of its test domain for them,
  and the store, resolver and options tests run in the new `Server.Local.Tests` project of the
  Identity tree, through the build target `DotnetIdentityDatabaseTestServerLocal`.
- `UseAllorsServer` maps the Allors API only. Breaking for a server with Razor Pages: it maps
  them in the new endpoints callback, `app.UseAllorsServer(endpoints => endpoints.MapRazorPages())`,
  as the Identity test server does. Before, `UseAllorsServer` mapped Razor Pages itself, so every
  server needed their services or failed at startup.
- The database side of ASP.NET Core Identity lives in the Identity tree too. The Identity domain
  declares the authentication fields of `User`, with the ids they had in Core: the user name,
  e-mail, password hash, security stamp, phone number, two-factor, lockout and disabled fields and
  `Logins`, with the `Login` class, the rules that normalize the name and the e-mail and lock a
  disabled user out, `SetPassword` and `VerifyPassword`, the `IPasswordHasher` service with its
  ASP.NET Core implementation, and the migration `BackfillSecurityRoles`. Core's `User` keeps no
  authentication field: it is a `UniquelyIdentifiable`, `SecurityTokenOwner` and `Deletable`, which
  is all authorization needs, and Core's `Configuration` project no longer references
  `Microsoft.Extensions.Identity.Core`. Breaking for a repository that reads these fields off `User`
  without extending Identity. Two guard tests in `InheritableSurfaceTests` check that nothing under
  `dotnet/Core` references ASP.NET Core Identity and that Core's inheritable folders name no
  authentication field. The Core test domain gives `User` a `UserName` of its own, a name of the
  test population; the test domain of the Identity tree runs the migration in its `Upgrade`, and the
  login, user and upgrade tests run there.
- An authentication plug-in no longer creates users itself: the application's own domain does,
  through the new `IUserFactory` in Core's server folder. Core and the plug-ins know `User` as an
  interface only, so `AllorsUserStore.CreateAsync` of the Identity plug-in asks the factory for
  the user and then writes the fields of ASP.NET Core Identity on it. Registering a factory is
  optional and Core registers none: without one no plug-in creates users, and `CreateAsync`
  returns a failed result that names the seam. Breaking for an application that creates users
  through `UserManager.CreateAsync`: it registers the factory of its domain in `Startup`, with
  `services.AddSingleton<IUserFactory, …>()`. Before, the store built a `Person`, so the plug-in
  compiled only for a domain with a class of that name. A guard test in `InheritableSurfaceTests`
  checks that the folders of a plug-in name no class of the concrete domain that selects it.
- Core owns the browser session, so that every authentication plug-in gets the same one. A
  plug-in names the cookie scheme that keeps a browser signed in, in
  `AllorsAuthenticationOptions.SessionScheme`, and Core applies the rules to that scheme,
  whichever plug-in registers it:
  - The cookie's defaults: the name `__Host-Allors.Auth`, `HttpOnly`, `SameSite=Lax`, `Secure`,
    and a sliding lifetime of 8 hours. What the plug-in or the application configures after
    `AddAllorsServer` wins; the Identity plug-in keeps `Identity:Cookie:ExpireTimeSpan`.
  - A challenge or a refusal for the Allors API answers 401 or 403 instead of a redirect, and
    signing in or out drops the antiforgery cookie. Core wraps the events a plug-in sets on its
    cookie, such as the security stamp validator of ASP.NET Core Identity, and refuses a session
    cookie that takes its events from `EventsType`.
  - Antiforgery asks which scheme authenticated the request, not which type the identity has: an
    unsafe API request that the session scheme authenticated needs a token. A session that signed
    in with OpenID Connect and a bearer token carry the same identity type.

  These rules were part of `AddAllorsIdentity`. One difference for an application with the
  Identity plug-in: outside the Allors API the cookie now follows ASP.NET Core's own rule, which
  answers 401 for an endpoint marked as an API, where the plug-in redirected every request.

### Removed

- Public cold/legacy connection constructors, scoped mode selection and raw legacy operations on
  normal clients. Await the normal factory; all three protected transport capabilities are required.
  Direct legacy server endpoints and shared wire DTOs remain for their separate removal.
- Invoke isolation and continue-on-error options. Invoke executes one protected atomic batch;
  expected rejections return shared results in .NET and TypeScript.
- Separate public Session `PushReadAsync` / `pushRead` operations and their option/result aliases.
  Ordinary Push uses the protected transport with refresh enabled by default.

- Memory-backed workspace support. Local connections, the JSON API and its six public operation
  builders require PostgreSQL or SQL Server at construction. Local integration tests use lazy SQL
  setup; protocol and controlled-transport cases remain service-free. The Memory database adapter,
  database/domain tests and authentication scaffolding are retained.

- Named workspace query routes, the .NET named Session/connection APIs and manual response factory,
  and nine Core/Identity test query controllers. Authentication and antiforgery coverage now uses
  complete reads; ordinary declarative Pull, object Invoke and named outputs remain supported.

- Standalone query result values from .NET and TypeScript workspace APIs, completed reads,
  response messages and generated declarations. Named object/collection outputs, ordinary unit
  roles and scalar inputs remain supported. Complete responses reject the removed root `v`,
  including empty/null maps; clients and hosts must update together.

- Allors procedures across database APIs, workspace APIs in .NET and TypeScript, service
  registration, protocol payloads and server execution. Unit-role persistence and permission
  refresh coverage use ordinary object operations and isolated test setup.

- Prepared declaration registries and `ReadCatalog`, including extent/select reference APIs and
  wire fields in .NET and TypeScript. Queries use inline declarations with arguments, selects,
  includes and named outputs. Strict reads and PushRead discovery reject obsolete reference fields,
  including null values; representation revisions and inline query coverage remain.

- `AllorsAntiforgeryOptions` and its `AuthenticationTypes` collection. Custom authentication
  integrations must set `AllorsAuthenticationOptions.SessionScheme` to their browser cookie's
  registered scheme name instead. The Identity and Entra plug-ins configure this automatically.
- The empty transaction after the Entra test server's database setup. `Setup.Apply()` already
  derives and commits the test population and its permissions.
- The guest user: the setting `Security:AnonymousUserName`. An anonymous request has no user.
- The JSNLog endpoint and the `JSNLog` package from Core's server. No client in the repository
  used it after the TypeScript `jsnlog` package went with Base; an application can add JSNLog
  itself.
- The exception handler's own log entry through NLog. ASP.NET Core's exception handler
  middleware already logs the exception through the application's logger.
- NLog: the packages `NLog`, `NLog.Extensions.Logging` and `NLog.Web.AspNetCore`, the
  `nlog.config` files and the static loggers, from Core, Identity and the code generator. Their
  file targets wrote below `/logs`, which a normal user cannot create on macOS or Linux. A guard
  test in `InheritableSurfaceTests` checks that no project references a logging framework.
- The JSON API controllers' own log entry for a failed request. They rethrew the exception, and
  ASP.NET Core logs it through the application's logger.
- Apps: the domain `dotnet/Apps`, the Angular application and libraries `apps-intranet`, the
  end-to-end tests under `typescript/e2e/AppsIntranet` and `typescript/e2e/old`, their
  configuration templates, build targets and CI jobs. Base and Apps continue on the `v3.1`
  branch.
- Base: the domain `dotnet/Base` with its Blazor component libraries, the Angular applications
  and libraries under `typescript/modules/apps/base` and `typescript/modules/libs/base`, the
  end-to-end tests under `typescript/e2e/Base` with the scaffold generator
  `typescript/e2e/Scaffold`, their configuration templates, build targets and CI jobs, and the
  `libfontconfig1` CI step that only the Base barcode tests needed. The inheritance guard tests
  in Core now cover Core and its test domain only.
- Packages that only Base used, from Core's `Configuration` and `Server` projects:
  `Allors.Documents`, `DataUtils.DataUrl`, `MailKit`, `SkiaSharp`, `SkiaSharp.NativeAssets.Linux`
  and `ZXing.Net.Bindings.SkiaSharp`.
- The Angular toolchain and the packages that only the Base applications used, from the
  TypeScript workspace `typescript/modules`: every `@angular` and `@angular-eslint` package,
  `@nrwl/angular`, `jest-preset-angular`, `jest-chain`, `bootstrap`, `common-tags`, `date-fns`,
  `easymde`, `jsnlog`, `luxon`, `rxjs` and `zone.js`, with the `ngcc` post-install step. The
  workspace keeps nx, jest and eslint; its libraries depend on `cross-fetch` and `tslib` only.
- The obsolete bugfix integration review checklist.
- The `CLAUDE.md` symlink to `AGENTS.md`. Claude Code reads `AGENTS.md` directly from version
  2.1.277.
- Dependabot version updates for the GitHub Actions used in the workflows. Action versions are
  updated by hand.

### Fixed

- Legacy workspace Push validates complete new-ID mappings before any local remap and reports
  malformed acknowledgements as uncertain without replay. Protected derivation diagnostics accept
  nullable or empty messages while requiring relation arrays; both serializers and TypeScript share
  the same validation cases.

- Provision PostgreSQL alongside SQL Server for the Entra CI job so its read-admission tests
  execute on both providers. Other SQL Server jobs retain a single database service.

- Core security recomputes database and workspace token, grant, and revocation entries from
  the supplied transaction instead of retaining six shared derived caches. Rolled-back permissions,
  effective users, and token membership no longer leak into another transaction, and replacing a
  grant's role with an equivalent role reports its current version even when the token version is
  unchanged. Workspace permission metadata, prefetch policies, and per-API ACL memoization remain.

- Workspace Pull merge checks compare .NET binary arrays by byte contents and TypeScript Date
  values by timestamp. An unrelated remote change no longer causes a conflict solely because
  an unchanged unit value was decoded into a new array or Date instance. Actual remote unit
  changes, including transitions to or from null, still conflict with local edits. The existing
  baseline, Reset and Push error-reporting rules remain unchanged.

- The TypeScript JSON workspace client refreshes cached grant and revocation permissions
  referenced by synchronized objects, matching the .NET client's behavior. Refresh requests
  deduplicate the referenced IDs; pulls that need no synchronization keep their existing
  request count. Synchronizing objects can now require an additional access request when
  all referenced access records were already cached. This does not add a general
  authorization-freshness guarantee.
- The .NET JSON workspace client refreshes the grant and revocation permissions referenced
  by synchronized objects, including access records already held by the connection. This
  preserves local permission refresh when routing the local adapter through JSON and also
  applies to both HTTP adapters (`RefreshesKnownGrantWhenAnObjectIsSynchronized` and
  `RefreshesKnownRevocationWhenAnObjectIsSynchronized`). Pulls that need no synchronization
  keep their existing behavior; this does not add a general authorization-freshness guarantee.
  A rejected push of existing objects now returns access errors when no new objects are in
  the request, instead of throwing on the absent new-object map
  (`RejectedExistingObjectPushReturnsAccessErrorAndRollsBack`).
- Local workspace permissions now honor revocations through the JSON API's access and
  permission responses (`SecurityTests.WithRevocation`). System.Text.Json unit conversion
  accepts CLR date/time, Boolean, floating-point and integer values passed directly by
  the in-process adapter, as well as deserialized JSON values (`UnitConversionWithoutSerialization`
  and `UnitConversionWithSerialization`). `JsonApiPullDisposesItsTransaction` checks API
  dispatch, the request user, and transaction rollback on success and failure.
- The Entra guide links to the shared user-factory contract and clarifies that existing users
  need no factory. Factory calls are not limited to an identity's first sign-in.
- Entra resolves user names from raw or mapped `unique_name` and then `email` when neither
  `preferred_username` nor `upn` is available. A v1 guest token without `upn` can now satisfy
  the test factory's user-name requirement, and `UserInfo` and browser sessions retain the
  available name. Existing claim precedence and application-selected bearer name claims are
  preserved; account identity still uses `tid` and `oid`. The fake emits v1 guest tokens
  without `upn` to cover this case.
- Core sets `Path=/` for its default `__Host-Allors.Auth` session cookie outside Development,
  so browsers can accept it when the application has a path base such as `/app`. Sign-in,
  renewal and sign-out use the same root path. Development keeps its existing path-base scope,
  and application overrides of the cookie name and path remain supported.
- Core rejects a cookie session's `ChallengeScheme` at startup when it names that session or
  the `Allors` selector, preventing recursive login challenges. The error directs applications
  to a distinct sign-in scheme, or an unset value for the cookie's own login page. These names
  remain invalid when their forwarding is overridden; a separately named sign-in policy is
  still supported.
- Core preserves the original absolute session lifetime when an application refreshes a cookie
  with its existing authentication properties, including through Identity's `RefreshSignInAsync`.
  Reissuing a cookie no longer restarts `SessionLifetime`; a new sign-in with fresh properties
  still starts a new lifetime, and invalid existing start timestamps remain rejected.
- Entra uses ASP.NET Core's local-URL validation for the sign-in return URL. Control characters
  return 400 instead of causing a 500 during redirect execution, and valid `~/` application-relative
  paths are accepted. External destinations and authority prefixes remain rejected.
- Entra disables incoming OpenID Connect front-channel logout, so unsolicited requests to
  `/signout-oidc` cannot clear a browser session without antiforgery validation. Startup rejects
  later configuration that re-enables `RemoteSignOutPath`, including for custom connected
  schemes. The protected `/entra/sign-out` POST still ends the local and Entra sign-ins and
  returns through the sign-out callback; sign-out from other Entra applications is unsupported.
- Entra browser sign-in preserves stored profile values when claims are missing or empty,
  and changes guest status only for an explicit `acct=0` or `acct=1`. Missing `idp` no longer
  replaces a known home provider with the token issuer. Equivalent public-cloud v1/v2 provider
  URLs keep the user's stored representation; explicit provider changes still update it.
  Document that the provider string is profile metadata, not a canonical customer key.
- Entra validates `SessionLifetime` at startup and uses the validated value for its session
  options. Only an omitted setting defaults to 12 hours; supplied values must be positive
  `hh:mm:ss` or `d.hh:mm:ss` durations, with optional fractional seconds. Malformed, null,
  empty, nonpositive, overflowing and shorthand values now fail with an actionable error
  instead of silently falling back or creating immediately expired sessions.
- Entra rejects and clears a browser session whose local user is missing, so the next sign-in
  challenges Entra instead of repeatedly redirecting to an API that returns 401. The check
  runs inside Core's protected cookie validation, including for custom sessions without an
  absolute lifetime, and preserves application callbacks. Automatic admission remains enabled:
  deletion alone does not prevent a later sign-in or bearer request from creating another user
  when the application's factory still admits the identity.
- Entra sign-out returns 204 when the browser session has expired or is absent, instead of
  starting a new sign-in and returning to a POST-only endpoint with GET. This leaves Entra SSO
  unchanged. Active sessions still require a valid antiforgery token before local and Entra
  sign-out, validated against the cookie's user even when a bearer token is also present.
- Entra and Core validate their authentication event callbacks after all post-configuration
  and refuse invalid configuration at startup. `EventsType` and later replacement of the
  protected callbacks can no longer silently bypass Entra admission, certificate code
  redemption or Core's session rules. Event subclasses cannot override the protected dispatch
  methods. Application callbacks configured before wrapping and later customization of
  unprotected callbacks remain supported.
- Entra refuses admission when reading an existing user or refreshing its profile fails,
  instead of letting the exception escape the authentication callback. Profile validation
  errors and exceptions are logged with the Entra object id; callers receive an admission
  reason without the internal failure details, and uncommitted profile changes are discarded.
- Identity refuses an existing user returned by the application's `IUserFactory` before writing
  authentication fields, preventing account creation from replacing that user's credentials.
  The refusal returns a specific error, logs the factory contract violation and rolls back its
  uncommitted changes.
- Entra browser sign-in redeems authorization codes with certificate credentials from
  `Entra:ClientCredentials`, using a signed client assertion while retaining PKCE and nonce
  validation. Certificate-only configuration no longer sends an unauthenticated token request.
- Core requires antiforgery for session-cookie API requests authorized by a policy naming
  multiple authentication schemes, including when the `Allors` selector forwards to the session.
  A joined authorization ticket no longer bypasses validation; bearer-only requests remain exempt.
- Entra distinguishes programs by the explicit `idtyp=app` claim. A browser user's ID token
  with app roles and no scopes no longer looks like a program or bypasses a factory's rules
  for people. Applications whose factories use `IsApplication()` request `idtyp` in access tokens.
- Entra admits a browser user only after OpenID Connect protocol validation, including the nonce.
  A rejected sign-in no longer leaves a committed user behind; admission refusals still return
  403 without a session, and application callbacks keep control of their responses.
- Entra fills the default bearer principal's name from raw or mapped `upn` in v1 tokens, so
  `UserInfo` returns the user name for both token versions. An application's custom name claim
  or name-claim retriever is preserved.
- The .NET workspace sorts the collection that a composites role is set from. It kept the order
  of the collection, such as objects in the order they were created, and lookups in the role
  then missed objects: the association of such an object was empty and removing it from the role
  had no effect.
- `ITransaction.Instantiate(IObject)` returns null for null in the SQL adapters, as it does in
  the memory adapter. The SQL adapters threw a `NullReferenceException`.
- The generated lazy meta of the .NET workspace compiles for a workspace without inheritance.
  The template `meta.lazy.cs.stg` wrote an untyped array in `BuildInheritances`, which is error
  CS0826 when the array is empty.
- The PostgreSQL adapter tests pool their connections. `Provisioning.ConnectionString` in
  `Allors.Database.Adapters.Sql.Npgsql` set `Pooling=false`, so every transaction opened and
  closed a connection, and every close left a port in `TIME_WAIT`. Against a PostgreSQL in a
  local container, whose port forwarder holds a second port per connection, the suite in
  parallel used up the ephemeral ports of the host within seconds (`Can't assign requested
  address`). It now passes in parallel.
- Pulls retry a transient `DbException` again, as syncs do. `PullController` ran under
  `InvokePolicy` instead of `PullPolicy`, so pulls stopped retrying when Invoke and Push were
  given a policy without retries.
- The server logs the errors of an invalid model. `AddAllorsServer` passed the title of the
  problem details as the message template and the errors as an argument without a placeholder,
  so the log said only "One or more validation errors occurred.".
