# Domain inheritance

> **Status: Current.**

Which part owns what, from the `[Extends]` of a domain to the order in which its hooks run.
[Domains](../domains.md) holds the model and the order rule, for users; this page maps the code
that carries them and the tests that pin them. Depth stays in the code comments and the tests.

## From the declaration to the hooks

| Step | Owner | What it does | Pinned by |
| --- | --- | --- | --- |
| Declaration | `ExtendsAttribute`, `dotnet/System/Repository/Attributes` | Takes the names of the domains as `params string[]`. One attribute per `[Domain]` struct: `AllowMultiple` stays off, so a second `[Extends]` is a compile error. | `RepositoryDomainsTests` |
| Parser | `Repository.CreateDomains`, `dotnet/System/Repository/Allors.Repository` | Reads every name into `Domain.DirectSuperdomains`, in the declared order. A duplicate domain name, a domain that extends itself, a name listed twice and a name that is no declared domain are logged with their fix and set `HasErrors`, which stops generation. | `RepositoryDomainsTests` in `Generate.Tests`, on repository projects built in memory |
| Template | `meta.cs.stg`, `dotnet/System/Repository/Templates` | `build_domain_inheritance` emits one `AddDirectSuperdomain` per name. The generated `MetaBuilder.Build` calls `StructuralDerive`, then the hand-written `Build<Domain>` of each domain in the order of `SortedDomains` reversed, base first. | the Diamond `DomainsTests` |
| Order | `DomainLinearization`, `dotnet/System/Database/Allors.Database.Meta.Props` | The one implementation of the rule: a domain's direct superdomains, minus one that another of them already extends, sorted by id and merged as C3 does. `MetaPopulation.StructuralDerive` runs it for every domain, which gives `Domain.Superdomains` nearest first, and for the population, which gives `SortedDomains` most derived first; generation, the tests and the start of every application use the same code. A cycle, a second domain that nothing extends and opposite orders throw an exception that names the domains. | `DomainOrderTests` in `Meta.Tests`, on hand-built graphs |
| Hooks | `MethodCompiler`, the same project | Takes `SortedDomains`. For a method of a class it binds, per interface of the class with the supertypes first and per domain in that order, the extension method `<Domain><Method>` on the interface; then, per domain in that order, the class's own `<Domain><Method>`. `ObjectFactory` collects the extension methods of the domain assembly and binds the population. | Core `MethodsTests`, the Diamond `HookOrderTests` |
| By hand | the concrete domain | The dispatch shims `Virtual/*.v.cs` call the setup and security hooks of every domain, base first; `Rules.Create` lists the rules of every domain; the build merges the resource folders of the domains, base first. The platform has no notion of domains in these three places. | `VirtualDispatchTests` for the phase, the Diamond `SetupOrderTests` for the order |

Nothing else knows about domains. The database, workspace, TypeScript and adapter templates read
a domain's id, tag and name only, and the derivation engine takes the flat list of rules.
Extending several domains therefore changed the attribute, the parser, the meta template and
the meta population, and nothing after them.

## Why the ids decide

The requirement is an order that is deterministic and stable: it does not change when a domain
is renamed, nor when a domain shuffles the names in `[Extends]`.

- A topological sort leaves the order of domains that do not extend each other open, and every
  tie-break by name or by the written order fails the requirement. The ids are the one stable
  input: generated once, carrying no meaning.
- C3, the linearization of Dylan, Python and Raku, merges the orders of the superdomains and
  keeps each of them: a domain keeps the order of its superdomains, and its own order is the
  same in every population that holds it, which no topological sort with a tie-break gives. The
  price is that two superdomains with opposite orders have no merge, where a sort would silently
  pick one; the exception names both orders.
- The superdomains are sorted by id before the merge, so that the written order plays no part,
  and a superdomain that another one already extends is dropped before that, so that the
  verdict does not depend on an accident of ids.
- Known and accepted: a new domain that becomes the top of a branch can move that branch, and
  the order is not visible in the source. The Diamond `DomainsTests` pin it and say why.
- `Guid.CompareTo` compares the ids as written, character by character, ignoring case. The .NET
  Framework compared the first groups as signed numbers;
  `DomainOrderTests.TheIdsCompareAsWritten` pins the order with an id that a signed compare
  would place first.
- A new domain's id is generated, with `uuidgen` or the like, and never chosen to steer the
  order. The tests pin the order that follows from the ids.

## The platform test domains

The Diamond tree, `dotnet/Diamond`, holds the platform test domains. Its population is
Core ← `Level1` ← `Level2` ← Test and Core ← `Plugin1` ← Test:

- `Level1`, a functional domain on Core: `Level1Item` with `Level1Name` and the derived
  `Level1NormalizedName`, the rule that derives it, and the service `ILevel1Log`.
- `Level2`, a functional domain on `Level1`: the role `Level2Note` on `Level1Item`, declared in
  a partial in `Level2`'s own folder, and `Level2Item` with a name, a derived normalized name and
  its rule.
- `Plugin1`, the test plug-in, hosted by Core: `Plugin1Key` and the derived
  `Plugin1NormalizedKey` on `User`, as an authentication plug-in adds its fields, the rule, and
  the service `IPlugin1Log`.
- Test, the concrete domain: `Person : User`, `HookLog`, which implements both log services as
  one list, the dispatch shims, `Rules.Create`, the setup and the security.

Every hook of the four domains records that it ran, with the hook's name and its subject: the
`OnPostBuild` extension on `Object`, the four virtuals of `ObjectsBase<T>`, the four phases of
`Setup` and the two of `Security`. Core records nothing, since every tree inherits its code, so
the tests compare with `SortedDomains` without Core. `DomainsTests` pins the five domains, their
superdomains and the order Test, `Plugin1`, `Level2`, `Level1`, Core, which follows from the
generated ids; `HookOrderTests` reads the log of a built `Person` and of a built `Level1Item`;
`SetupOrderTests` reads the log of each phase, base first, and per `IObjects` for the phases of
`ObjectsBase<T>`. `DomainIsolationTests` scans the folders of `Level1`, `Level2` and `Plugin1`
for a type of a domain they do not extend, the builders and extents the generator derives
included: a folder that broke this would compile in this tree, where the concrete domain
extends everything, and fail in an application that extends less.

The tree inherits Core through compile globs on Core's `Core*` folders, as the Identity tree and
an application do, and has a database side with the domain tests on the memory adapter; it has
no commands, server or workspace. Build targets `DotnetDiamondMerge`, `DotnetDiamondGenerate`,
`DotnetDiamondDatabaseTest` and `DotnetDiamondTest`; CI job `CiDotnetDiamondTest` in the
`memory` job. The test domains carry no production behavior; [AGENTS.md](../../AGENTS.md) says
where test code may live.
