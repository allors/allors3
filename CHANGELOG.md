# Changelog

This changelog starts with the v3.2 platform direction. For the changelog up to v3.1, see the
[`v3.1` branch](https://github.com/allors/allors3/blob/v3.1/CHANGELOG.md).

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
[version.json](version.json) specifies `3.2.0-alpha.{height}` for development builds.
Changes accumulate under **[Unreleased]** until a version is released.

## [Unreleased]

### Added

- Documentation for users and maintainers under `docs/`, starting with the domain model:
  functional domains, plug-ins and their hosts, and the concrete domain that selects plug-ins.
  A page on logging says how the host receives the logs of Allors. `AGENTS.md` holds the rules
  for these pages.
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
  signing in with Microsoft Entra ID: Core ← Entra ← Test. So far it has a database side only;
  the server that signs users in follows. The Entra domain declares the Entra identity of a
  `User`, the pair `EntraTenantId` and `EntraObjectId`, and what the directory says about the
  person: `EntraUserName`, `EntraDisplayName` and `EntraEmail`. All five are derived, so nobody
  writes them through the API. Build targets `DotnetEntraMerge`, `DotnetEntraGenerate`,
  `DotnetEntraDatabaseTest` and `DotnetEntraTest`, CI job `CiDotnetEntraDatabaseTest`.
  `VirtualDispatchTests` and the plug-in guards of `InheritableSurfaceTests` cover the Entra
  tree too.
- `Agent`, a second `User` class in the Core test domain next to `Person`, so that the platform
  tests do not assume that every user is a `Person`. The test population has an agent in the
  Administrators group, and tests check access lists, pulls, the test sign-in header, the .NET
  workspaces and the TypeScript adapters for it. With two classes, SQL extents over `User` take
  the path for interfaces with more than one class.

### Changed

- Document the v3.2 platform scope: System, Core and the Identity plug-in for authentication,
  continued domain inheritance, and a planned signals-based API for the .NET and TypeScript
  workspaces with thin UI integrations. Base and Apps are removed without a separate
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
