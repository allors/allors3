# Domains

> **Status: Current.** [Current implementation](#current-implementation) names the trees and
> the tests.

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
authentication with ASP.NET Core Identity; Entra provides it with Microsoft Entra ID.

A plug-in extends its host, the domain that leaves the functionality open. Any domain can be a
host. Core is the host of Identity and of Entra.

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

## Extending several domains

`[Extends]` names every domain that a domain extends. The concrete domain of the Diamond tree
extends the functional domain `Level2` and selects the plug-in `Plugin1`:

```csharp
[Domain("bec75779-2d99-4980-866b-0e75755ae9bf")]
[Extends(nameof(Level2), nameof(Plugin1))]
public struct Test
{
}
```

The order of the names plays no part. Generation stops with a message that names the domains
when a name in `[Extends]` is no declared domain, when a name is listed twice, when a domain
extends itself, or when two domains share a name.

## The order of the domains

Allors binds a hook by the name of its domain, `Level1OnPostBuild` for example, and runs the
hooks of the domains in one order, the order of the domains:

- a domain comes before the domains it extends;
- domains that do not extend each other are ordered by branch. A branch is a domain with the
  domains it brings in. The branches follow the ids of their top domains, and each branch is
  kept whole.

In the normal arrangement above, `Custom` comes first and Core last; between them come the
branches of `Sales`, of `Stock` and of Identity, in the order of their three ids. In the Diamond
tree, where the concrete domain extends `Level2` and `Plugin1`, the id of `Plugin1` sorts before
the id of `Level2`, so the order is Test, `Plugin1`, `Level2`, `Level1`, Core: `Level1` follows
`Level2` because the branch of `Level2` is kept whole.

The ids decide so that the order is stable. It depends on nothing but the graph and the ids: a
domain keeps its place when a domain is renamed and when the names in `[Extends]` are reordered,
and the domains that a domain extends keep their order in every application that holds that
domain. The ids compare as written, character by character, ignoring case. A domain's id is
generated, never chosen to steer the order.

Two structures have no order. Generation and the start of the application stop with a message
that names the domains:

- a cycle: a domain extends itself through other domains;
- two domains that nothing extends: an application has one domain of its own, so one of the two
  extends the other, or a domain extends both;
- opposite orders: two domains that one domain extends order the same two domains in opposite
  ways, for example `Sales` with `Level1` before `Plugin1` and `Stock` with `Plugin1` before
  `Level1`, both extended by `Custom`. The fix is structural, for example letting one of the two
  extend the other. This needs two domains that each extend several domains; it cannot arise
  when the concrete domain is the only domain that extends several domains.

The order of the domains covers the hooks that Allors binds by name. Three lists that the
concrete domain writes by hand follow the same order, base first: the dispatch of the setup and
security phases to the hooks of each domain in its `Virtual` shims, the rules in `Rules.Create`,
and the resource folders that its build merges. The Diamond tree is the example.

## Current implementation

- A domain extends one or more domains, and the order of the domains is as described above.
  `DomainOrderTests` checks the rule on hand-built graphs: the chain, the diamond with both id
  orders, the dropped superdomain, the cycle, the second domain that nothing extends, the
  opposite orders, and the stability under a rename and a reorder of `[Extends]`;
  `DomainOrderTests.TheIdsCompareAsWritten` checks that the ids compare as written.
  `RepositoryDomainsTests` checks the errors of `[Extends]` at generation.
- The Diamond tree, `dotnet/Diamond`, holds the platform test domains: the functional domains
  `Level1`, which extends Core, and `Level2`, which extends `Level1`; the test plug-in `Plugin1`,
  hosted by Core; and the concrete domain Test, which extends `Level2` and `Plugin1`, so that
  the population is a diamond. `DomainsTests.TheSortedDomainsFollowTheIdOrderOfTheBranches`
  checks the order Test, `Plugin1`, `Level2`, `Level1`, Core; `HookOrderTests` checks that the
  hooks run in that order; `SetupOrderTests` checks that the shims run the setup and security
  hooks base first; `DomainIsolationTests` checks that the folders of a domain name no type of a
  domain it does not extend.
- Identity provides authentication with ASP.NET Core Identity. Its tree, `dotnet/Identity`, is
  Core ← Identity ← Test. Entra provides authentication with Microsoft Entra ID. Its tree,
  `dotnet/Entra`, is Core ← Entra ← Test. Core's `User` has no authentication field; each
  plug-in declares its own, which
  `InheritableSurfaceTests.InheritableCoreFoldersNameNoAuthenticationField` checks.
  [Authentication](authentication.md) says how a plug-in connects to Core and to the
  application's domain.
- An application that selects no plug-in hosts the Allors API only if it registers an
  `IUserResolver` of its own, as Core's test server does for its test sign-in header. Without
  one, `UseAllorsServer` stops with an actionable error, which
  `AllorsServerTests.UseAllorsServerWithoutAUserResolverFails` checks.
