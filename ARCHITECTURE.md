# Allors architecture — domains, inheritance, and where hardening lives

This note exists to prevent a specific confusion (it has bitten at least one downstream
reader): **the word "Custom" used to mean two different things** depending on whether you were
working *in* the Allors framework repository or *on top of* it. That overload is resolved: the
framework's internal scaffolding domain is now named `Test`, and `Custom` is reserved for a
downstream inheritor's own extension domain.

## Abstract domains: Core ← Base ← Apps

Allors ships three **abstract** domains, layered by inheritance:

```
Core   (foundational: security model, identity, the JSON API, hosting)
  ▲
Base   (workflow, content/media, mail, …)
  ▲
Apps   (business: relations, orders, …)
```

They are **abstract**: none of `Core`, `Base`, `Apps` is deployed to production directly. A real
product is a **downstream inheritor** that builds on one of these layers.

## Inheritance works by compile-globbing the layer folders

Inheritance is not a runtime mechanism — it is the build. Each layer's projects **glob the
layer-named folders of their ancestors**. For example the Base server compiles

```xml
<Compile Include="..\..\..\Core\Database\Server\Core*\**\*.cs" />
```

and the Apps server adds `Base*\**` on top. The load-bearing rule:

> **Inheritable code lives in a folder named after its layer** (`Core/`, `Base/`, `Apps/`).
> Only `Core*` / `Base*` / `Apps*` folders are globbed, so only those are inherited.

A downstream inheritor's server does the same: it globs `Core*`, `Base*`, `Apps*` from the Allors
layers it builds on, and compiles its own extension code alongside.

## "Test" is internal scaffolding — and is **never inherited**

Within this repository, the `Test/` folders (renamed from `Custom/` in 2026-07; historically the
Apps server's `Controllers/` folder played the same role) hold **internal scaffolding**, of two
kinds — neither of them production:

- **Test scaffolding:** the concrete `Setup`/population that makes an abstract domain runnable
  *for automated tests*, plus test-only controllers such as the database-reset endpoints
  (`Test/Init`, `Test/Setup`, `Test/Restart`) and the passwordless token minter used by the suites.
- **Legacy showcase:** examples of superseded patterns. For instance, the Apps
  `Test/Relation/{Person,OrganisationContactRelationship}Controller` are dedicated per-type pull
  controllers kept to illustrate the Allors2-era style; in production the idiomatic way to fetch
  those objects is the generic `/allors/pull` JSON API, not a hand-written controller per type.

`Test` is a first-class domain: each layer's Repository declares a `Test` struct extending that
layer, and Allors binds hook implementations by domain name (`TestOnPostDerive`, `TestSetup`, …).
Because `Test` does not match the `Core*` / `Base*` / `Apps*` globs, **no inheritor ever
compiles it.** The destructive test endpoints therefore cannot reach a downstream production
build. The only projects that compile `Test/` are the abstract servers themselves, which act as
the test harness (and are never deployed).

> **Naming rule:** a downstream inheritor conventionally names *its own production domain*
> `Custom`. Nothing inside this repository is named `Custom` anymore, so every `Custom` an
> inheritor meets is their own code. The remaining in-repo uses of the word mean something else:
> the TS `extent/custom` panel base classes are the seam *for* hand-authored (inheritor) panels,
> `CustomOrganisationClassification`/`CustomEngagementItem` are business types where "custom"
> means bespoke/user-defined, and Blazor's `CustomValidator` is the standard forms pattern.
> Still planned: a dedicated demo `custom` domain under `apps`, carrying **only minimal
> infrastructure** (the generic pull/push/invoke endpoints plus Image/Media controllers — nothing
> else), so `apps` ships two example inheritors (`test` and `custom`) that model the reusable
> pattern (abstract domain → test domain → custom domain) for downstream developers.

## Where hardening lives — best defaults, overridable

Security hardening is **for inheritors**: the abstract layers are where the good defaults live so
that every downstream product inherits them without effort — "the pit of success."

- **Inheritable, foundational security code goes in the layer folders** — most in
  `dotnet/Core/Database/Server/Core/**` (e.g. `Core/Hosting/` holds the shared
  `AddAllorsServer`/`UseAllorsServer` seam: DataProtection, forwarded headers, security headers,
  rate limiting, Identity lockout/password policy, the production-secrets guard).
- **Defaults are secure by default but overridable by configuration.** Inheritors override via
  `appsettings.json` sections — e.g. `Identity`, `Security`, `DataProtection`, `ForwardedHeaders`,
  `Logging:JSNLog` — without touching Allors source.
- **Test-only scaffolding must never leak into a layer folder.** Anything that must not reach
  production stays in `Test/`. An automated test guards this boundary (the inheritable
  `Core`/`Base`/`Apps` server folders must expose no test/bypass controllers).

If you are hardening Allors: put the inheritable default in the layer folder (usually `Core`),
give it a configuration override, and keep any test hook in `Test/`.
