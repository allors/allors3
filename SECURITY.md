# Allors — Authentication Security Review & Recommended Hardening Architecture

**Target:** Allors framework (`allors3`), .NET 10, Angular 14
**Scope:** The server-side authentication/authorization path — Identity integration (`AllorsUserStore`), JWT issuance/validation, the JSON API controllers, per-request user resolution — plus the browser (Angular) and machine (C#/Jest) clients.
**Audience:** Allors upstream maintainers.
**Nature:** Design/defaults review with a concrete recommended architecture. Findings are code-grounded with file references (line numbers may drift; symbol names are given for durability).

---

## Executive summary

The authentication design is functional but ships defaults and structural gaps that leave **every** downstream application weak out of the box, because the weaknesses live in the framework tree that all consumers inherit. The most serious are:

1. **The entire JSON data API is `[Authorize]` *and* `[AllowAnonymous]`** — the latter wins, so authentication is effectively optional on `pull/sync/push/invoke/access/permission`.
2. **Destructive, unauthenticated test endpoints ship in the server tree.** `TestController.Init()/Setup()` are anonymous GETs that **wipe & reseed the database and create an administrator**; `TestAuthenticationController` **mints a valid token for any username with no password** — and the Angular `environment.prod.ts` points the login at it.
3. **Tokens cannot be revoked.** Per-request principals are trusted blindly; there is no security-stamp check, no logout, no refresh, a 30-day default lifetime, and weak JWT validation (issuer/audience validation disabled, an ASCII/UTF-8 key-encoding mismatch, a sample signing key checked into `config/**`).
4. **Account lockout is wired but inert** — nothing sets the per-user `UserLockoutEnabled` flag, so `CheckPasswordSignInAsync(lockoutOnFailure: true)` never locks anyone.
5. **Anonymous requests fault rather than degrade safely.** With no token the request runs as `User == null`, and object-ACL evaluation dereferences `user.Id` — it crashes rather than returning a curated guest view.

This document confirms and extends a prior review, corrects two of its findings, and then proposes a **defense-in-depth architecture**: retire JWT in favour of ASP.NET Core Identity's revocable **HttpOnly cookie**, issued and consumed by **one origin per app** — a single browser-visible origin serving the Angular SPA, the JSON data API, and the shipped Identity account UI (login + manage), composed at the reverse proxy (Caddy serves the SPA; `/allors` + `/Identity` forward to the app server) or self-contained from one Kestrel. There is no separate identity deployable, no cross-host cookie sharing, and no shared DataProtection key ring. Credentials and account-management UX leave Angular entirely (deletion, not rewrite); the SPA calls a same-origin, default-deny API protected by antiforgery. Passkeys are deliberately deferred; TOTP two-factor keeps a reserved late phase.

| # | Finding | Severity |
|---|---------|----------|
| F1 | Entire JSON API is `[AllowAnonymous]` | **Critical** |
| F2 | Unauthenticated destructive test endpoints in the server tree (`Test/Init`, `Test/Setup`, passwordless token minter) | **Critical** (deploy-dependent) |
| F3 | No token revocation / logout / refresh; `UserSecurityStamp` persisted but never validated | **High** |
| F4 | Per-request principal trusted blindly; no stamp / existence / active check | **High** |
| F5 | Account lockout non-functional by default (`UserLockoutEnabled` never set) | **High** |
| F6 | Weak JWT defaults (no issuer/audience validation, 30-day expiry, ASCII/UTF-8 key mismatch, checked-in sample key) | **High** |
| F7 | Anonymous request runs as `null` user → ACL evaluation faults (guest fallback commented out) | **High** |
| F8 | `AllorsUserStore.HasPasswordAsync` inverted | **Medium** |
| F9 | Anonymous business controllers in Apps (`PersonController`, `OrganisationContactRelationshipController`) | **Medium** |
| F10 | No "account disabled/active" concept | **Medium** |
| F11 | No login rate limiting / anti-automation | **Medium** |
| F12 | Minimal password policy; no breach/history checks; no rehash-on-verify | **Medium** |
| F13 | Username enumeration / timing side-channel in the token endpoint | **Low** |
| F14 | Transport hardening left to the consumer (`UseHttpsRedirection` commented out; no forwarded headers/security headers; JSNLog CORS `.*`) | **Low** |
| F15 | No account-recovery flow wired (real SMTP mailer exists but the reset flow + queue drainer are missing) | **Low / Info** |

**Corrections to the prior review:** (a) `EmailSender`/`MailKitMailer` is a *real* SMTP sender (`dotnet/Base/Database/Configuration/Base/Database/Mailer/MailKitMailer.cs`), registered in Base/Apps — only the reset *flow* and a queue drainer are missing, not the sender (F15). (b) Anonymous access is not a benign "guest view"; with the guest fallback commented out it hands a `null` user to the ACL and faults (F7).

---

## Findings (detail)

**F1 — JSON API is `[AllowAnonymous]`.** `dotnet/Core/Database/Server/Core/Api/Json/{Pull,Sync,Push,Invoke,Access,Permission}Controller.cs` each carry both `[Authorize]` and `[AllowAnonymous]`; `[AllowAnonymous]` wins unconditionally and cannot be overridden by a fallback policy. Every data endpoint is reachable without a token; the only protection is per-object ACL evaluated against whatever user resolves (null for anonymous — see F7).

**F2 — Unauthenticated destructive/bypass endpoints in the tree.** `dotnet/{Core,Base,Apps}/Database/Server/{Custom,Controllers}/Test/TestController.cs` expose anonymous GETs `Init()` (`database.Init()` — wipes schema), `Setup()` (wipe + reseed + create an `administrator`), `Restart()`, `TimeShift()`. `TestAuthenticationController` does `FindByNameAsync(l)` → `CreateToken(...)` with **no password check** — a valid token for any user by name. No environment guard or Release exclusion; a downstream build that compiles these into production exposes DB destruction and full auth bypass. `environment.prod.ts` compounds it by defaulting `authUrl` to `TestAuthentication/Token`.

**F3 — No revocation.** Tokens are stateless HS256 JWTs (`dotnet/Core/Database/Server/Core/Identity/IdentityUserExtensions.cs` `CreateToken`) with `Jti` generated but never stored, no security-stamp claim, no logout/refresh endpoint, no `OnTokenValidated` hook. `User.UserSecurityStamp` exists and is seeded once (`UserExtensions.CoreOnPostBuild`) but never emitted or validated. A captured token is valid for its full 30-day life regardless of password change or account deletion; the only revocation is rotating the global signing key.

**F4 — Blind principal trust.** `dotnet/Core/Database/Server/Core/Services/Transaction/TransactionService.cs` reads `ClaimTypes.NameIdentifier` and `Instantiate(userId)` with no stamp/existence/active check.

**F5 — Lockout inert.** `AuthenticationController.cs` requests `lockoutOnFailure: true` and `AllorsUserStore` implements `IUserLockoutStore` fully, but nothing sets per-user `UserLockoutEnabled = true` and no `IdentityOptions.Lockout` is configured, so `IsLockedOut` is always false.

**F6 — Weak JWT defaults.** In all three `Startup.cs`: `ValidateIssuer = false`, `ValidateAudience = false`; validator uses `Encoding.ASCII` while issuance uses `Encoding.UTF8` (latent break for non-ASCII keys); 30-day default; `config/{npgsql,sqlclient}/{core,base,apps}/appsettings.json` ship `"Key": "0123456789ABCDEF0123456789ABCDEF"` and `"Expiration": "30d"` (which never parses via `TimeSpan.TryParse`, silently falling back to 30 days). Token expiry uses `DateTime.Now` (local, not UTC).

**F7 — Anonymous faults.** `TransactionService` leaves `IUserService.User == null` for anonymous requests (the guest fallback is commented out, `:33-36`), and `dotnet/Core/Database/Configuration/Core/Database/Security/Security.cs` does `versionedGrant.UserSet.Contains(user.Id)` — a null user dereferences.

**F8 — `HasPasswordAsync` inverted.** `dotnet/Core/Database/Server/Core/Identity/AllorsUserStore.cs:205` returns `string.IsNullOrWhiteSpace(user.PasswordHash)` — true when there is *no* password. (Evidence the framework change/verify write-path has never been exercised.)

**F9 — Anonymous business controllers.** Apps `Custom/Relation/PersonController.cs` and `OrganisationContactRelationshipController.cs` are `[HttpPost]` with no auth attribute → anonymous object disclosure/mutation.

**F10 — No disable concept.** `User` carries lockout/2FA/confirmed flags but no `IsDisabled`/`IsActive`. Offboarding/compromise response has no first-class mechanism.

**F11 — No rate limiting.** No throttling on the token endpoint; per-account lockout (once fixed) does not stop password spraying.

**F12 — Password policy.** `dotnet/Core/Database/Configuration/Core/Database/PasswordHasher/PasswordHasher.cs` enforces a regex policy (≥8, digit, mixed case, special) only on the domain change path; `IdentityOptions.Password` is unconfigured; no breach/history checks; `VerifyHashedPassword` discards `SuccessRehashNeeded` (no rehash-on-verify). Seed users get passwords equal to their first name.

**F13 — Enumeration/timing.** The token endpoint only computes a hash when the user exists → response timing reveals account existence.

**F14 — Transport.** `app.UseHttpsRedirection()` is commented out in all three servers; `UseHsts()` only in the non-dev branch; no `UseForwardedHeaders`, no security headers; JSNLog `corsAllowedOriginsRegex = ".*"`.

**F15 — Recovery.** A real `MailKitMailer` and an `EmailMessage` queue exist, but nothing drains the queue (`EmailMessages.Send` is never called) and there is no token-based reset/confirm flow; `Person.ResetPassword()` is invoked from Angular but is a server-side no-op.

---

## Recommended architecture

Make each app a **single browser-visible origin** serving its SPA, its data API, and its authentication — composed at the reverse proxy in the recommended deployment (Caddy serves the SPA and forwards `/allors` + `/Identity`), or self-contained from one Kestrel where no proxy exists. Do not centralize logins on a separate identity host, and do not share cookies or key material across hosts.

```
        one browser-visible origin per app, e.g. intranet.example.com
   ┌───────────────────────────────────────────────────────────────┐
   │  Caddy (edge) — automatic HTTPS/certs, http→https redirect    │
   │   ├─ /            Angular SPA (file_server + SPA fallback)    │
   │   ├─ /allors/*    reverse_proxy → 127.0.0.1:5000              │
   │   └─ /Identity/*  reverse_proxy → 127.0.0.1:5000              │
   │       (sets X-Forwarded-For/-Proto; preserves Host)           │
   └───────────────────────────────┬───────────────────────────────┘
                                   │  loopback only — never exposed
   ┌───────────────────────────────▼───────────────────────────────┐
   │  Allors Server (ASP.NET Core, .NET 10)                        │
   │   ├─ /allors/*    JSON data API — default-deny, antiforgery,  │
   │   │               returns raw 401 (no login redirect)         │
   │   └─ /Identity/*  shipped Identity Razor UI:                  │
   │                   Account/{Login,Logout,Lockout}              │
   │                   Account/Manage/* (password; later 2FA)      │
   │   SignInManager/UserManager → AllorsUserStore → Allors DB     │
   │   cookie: __Host- (prod) · HttpOnly · Secure · SameSite=Lax   │
   │   SecurityStampValidator (~5 min) → revocation                │
   │   per-app persisted DataProtection keys (NOT shared)          │
   │   rate limiting on /Identity endpoints · security headers     │
   └───────────────────────────────────────────────────────────────┘
     multiple apps (e.g. intranet) = separate deployments, own cookies
     dev/e2e: the Angular dev-server proxy plays Caddy's role (same paths)
     machines: dev/e2e via TestServer sign-in; production via API keys
     proxyless variant: Kestrel also serves the SPA itself (Phase 7)
```

**Why this shape.**

- **A session cookie must be first-party to the origin that consumes it — and the only consumer is the data API.** Hosting the SPA (static files) and the shipped Identity UI on that same origin removes every cross-origin concern at once: CORS is deleted, Angular's built-in XSRF support engages by itself (it only attaches the header on relative URLs), the cookie can carry the strongest hardening available (the `__Host-` prefix, impossible on `Domain=` cookies), and deployments have no parent-domain constraint.
- **This is activation, not construction.** Every server already calls `AddDefaultIdentity<IdentityUser>().AddAllorsStores()` (`dotnet/{Core,Base,Apps}/Database/Server/Startup.cs`), which internally registers `SignInManager`/`UserManager`, the application cookie (`LoginPath=/Identity/Account/Login`), Razor Pages services, the compiled Identity UI as application parts (`Microsoft.AspNetCore.Identity.UI` 10.0.7 is already referenced), and a no-op mail sender. The JWT bearer registration merely overrides the default authenticate scheme. Lighting up the recommended shape is essentially `MapRazorPages()` + `UseStaticFiles()` + cookie options — plus the page overrides listed under *Framework reuse*.
- **The database is the identity provider; web hosts are stateless views onto it.** `AllorsUserStore` persists passwords, security stamps and lockout state (later: 2FA secrets) in the Allors database. Revocation is therefore DB-side: a password change or account disable rotates `UserSecurityStamp`, and each host's built-in `SecurityStampValidator` invalidates its own cookies within the validation interval. "Log out everywhere" never needed cross-host cookies.
- **No shared DataProtection key ring — by design, not omission.** A shared ring is symmetric trust: any host that can *validate* a shared cookie can equally *forge* one. With per-app keys, a compromised host can impersonate no other host's users; sibling apps hold independent sessions with independent blast radii.
- **Cross-app SSO is deliberately out of scope.** Each app authenticates its own users. (Decided while intranet and extranet coexisted; extranet has since been removed upstream (#253) — the principle stands for any future sibling app. If two apps must genuinely share a login, see Option B below and Appendix A.)

**Principles.**
- **One origin, one cookie.** The session credential never leaves the origin that minted it; no other host can read or mint it.
- **Default-deny.** The data API rejects unauthenticated requests (`FallbackPolicy = RequireAuthenticatedUser`); anonymous/guest access is opt-in via config and, when enabled, resolves to a real Guest agent (never `null`).
- **Config- & environment-driven.** Cookie name/secure switch, workspace host map, guest mode, rate limits — all from `$ALLORS_CONFIG_ROOT/<domain>/appsettings.json`; production fails fast on missing or placeholder secrets.

### Deployment: behind a reverse proxy (Caddy)

The recommended production deployment puts a TLS-terminating reverse proxy — Caddy — in front of each app, and the proxy is what composes the origin: it serves the built Angular bundle and forwards only `/allors/*` and `/Identity/*` to the loopback-bound app server. None of the security model changes (cookies, antiforgery, `SameSite` and `__Host-` are all keyed to the browser-visible origin), but several server details flip from optional to load-bearing:

- **Forwarded headers, first in the pipeline.** Kestrel sees plain http from the proxy. Without `UseForwardedHeaders` (X-Forwarded-For + X-Forwarded-Proto): HSTS never emits (it only fires on https requests), generated absolute URLs (Phase 9 mail links) come out `http://`, per-IP rate limiting collapses every user into the proxy's single bucket, and the Phase 11 audit trail records the proxy as the actor. Trust defaults to loopback (same-host Caddy); `KnownNetworks`/`KnownProxies` come from config for containerized or remote proxies.
- **`CookieSecurePolicy.Always` is required in production, not stylistic.** Behind plain-http Kestrel, `SameAsRequest` would strip the `Secure` attribute and silently break the `__Host-` prefix; `Always` is correct because the browser-side connection is https at the edge. The same applies to the `XSRF-TOKEN` cookie.
- **Division of labour.** Caddy owns TLS, certificates and its automatic http→https redirect; the app keeps emitting security headers + HSTS itself so the framework stays safe in proxyless deployments — emit HSTS in exactly one place, and that place is the app. The app's `UseHttpsRedirection` becomes a natural no-op once X-Forwarded-Proto reads https. Kestrel binds loopback (or a unix socket); the app port is never exposed.
- **Host header.** Caddy preserves the incoming `Host` by default, so the `WorkspaceConfig` host map works unchanged. Other proxies (nginx et al.) rewrite it unless configured (`proxy_set_header Host $host`, or forward `X-Forwarded-Host` and enable it in `ForwardedHeadersOptions`).

Illustrative site block (per app):

```
intranet.example.com {
    handle /allors/*   { reverse_proxy 127.0.0.1:5000 }
    handle /Identity/* { reverse_proxy 127.0.0.1:5000 }
    handle {
        root * /srv/intranet/browser
        try_files {path} /index.html
        file_server
        header /index.html Cache-Control "no-cache"
    }
}
```

Dev and e2e need no proxy binary: the Angular dev-server proxy (`proxy.conf.json`) composes the same origin with the same two paths, so the browser-visible topology is identical in every environment.

### Applicability to Blazor (Server and WebAssembly)

**Verdict: the architecture applies — and for Blazor Server it is not even new.** The repo already contains a working reference implementation: `dotnet/Base/Workspace/Blazor/Blazor.Bootstrap.Site.Server` wires `AddDefaultIdentity<IdentityUser>().AddAllorsStores()`, ships the Identity UI (login/logout are full-page navigations to `/Identity/Account/*`), revalidates the **security stamp** on open circuits via `Areas/Identity/RevalidatingIdentityAuthenticationStateProvider.cs`, and reaches Allors data **in-process** (`Adapters.Local`; the cookie principal is bridged into `DatabaseConnection.UserId` by `ClaimsPrincipalCircuitHandler`/`ClaimsPrincipalServiceMiddleware`). Its data path involves no JSON API and no JWT, so the JWT retirement does not touch it.

Blazor **Server** specifics:
- **Circuits authenticate once, at connect.** No HTTP requests flow over an open circuit, so cookie revocation reaches it only through the revalidating provider — align its `RevalidationInterval` (currently 30 min) with Phase 3's ~5-minute `SecurityStampValidator` interval so disable/"log out everywhere" hits circuits on the same clock.
- **Sign-in/out cannot happen over the circuit** (the response has already started; cookies cannot be set) — full-page navigation to server-rendered Identity pages is *structurally required*. Blazor Server doesn't merely tolerate the shipped-UI design; it mandates it.
- **Behind Caddy:** `reverse_proxy` upgrades the SignalR WebSocket automatically, and the forwarded-headers guidance applies unchanged. `SameSite=Lax` on the auth cookie also blocks cross-site WebSocket hijacking of the circuit handshake (the cookie is not sent on a cross-site WS upgrade).
- **Shared-source caveat:** the project compile-globs the shared Core/Base server sources, so it also *hosts* the `/allors/*` JSON controllers and today's token endpoint (with a leftover `JwtToken` block in its appsettings) even though its own UI never calls them. The shared-tree phases (2, 4, 6) will change/remove that surface — fine, but the project sits in **no solution and no Nuke target**: wire it into CI or expect it to drift silently as the shared tree evolves.

Blazor **WebAssembly** is just another SPA: Option A applies unchanged (same-origin cookie, XSRF header, 401 → login redirect). The dormant WASM demo (`Blazor.Bootstrap.Site.Wasm`) currently does the exact thing being retired — a cross-origin `HttpClient` to `localhost:5000/allors/` with a `TestAuthentication/Token` Bearer login via `Adapters.Remote.SystemText` — and must migrate or retire by Phase 6.

### Options considered

| Option | Shape | Verdict |
|---|---|---|
| **A — single origin per app** (SPA + API + Identity on one browser-visible origin, composed at the edge proxy or self-contained) | No CORS; Angular XSRF works out of the box; `__Host-` cookie; one app server per app; no parent-domain constraint | **Recommended default** |
| B — SPA hosted separately (CDN/static host); API + Identity on one origin | Same registrable domain required; CORS + credentialed XHR + manual XSRF header; SSO across SPAs sharing the API host | For CDN-hosted SPAs, or when exactly two apps must share a login |
| C — dedicated identity host + shared-key-ring domain cookie (this review's v1) | Extra deployable; shared key ring = mutual forgery trust; domain-wide cookie; parent-domain constraint | Appendix A only — many-app SSO estates |
| D — real OIDC identity provider (e.g. OpenIddict) | Protocol machinery; asymmetric signing (no forgery trust); works across registrable domains | The upgrade path if federation or third-party clients ever appear |
| F — keep JWT, harden it (short-lived + refresh rotation) | Refresh-token state ≈ server sessions anyway; rebuilds revocation/sliding/storage that cookie auth ships free; token remains readable by any XSS | Rejected |

**Hybrids.** These compose because identity state is DB-central: start with A and later peel `Account/Manage/*` (or login) onto a dedicated host as a pure re-mapping — that host is just another stateless view of the same store (the v1 "security site", reduced from architecture to configuration). A + API keys for headless machines (Phase 10). B for the one app pair that needs a shared login, A everywhere else. Ship the stock Razor pages now and override individual `.cshtml` files with branded versions later via the shared Razor Class Library.

### Framework reuse — take from Identity vs. keep custom

**Take from Identity (little/no custom code):** the shipped account **UI** (`Account/*`, `Account/Manage/*`) with the overrides below; `SignInManager`/`UserManager` **mechanics** via `AddDefaultTokenProviders()`; **rehash-on-verify** (automatic on the login path — closes the F12 gap); **security-stamp rotation** (automatic on password change/reset) and the built-in `SecurityStampValidator` for **revocation** (no custom revalidator); **password policy** via `IdentityOptions.Password`; a **breach check** as a framework `IPasswordValidator<IdentityUser>`; **lockout** via the already-implemented `IUserLockoutStore` + `IdentityOptions.Lockout`; the built-in **antiforgery** and **rate-limiting** middleware; **DataProtection** for all cookie/token crypto.

**Override before shipping** (as a shared Razor Class Library — the servers share sources via `.cs` compile globs, which never carry `.cshtml`):
- `Account/Login` — the built-in page binds `Input.Email` with `[EmailAddress]`; usernames like `administrator` can never pass validation. Scaffold and relax to a username field.
- `Account/Register` + `RegisterConfirmation` → `NotFound()` — `AllorsUserStore.CreateAsync` builds a real `Person`, so open registration is anonymous user creation. Also remove the Register link from the leftover `Pages/Shared/_loginpartial.cshtml`.
- `Account/Manage/{PersonalData,DeletePersonalData}` → `NotFound()` — `DeleteAsync` cascade-deletes the user.
- `Account/Manage/TwoFactorAuthentication` — hide until Phase 12; the missing store interfaces currently make it fail with `NotSupportedException`.

**Keep custom (Allors persistence/model — not reinvention):** `AllorsUserStore` (+ `IUserAuthenticatorKeyStore`/`IUserTwoFactorRecoveryCodeStore` when Phase 12 lands); the domain model backing it; recovery mail through the Allors `EmailMessage` queue.

**Retire:** the JWT stack (`AddJwtBearer`, `CreateToken`, `JwtToken` config + sample keys, both token endpoints); the dev CORS policy (the dev proxy makes everything same-origin); the interactive domain password path (`UserInUserPasswordRule` + `UserPasswordReset` transient roles + the regex `CheckStrength`) — today it skips the old-password check whenever `InExistingUserPassword` is not supplied, never rotates the security stamp, and remains reachable through the JSON API even after the Angular fields are deleted (object permissions, not UI, are the gate). `SetPassword` stays for seed/programmatic use.

---

## Recommended remediation roadmap

Ordering is dependency-driven so that **every phase ships green** (build + e2e): the destructive test endpoints move out *before* default-deny (Nuke/e2e depend on anonymous `Test/Ready`/`Test/Restart`), JWT and cookies overlap in a **dual-scheme window** until every client is migrated, and antiforgery is scoped so non-browser clients are exempt by construction.

**Phase 0 — hosting seam (refactor).** Extract the near-duplicate `Startup` bodies into `AddAllorsServer`/`UseAllorsServer` under the glob-shared `dotnet/Core/Database/Server/Core/**`, so all hosts (three data servers + the coming TestServers) share one implementation and later phases land once.

**Phase 1 — baseline hardening.** Persisted **per-app** DataProtection + `SetApplicationName` — a hard prerequisite for cookies and antiforgery (without persisted keys, every restart signs everyone out), never shared across apps. `UseForwardedHeaders` **first in the pipeline** (X-Forwarded-For + X-Forwarded-Proto; loopback trust by default, `KnownNetworks`/`KnownProxies` from config) — load-bearing behind the reverse proxy, see *Deployment* above; HSTS + HTTPS redirect (prod; behind Caddy the redirect is a natural no-op, and HSTS is emitted by the app only); a security-headers middleware (nosniff, `frame-ancestors 'none'`, Referrer-Policy, Permissions-Policy, configurable CSP); built-in rate limiting on the auth endpoints (partitioned per **forwarded** client IP + username, with loopback headroom so e2e login loops don't trip it — F11); `IdentityOptions` lockout + password policy (F5 config half, F12); tighten JSNLog CORS (F14). Fix F8 (`HasPasswordAsync` inversion, `AllorsUserStore.cs:205`) — left unfixed, Identity's `Manage/ChangePassword` page later misroutes every real user to `SetPassword`, which dead-ends. Add `[Authorize]` to the anonymous XHR-only business controllers (F9); `Media`/`Image` are consumed via `<img src>` URLs and can only be closed once cookies ride along (Phase 5). Fail fast outside Development on missing/placeholder secrets (the checked-in `0123456789ABCDEF…` key — F6). Two cheap transition mitigations that cannot wait: point `environment.prod.ts` `authUrl` back at `Authentication/Token` (production currently ships **passwordless** login — F2, client half), and drop `JwtToken:Expiration` from 30 days to hours (existing tokens would otherwise outlive the whole migration).

**Phase 2 — TestServer split (early on purpose — F2, server half).** Per stack, a `TestServer` web project that references the production `Server` (controller discovery via application parts) plus the moved test controllers (`Core/Custom/Test`, `Base/Custom/Test`, `Apps/Controllers/Test`); production `Server` stops compiling them. `TestAuthenticationController` moves too but **stays JWT-based for now** — zero client change, e2e stays green. Rewire the Nuke boot (`build/Allors/Server.cs`, publish + e2e targets) to run TestServer in Development; readiness (`/allors/Test/Ready`) and DB reset (`/allors/Test/Restart`) keep working. Add an anti-regression test asserting the production `Server` maps no `/allors/Test/*` route. This unblocks default-deny (Phase 4) and evicts DB-wipe endpoints from production builds at the earliest possible moment.

**Phase 3 — cookie + Identity UI: the dual-scheme window opens.** Replace the JWT default-scheme override with an `AddPolicyScheme` whose `ForwardDefaultSelector` picks JwtBearer when an `Authorization: Bearer` header is present and `IdentityConstants.ApplicationScheme` otherwise — existing Bearer clients are bit-identical while cookies become real. `MapRazorPages()` + `UseStaticFiles()` (the Identity UI's `/Identity/lib/*` static web assets). `ConfigureApplicationCookie`: HttpOnly, `SameSite=Lax`, sliding expiration; environment-switched name/secure policy — `__Host-Allors.Auth` + `SecurePolicy.Always` in production (required behind the TLS-terminating proxy: Kestrel sees plain http, so `SameAsRequest` would strip `Secure` and break `__Host-`), a plain name + `SameAsRequest` in Development (.NET's `CookieContainer` refuses to return `Secure` cookies over plain http, which would silently break the C#/Playwright e2e fixtures); `OnRedirectToLogin`/`OnRedirectToAccessDenied` return raw 401/403 for `/allors/*` and redirect otherwise (a cookie scheme's default login redirect is wrong for JSON callers). `SecurityStampValidatorOptions.ValidationInterval` ≈ 5 minutes — the revocation lever (F3/F4). Ship the must-override pages (shared RCL) listed under *Framework reuse*: username login, Register/PersonalData disabled, 2FA tab hidden. Funneling all failures through one shipped, rate-limited login page also collapses the F13 enumeration/timing surface.

**Phase 4 — default-deny + scoped antiforgery.** Strip the contradictory `[Authorize]` + `[AllowAnonymous]` pairs from the six JSON API controllers and set `FallbackPolicy = RequireAuthenticatedUser` (F1) — the policy authenticates through the Phase 3 policy scheme, so Bearer clients still pass, and the test endpoints now exist only in TestServer (marked `[AllowAnonymous]` there). Fix `TransactionService` (F7): key off `IsAuthenticated`, and when anonymous access is opted in, resolve a real Guest agent — never `null`. Antiforgery: `AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN")`; issue the `XSRF-TOKEN` cookie (non-HttpOnly; `Secure` in production, like the auth cookie) on safe `/allors` responses; validate unsafe `/allors` requests **only when the request authenticated via the cookie scheme** — Bearer, future API-key, and test-header clients are exempt by construction, which is exactly what lets this phase ship before the Angular cutover.

**Phase 5 — client cutover (per app; may sub-phase).** Switch `baseUrl` to relative `'/allors/'` and route the browser through the existing dev proxies, adding `/Identity` to all three `proxy.conf.json` (today only `/allors` and `/jsnlog.logger` are proxied) — the SPA becomes same-origin in every environment. Delete the client auth surface: `AuthenticationSessionStoreService` (the `sessionStorage['ALLORS_JWT']` token), `AuthenticationInterceptor` (Bearer header), and the per-app login routes/components. Add a 401 interceptor that does a top-level `window.location.assign('/Identity/Account/Login?ReturnUrl=<current route>')`; logout is a POST to `/Identity/Account/Logout` carrying the XSRF header, then a hard navigation. Add an authenticated `GET /allors/UserInfo` endpoint: it replaces the token response's `u` field as the SPA's user-id source, re-homes the login component's bootstrap pulls into an `APP_INITIALIZER`, and primes the XSRF cookie before the SPA's first `POST /allors/pull`. Remove the `InUserPassword`/`InExistingUserPassword` fields from the person/userprofile forms; link "Account settings" to `/Identity/Account/Manage`. Close `Media`/`Image` (cookies now accompany `<img>` requests — leaving them anonymous would be the default-deny loophole). Tests: TestServer gains a passwordless **cookie** sign-in (`SignInManager.SignInAsync`) which the Playwright page objects call through the `:4200` proxy so the cookie lands in the browser context; Jest's `fetch-client.ts` switches to a TestServer header credential (non-cookie → antiforgery-exempt).

**Phase 6 — JWT retirement.** Delete `AddJwtBearer` and the policy scheme (`AddDefaultIdentity`'s own application-cookie default takes over), `AuthenticationController` + `TestAuthenticationController`, `IdentityUserExtensions.CreateToken`, the `JwtToken` config sections + sample keys in `config/**`, the `SecurityTokenExpiredException` branch in the exception handler, the JwtBearer package references, the dev CORS policy (same-origin now), and the token request/response DTOs on both sides. Green because Phase 5 left zero built Bearer consumers — the dormant Blazor WASM demo's Bearer login (`Adapters.Remote.SystemText` + `TestAuthentication/Token`) is in no solution or build and must migrate to cookie/API-key auth or retire here. Closes F3/F6 permanently.

**Phase 7 — SPA served at the edge.** Production default: the per-app Caddy site serves the built Angular bundle (`file_server` + `try_files {path} /index.html` SPA fallback; `index.html` no-cache, hashed assets long-cache) and reverse-proxies `/allors/*` + `/Identity/*` to the loopback-bound server — see *Deployment: behind a reverse proxy (Caddy)*. Kestrel needs no new static hosting for this: Phase 3's `UseStaticFiles()` already serves the Identity UI's own `/Identity/lib/*` assets. The self-contained variant (no proxy) hosts the SPA from Kestrel instead: `UseStaticFiles` for the bundle + `MapFallbackToFile("index.html").AllowAnonymous()` (the fallback is an endpoint, so the fallback policy would otherwise 401 the login bounce), with unknown `/allors/*` and `/Identity/*` requests 404ing rather than falling back to the SPA. Either way: delete the dormant `Pages/index.cshtml` in Core/Base — `MapRazorPages` (Phase 3) resurrected it on `/`, where it answers internal traffic and shadows the variant's SPA fallback — and make `WorkspaceConfig` configuration-driven: today it maps only `localhost:5000 → "Default"` and its raw dictionary indexer throws `KeyNotFoundException` for any other host (behind Caddy the public `Host` arrives preserved; other proxies must be configured to forward it). Dev/e2e stay on `ng serve` + proxy. (A different-origin CDN remains Option B.)

**Phase 8 — domain additions.** New model (Core, then `./build.sh Generate`): a dedicated `[Required] [Workspace(Default)] bool IsDisabled` whose rule sets `UserLockoutEnd = MaxValue` and rotates the security stamp — enforcement reuses the framework lockout gate, and the rotation makes disable effective within the validation interval (F10); password history + HIBP breach check as `IPasswordValidator`s (F12). `Upgrade.Execute()` backfills `UserLockoutEnabled = true` (F5 — a behavioural change to call out: lockout starts actually locking), `IsDisabled = false`, **and `UserSecurityStamp`** — populations restored from pre-stamp XML snapshots otherwise throw `InvalidOperationException` at cookie sign-in. Retire the interactive domain password path (see *Framework reuse — Retire*).

**Phase 9 — account recovery (F15).** Register a real `IEmailSender` that enqueues Allors `EmailMessage`s (replacing the `NoOpEmailSender` that `AddDefaultIdentity` registered); add a hosted-service queue drainer (`EmailMessages.Send` is currently never called); enable the shipped ForgotPassword/ResetPassword/ConfirmEmail pages; implement `Person.ResetPassword()` as an admin-triggered reset email. Confirmation/reset links derive from `Request.Scheme`/`Request.Host` — behind the proxy they only come out `https://` with Phase 1's forwarded headers in place.

**Phase 10 — machine / service-account auth.** Scoped, revocable **API keys** bound to service-account `AutomatedAgent`s (Allors already models `System`/`Guest` as agents), validated by a custom auth handler alongside the cookie scheme and scoped by the account's ACL — a narrow, revocable credential for headless callers, not a resurrected blanket bearer token.

**Phase 11 — security audit trail.** An append-only `SecurityEvent` domain object (login success/failure, password change/reset, disable/enable, API-key create/revoke, admin actions) with actor/IP/user-agent (the IP being the forwarded client IP from Phase 1, not the proxy's); Create-only ACL (no update/delete), optional hash-chain tamper-evidence, configurable retention.

**Phase 12 — session lifecycle, step-up & TOTP 2FA.** Absolute + idle timeouts, "remember me"; a DB-backed `ITicketStore` for "my active sessions" + per-session remote sign-out; step-up re-authentication for sensitive operations. TOTP lands here: `IUserAuthenticatorKeyStore` + `IUserTwoFactorRecoveryCodeStore` on `AllorsUserStore`, the backing domain roles (encrypted authenticator key, recovery codes) + `Generate`, and un-hiding the shipped `Manage/TwoFactorAuthentication` pages. **Passkeys stay deferred**; if they are ever added after hosts split, WebAuthn RP-ID scoping (registrable domain) becomes load-bearing again.

---

## Backward-compatibility & migration notes

- **JWT retirement is phased, not big-bang.** The Phase 3–5 dual-scheme window keeps every Bearer client working; Phase 6 removes JWT only once no consumers remain. Browser users are force-logged-out once at their app's Phase 5 cutover — release-notes item.
- **The edge gains the SPA (Phase 7).** In the Caddy default each app's site block adds the SPA root and two proxy routes; the self-contained variant (Kestrel serves the SPA) remains for proxyless consumers. A different-origin CDN remains Option B at the price of CORS + credentialed XHR + a manual XSRF header and a same-registrable-domain constraint.
- **Data migrations** via the established `Commands.dll Upgrade` → `Upgrade.Execute()` hook: `UserLockoutEnabled=true` (lockout starts working — a behavioural change to confirm), `IsDisabled=false`, and `UserSecurityStamp` for populations restored from pre-stamp snapshots. Model changes require `./build.sh Generate`.
- **SameSite:** `Lax` recommended. `Strict` breaks deep links arriving from mail or external documents (the cookie is withheld on cross-site top-level navigation, so users bounce through login); `Lax` + same-origin + antiforgery is CSRF-safe here.
- **The cookie name/secure switch is environment config:** `__Host-Allors.Auth` + always-`Secure` in production (`__Host-` additionally requires `Path=/` and forbids `Domain` — both natural on a single origin); a plain name + `SameAsRequest` in Development, because .NET's `CookieContainer` (the C# e2e fixtures) won't return `Secure` cookies over http.
- **Admin-over-others** is intentionally not a raw-password operation: admins disable (an Allors data op) and trigger reset **emails**; users complete the flow on their app's Identity pages.

## Quick reference — key files

| Area | Path |
|------|------|
| API controllers (`[AllowAnonymous]`) | `dotnet/Core/Database/Server/Core/Api/Json/{Pull,Sync,Push,Invoke,Access,Permission}Controller.cs` |
| Destructive/bypass test controllers | `dotnet/{Core,Base,Apps}/Database/Server/{Custom,Controllers}/Test/{TestController,TestAuthenticationController}.cs` |
| Login endpoint / token issuance | `dotnet/Core/Database/Server/Core/Identity/{AuthenticationController,IdentityUserExtensions}.cs` |
| Identity DI wiring (`AddAllorsStores`) | `dotnet/Core/Database/Server/Core/Identity/IdentityBuilderExtensions.cs` |
| JWT validation config | `dotnet/{Core,Base,Apps}/Database/Server/Startup.cs` |
| Identity store (bug at `:205`) | `dotnet/Core/Database/Server/Core/Identity/AllorsUserStore.cs` |
| Per-request user resolution (null-user fault) | `dotnet/Core/Database/Server/Core/Services/Transaction/TransactionService.cs`; `dotnet/Core/Database/Configuration/Core/Database/Security/Security.cs` |
| Host→workspace map (throws on unmapped hosts) | `dotnet/Core/Database/Server/Core/Services/Workspace/WorkspaceConfig.cs` |
| User domain model | `dotnet/Core/Repository/Domain/Core/Security/User.cs` |
| Password policy / domain rule | `dotnet/Core/Database/Configuration/Core/Database/PasswordHasher/PasswordHasher.cs`; `dotnet/Core/Database/Domain/Core/Rules/User/UserInUserPasswordRule.cs` |
| Anonymous business controllers | `dotnet/Apps/Database/Server/Custom/Relation/{Person,OrganisationContactRelationship}Controller.cs` |
| Mailer / recovery | `dotnet/Base/Database/Configuration/Base/Database/Mailer/MailKitMailer.cs`; `dotnet/Base/Database/Domain/Base/Workflow/EmailMessages.cs` |
| Checked-in sample key | `config/{npgsql,sqlclient}/{core,base,apps}/appsettings.json` |
| Angular auth client (deleted in Phase 5) | `typescript/modules/libs/base/workspace/angular/foundation/src/lib/authentication/{authentication-session-store.service.ts,authentication-interceptor.ts}`; per-app `src/app/auth/login.component.ts` |
| Domain password forms (fields removed in Phase 5) | `typescript/modules/libs/apps-intranet/workspace/angular-material/src/lib/domain/{person,userprofile}/form/` |
| Dev proxies (gain `/Identity` in Phase 5) | `typescript/modules/apps/apps-intranet/workspace/angular-material-app/proxy.conf.json`; `typescript/modules/apps/base/workspace/angular/foundation-app/proxy.conf.json`; `typescript/modules/apps/base/workspace/angular-material/application-app/proxy.conf.json` |
| Angular environments (prod `authUrl` bug) | `typescript/modules/apps/*/workspace/**/src/environments/environment*.ts` |
| e2e boot & fixtures | `build/Allors/{Server,Angular}.cs`; `build/Typescript/E2E/**`; `typescript/e2e/{Base,AppsIntranet}/**` |
| In-repo cookie reference (Blazor Server) | `dotnet/Base/Workspace/Blazor/Blazor.Bootstrap.Site.Server/{Program.cs,Areas/Identity/RevalidatingIdentityAuthenticationStateProvider.cs}` |

---

## Appendix A — if you ever need SSO across many apps

The v1 of this review proposed a dedicated identity host (`security.example.com`) issuing a cookie with `Domain=.example.com`, validated by every app through a shared DataProtection key ring. That shape works and maximally reuses `SignInManager` — but weigh what it costs before reaching for it:

- **A shared key ring is mutual forgery.** DataProtection is symmetric: every host that can validate the shared cookie can also mint one. A box compromised behind one app can forge sessions for every other app.
- The domain cookie is presented to **every** subdomain — including future ones with no business seeing it (cookie tossing, sibling-XSS blast radius) — and `__Host-` hardening is unavailable (`Domain=` is forbidden with it).
- The estate is chained to one registrable domain, one extra deployable, one extra TLS identity, and key-ring distribution/rotation ops.
- Logout semantics are all-or-nothing: deleting the shared cookie signs every app out at once.

(Terminology worth keeping straight: subdomain cookie sharing is **same-site**; true *cross-site* cookies were never needed by any option here.)

If a real multi-app SSO estate emerges, prefer a proper **OIDC identity provider** (e.g. OpenIddict) over the shared-cookie shape: asymmetric signing removes the forgery trust, and standard flows carry sign-on across registrable domains. Because all identity state already lives in the Allors database (the store, the stamps, the lockout state), an OIDC server is an *additive* deployable later — nothing in the single-origin default forecloses it. Passkey caveat for that day: WebAuthn credentials are scoped to an RP ID (a registrable domain), so passkeys registered on one host are only usable on another under a common parent domain.

---

*Prepared as a follow-up hardening review of Allors. Line numbers refer to the reviewed tree and may drift; symbol/method names are given for durability.*
