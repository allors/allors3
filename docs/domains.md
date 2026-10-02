# Domains

> **Status: Planned.** The model is agreed. [Current implementation](#current-implementation)
> says which parts the code supports today.

An Allors3 application is built from domains. Inheritance is the only relationship between
domains: the application has one domain of its own, and every other domain takes part because a
domain extends it.

A domain can extend more than one domain. Every combination is allowed, a diamond included, as
long as no domain extends itself through other domains: the domains of an application form a
directed acyclic graph.

## Kinds of domains

| Kind | Optional | What it is |
| --- | --- | --- |
| Functional domain | No | Model and behavior that every domain after it builds on. Core is the first one. |
| Plug-in | Yes | One way to provide a functionality that its host leaves open. Identity is one. |
| Concrete domain | | The application's own domain. It extends the functional domains it builds on and selects the plug-ins. |

### Functional domains

Functional domains build on Core. A functional domain extends Core or other functional domains,
one or more. A domain that extends a functional domain inherits all of it. A functional domain
is not optional: an application that builds on it gets all of it.

### Plug-ins

A plug-in provides a functionality that can be provided in more than one way. Identity provides
authentication with ASP.NET Core Identity. Another plug-in could provide authentication against
an external OAuth server.

A plug-in extends its host, the domain that leaves the functionality open. Any domain can be a
host. Core is the host of Identity.

Plug-ins that provide the same functionality are alternatives, and an application selects one.

### The concrete domain

The concrete domain is the application's own, conventionally named `Custom`. It extends each
functional domain it builds on and each plug-in it selects.

## The normal arrangement

```text
Custom --extends--> Sales --extends--> Core         functional domain
Custom --extends--> Stock --extends--> Core         functional domain
Custom --extends--> Identity --extends--> Core      plug-in, hosted by Core
```

- The concrete domain extends the functional domains it builds on, one or more, and selects the
  plug-ins, one for each functionality. The example is a diamond: `Custom` reaches Core through
  `Sales`, through `Stock` and through Identity.
- Functional domains do not extend a plug-in and do not depend on a particular one. That keeps
  the choice open for the concrete domain.
- An application selects an authentication plug-in.

## Exceptions

These are possible, but not common.

- **A functional domain extends a plug-in.** Every domain after it is then tied to that plug-in,
  and the concrete domain can no longer select an alternative.
- **An application selects no authentication plug-in.** It then has direct database access only,
  as a console application has.

## Current implementation

- A domain extends one domain. An application whose domains form one chain needs nothing more:
  `Custom` extends Identity, and Identity extends Core.
- Extending more than one domain is planned, together with a defined order for the hooks of
  domains that do not extend each other. Until then a concrete domain cannot extend two
  functional domains, or a functional domain and a plug-in.
- Identity provides authentication with ASP.NET Core Identity. Its tree, `dotnet/Identity`, is
  Core ← Identity ← Test. Core's `User` has no authentication field; Identity declares them.
  `InheritableSurfaceTests.InheritableCoreFoldersNameNoAuthenticationField` checks that.
- An application that selects no plug-in hosts the Allors API only if it registers an
  `IUserResolver` of its own, as Core's test server does for its test sign-in header. Without
  one, `UseAllorsServer` stops with an actionable error, which
  `AllorsServerTests.UseAllorsServerWithoutAUserResolverFails` checks.
