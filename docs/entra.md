# Sign in with Microsoft Entra ID

> **Status: Current.**

How an application signs its users in with Microsoft Entra ID through the Entra plug-in: the
members of its tenant, the guests invited into it, and the programs registered in it. The
plug-in is a thin layer over Microsoft.Identity.Web, which validates every token; the plug-in
connects a validated principal to an Allors user. [Authentication](authentication.md) explains
how Core, the plug-in and the application's domain share the work; this page gives the steps.

The application's domain extends Entra, as the concrete `Test` domain of the Entra tree does:
Core ← Entra ← Test. The plug-in signs in one tenant, and it never decides whom the application
admits: that is the application's factory, step 4.

## 1. Register the application in the tenant

The plug-in needs one app registration, with:

- A **Web** redirect URI for the sign-in, `https://<host>/signin-oidc`, and one for the
  sign-out, `https://<host>/signout-callback-oidc`: the paths of the OpenID Connect handler.
  `SignInTests.TheSignInEndpointSendsTheBrowserToEntraWithTheCodeFlow` checks the first.
- A **client secret**, or a certificate; the browser sign-in redeems its authorization code
  with it.
- For clients that call the Allors API on behalf of a person: an **Application ID URI** with a
  **scope**, which the client asks a token for. A v2.0 token names the application by its client
  id and a v1.0 token by `api://<client id>`; the plug-in takes both, as
  `BearerTests.AV1TokenIsAcceptedToo` checks.
- For programs that call the API with a token of their own: an **app role** that applications
  may hold, granted to the program's registration.
- Optional claims where the application needs them: `acct` for guests, `email` for the
  profile, and `idtyp` on access tokens when the factory uses `IsApplication()` to distinguish
  programs (step 4). Without `acct`, `IsGuest()` returns false; browser sign-in preserves a
  previously recorded guest status when the claim is missing (step 7).

The tenant id must be the GUID of the application's own tenant: the plug-in signs in that
tenant's members and guests, and refuses `common`, `organizations` and `consumers` at start-up
(`EntraOptionsTests.StartUpRefusesAMissingOrSharedTenant`).

## 2. Configure the plug-in

The plug-in reads the `Entra` section, in the shape of the template
`config/<provider>/entra/appsettings.json`:

```json
"Entra": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<directory (tenant) id>",
  "ClientId": "<application (client) id>",
  "ClientSecret": "<client secret>",
  "SessionLifetime": "12:00:00"
}
```

`Instance` defaults to the public cloud and `SessionLifetime` to 12 hours, both in
`EntraDefaults`. A secret goes in the environment rather than the file:
`Entra__ClientSecret=…`, as the [README](../README.md#configuration) describes.

Omit `SessionLifetime` to use the default. If supplied, it must be a positive duration in
`hh:mm:ss` or `d.hh:mm:ss` format, with two digits each for hours (00–23), minutes and seconds
(00–59). An optional fractional second has a dot followed by one to seven digits: `00:00:00.5`
is half a second. Use `12:00:00` for 12 hours and `1.00:00:00` for one day. Empty or null values,
zero, negative durations, overflow, and shorthand such as `12`, `12h`, `12:00` or `24:00:00`
stop startup with an actionable error. The value is validated and captured when the plug-in is
registered; restart the server after changing it. `EntraOptionsTests.StartUpRefusesAnInvalidSessionLifetime`,
`SessionLifetimeAcceptsExplicitPositiveDurations` and `SessionLifetimeUsesTheValueValidatedAtRegistration`
check this contract.

For a certificate, omit `ClientSecret` (or clear an inherited value) and configure
`ClientCredentials` in the same section:

```json
"ClientCredentials": [
  {
    "SourceType": "Base64Encoded",
    "Base64EncodedValue": "<base64-encoded PFX with its RSA private key>"
  }
]
```

Supply the PFX through a secret store or `Entra__ClientCredentials__0__Base64EncodedValue`,
not a checked-in file. Register its public certificate on the app registration and give the
server access to the private key. The plug-in uses Microsoft.Identity.Web's credential loader;
its [certificate configuration](https://learn.microsoft.com/en-us/entra/msidweb/authentication/certificates)
also describes file, certificate-store and Key Vault sources. Certificate loading is lazy and
cached; restart the server after replacing its configured certificate.

`AddAllorsEntra` signs a short-lived client assertion for each code exchange. ASP.NET Core
still redeems the code with the PKCE verifier and validates the nonce. No downstream token
acquisition or token cache is needed. `EntraOptionsTests.ACertificateCredentialCompletesTheBrowserCodeFlow`
checks two sign-ins against a token endpoint that verifies the certificate signature;
`AnUnregisteredCertificateCannotCreateASessionOrUser`,
`ACertificateWithoutAPrivateKeyCannotCreateASessionOrUser` and
`ACertificateSignInWithTheWrongNonceCreatesNoSessionOrUser` check the refusal paths.
The application's code-received callback can still handle redemption or supply its own credential
(`TheApplicationsCodeHandlerControlsCertificateRedemption`).

The server refuses to start without a tenant id, a client id or a client
credential, each with a message that says what to set
(`EntraOptionsTests.StartUpRefusesAMissingClientId` and `StartUpRefusesAMissingClientCredential`).

## 3. Register the plug-in in `Startup`

After `AddAllorsServer`, register the plug-in and the application's factory; after
`UseAuthentication` and `UseAuthorization`, map the plug-in's endpoints in the callback of
`UseAllorsServer`. The test server of the Entra tree, `dotnet/Entra/Database/Server/Startup.cs`,
does this, and adds its fake Entra for the tests:

```csharp
services.AddAllorsServer(this.Configuration, this.Environment, new AllorsServerOptions
{
    ApplicationName = "Allors.Entra",
});

services.AddAllorsEntra(this.Configuration, this.Environment);

// The concrete domain creates the users that the plug-in asks for.
services.AddSingleton<IUserFactory, TestUserFactory>();
…
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAllorsServer(endpoints => endpoints.MapAllorsEntra());
```

`AddAllorsEntra` registers the authorization code flow with PKCE under the scheme
`EntraDefaults.OpenIdConnectScheme`, the session cookie under `EntraDefaults.SessionScheme`
and the JWT bearer scheme under `EntraDefaults.BearerScheme`, and names them to Core together
with the session lifetime; `EntraOptionsTests` checks each. An application that registers
Microsoft.Identity.Web itself, with scheme names of its own, calls `AddAllorsEntraUsers` with
those names instead, and names its schemes to Core in `AllorsAuthenticationOptions`.

Configure application event callbacks with `services.Configure<OpenIdConnectOptions>` or
`services.Configure<JwtBearerOptions>`, using the corresponding scheme name. The plug-in wraps
`OnTicketReceived` for browser admission and `OnTokenValidated` for bearer admission, and
also wraps `OnAuthorizationCodeReceived` to support certificate credentials when registered
with `AddAllorsEntra`. Leave `EventsType` unset, including on `MicrosoftIdentityOptions`.
Startup refuses a later `PostConfigure` that replaces
`Events` or one of those protected callbacks. Other callbacks, including `OnRemoteFailure`,
remain customizable in `PostConfigure`. An event subclass must inherit the corresponding
framework event methods, so they still invoke the protected callbacks. This applies to custom
scheme names registered with `AddAllorsEntraUsers` too
(`EntraOptionsTests.EntraRefusesConfigurationThatReplacesItsEvents`,
`StartUpRefusesReplacedEntraEvents` and `CustomEntraSchemeNamesCannotBypassAdmission`).
The [session cookie follows Core's event rules](authentication.md#what-core-does).

## 4. Decide whom the application admits

The plug-in identifies every principal by its tenant id and object id, a person's and a
program's alike, and finds the user of that identity with `Users.FindByEntraIdentity`. For an
identity it has no user for, it asks the application's `IUserFactory`, once, at the first
sign-in. The factory sees the validated principal’s claims and decides two things: whether
the principal is admitted, and of which class the user is. `EntraClaims` reads the claims for it:
`IsApplication()` for a program's own token, `IsGuest()` for a guest, `IdentityProvider()`
for where the account lives, `UserName()`, `DisplayName()`, `Email()`, `Scopes()`, `Roles()`
and `ClientApplicationId()`.

`IsApplication()` recognizes only `idtyp=app`. An application that uses this helper must
configure the optional `idtyp` access-token claim in its registration. A person's ID token
can contain application roles without scopes, so those claims alone do not identify a
program. `EntraAdmissionTests.APersonWithAnApplicationRoleAndNoScopesIsAdmittedAsAPerson` and
`ARefusedPersonWithAnApplicationRoleLeavesNoUser` check that these users remain subject to
the factory's person-admission rules.

The factory of the Entra tree's test domain, `TestUserFactory`, is the example: a program
becomes an `Agent`, a person a `Person`, and an account whose user name starts with `refused`
is not admitted:

```csharp
public User Create(ITransaction transaction, ClaimsPrincipal principal)
{
    if (principal.IsApplication())
    {
        return new AgentBuilder(transaction).Build();
    }

    var userName = principal.UserName();
    if (userName == null || userName.StartsWith(RefusedPrefix, StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    return new PersonBuilder(transaction).Build();
}
```

The factory returns a new user or null, never an existing one
(`EntraAdmissionTests.AFactoryThatReturnsAnExistingUserIsRefused`). It gives the user no
group unless the application wants it to: a new user sees what the access control gives every
authenticated user. After the factory, the plug-in writes the seven derived fields of the
Entra domain on the user: the identity, `EntraTenantId` and `EntraObjectId`; what the directory
says about the account, `EntraUserName`, `EntraDisplayName` and `EntraEmail`; and for a guest
its home and status, `EntraIdentityProvider` and `EntraIsGuest`. Nobody writes them through
the API (`UserTests.NobodyWritesTheEntraFieldsThroughAnAccessList`). A browser sign-in
refreshes the profile fields supplied by the token; a bearer token does not
(`EntraAdmissionTests.ASignInRefreshesTheProfileFieldsAndABearerTokenDoesNot`). Missing or
empty user name, display name and email claims preserve their stored values, so these fields
hold the last information supplied, not necessarily a complete snapshot of the directory
(`EntraAdmissionTests.ASignInKeepsProfileFieldsWhoseClaimsAreMissing`). Guest status and
provider follow the rules in [step 7](#7-guests-the-extranet).

If the existing user cannot be read or its refreshed profile cannot be saved, admission is
refused and the server logs the details. A rejected or failed profile derivation leaves the
stored profile unchanged (`EntraAdmissionTests.AFailedProfileRefreshIsRefusedAndRolledBack`).
Failures during lookup or refresh return an admission reason without exposing the exception
details; by default, the browser receives 403 and the bearer handler fails authentication
(`EntraAdmissionTests.ATransactionFailureRefusesAdmissionAndIsLogged` and
`ATransactionFailureStopsTheAuthenticationHandler`).

Without a factory the plug-in creates nobody, refuses the sign-in and logs why
(`EntraAdmissionTests.WithoutAFactoryNobodyIsCreated`). A principal whose token carries no
`tid` or `oid` claim is refused before the factory is asked.

## 5. A browser

The application sends a browser that has no session to `/entra/sign-in?returnUrl=/…`: the
plug-in signs it in with Entra and sends it on to the `returnUrl`, a local path only
(`SignInTests.TheReturnUrlMustBeLocal`). Root-relative paths such as `/orders` and
application-relative paths such as `~/orders` are accepted, including query strings and
fragments (`SignInTests.AValidLocalReturnUrlKeepsItsDestination`). Control characters return
400 instead of failing during redirect execution (`SignInTests.AReturnUrlWithControlCharactersIs400`);
authority prefixes, including slash/backslash variants, remain invalid
(`SignInTests.AReturnUrlCannotStartWithAnAuthority`).

The session cookie keeps the Entra identity and the user name, nothing else of the token
(`EntraAdmissionTests.TheSessionPrincipalCarriesTheIdentityAndTheName`), and Core's rules apply
to it: an anonymous request to the Allors API gets 401 without a redirect or a cookie
(`SignInTests.AnAnonymousApiRequestGets401WithoutARedirectOrACookie`), and
an unsafe API request needs the antiforgery token. An account the factory refuses gets 403, no
session and no user (`SignInTests.ARefusedAccountGets403WithoutASessionOrAUser`). Admission
runs only after the protocol checks succeed, including the nonce; a rejected callback leaves
no user or session (`SignInTests.ASignInWithTheWrongNonceLeavesNoSessionOrUser`).

The session ends `SessionLifetime` after its sign-in, 12 hours by default, however often the
browser renewed it. The plug-in does not accept incoming Entra sign-out requests, so the
application bounds the session locally.
Each request looks the user up by its identity (`EntraUserResolverTests`). If the browser
session's user is missing, the plug-in rejects the session and clears its cookie. The API
returns 401, and `/entra/sign-in` starts a new Entra sign-in instead of sending the browser
back to an API that still refuses it
(`SignInTests.ADeletedUserLosesItsBrowserSessionAtTheNextRequest` and
`ADeletedUserMustSignInAgainAndMayBeReadmitted`). This check also applies to custom session
schemes without an absolute lifetime, and runs on the principal left by the application's
validation callback (`EntraOptionsTests.CustomSessionsWithoutALifetimeStillCheckEntraUsers`
and `SessionValidationChecksTheApplicationsFinalPrincipal`).

Deleting a local user does not permanently revoke its Entra identity. A later browser sign-in
or bearer request can create another user if the application's factory still admits it
(`SignInTests.ADeletedUserMustSignInAgainAndMayBeReadmitted` and
`BearerTests.ADeletedUserMayBeReadmittedWithTheSameBearerToken`). The replacement receives
the factory's current grants, not the deleted user's memberships. If a replacement exists
before an old browser session is checked, that session resolves to the replacement
(`BearerTests.AnExistingSessionResolvesAUserReadmittedBeforeItsNextRequest`). To prevent
readmission, the application must retain its refusal policy independently of the deleted user
and have its factory refuse that identity.

To sign out, the browser posts to `/entra/sign-out` with the header `X-XSRF-TOKEN` set to the
value of the `XSRF-TOKEN` cookie that a safe API request handed out, `GET /allors/UserInfo` for
instance. For an active session, that ends the session and the sign-in with Entra; without a
valid token the request is refused with 400. The token is validated against the session's user,
even if the request also carries a bearer token
(`SignOutTests.SignOutWithABearerHeaderStillProtectsTheSession`).

Incoming Entra front-channel logout is disabled. The session does not retain the `sid` and
`iss` claims needed to correlate those requests, so the framework's remote logout handler
could otherwise clear the session on an unsolicited request. `/signout-oidc` does not end the
session, with or without issuer and session parameters, on GET or form POST
(`SignOutTests.RemoteSignOutRequestsCannotEndTheSession`). Do not register that path as a
front-channel logout URL. Signing out of another Entra application does not end this app's
session.

`AddAllorsEntraUsers`, also called by `AddAllorsEntra`, clears `RemoteSignOutPath` for each
connected OpenID Connect scheme, including custom scheme names. Leave that option empty:
startup rejects a later `PostConfigure` that re-enables it
(`EntraOptionsTests.StartUpRefusesReenabledRemoteSignOut`). Other authentication schemes are
unaffected. Outgoing sign-out through `/entra/sign-out` and the separate
`/signout-callback-oidc` return path remain enabled (`SignOutTests.SignOutReturnsThroughTheOidcCallback`).

If the app session has expired or is absent, sign-out returns 204 without redirecting or
starting a new sign-in. This is a successful no-op and needs no antiforgery token, including
when the browser still holds a token from its expired session. Any Entra SSO session remains
active, so a later sign-in to the app may not ask for a password
(`SignOutTests.SignOutWithoutASessionIsANoOp`, `SignOutWithAnExpiredSessionIsANoOp` and
`SignOutWithOnlyABearerTokenIsANoOp`).

## 6. A client or a program

A client sends an access token of the tenant in the `Authorization: Bearer` header, and Core
forwards the request to the bearer scheme. A person's token, with a scope of the application,
creates the person's user at its first request as a browser sign-in does
(`BearerTests.APersonsTokenCreatesThePersonAtItsFirstRequest`); a program's own token, with an
app role, creates the user the factory decides on, an `Agent` in the test domain
(`BearerTests.AProgramsTokenCreatesAnAgent`). The same token finds the same user, and parallel
first requests of one principal create one user
(`BearerTests.TheSameTokenFindsTheSameUserAndParallelFirstRequestsCreateOneUser`).
`UserInfo` returns the person's name from either a v2 token's `preferred_username` or a v1
token's `upn`, including its mapped form (`BearerTests.AV1TokenReturnsTheUserName` and
`EntraOptionsTests.AV1UserNameIsAvailableToTheApplicationsTokenHandler`). An application that
selects its own name claim or retriever keeps that choice
(`EntraOptionsTests.TheApplicationsBearerNameSelectionIsPreserved`).

The token rules are Microsoft.Identity.Web's: the signature, the lifetime, the audience, the
issuer of the tenant, and a scope or an app role. `BearerTests.ATokenMicrosoftWouldNotIssueIs401`
lists tokens that are refused, among them a token of another tenant and a token with neither
scope nor role. A request with a bearer token needs no antiforgery token
(`AntiforgeryTests.ABearerPostNeedsNoXsrfHeader`).

## 7. Guests: the extranet

An application that serves the employees of its customers invites them as B2B guests into its
own tenant; inviting is the tenant's job, by hand or through Microsoft Graph, not the
plug-in's. A guest signs in like a member, and the token says where the account comes from:
`tid` is the application's tenant, `oid` the guest's object in it, `idp` where the account
lives, the issuer of the home tenant for the employee of another organization, and `acct` is
`1` when the registration asks for that optional claim.

The plug-in keeps that in two fields. `EntraIsGuest` records an explicit `acct=1` as guest and
`acct=0` as member. Missing, empty or unrecognized values preserve the stored status; without
a previously known status it defaults to false. An explicit change still updates the status
(`EntraAdmissionTests.ASignInKeepsGuestStatusWithoutAnExplicitAccountType` and
`ASignInAppliesAnExplicitGuestStatus`).

`EntraIdentityProvider` records a supplied `idp`, falling back to `iss` only when no provider
is stored yet. A later sign-in without `idp` preserves the known provider, even if `acct`
changes: member status does not imply that the account authenticates in the application's
tenant ([Microsoft's account conversion documentation](https://learn.microsoft.com/en-us/entra/identity/users/convert-external-users-internal)).
`EntraAdmissionTests.ASignInKeepsTheKnownProviderWhenIdpIsMissing` and
`ASparseBrowserSignInKeepsAV1GuestsProfile` check that partial tokens keep the stored home.

For the same tenant GUID, the public-cloud forms `https://sts.windows.net/{tenant}/` and
`https://login.microsoftonline.com/{tenant}/v2.0` are treated as the same provider during
refresh, keeping that user's stored representation. Other explicit provider changes update
the field (`EntraAdmissionTests.ASignInKeepsTheProviderRepresentationWhenOnlyTheTokenVersionChanges`
and `ASignInRecordsAnExplicitlyDifferentProvider`). These strings are profile metadata, not a
canonical customer key across users: different users can retain different representations,
and other identity providers need not name an organizational tenant. The application's domain
owns the mapping to its customers. Its factory sees the current token's claims and can map a
guest to a class of its own or refuse a home tenant it does not know.

## 8. Check it

The Entra tree's test server signs in against a fake Entra of its own by default, in its
non-inherited `Test/FakeEntra` folder: it serves Microsoft's documents under Microsoft's issuer,
with endpoints on the test server, and signs tokens of Microsoft's shape, so the plug-in runs as
configured for production, with every validation on, without a tenant. Its accounts are in
`FakeEntraAccounts`: a member, a refused member, a guest and a program.

Pointed at a real tenant, the same server signs in against Microsoft: set `Entra__TenantId`,
`Entra__ClientId` and a secret or certificate from step 2 to a registration of step 1 whose redirect URIs
name the server's own address, `https://localhost:5001/signin-oidc` and
`https://localhost:5001/signout-callback-oidc` for its launch profile, and set
`FakeEntra__Enabled=false`. Then open `/entra/sign-in?returnUrl=/allors/UserInfo` in a browser
and sign in.

## What the plug-in leaves to the application

- Authorization. A new user has no group; which Entra groups or roles mean what is the
  application's mapping, if it wants one.
- Acquiring tokens for Microsoft Graph or other downstream APIs.
- Other tenants, and Microsoft Entra External ID: the plug-in signs in one tenant, its members
  and its guests.
- Inviting guests and provisioning users ahead of their first sign-in.
