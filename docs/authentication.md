# Authentication

> **Status: Current.**

Three parties share the work of signing a user in. Core authorizes: it decides what a user may
do, and it owns what every sign-in needs, the browser session and the choice of scheme per
request. An authentication plug-in authenticates: it registers its schemes, tells Core which
Allors user a signed-in principal stands for, and writes its own fields on that user. The
application's own domain admits: it decides who becomes a user, and of which class.

Two plug-ins provide authentication today, Identity with ASP.NET Core Identity and Entra with
Microsoft Entra ID. An application selects one by extending it; [Domains](domains.md) describes
the kinds of domains and what selecting a plug-in means.

## What Core does

The Allors API always requires an authenticated user. Its controllers carry `[Authorize]`,
whatever the application decides for its own endpoints;
`InheritableSurfaceTests.InheritableServerControllersRequireAuthorization` checks every
controller in the inheritable server folder.

Core knows the user of a request through one seam, `IUserResolver` in Core's server folder: the
plug-in tells Core which Allors user a signed-in principal stands for, and Core authorizes that
user without reading how it signed in. `UseAllorsServer` stops with an actionable error unless
exactly one resolver is registered; `AllorsServerTests.UseAllorsServerWithoutAUserResolverFails`
and `UseAllorsServerWithTwoUserResolversFails` check that.

The rest depends on what the plug-in names in `AllorsAuthenticationOptions`. A plug-in only
names its schemes; Core applies the rules that every plug-in shares, in one place, and nothing
changes for a plug-in that names none of them.

- **`SessionScheme`**, the cookie that keeps a browser signed in. Core gives the cookie its
  defaults: the name `__Host-Allors.Auth`, `HttpOnly`, `SameSite=Lax`, `Secure` and a sliding
  lifetime of 8 hours; in Development the cookie is `Allors.Auth` and follows the scheme of the
  request, so plain http works. What the plug-in or the application configures afterwards wins.
  A challenge or a refusal for the Allors API answers 401 or 403 instead of a redirect, and
  signing in or out drops the antiforgery cookie. An unsafe API request that this scheme
  authenticated needs the antiforgery token: a safe API request hands out the readable cookie
  `XSRF-TOKEN`, and the browser sends its value back in the header `X-XSRF-TOKEN`. A request
  authenticated only by another scheme, with a bearer token for instance, needs none. When an
  endpoint's authorization policy names several schemes, a session that authenticated the
  request still requires the token, even if a bearer token authenticated too. This also applies
  when a policy names the `Allors` selecting scheme and it forwards to the session.
  `AllorsServerTests.SessionCookieGetsCoresDefaults`, `SessionChallengeAnswersTheApiWith401`
  and `SessionSignInAndSignOutDropTheAntiforgeryCookie` check the cookie, and
  `AllorsAntiforgeryMiddlewareTests.UnsafeRequestAuthenticatedByTheSessionSchemeIsValidated`
  the antiforgery. The Entra HTTP `AntiforgeryTests` check multi-scheme endpoints with
  `AMultiSchemeSessionPostWithoutTheXsrfHeaderIs400`,
  `AMultiSchemeSessionPostWithTheXsrfHeaderSucceeds` and
  `AMultiSchemeBearerPostNeedsNoXsrfHeader`.
- **`BearerScheme`**, the scheme that takes a bearer token. `AddAllorsServer` registers the
  selecting scheme `AllorsAuthenticationDefaults.AuthenticationScheme`, `Allors`, which forwards
  a request with an `Authorization: Bearer` header to the bearer scheme and every other request
  to the session scheme. A plug-in with both makes it the default scheme; until one does, it
  does nothing. The selection is checked by
  `AllorsServerTests.TheSelectingSchemeTakesABearerTokenToTheBearerSchemeAndTheRestToTheSession`.
- **`ChallengeScheme`**, the scheme that signs a browser in elsewhere, an OpenID Connect scheme
  for instance. Outside the Allors API the session's challenge goes to it; the API still answers
  401. `AllorsServerTests.SessionChallengeOutsideTheApiGoesToTheNamedScheme` checks that.
- **`SessionLifetime`**. A session ends that long after its sign-in, however often its sliding
  expiration renewed it: for a plug-in whose identity provider cannot end the application's
  session. `AllorsServerTests.SessionLifetimeEndsTheSessionAfterItsLifetime` checks that.

`UseAllorsServer` stops at start-up when a named scheme is not registered, or when the selecting
scheme is the default and no scheme is named, so that a misconfigured server fails before its
first request; `AllorsServerTests.UseAllorsServerFailsWhenANamedSchemeIsNotRegistered` and
`UseAllorsServerFailsWhenTheSelectingSchemeIsTheDefaultAndNoSchemeIsNamed` check that.

## What a plug-in does

A plug-in registers its schemes with ASP.NET Core, names them to Core, and registers its
`IUserResolver`. For a principal it has no user for, it asks the application's `IUserFactory`,
then writes its own authentication fields on the new user, in a transaction of its own, and
commits. Core's `User` keeps no authentication field, so a plug-in declares the fields it needs
in its own domain; `InheritableSurfaceTests.InheritableCoreFoldersNameNoAuthenticationField`
keeps them out of Core.

- **Identity** signs users in with a user name and a password on its own login page,
  `/Identity/Account/Login`. `AddAllorsIdentity(configuration, environment)` registers ASP.NET
  Core Identity with the Allors user and role stores, reads the `Identity` section, and names
  its application cookie as the session. The application maps the Identity pages in the
  callback of `UseAllorsServer`, `app.UseAllorsServer(endpoints => endpoints.MapRazorPages())`,
  and passes `IdentityPaths.Authentication` to `AddAllorsRateLimiting` when it limits sign-in
  attempts.
- **Entra** signs users in with Microsoft Entra ID: a browser with OpenID Connect and a cookie
  session, a client or a program with an access token of the tenant.
  `AddAllorsEntra(configuration, environment)` reads the `Entra` section and names the session,
  the bearer scheme, the challenge scheme and a session lifetime to Core; `MapAllorsEntra()`
  maps the sign-in and the sign-out. [Sign in with Microsoft Entra ID](entra.md) takes an
  application through it.

## What the application does

The application selects the plug-in by extending it, registers it in `Startup`, and decides who
becomes a user. Core and the plug-ins know `User` as an interface only; the concrete domain
knows its classes, so it is the one that creates users, through `IUserFactory` in Core's server
folder. The application registers its factory in `Startup`, with
`services.AddSingleton<IUserFactory, CustomUserFactory>()`.

The factory sees the claims of the principal and decides both admission and class: it returns
a new user, or null to refuse. It never returns an existing user: a user is created for an
identity, not chosen for it; `EntraAdmissionTests.AFactoryThatReturnsAnExistingUserIsRefused`
checks that for Entra. Registering a factory is optional, and without one no plug-in creates a
user: the plug-in refuses to, names the seam in its answer and in its log, and an application
that has not said whom it admits admits nobody new.
`EntraAdmissionTests.WithoutAFactoryNobodyIsCreated` and
`AllorsUserStoreTests.CreateAsyncWithoutAUserFactoryFailsAndNamesTheSeam` check that for Entra
and for Identity.

What a new user may do is the application's decision too. A plug-in gives a new user no group;
the factory can.

An application that selects no plug-in hosts the Allors API only with a resolver of its own, as
[Domains](domains.md#current-implementation) says.
