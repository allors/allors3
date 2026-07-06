# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(`3.1.0-alpha.{height}`), so the version auto-increments with each commit on `main`.
Changes accumulate under **[Unreleased]** until a version is cut, at which point they move
under a dated version heading.

## [Unreleased]

<!-- Add entries under one of: Added, Changed, Deprecated, Removed, Fixed, Security -->

### Changed

- **The internal `Custom` domain is renamed to `Test`; the name `Custom` is now reserved for
  downstream inheritors' extension domains.** The rename covers the 26 dotnet and 4 e2e `Custom/`
  folders, the domain struct in each layer's Repository (domain GUID unchanged), the runtime-bound
  hook implementations (`CustomOn*` → `TestOn*`, `Custom{Setup,Secure,Prepare}` → `Test…` — Allors
  binds method implementations by `domainName + methodName`), the `Virtual/*.v.cs` dispatch shims,
  the test role `CustomFullName` → `TestFullName` with `PersonTestFullNameRule`, the resource key
  `CustomError` → `TestError`, the Commands scratchpad subcommand (CLI verb `custom` → `test`), and
  the TS workspace libs `derivations-custom` → `derivations-test`. Left unchanged because they mean
  something else: the `extent/custom` panel base classes (the seam for hand-authored inheritor
  panels), the `CustomOrganisationClassification`/`CustomEngagementItem` business types
  (bespoke/user-defined), and Blazor's `CustomValidator`. One behavioral fix folded in: the
  UnifiedProduct default `Scope` hook moved from `CustomOnBuild` to `AppsOnBuild` — the inheritable
  Apps layer must not occupy the custom-domain hook slot (after the rename it would be dead code
  here, and it blocked an inheritor from defining its own `CustomOnBuild` for UnifiedProduct).

### Security

- The Blazor Bootstrap demo site (`Blazor.Bootstrap.Site.Server`) now runs the authentication
  middleware. Its pipeline had `UseAuthorization` without `UseAuthentication`, so
  `HttpContext.User` was never populated from the Identity cookie and the `[Authorize]`
  image/media endpoints (which its pages render via relative `/allors/image` URLs) denied even
  signed-in users; the Blazor-side `AuthorizeRouteView` saw an anonymous user for the same reason.
  Known pre-existing gaps of this local-only demo, left as future work: its plain `AddRazorPages()`
  does not apply `DisableIdentityPagesConvention` (open `/Identity/Account/Register`), and it has
  no authorization fallback policy or antiforgery middleware.
- All 2FA pages of the Identity UI are disabled (404) by default, not just the
  `TwoFactorAuthentication` menu page: `EnableAuthenticator`, `ResetAuthenticator`,
  `GenerateRecoveryCodes`, `ShowRecoveryCodes`, `Disable2fa`, `LoginWith2fa` and
  `LoginWithRecoveryCode` were still directly URL-addressable and threw 500s, because
  `AllorsUserStore` implements neither `IUserAuthenticatorKeyStore` nor
  `IUserTwoFactorRecoveryCodeStore` (2FA remains future work). Note for deployments that override
  `Identity:DisabledPages` in configuration: the array replaces the defaults wholesale, so add the
  new entries to your override.
- The `XSRF-TOKEN` cookie's lifetime now tracks the authentication session. The token was minted
  once — typically on the SPA's *anonymous* bootstrap GET — and antiforgery request tokens are
  bound to the authenticated identity, so after the Identity login every cookie-authenticated POST
  (JSON API and Identity logout) failed with 400 until the browser's cookies were cleared. Sign-in
  and sign-out now delete the cookie (the next safe `/allors` GET re-issues one bound to the
  current user), and an antiforgery validation failure deletes it too, so a stale or undecryptable
  token (e.g. after data-protection key loss) heals on the next request instead of wedging the
  session in a permanent 400 loop.
- Mailer configuration for account recovery is completed and CI-verified. `MailKitMailer` reads
  `Mail:Smtp` as `host` or `host:port` (defaulting to port 25), so a dev SMTP sink on a
  non-privileged port (e.g. 1025) works; an unset `Mail:Smtp` now fails with an actionable message
  instead of a cryptic MailKit error. The dev appsettings (Base + Apps, both providers) gain a
  `Mail` section (`DefaultSender`, `DefaultSenderName`; apps' legacy top-level `DefaultSender` moved
  into it with the fallback kept); `Mail:Smtp` is left for the deployment to set. The send path is
  covered by an in-process SMTP test, so recovery mail is verified without an external mail server.
- **Breaking (Apps domain): the unused admin `ResetPassword` surface is removed.** With self-service
  recovery in place (Identity ForgotPassword → e-mailed reset link), the admin-triggered
  `Person.ResetPassword` — a server-side no-op wired to an orphaned intranet method with no button —
  added no value (it would have e-mailed the same reset link, not let an admin set a password).
  Removed: the `User.ResetPassword` method and its `Person`/`AutomatedAgent` implementations, the
  generated `ResetPassword` permission, the `PersonResetPasswordRevocation` (definition, seed, and its
  branch in `PersonDeniedPermissionRule`), and the dead Angular method. Password recovery is now solely
  the self-service ForgotPassword flow.
- The demo/seed users (jane, john, jenny, and the Apps `administrator`) now have a confirmed
  `UserEmail` equal to their username, so account recovery is reachable: Identity's ForgotPassword
  no longer silently no-ops (it requires a confirmed e-mail). The normalized e-mail derives
  automatically, so lookup by e-mail resolves them (F15 groundwork).
- Queued e-mails are transmitted by a `Mailing` console command rather than an in-process background
  service. `Commands.dll Mailing` opens a transaction as the System automated agent and drains the
  send queue via `EmailMessages.Send`; the deployment's scheduler (the Immediate scheduler, run by
  Windows Task Scheduler) invokes it. The Base and Apps `Commands` now build `DefaultDatabaseServices`
  with configuration so the console mailer is configured (`Mail:Smtp`, `Mail:DefaultSender`). A failed
  transport parks the message (its `DateSending` is set, so it is not retried on the next run) —
  existing `EmailMessages.Send` behaviour, now covered by tests (F15 groundwork).
- ASP.NET Core Identity account-recovery e-mails are now delivered through the Allors mail pipeline
  instead of being discarded. A new `AllorsEmailSender` (`Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`)
  persists each Identity e-mail — the ForgotPassword reset link, e-mail confirmations, admin-triggered
  resets — as an Allors `EmailMessage` in the send queue. Identity's `DefaultMessageEmailSender<TUser>`
  composes the subject/body and delegates to it; registering `AllorsEmailSender` after `AddAllorsServer`
  makes it win over the framework's `NoOpEmailSender`. The sender lives in the Base layer folder, so it
  is compiled into the Base and Apps servers but never into Core (no `EmailMessage` model → keeps NoOp).
  The queued messages are transmitted by the hosted drainer that follows (F15 groundwork).
- Mailer configuration plumbing for the account-recovery work (F15 groundwork): the Base and Apps
  `DatabaseServices` now build their `IMailer` through a `virtual CreateMailer()` seam that reads
  `Mail:Smtp`, `Mail:DefaultSender` (falling back to the legacy top-level `DefaultSender` key) and
  `Mail:DefaultSenderName` from configuration. The hosting seam threads its `IConfiguration` into
  `DefaultDatabaseServices` via an optional constructor argument, so existing call sites and the
  test scopes are unaffected — with no configuration they get an unconfigured mailer, exactly as
  before. No mail is sent yet; this is the prerequisite for the Identity e-mail sender and the
  queue drainer that follow.
- The JSON API is now **default-deny**: the server's authorization `FallbackPolicy` requires an
  authenticated user, so every endpoint is closed unless it explicitly opts out with
  `[AllowAnonymous]`. The six JSON API controllers (pull/push/sync/invoke/access/permission) drop
  their per-action `[Authorize]`/`[AllowAnonymous]` and rely on the fallback; the test-harness
  controllers (`Test`/`TestAuthentication`) are marked `[AllowAnonymous]` so the test rigs can still
  reset, populate and sign in. A controller added without an explicit policy is now denied by
  default rather than silently exposed — the core of the F1 fix for downstream inheritors.
- Added a test-only `X-Allors-TestUser` header credential (scheme `AllorsTestUser`): a request
  carrying the header authenticates as that user without a password, resolving to the same Allors
  user as the JWT/cookie (identical `NameIdentifier` claim). The handler is registered only in the
  Core abstract test-harness server's `Startup` — never in the inherited hosting seam — so it can
  never reach a downstream inheritor. It gives jest and the remote C# suites a password-free
  credential ahead of the JWT retirement. Test infrastructure.
- The jest adapters-json test client authenticates with the `X-Allors-TestUser` header instead of a
  bearer token: it drops the JWT token POST (and the dead `authUrl`), resolves the user id from
  `GET /allors/UserInfo`, and sends the header on every request. Spec behaviour is unchanged. Test
  infrastructure.
- The remaining C# bearer test clients move off JWT: the remote `ApiTest.SignIn` and the
  workspace-remote `Profile.Login` authenticate with the `X-Allors-TestUser` header, and the
  login-mechanism tests (`SignInTests`, `LockoutTests`) drive the Identity cookie form login (whose
  endpoint is the one that remains). This clears the last non-deletion bearer consumers ahead of
  removing the JWT stack. Test infrastructure.
- Retired the `Blazor.Bootstrap.Site.Wasm` demo (removed from the solution). It authenticated with
  the bearer `TestAuthentication/Token` flow, was built by no CI target and referenced by nothing;
  it is removed ahead of the JWT retirement rather than migrated to cookie auth. The other Blazor
  sample projects are unaffected.
- **Breaking: the JWT authentication stack is removed** — the dual-scheme window closes and the
  ASP.NET Core Identity **cookie is now the only authentication scheme**. Deleted: the JWT bearer
  handler + policy scheme from the hosting seam, `AuthenticationController` (the `Authentication/Token`
  endpoint), `IdentityUserExtensions.CreateToken`, the `Token` action on the test authentication
  controllers, `ProductionSecretsGuard` (it guarded the JWT signing key), the `JwtToken` config
  sections, and the `Microsoft.AspNetCore.Authentication.JwtBearer` package. Closes F3 (no unrevocable
  long-lived token remains — only the revocable cookie) and F6 (the weak JWT defaults are gone), and
  removes the token endpoint's enumeration/timing surface (F13). The test harness now authenticates
  via the cookie (browser) or the `X-Allors-TestUser` header (jest / remote suites).
- Removed the development CORS policy (`AddCors`/`UseCors` + the `CorsOrigins` origin lists). Every
  client is now same-origin through the dev proxy (Phase 5), and the C# test clients call the server
  directly (CORS is browser-enforced only), so the policy is dead. JSNLog keeps its own origin regex
  (F14).
- Retired the bearer login from the remote workspace adapters: `DatabaseConnection.Login`/`Logoff`
  (which posted credentials and set the `Authorization: Bearer` header) are removed from both the
  System.Text.Json and Newtonsoft adapters — the last bearer *behaviour* in the tree. Test clients
  authenticate with the `X-Allors-TestUser` header instead. The `AuthenticationToken{Request,Response}`
  DTOs are retained as the test sign-in shapes, with their JWT-era `p`/`t` (password/token) fields
  dropped.
- Added a required `IsDisabled` role to `User`, defaulting to `false` for new users (seeded in the
  post-build), giving the model a first-class account-disabled flag (F10). The rule that acts on it —
  lockout + security-stamp rotation — follows in the next change.
- Disabling a user now takes effect: a rule on `User.IsDisabled` locks the account
  (`UserLockoutEnabled` + `UserLockoutEnd = MaxValue`) and rotates the security stamp, so the
  framework's lockout gate rejects sign-in and the stamp validator invalidates any live cookie within
  the revalidation interval. Re-enabling clears the lockout, resets the failure count, and rotates the
  stamp again. The stamp revalidation interval is now immediate (0) in Development and 5 minutes in
  production, so a disable is observed at once by the test rigs (F10).
- `UserExtensions.SetPassword` now rotates the security stamp, so a programmatic (domain-code)
  password change invalidates any live session on the next revalidation. Identity's own
  password-change path (`AllorsUserStore.UpdateAsync`) already rotates the stamp; this covers the
  domain path without a derive rule that would clobber it (F3).
- **Breaking (domain): the interactive domain password path is retired (F12).** `User` no longer
  inherits `UserPasswordReset`; the `InExistingUserPassword`/`InUserPassword` transient roles, the
  `UserInUserPasswordRule` that consumed them (verify-old-password → strength-check → set hash), and
  the `IPasswordHasher.CheckStrength` composition check are removed. That path skipped the old-password
  proof when no existing password was supplied, never rotated the security stamp, and stayed reachable
  over the JSON API after the UI fields were removed (P5.7). Password management is now the Identity
  `/Identity/Account/Manage` flow; strength is enforced by the configured Identity policy; and
  programmatic `SetPassword` (which rotates the stamp) remains.
- Account lockout is now real: new users default to `UserLockoutEnabled = true` in the post-build, so
  the configured failed-attempt threshold actually locks the account (a required bool otherwise
  defaults to false, leaving lockout inert) (F5). `Upgrade.Execute()` in all three layers backfills
  existing populations via `Users.BackfillSecurityRoles()` — a security stamp for pre-stamp users
  (required for cookie sign-in), the required `IsDisabled=false`, and lockout enabled. **Run
  `Commands.dll Upgrade` against an existing population before serving it.**
- The base **application-app** now authenticates with the Identity cookie instead of a bearer token:
  its API base URL is relative (`/allors/`, same-origin through the proxy, which engages Angular's
  built-in `X-XSRF-TOKEN`), an `APP_INITIALIZER` reads `GET /allors/UserInfo` to learn the user, a
  401 interceptor redirects to `/Identity/Account/Login`, logout posts to `/Identity/Account/Logout`,
  and the in-app login screen + token guard are removed (auth is enforced by the server). The
  `WorkspaceConfig` host map now also covers the dev-server origin (`localhost:4200`) so same-origin
  proxied requests resolve their workspace.
- The **apps-intranet** application-app is likewise cut over to the Identity cookie (relative base
  URL, `UserInfo` bootstrap, 401 redirect, logout, no in-app login screen or route guard). The
  login component's post-authentication bootstrap pull — default internal organisation and singleton
  — moves into the `APP_INITIALIZER` so those ids are primed before the app renders.
- The intranet UI no longer offers domain password entry: the password fields are removed from the
  person form and the user-profile form, and the user-profile menu gains an **Account settings** link
  to the shipped Identity `/Identity/Account/Manage` page. Password management is handled by ASP.NET
  Core Identity; the interactive domain password path is retired from the model in a later phase.
- The **foundation-app** (the Core showcase demo) is cut over to the Identity cookie: relative base
  URL, `/Identity` proxy entry, the 401 `UnauthorizedInterceptor`, and a `UserInfo` bootstrap gate,
  with the dead `authUrl` plumbing removed. It previously reached the API anonymously; it now
  requires an authenticated cookie like the material apps.
- **Breaking (Angular foundation lib):** the bearer/JWT client surface is removed from
  `@allors/base/workspace/angular/foundation` — `AuthenticationService`,
  `AuthenticationSessionStoreService`, `AuthenticationInterceptor`, `AuthenticationConfig`, and the
  `AuthenticationTokenRequest`/`AuthenticationTokenResponse` DTOs. Cookie authentication replaces
  them: register `UnauthorizedInterceptor` (401 → Identity login) instead of the bearer interceptor,
  prime the user with `UserInfoService.init(baseUrl)` in an `APP_INITIALIZER`, and sign out with
  `LogoutService`.
- Media, image and print content is no longer anonymously accessible: `BaseMediaController`
  (print + media) and `BaseImageController` now require authentication. The media actions already
  carried `[Authorize]` behind an `[AllowAnonymous]` override, which is removed; the image action
  gains `[Authorize]`. Cacheable responses switch from `ResponseCacheLocation.Any` (`Cache-Control:
  public`) to `Client` (`private`), so authenticated content is cached only by the browser, never by
  shared proxies/CDNs. Media URLs are relative and same-origin, so the `SameSite=Lax` cookie rides
  `<img>` and print (`window.open`) requests (F1).
- The e2e harness signs a default Identity cookie into the browser context before each test (through
  the dev proxy, same-origin), so once an app is on cookie auth the pre-test navigation loads
  authenticated. Invisible to the still-bearer apps. (Test infrastructure.)
- The test authentication controller gained a passwordless cookie `SignIn` action
  (`SignInManager.SignInAsync`) beside the JWT `Token` action, so browser-context tests can
  authenticate the way the real app will (Identity application cookie, no bearer token). Test-only.
- Added an authenticated `GET /allors/UserInfo` endpoint returning the current user's id and name.
  It is the SPA's user-id source once the browser no longer receives a JWT token response; an
  anonymous request is challenged (401 for `/allors`), which the SPA turns into a login redirect.
- The JSON API now has antiforgery protection scoped to browser (cookie) callers. Safe `/allors`
  responses issue a readable `XSRF-TOKEN` cookie; unsafe `/allors` requests are validated (header
  `X-XSRF-TOKEN` against the antiforgery cookie) **only when the caller authenticated via the Identity
  application cookie**. Bearer, test-header and future API-key clients carry a different authentication
  type and are exempt by construction, so the check applies to exactly the surface that needs it.
- Anonymous requests no longer fault in the object-level security check. Per-request user resolution
  keys off `IsAuthenticated` (rather than a non-null principal), and the grant evaluation
  (`Security.GetVersionedGrants`) null-guards the user before dereferencing its id — so an
  unauthenticated request degrades safely instead of throwing `NullReferenceException`. Anonymous
  access is opt-in via `Security:AnonymousUserName`, which resolves to a real (guest) user by name
  rather than a null user; absent that setting, the request simply has no user, as before (F7).
- The ASP.NET Core Identity UI is now served (`AddRazorPages` + `UseStaticFiles` + `MapRazorPages`),
  so `/Identity/Account/*` (login, manage, logout, …) is live.
- The Identity login page is overridden to accept a **user name** (not an `[EmailAddress]`), since
  Allors user names are not necessarily e-mail addresses. Dangerous or unsupported account pages are
  disabled (404) by a configurable convention (`Identity:DisabledPages`): open registration
  (anonymous user creation), the personal-data pages (which cascade-delete the user), and two-factor
  management (unsupported until a later phase). Override pages live in the inheritable `Core` layer
  folder and are globbed into every server, so inheritors receive them and can further override.
- Authentication now runs through a policy scheme: requests with an `Authorization: Bearer` header
  authenticate via JWT (unchanged), everything else via the ASP.NET Core Identity **application
  cookie**. The cookie is hardened — `HttpOnly`, `SameSite=Lax`, sliding 8-hour expiry, and an
  environment-switched name/secure policy (`__Host-Allors.Auth` + always-`Secure` in production, a
  plain name + `SameAsRequest` in Development) — and `/allors` requests receive a raw 401/403 rather
  than a login-page redirect. A 5-minute security-stamp revalidation interval is the revocation lever
  (stamp rotation on password change / disable follows in a later phase). This opens the window in
  which JWT and cookie auth coexist until every client is migrated off Bearer.
- Added an anti-regression test asserting the inheritable server layer folders (`Core`/`Base`/`Apps`)
  expose no test or bypass controllers — enforcing that destructive/test scaffolding stays in the
  non-inherited `Custom/` folder and can never reach a downstream inheritor's production build (F2).
- JWT lifetime is now 8 hours instead of 30 days. The previous `"30d"` value never parsed as a
  `TimeSpan` and silently fell back to a 30-day default; `"08:00:00"` parses correctly. Production
  Angular builds now point `authUrl` at the real password endpoint (`Authentication/Token`) instead
  of the passwordless test minter (`TestAuthentication/Token`), so a production build no longer ships
  passwordless login.
- Servers now fail fast at startup outside Development when `JwtToken:Key` is missing, still the
  checked-in template value, or shorter than 32 characters — turning a silently-weak signing key
  into a loud, actionable configuration error. Development (and the test rigs) keep using the
  template key.
- The Apps `Person` and `OrganisationContactRelationship` pull controllers now require
  authentication (`[Authorize]`). They previously answered anonymous POSTs with object data;
  unauthenticated callers are now challenged (the authenticated intranet client is unaffected, as it
  already sends its bearer token on every request).
- `AllorsUserStore.HasPasswordAsync` returned the inverse of the truth (true when the user had *no*
  password). It now reports whether a password hash is set, so Identity's account-management pages
  route users to change-password rather than set-password.
- Identity lockout and password policy are now configured explicitly, following NIST 800-63B /
  OWASP ASVS guidance: lockout after 10 failed attempts with a 15-minute auto-unlock (bounded
  lockout beats a hair-trigger hard lock, which is a denial-of-service lever), and passwords
  require length (12+, at least 4 unique characters) instead of composition rules (digit/upper/
  special requirements are off — they push predictable substitutions without adding entropy).
  Deployments can override any value via the `Identity` configuration section. Lockout remains
  inert until `UserLockoutEnabled` is backfilled (upcoming domain phase); breached-password
  checking is a known deferred gap.
- Authentication endpoints are now rate limited per client IP (the forwarded IP behind a trusted
  proxy): fixed window, HTTP 429 on rejection, defaults 10/minute per IP with generous loopback
  headroom for local test rigs. Configure via `Security:AuthenticationRateLimit:{Paths, PermitLimit,
  WindowSeconds, LoopbackPermitLimit}`; the default path list covers the token endpoints and the
  upcoming `/Identity/Account/{Login,ForgotPassword,ResetPassword}` pages. Per-account protection
  remains Identity lockout.
- The server pipeline now processes `X-Forwarded-For`/`X-Forwarded-Proto` first (trusting loopback
  proxies by default; `ForwardedHeaders:KnownProxies`/`:KnownNetworks` configure others), redirects
  http to https outside Development, and emits baseline security headers on every response
  (`X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, a restrictive
  `Permissions-Policy`, and `Content-Security-Policy: frame-ancestors 'none'`, overridable via
  `Security:ContentSecurityPolicy`). JSNLog's browser-log endpoint no longer allows any origin
  (`.*`); it defaults to localhost and is configurable via `Logging:JSNLog:CorsAllowedOriginsRegex`.
- Each server now persists its DataProtection key ring to disk (config `DataProtection:KeysDirectory`,
  default `<ContentRoot>/.allors/dataprotection-keys`) and sets a per-app application name
  (`Allors.Core`/`Allors.Base`/`Allors.Apps`). This keeps future auth cookies and antiforgery tokens
  valid across restarts, and makes protected payloads deliberately non-interchangeable between apps
  (no shared key ring).

### Changed

- Nuke-booted servers (remote/workspace/jest/e2e test rigs) now run with
  `ASPNETCORE_ENVIRONMENT=Development` instead of defaulting to Production. This aligns the test
  rigs with upcoming production-only hardening (fail-fast secret checks, HTTPS redirect) that must
  not apply to local test boots.
- The three server `Startup` classes now delegate to a shared hosting seam
  (`AddAllorsServer`/`UseAllorsServer` under `Core/Database/Server/Core/Hosting/`), so hosting changes
  land once for Core, Base and Apps. Per-app deltas (CORS origins, controllers-with-views) moved into
  `AllorsServerOptions`. Behavioural deltas: the Core server now also sets the `IDatabaseService.Build`
  delegate (Base/Apps already did), and the invalid-model-state log category changed from
  `Allors.Server.Startup` to `Allors.Server`. `ConfigureExceptionHandler` gained an
  `IWebHostEnvironment` overload. The Blazor `Blazor.Bootstrap.Site.Server` project, which
  compile-globs the shared server sources, gained the `JSNLog` and
  `Microsoft.AspNetCore.Authentication.JwtBearer` package references the seam requires.
- Updated the e2e test projects' `Microsoft.Playwright` dependency from 1.59.0 to 1.61.0.
- The part weighted-average cost is now maintained by a dedicated `PartWeightedAverageRule` that only
  recomputes when stock is received, instead of `PartQuantitiesRule` re-reading a part's entire
  inventory-transaction history on every inventory transaction. Consumption transactions (e.g. parts
  scanned on the shopfloor) no longer trigger the full-history recompute, which dramatically speeds up
  scanning of parts with long transaction histories. Behaviour is unchanged — the weighted average
  still updates identically on receipts — and the write is now guarded so an unchanged value no longer
  re-triggers downstream cost derivations.

### Added

- `ARCHITECTURE.md` documenting the abstract-domain model (Core ← Base ← Apps), how inheritance
  works by compile-globbing the layer folders, why `Custom/` scaffolding is never inherited, the
  `Custom` naming overload (and the planned split into a `Test` domain plus a demo `Custom`), and
  the hardening principle: secure defaults in the inherited layer folders, overridable by config.

### Removed

- The **extranet** application has been removed: the Angular app (`apps/apps-extranet`), its five
  workspace libraries (`libs/apps-extranet`), the dead legacy e2e project, and the non-functional
  Nuke E2E build harness (and its CI target). The unused backend `Extranet` meta-workspace
  registration was dropped as well. `apps-intranet` and `base` are unaffected.

### Fixed

- The AppsIntranet e2e `RequestForQuoteTest.CreateRequestForQuoteMaximal` no longer fails between
  local midnight and UTC midnight: its expected request date now derives from the UTC transaction
  clock (`Transaction.Now().Date`) instead of the local `DateTime.Today`, matching the datepicker's
  UTC-midnight storage convention (same pattern as the Base `DatepickerTest`).
- Apps `Setup.v.cs` dispatched `BaseOnPreSetup` from `OnPrePrepare` instead of `BaseOnPrePrepare`
  (latent; both hooks are empty today).
- SQL Server `AllowSnapshotIsolation` now brackets the database name in its `ALTER DATABASE`
  statement, so databases named after reserved T-SQL keywords (e.g. `Identity`) provision correctly.

- `WorkRequirement.WorkEffortPurpose` (enumeration Refurbishment / Maintenance / Repair): defaults to
  **Repair** on init, is copied onto the `WorkTask` created by `CreateWorkTask`, and is mirrored into
  `WorkRequirementVersion` for version history.
- Configuration is now delivered from outside the source tree via the **required** `ALLORS_CONFIG_ROOT`
  environment variable. Each server, command-line tool and integration test loads
  `$ALLORS_CONFIG_ROOT/<domain>/appsettings.json` (domain = `core`/`base`/`apps`) through the new
  `IConfigurationBuilder.AddAllorsConfiguration(domain, …)` helper, with environment variables layered
  last so they override the JSON. A missing variable or missing file fails fast with an actionable error
  instead of silently falling back to per-OS defaults.
- `InstallConfig` Nuke target copies a provider's templates into the config root, e.g.
  `./build.sh InstallConfig --provider npgsql --config-root /opt/allors`.
- Test databases are created on demand from an admin connection: `ALLORS_NPGSQL` / `ALLORS_SQLCLIENT`
  (matching the allors4 CI names) hold a connection allowed to create databases, and each test's connection
  string is derived from them by swapping the database name. The shared `DatabaseProvisioning` helper (over
  the provider-specific `Provisioning` types) drops/creates the database, and the in-process tests (the
  static adapter tests and the Core/Base `Server.Local.Tests`) self-provision a per-test-class database — so
  `dotnet test` runs against the containers with no pre-existing database and no SQL LocalDB. The legacy
  `ALLORS_TEST_SQLCLIENT_CONNECTION` / `ALLORS_TEST_NPGSQL_CONNECTION` names are still accepted as aliases;
  if neither is set the helper fails fast with an actionable error.
- `Commands Init` (Core/Base/Apps) drops and (re)creates the configured database from the admin connection,
  giving the out-of-process server tests and e2e flows a cross-platform provisioning step.
- `launchSettings.json` profiles for the Core/Base/Apps servers that select the database provider
  (e.g. *Core (Postgres)* / *Core (SqlClient)*) by setting `ALLORS_CONFIG_ROOT`.
- A `Merge.Tests` project for the resource `Merger` (`Core/Database/Merge`), driven black-box through its
  public `Input`/`Output` API. It covers structure preservation for overlapping keys (the regression that
  motivated the resx-merge fix — `<value>`/`<comment>` children and `xml:space`/`type` attributes survive),
  last-writer-wins precedence across input directories, key union, `<xsd:schema>`/`<resheader>` survival, and
  `Input` robustness (missing directories skipped, non-`.resx` files ignored, case-insensitive extension). The
  project is wired into the `DotnetCoreDatabaseTest` target (CI `CiDotnetCoreDatabaseTest`) so it actually gates.
- Comprehensive regression coverage for object versioning in `VersioningTests` (Base) — 16 tests over the
  `Order`/`OrderLine` model: changed vs unchanged unit / single-composite / many-composite roles;
  add / remove / clear / repopulate and order-independent set comparison on the many-role; version-snapshot
  history (each version keeps the value at its own derivation); sequential versions; several versioned roles
  changing in one cycle producing a single new version; independent child (`OrderLine`) versioning; and
  non-versioned roles not creating versions. Run by `CiDotnetBaseDatabaseTest`.

### Changed

- The database provider (SqlClient / Npgsql) is now selected by which template populates
  `ALLORS_CONFIG_ROOT` rather than by the host operating system. The in-repo `config/<provider>/<domain>`
  files are templates (with development defaults) copied to the config root (e.g. `/opt/allors`); real
  secrets belong in the deployed copy or in environment variables, not in source.
- Configuration file names are normalized to lowercase `appsettings.json` so the same files resolve on
  case-sensitive filesystems (Linux); previously the loader looked for `appSettings.json`.
- The SqlClient adapter's `LIKE` filter now follows ANSI semantics like the Npgsql and in-memory adapters:
  `%` and `_` are the only wildcards and `[` is matched literally. Previously T-SQL character classes
  (e.g. `[abc]`, `[a-z]`, `[^…]`) were active only on SqlClient, so the same `LIKE` pattern could match
  differently across adapters. Patterns that relied on SqlClient char-classes no longer match as classes
  (that behaviour was never portable to Npgsql/Memory).
- Database provisioning for the Nuke test/e2e targets now runs cross-platform via `Commands Init` against the
  Postgres/SQL Server containers instead of the Windows-only SQL LocalDB step; `--provider` selects both the
  admin connection and the derived `ConnectionStrings__DefaultConnection` passed to the child processes.
- The Npgsql legacy `AppContext` switches (`EnableLegacyTimestampBehavior`, `EnableStoredProcedureCompatMode`)
  are now set by a module initializer in the Npgsql adapter, so the server, command-line tools and tests get
  the same behaviour (previously only the adapter test fixture set them).
- The Core `Server.Local.Tests` / `Server.Remote.Tests` build their database through the adapter-aware
  `DatabaseBuilder` instead of a hardcoded SqlClient type, so they honour the configured provider.

### Removed

- The Windows-only `SqlLocalDB` build helper and the `MartinCostello.SqlLocalDb` build dependency; the build
  no longer provisions SQL LocalDB.
- Legacy per-project `appSettings.json` and `appSettings.{development,osx,windows}.json` files next to the
  servers, command-line tools and server tests. Configuration now comes solely from `ALLORS_CONFIG_ROOT`.

### Fixed

- CI no longer fails intermittently with `Unable to process file command 'env' successfully. Invalid
  format '<version>'`. Under GitHub Actions, Nerdbank.GitVersioning's `SetCloudBuildVersionVars` MSBuild
  target appended its (unused) `Git*` version variables to the shared `$GITHUB_ENV` on every project
  build; because the Nuke build compiles projects in parallel, the concurrent appends interleaved and
  corrupted the file, failing a random matrix job. A new `dotnet/Directory.Build.targets` clears the
  `CloudBuildVersionVars` item before the target runs, skipping the emission. Assembly version stamping
  is unaffected, and `cloudBuild.setVersionVariables: false` does **not** gate this MSBuild-side write.
- The base app's Person overview pulled the wrong object: `onPreSharedPull` fetched `p.Organisation` by the
  Person's `scoped.id`, so no object came back and the overview's `object` (the Person) was null — e.g. the
  breadcrumb `{{ object?.FirstName }}` rendered blank. It now pulls `p.Person`. (The dynamic panels were
  unaffected because they drive off `scoped.id` directly.)
- The extranet app's `MainComponent` leaked its `router.events` subscription: unlike the sibling
  toggle/open/close side-nav subscriptions it was neither stored nor unsubscribed, so it survived component
  teardown. It is now kept in a `routerSubscription` field and unsubscribed in `ngOnDestroy`.
- The intranet `DisplayService.description()` read the `nameByObjectType` map instead of
  `descriptionByObjectType`, so the dynamic summary panel showed an object's name as both its name and its
  description. It now reads `descriptionByObjectType`, falling back to `nameByObjectType` when no description
  role is configured for a type (the map is not yet populated, so behaviour is unchanged until it is).
- The purchase-invoice summary panel's "ship to" and "Bill to End Customer" cards both navigated to the
  `BilledFrom` party on click (copied from the "Billed from" card) instead of the party each card shows. They
  now navigate to `ShipToCustomer` and `BillToEndCustomer` respectively.
- The intranet proposal (quote) list declared `origin` and `destination` columns that were never populated —
  a Proposal has no such roles, and they were absent from the row interface, the row-builder, and the Proposal
  sorter — so they rendered permanently blank (and their `sort: true` was dead). Both columns are removed.
- The intranet non-unified-part list's `type` column was always blank: the column is defined (and sortable),
  but the row-builder never set a `type` key. It now populates `type` from `v.ProductTypeName` (the role the
  column already sorts by, alongside the sibling `brand`/`model`/`kind` derived-name columns).
- The intranet purchase-order list's `customerReference` column showed the order's `Description` instead of
  its `CustomerReference`. The row-builder read `v.Description`; it now reads `v.CustomerReference` (the role
  the column is named for and the list already sorts by).
- The intranet product-characteristic list never sorted: it fetched `sorterService.sorter(m.Brand)`, and the
  matching sorter — whose values are `SerialisedItemCharacteristicType` roles — was registered under the wrong
  composite key `m.SerialisedItemCharacteristic` while the list pulls `SerialisedItemCharacteristicType`. The
  sorter is now registered under `m.SerialisedItemCharacteristicType` and the list fetches it, so the name /
  active columns sort. (No other component fetched the old key.)
- The intranet serialised-item list never sorted: its pull fetched `sorterService.sorter(m.Brand)` — a
  composite with no entry in the sorter service, so `sorter(...)` returned undefined and no sorting was
  applied — even though the pull and result are `SerialisedItem`. It now fetches `sorter(m.SerialisedItem)`,
  so the id / name / categories / availability columns sort.
- The intranet ProductCategory list could not be sorted: its sorter was a copy of the Catalogue sorter, so
  its keys pointed at the foreign `m.Catalogue.*` / `m.Scope.*` roleTypes (which a `ProductCategory` pull has
  no business sorting by) rather than ProductCategory's own roles. It now sorts by `m.ProductCategory.Name`.
  (Sorting the `scope` / parent columns would need derived `<Composite>Name` roles on ProductCategory — a
  dotnet-domain change tracked separately.)
- The intranet WorkRequirement list's `priority` column sorted by the requirement number, not the priority.
  Its sort key was `m.WorkRequirement.SortableRequirementNumber` — the same roleType the `number` column uses
  — so sorting by priority reproduced the number order. It now sorts by `m.WorkRequirement.PriorityName`
  (mirroring how the `state` column uses `RequirementStateName`).
- The party `DisplayPhone` workspace derivation prefixed its output with a spurious `", "`. It joined the
  telecommunications-number display names with `.reduce((acc, cur) => acc + ', ' + cur, '')` seeded with an
  empty string, so the seed contributed a leading separator (e.g. `", Office, Mobile"`). It now uses
  `.join(', ')`.
- The purchase-order-item `TotalIncVat` workspace derivation computed unit VAT on the gross base price
  instead of the net unit price — the order-side sibling of the purchase-invoice-item fix. `unitVat` was
  `unitBasePrice * vatRate`, so the line's discounts and surcharges (accumulated into
  `unitDiscount`/`unitSurcharge` just above) were excluded from the VAT base, and `TotalIncVat` was left
  inconsistent with the sibling `UnitVat` rule. It now applies the rate to
  `unitBasePrice - unitDiscount + unitSurcharge`.
- The purchase-invoice-item `TotalIncVat` workspace derivation computed unit VAT on the gross base price
  instead of the net unit price. `unitVat` was `unitBasePrice * vatRate`, so the line's discounts and
  surcharges — already accumulated into `unitDiscount`/`unitSurcharge` just above — were excluded from the VAT
  base, overstating VAT on discounted lines and understating it on surcharged ones (and leaving `TotalIncVat`
  inconsistent with the sibling `UnitVat` rule, which already applied the rate to the net price). It now
  applies the rate to `unitBasePrice - unitDiscount + unitSurcharge`.
- The extranet work-task create + edit forms bound the FullfillContactMechanism select's options to
  PartyContactMechanisms instead of ContactMechanisms. `onPostPull` assigned the pulled
  `CurrentPartyContactMechanisms` (a PartyContactMechanism collection) straight to
  `contactMechanisms: ContactMechanism[]`; it now maps each to its `.ContactMechanism`, so the picker offers
  the contact mechanisms the `FullfillContactMechanism` role expects.
- The employment form no longer overwrites an existing employment's FromDate on edit. `onPostPull` set
  `this.object.FromDate = new Date()` unconditionally, so opening an employment to edit it reset its start
  date to today (persisted on save). The default is now guarded by `this.createRequest`, so only a new
  employment gets today's date; a loaded one keeps its own.
- The purchase-invoice create and edit forms had several mis-wired "add new …" inline cards and
  contact-added handlers, now corrected: the ShipToCustomer add-person card ran
  `billedFromContactPersonAdded` (now `shipToCustomerContactPersonAdded`); the ShipToEndCustomer
  add-customer card ran `billToEndCustomerAdded` (now `shipToEndCustomerAdded`); the BillToEndCustomer
  add-customer card was shown by `*ngIf="addShipToCustomer"` instead of `addBillToEndCustomer`; and two
  contact-added handlers linked the new `OrganisationContactRelationship` to the wrong organisation —
  `billToEndCustomerContactPersonAdded` used `ShipToEndCustomer` (now `BillToEndCustomer`) and
  `shipToCustomerContactPersonAdded` used `BilledFrom` (now `ShipToCustomer`). Each defect was present on
  both the create and edit forms.
- Editing a CustomerShipment or PurchaseReturn no longer silently clears its `ShipToAddress` and
  `ShipToContactPerson` on load. `onPostPull` called `updateShipToParty` without first setting
  `previousShipToparty`, so the `ShipToParty !== previousShipToparty` guard inside it was true on the initial
  load and nulled `ShipToAddress` + `ShipToContactPerson` — which then persisted on save (silent data loss on
  every edit). `onPostPull` now initializes `previousShipToparty` to the loaded `ShipToParty`, so a load is no
  longer treated as a party change. (The customershipment instance was an unflagged sibling of the reported
  purchasereturn defect.)
- The email-communication form's "add a To email" inline card saved the new address to `FromEmail` instead
  of `ToEmail` (`toEmailAdded`), overwriting the From address and never setting the recipient. It now assigns
  `ToEmail`. (The sibling `fromEmailAdded` was already correct.)
- The bill-to-end-customer autocomplete on the sales-invoice (create + edit) and sales-order (edit) forms now
  fires `billToEndCustomerSelected` instead of `billToCustomerSelected`. The autocomplete binds the
  `BillToEndCustomer` role correctly, but its `(changed)` handler ran the bill-to-*customer* side-effect
  (`updateBillToCustomer`), so selecting a bill-to-end-customer loaded the wrong party's contacts and
  contact-mechanisms into the dependent dropdowns. It now runs `updateBillToEndCustomer`.
- The UnifiedGood edit form no longer drops product categories on save — the same alias + splice-during-
  iteration defect as the NonUnifiedGood/NonUnifiedPart edit forms. `onPostPull` set `selectedCategories` to
  the *same array reference* as `originalCategories`; `onSave()` then iterated `selectedCategories` while
  `splice`-ing the aliased `originalCategories`, skipping every other `ProductCategory`, which the second
  loop then `removeProduct`-ed. `selectedCategories` is now an independent copy (`[...originalCategories]`),
  so all categories are preserved.
- The work-effort / purchase-order-item assignment form no longer crashes on open. `onPostPull` filtered the
  available purchase orders using `this.workEffort.TakenBy` before `workEffort` was assigned (it was set only
  afterwards), throwing a `TypeError` whenever a candidate order existed. `workEffort` is now established
  before the filter runs.
- The customer-shipment create + edit forms now populate the ship-from contact dropdown with the ship-from
  party's contacts instead of overwriting the ship-to contact options. `updateShipFromParty` assigned the
  ship-from party's `CurrentContacts` to `shipToContacts` (leaving the declared `shipFromContacts` unused),
  so on load — where `updateShipToParty` runs first and `updateShipFromParty` overwrites — the ship-to
  contact dropdown was clobbered with the ship-from party's contacts. It now assigns `shipFromContacts`.
- The purchase-return create + edit forms had the identical `updateShipFromParty` defect: the ship-from
  party's `CurrentContacts` were assigned to `shipToContacts` (leaving the declared `shipFromContacts`
  unused), so the ship-from contact picker stayed empty and the ship-to contact options were overwritten
  with the ship-from party's contacts. Both forms now assign `shipFromContacts`.
- The base-price form's create-time defaults are no longer applied on edit. `onPostPull` unconditionally set
  `FromDate = new Date()` and `PricedBy = internalOrganisation`, so editing an existing BasePrice reset its
  effective-from date to today and its priced-by (persisted on save). Both are now guarded by
  `this.createRequest`, so a loaded BasePrice keeps its own values.
- The NonUnifiedPart edit form no longer drops part categories on save — the same alias + splice-during-
  iteration defect as the NonUnifiedGood edit form. `onPostPull` set `selectedCategories` to the *same array
  reference* as `originalCategories`; `onSave()` then iterated `selectedCategories` while `splice`-ing the
  aliased `originalCategories` inside the loop, skipping every other `PartCategory`, which the second loop
  then `removePart`-ed. `selectedCategories` is now an independent copy (`[...originalCategories]`), so all
  part categories are preserved.
- The PositionTypeRate form no longer nulls kept PositionType assignments on save. `onPostPull` set
  `originalPositionTypes` to the *same array reference* as `selectedPositionTypes`; `save()` then iterated
  `selectedPositionTypes` while `splice`-ing the aliased `originalPositionTypes`, skipping every other
  PositionType, which the second loop then set to `PositionTypeRate = null`. Editing a rate with two or more
  assigned position types and saving unassigned roughly half of them. `originalPositionTypes` is now an
  independent copy (`[...(selectedPositionTypes ?? [])]`), so all assignments are preserved.
- The purchase-invoice create + edit forms no longer clear the wrong assigned ship-to address when the
  ShipToCustomer changes. `updateShipToCustomer`'s change-guard nulled `AssignedShipToEndCustomerAddress`
  (the *end*-customer's field, correctly owned by `updateShipToEndCustomer`) instead of the ship-to-customer's
  own `AssignedShipToCustomerAddress` — so changing the ShipToCustomer left its stale address in place (saved
  against the new customer) while wiping the end-customer's chosen address. It now nulls
  `AssignedShipToCustomerAddress`, matching the adjacent `ShipToCustomerContactPerson` clear.
- The supplier-offering form's `currencySelected` no longer throws when a currency is picked before a
  supplier. Its condition optional-chained `this.object.Supplier?.PreferredCurrency`, but the body assigned
  `this.object.Supplier.PreferredCurrency` unguarded, so with no supplier selected (the `== null` branch
  true) it raised a `TypeError`. The assignment is now guarded by `this.object.Supplier`.
- Apps `Setup.v.cs` dispatched `BaseOnPreSetup` from `OnPrePrepare` instead of `BaseOnPrePrepare`
  (latent; both hooks are empty today).
- The UnifiedGood create form showed its manual ProductNumber input based on `settings.UseGlobalProductNumber`,
  but the bound `ProductNumber` is created (and added to the good) based on `settings.UseProductNumberCounter`.
  With the settings disagreeing, the field could be hidden while an empty product-number identification was
  attached, or shown bound to `undefined`. The input is now gated on `!settings.UseProductNumberCounter`,
  matching its creation.
- SQL Server `AllowSnapshotIsolation` now brackets the database name in its `ALTER DATABASE`
  statement, so databases named after reserved T-SQL keywords (e.g. `Identity`) provision correctly.
- The NonUnifiedGood edit form no longer drops product categories on save. `onPostPull` assigned the same
  array reference to both `selectedCategories` and `originalCategories`, so `save()` iterated
  `selectedCategories` while `splice`-ing the aliased `originalCategories` inside that loop — a
  splice-during-iteration that skipped every other category, which the second loop then `removeProduct`-ed
  from the good. Editing a good with two or more categories and saving dropped roughly half of them.
  `selectedCategories` is now an independent copy (`[...originalCategories]`, with the source `?? []`-guarded
  for the spread), so all categories are preserved.
- Effects watching composites roles (or derived list roles) no longer rerun on every unrelated session
  write. Those signals rebuild their list on every recompute, and the default reference-equality comparer
  counted each fresh array as a change. `ISignalFactory.Computed` now accepts an optional
  `IEqualityComparer<T>` (mirroring `State`), and the adapter passes element-wise comparers for composites
  and derived role signals, restoring the value cutoff.
- Effects no longer run between the record merges of a single pull, push response or reset. Every merged
  record bumped the session graph revision — and flushed effects — individually, so an effect reading roles
  of two pulled objects observed the first object updated while the second still held its stale values
  (and large pulls paid one full propagation per record). Multi-record operations now hold the revision and
  bump once at the end (`Session.HoldGraph`/`ReleaseGraph`), so effects observe only the fully merged state.
- Effects no longer observe torn state or run twice per change. The effect scheduler flushed as soon as
  the first effect was enqueued, while the propagation walk was still marking the remaining subscribers —
  an effect reading two computeds derived from the same signal ran once with one fresh and one stale value,
  then again after the walk finished. `Propagation.Propagate` now holds every touched scheduler for the
  duration of the walk and releases (flushes) them only after all reachable nodes are marked, so each
  change produces exactly one consistent effect run.
- A role write performed inside an effect no longer re-runs that effect forever. `Session.TouchGraph`
  bumped the graph revision by reading the revision signal through its tracked getter, so the writing
  effect subscribed itself to the session-wide revision — always recorded one version behind the bump —
  and was re-scheduled after every flush. The bump now comes from an untracked backing counter, so
  writers never become subscribers of the revision they bump.
- Disposing a parent effect scope while a nested child scope was the active scope no longer leaves the
  signals engine's active scope pointing at the disposed parent. `EffectScopeNode.Dispose` only restored the
  active scope when it was exactly the disposed node, but disposal recurses into child scopes, so the child's
  restore re-targeted its already-disposed (and already unlinked) parent — and effects created afterwards
  registered under that disposed scope, where no outer scope could ever dispose them. `Dispose` now restores
  the active scope whenever it lies anywhere in the disposed subtree, so it lands on the disposed scope's
  outer scope.
- The apps-intranet application's **production** bootstrap no longer crashes — the same `environment.prod.ts`
  `APP_INITIALIZER` defect as the base app. The four-parameter `appInitFactory` had only
  `deps: [WorkspaceService, HttpClient]`, so `createService`/`editService` were injected as `undefined` and
  the initializer threw a `TypeError` at bootstrap. `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`. Production-only: the dev `environment.ts` was already correct.
- The base application's **production** bootstrap no longer crashes. The `environment.prod.ts`
  `APP_INITIALIZER` factory (`appInitFactory`) takes four parameters and assigns
  `createService.createControlByObjectTypeTag` / `editService.editControlByObjectTypeTag`, but its `deps`
  listed only `[WorkspaceService, HttpClient]` — so Angular injected `undefined` for the third and fourth
  arguments and the initializer threw a `TypeError` during bootstrap. This is production-only: the dev
  `environment.ts` already lists all four deps (and the e2e harness serves the dev configuration, so it
  never exercised the prod file). `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`, matching the factory's parameters.
- The apps-extranet application's **production** bootstrap no longer crashes — the same `environment.prod.ts`
  `APP_INITIALIZER` defect as the base and intranet apps. The four-parameter `appInitFactory` had only
  `deps: [WorkspaceService, HttpClient]`, so `createService`/`editService` were injected as `undefined` and
  the initializer threw a `TypeError` at bootstrap. `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`. Production-only: the dev `environment.ts` was already correct.
- The `SalesInvoiceStateRuleTests.ChangedSalesInvoiceItemAmountPaidDeriveSalesInvoiceItemStatePartiallyPaid`
  domain test no longer flakes (~1% of CI runs). `SalesInvoiceItemBuilder.WithDefaults()` drew a random unit
  price in `[1, 100]`; when it rolled `1` the test's `TotalIncVat - 1` partial payment was `0`, so
  `SalesInvoiceStateRule` correctly derived `NotPaid` instead of the asserted `PartiallyPaid`. The test-data
  builders now floor the random unit price at `2`, and a deterministic regression test pins the minimal price
  so the "one unit short of full payment" boundary is always covered.
- Test-population organisation builders now generate unique `Organisation.Name` values by construction.
  `OrganisationBuilderExtensions.WithDefaults`, `WithManufacturerDefaults` and `WithInternalOrganisationDefaults`
  used Bogus `Company.CompanyName()`, which is not unique, so a population occasionally produced two organisations
  with the same name — invisible in allors3 core (no name-uniqueness rule) but an intermittent `DerivationException`
  ("Company with this name already exists") in downstream apps that enforce it. Each generated name now carries a
  monotonic `Interlocked.Increment` suffix, removing the collisions at the source. (Bogus' `faker.UniqueIndex`
  only advances inside the `Faker<T>.Generate()` pipeline, which these builders do not use, so a dedicated counter
  is required.)
- Reassigning a session-origin one-to-many role to a new association now detaches it from the old one.
  `SessionOriginState.addCompositesRoleOne2Many` set the role's association back-pointer to the role itself
  instead of the association, so the "remove from previous association" step targeted the wrong object and the
  role stayed in both associations (violating the one-to-many). It now stores the association.
- Binary unit values are no longer double base64-encoded when pulled. `unitFromJson` returned `btoa(value)` for
  a `Binary` unit, but the wire value is already base64 and the push path (`unitToJson`) sends it through
  unchanged, so the round-trip produced a doubly-encoded value. It now returns the value as-is, matching the
  other units.
- Missing-revocation detection now checks the cached revocations instead of the permissions.
  `ResponseContext.checkForMissingRevocations` tested `database.permissions` rather than
  `database.revocationById`, so it flagged the wrong ids as missing (cached revocations were re-requested and
  genuinely missing ones were not). It now checks `revocationById`, matching `checkForMissingGrants`.
- Removing an item from a session-origin one-to-many role now removes it.
  `SessionOriginState.removeCompositesRoleOne2Many` used `ranges.add` instead of `ranges.remove`, so the item
  was left in place; it now calls `ranges.remove`, matching the many-to-many case.
- Removing an item from a session-origin many-valued role now removes it. `Strategy.removeCompositesRole`
  routed the `Origin.Session` case to `sessionOriginState.addCompositesRole`, so the item was re-added instead
  of removed; it now calls `removeCompositesRole`, matching the `Origin.Database` case.
- Workspace objects are JSON-serializable again. `PrototypeObjectFactory` built each object's `toJSON` to call
  the non-existent `this.strategy.ToJSON()` (PascalCase), so `object.toJSON()` / `JSON.stringify(object)` threw
  `TypeError: this.strategy.ToJSON is not a function`. It now calls the real lowercase `this.strategy.toJSON()`
  (matching the sibling `toString`), so an object serializes to its `{ id }` projection instead of throwing.
- The workspace `nodeLeafs` pointer helper now returns the tree's leaf `Node`s instead of `undefined`.
  `resolveLeafs` is a standalone function, so `results.add(this)` added `this` (`undefined`, not a method
  receiver) for every leaf instead of the leaf `node`; `nodeLeafs` therefore returned a set containing
  `undefined`. It now adds `node`.
- The markdown field no longer leaks its EasyMDE/CodeMirror editor. The component created an EasyMDE editor
  (with a CodeMirror `change` listener) in `ngAfterViewInit` but never tore it down, so destroying the component
  left the editor and its listeners dangling (EasyMDE's `toTextArea` teardown never ran). It now overrides
  `ngOnDestroy` — `super.ngOnDestroy()` then `easyMDE.toTextArea()` — disposing the editor and restoring the
  original textarea.
- The localised-markdown field no longer leaks its EasyMDE/CodeMirror editor. The component created an EasyMDE
  editor (with a CodeMirror `change` listener) in `ngOnInit` but never tore it down, so destroying the component
  left the editor and its listeners dangling (EasyMDE's `toTextArea` teardown never ran). It now implements
  `ngOnDestroy` — `super.ngOnDestroy()` then `easyMDE.toTextArea()` — disposing the editor and restoring the
  original textarea.
- The filter-field dialog no longer saves a non-Between field as a Between range. `apply()` decided
  single-vs-Between solely from whether the `value2` control was truthy, but `value2` is never reset — so a
  value entered for a Between field leaked into a subsequently-selected non-Between field (whose `value2` input
  isn't shown, so the stale value survived) and the field was stored as a range (`FilterField.argument` →
  `[value, value2]`), producing the wrong predicate. `apply()` now takes the Between branch only when the field
  actually is Between (`this.isBetween && value2`).
- The Material single-file upload field can re-select a file after it was removed. `onFileInput` read the
  picked file but never reset the hidden `<input type="file">`, so its `value` kept the previous filename;
  after deleting the media, re-picking the **same** file did not fire the input's `change` event (a file input
  only re-fires when its value changes), so the file could not be re-selected. The input is now reset
  (`input.value = ''`) once the selection has been read.
- The Material prompt dialog's **Cancel** button no longer returns the typed value. Both the Ok and Cancel
  buttons in `dialog.component.html` bound `[mat-dialog-close]="value"`, so cancelling a prompt closed it with
  the same string as Ok; callers test the result for truthiness, so a cancelled prompt was indistinguishable
  from a confirmed one and the typed value was acted upon anyway. Cancel now closes with `undefined` (matching
  dismissal via Escape or a backdrop click), so a non-empty result unambiguously means the user pressed Ok.
- The dynamic **edit extent panel** no longer crashes when a displayed `DateTime` column is unset. Its row
  builder formatted every DateTime cell with `format(value, 'dd-MM-yyyy')` (date-fns), which throws
  `RangeError: Invalid time value` on a null value — so a single unset DateTime (e.g. an included organisation's
  `IncorporationDate`) threw while building the row array and the whole table failed to render. Both the display
  and include-display columns now guard the value (`value != null`) before formatting, matching the panel's
  existing period-date handling; an unset DateTime renders as an empty cell.
- The `Base` server and command-line tools loaded the `core` configuration instead of `base`, so the
  `config/<provider>/base` templates were never used. They now resolve the `base` domain.
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
- CI now runs on `ubuntu-latest` with PostgreSQL and SQL Server provided as **service containers** (the
  Windows-only SqlLocalDB install and the host PostgreSQL service/bootstrap steps are gone). The
  database/server/workspace/e2e suites run on **SqlClient**, each adapter has its own adapter test
  (Memory/SqlClient/Npgsql), and admin connections come from `ALLORS_NPGSQL` / `ALLORS_SQLCLIENT` with
  `Commands Init` provisioning the databases. Previously the database/workspace/e2e targets defaulted to
  the `sqlclient` build provider on a runner with no SQL Server, and aborted on the (fail-fast) missing
  admin connection. Running the full database/server/workspace/e2e suite on **Npgsql** is a tracked
  follow-up — it surfaces pre-existing npgsql-specific issues (result ordering, long index-name truncation,
  an `Equals` empty-pull).
- The Npgsql adapter now connects to the lower-cased database name, matching the database that
  `Provisioning`/`Commands Init` actually creates (PostgreSQL folds unquoted identifiers to lower-case).
  A configured non-lower-case `Database=` (e.g. a deployed `Database=AllorsCore`) previously created
  `allorscore` but left the server trying to connect to `AllorsCore`. `Provisioning.DatabaseName` is
  lower-cased for the same reason.
- Re-initializing a database (`Init`, as the server does on every `Test/Setup`) now resets the
  data-scoped database services (the identity/security/permission caches) in `DatabaseServices.OnInit`.
  `Init` recreates the schema and restarts object-id allocation, but the stale UniqueId→object-id
  mappings were kept, so a repeated `Setup` on Npgsql failed with
  `DerivationException: Grant.Subjects, Grant.SubjectGroups at least one!` — the security `Setup` merger
  resolved a Grant to a wiped id and never re-linked its subjects. Guarded by a new `RepeatedSetupTests`
  (runs under `CiDotnetCoreDatabaseTest` on every adapter). Unblocks the out-of-process Npgsql server
  tests previously noted under "Known limitations".
- The Angular e2e test harness (`Base`, `AppsIntranet`) now builds its database through the adapter-aware
  `DatabaseBuilder` and loads configuration via `ALLORS_CONFIG_ROOT` (`AddAllorsConfiguration`), instead
  of a hardcoded `SqlClient` adapter and local `appSettings.{platform}.json`. The e2e suites therefore run
  on whichever provider the build selects, not only SQL Server.
- The Server/Configuration projects now reference `SkiaSharp.NativeAssets.Linux` (and pin `SkiaSharp` to
  the Servers' `3.119.2`), so the ZXing/SkiaSharp barcode generation ships `libSkiaSharp.so` for Linux.
  Previously the native library was absent on Linux, crashing the Base/Apps domain tests, `Commands
  Populate` and the Server with `libSkiaSharp.so: cannot open shared object file`.
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
