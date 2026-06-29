# Power BI Integration via OData — Design

> Status: Draft / proposal. Branch: `claude/power-bi-integration-design-7uer34`.
> Scope: how Power BI should consume data from the Allors **Apps** server.
> Companion doc: [`security-paging-rca.md`](./security-paging-rca.md) — the access-control/paging
> root cause analysis that this design depends on.

## 1. Problem

Power BI must report on data held by an Allors application. The obstacle is architectural:

- The Apps server (`dotnet/Apps/Database/Server`) exposes a **bespoke JSON protocol** —
  `allors/pull`, `allors/push`, `allors/sync`, `allors/invoke` — behind **JWT bearer** auth
  (`Startup.cs`). Power BI cannot speak this protocol natively.
- Data persists to **SQL Server** in an **Allors-managed schema** (object + relation tables),
  normalised for the runtime, not shaped for analytics. Querying it directly bypasses
  Allors' access control.
- Allors enforces **fine-grained access control** (`IPolicyService`, permissions, workspaces).
  Any reporting surface must decide whose permissions apply.
- A runtime **meta model** (`IMetaPopulation`, `IComposite`, `IInterface`, `IClass`,
  `IRoleType`, `IRelationType`, `IUnit`) fully describes every type and relation — which we
  can exploit to *generate* the exposed surface.

So integration means: **give Power BI a source it understands while keeping Allors security
intact.**

## 2. Options considered

| # | Approach | Connector | Query folding | Honors security | Best for | Verdict |
|---|----------|-----------|---------------|-----------------|----------|---------|
| A | **OData v4 read endpoint** on the Apps server | OData feed (native) | Yes | Yes | Self-service, live exploration | **Recommended (primary)** |
| B | **Reporting star schema / SQL views** on a replica | SQL Server (native) | Yes (full SQL) | At projection time | Curated dashboards, heavy analytics | **Recommended (companion)** |
| C | **Push / streaming dataset** (Power BI REST API) | Push dataset | n/a | At push time | Real-time KPI tiles | Niche only |
| D | **Custom connector over the Pull protocol** | Custom (M) | No | Yes | — | Not recommended |
| E | **Direct SQL on operational tables** | SQL Server | Yes | **No — bypasses ACL** | — | Anti-pattern |

- **D** is bespoke, has no query fold (Power Query would POST opaque Pull requests and filter
  client-side), and needs ongoing maintenance against protocol changes.
- **E** bypasses `IPolicyService` entirely and couples reports to an internal schema the
  framework is free to change. Never point Power BI at the operational object/relation tables.

## 3. Recommended architecture

Two complementary layers, picked per use case (not mutually exclusive):

```
                         ┌──────────────────────────────────────────┐
                         │            Power BI (Desktop / Service)    │
                         └───────────────┬───────────────┬────────────┘
                          OData feed      │               │  SQL Server connector
                                          │               │  (Import / DirectQuery)
                ┌─────────────────────────▼──┐         ┌──▼───────────────────────────┐
                │  allors/odata (read-only)   │         │  Reporting projection          │
                │  ASP.NET Core OData v4       │         │  (star schema / views)          │
                │  EDM generated from meta     │         │  on a read replica              │
                │  $filter → Allors Extent     │         └──▲───────────────────────────┘
                └─────────────┬───────────────┘            │ populated via domain API /
                              │  reads through              │ derivations (security applied)
                              │  ITransactionService        │
                ┌─────────────▼─────────────────────────────┴────────────┐
                │            Allors domain + IPolicyService                │
                │                (access control, workspaces)              │
                └──────────────────────────┬───────────────────────────────┘
                                            │
                                  ┌─────────▼─────────┐
                                  │  SQL Server (ops)  │
                                  └────────────────────┘
```

### Layer A — OData v4 read endpoint *(primary)*

For analysts who build their own queries against live data. Power BI's OData connector folds
`$select`/`$filter`/`$expand`/`$top` to the server.

- Add `Microsoft.AspNetCore.OData` to the Apps server, mounted at `allors/odata`, alongside
  the existing controllers, reusing `ITransactionService`/`IWorkspaceService`/`IPolicyService`
  and the existing JWT/CORS setup.
- **Generate the EDM from the meta model** (see §4).
- **Fold OData query options to Allors `Extent`/`Filter`** (see §5).

### Layer B — Reporting star schema *(companion, for dashboards)*

For curated dashboards and high-volume analytics where Import/DirectQuery performance matters.

- Build a **denormalised reporting schema** (fact/dimension tables), populated **through the
  domain API** (scheduled export or derivations) so business rules and access scoping apply at
  projection time.
- Host on a **read replica** so BI load never touches the operational DB.
- Power BI uses the native **SQL Server** connector.

### Layer C — Push / streaming *(only if needed)*

Real-time KPI tiles only. Allors pushes metrics to a Power BI push dataset. Not a substitute
for A/B; not suitable for ad-hoc analysis.

## 4. Mapping the meta model to an EDM

Generate the whole EDM by walking `IMetaPopulation`, using the **database-scoped** collections
(`DatabaseComposites`, `DatabaseInterfaces`, `DatabaseClasses`, `DatabaseRelationTypes`) to
exclude workspace-only types. Key the EDM off each meta object's stable `Tag`
(`IMetaIdentifiableObject.Tag`), **not** the CLR name — names get refactored and would silently
break published reports.

### Core mapping

| Allors meta | OData EDM | Notes |
|---|---|---|
| `IUnit` | EDM primitive | `IsString`→`Edm.String`, `IsInteger`→`Edm.Int32`, `IsDecimal`→`Edm.Decimal`, `IsFloat`→`Edm.Double`, `IsBoolean`→`Edm.Boolean`, `IsDateTime`→`Edm.DateTimeOffset`, `IsBinary`→`Edm.Binary`, unique unit→`Edm.Guid` |
| object id (`long`) | `Edm.Int64` **key** | Positive for DB objects; the natural entity key |
| `IClass` | **concrete** entity type | `SingularName`→type name, `PluralName`→entity set name |
| `IInterface` | **abstract** entity type (`IsAbstract`) | polymorphism anchor — see §4.1 |
| `IRoleType` (unit, `IsOne`) | structural property | `Nullable=!IsRequired`; `MaxLength=Size`; `Precision`/`Scale` for decimals |
| `IRoleType` (composite, `IsOne`) | navigation property (single) | target may be an interface → polymorphic |
| `IRoleType` (composite, `IsMany`) | collection navigation property | |
| `IAssociationType` | partner navigation property (readonly) | relations are bidirectional; expose the inverse so clients can `$expand` both ways — readonly is fine, the feed is read-only |

`IRelationType.Multiplicity` (`OneToOne`/`OneToMany`/`ManyToOne`/`ManyToMany`) gives cardinality
on both ends; `IPropertyType.IsOne`/`IsMany` per side picks single vs. collection.

**Derived roles need no special handling.** `IRelationType.IsDerived` appears nowhere in the SQL
adapters/schema — it only affects *write* semantics (rule-populated vs. client-writable). A
derived role is a normal stored, indexed column (e.g. `Party.BillingInquiriesFax` is `[Derived]`
*and* `[Indexed]`). For a read-only feed, derived roles are ordinary filterable / sortable /
expandable properties.

### 4.1 Interfaces — the impedance mismatch

EDM entity types support inheritance (`BaseType`, `Abstract`), so the natural mapping is:
interfaces → abstract entity types, classes → concrete types deriving from their supertype, and
**entity sets typed at an interface** return all subtypes with real polymorphism
(`~/Parties/Allors.Person`, `$filter=isof(...)`, cast-scoped `$select`/`$expand`).

The mismatch: Allors composites can have **multiple** `Supertypes` (multiple inheritance);
EDM allows exactly **one** `BaseType`. You must linearise the DAG into a tree. Keep two
decisions separate:

- **Declaration** — *every* database interface becomes an abstract entity type
  (unconditional; lets navigation properties target it).
- **BaseType** — each class/interface picks *exactly one* supertype as its EDM base (the
  "spine"); the other supertypes' members are flattened in.

Example from the repo: `Person : Party, Deletable, Versioned` and
`Party : Localised, Auditable, UniquelyIdentifiable, Commentable, IDisplayName, Searchable`.
`Party` is the substantive domain spine; the rest are cross-cutting capability mixins.

#### Spine-selection algorithm

```
1. Build the Hasse diagram (direct supertypes) from IComposite.Supertypes (transitive set):
     A is a direct supertype of X  ⟺  A ∈ X.Supertypes
                                      ∧ ∄ Y ∈ X.Supertypes : A ∈ Y.Supertypes
2. For each composite X with direct supertypes {S₁…Sₙ}, score each and pick the primary base:
     (a) Explicit override wins — a (workspace, Tag) → preferred-base map / [ReportingBase]
         annotation. Deterministic escape hatch.
     (b) Substance over mixin — score by subtype-closure size + exclusive-role count, weighted
         by depth (length of the candidate's own supertype chain). A deep, structure-owning
         interface (Party) beats a shallow universal mixin (Deletable/Versioned).
     (c) Deterministic tie-break on Tag (never name or enumeration order — the EDM must be
         reproducible build-to-build).
3. Tree-validate: each node now has ≤1 chosen parent → forest; supertype graph is acyclic.
4. Flatten orphans: for class X with chosen base B,
     inherited = roles via B's chain
     flattened = ExclusiveDatabaseRoleTypes of each non-chosen supertype,
                 deduped by RelationType.Tag, minus anything on the spine.
     Name collisions (same SingularName, different Tag) → disambiguate via SingularFullName.
```

**Per-workspace spines are a feature.** The useful polymorphic axis is audience-dependent — a
CRM workspace wants `Party` as the spine; an audit workspace may want `Auditable`/`Versioned`
as the spine and flatten `Party`. Hence the override map is keyed by `(workspace, Tag)`. A
smaller per-workspace DAG also yields fewer orphans/collisions and a less ambiguous spine.

**Limitation:** EDM single inheritance cannot express "`Person` is-a both `Party` and
`Deletable`," so a navigation property *typed to a non-spine interface* can't polymorphically
return a `Person`. Mitigation: choose spines so that interfaces used as **relation endpoints**
sit on a spine. In practice relations point at substantive domain types, never at capability
mixins, so the flattened interfaces are exactly the ones nobody navigates *to*.

**Cross-feed caveat:** the same class has a different EDM shape across workspaces (different
`BaseType`, different inherited-vs-flattened split). Fine *within* a feed; it only bites a Power
BI **composite model joining two feeds**. The saving grace: the **entity key (object Id) is
stable across workspaces**, so a cross-feed join *on Id* still lines up row-for-row.

## 5. Folding OData query options to Allors

The reason OData beats plain REST: query folding. OData options map onto `ICompositePredicate`
and the `Extent` API so the database does the work.

### 5.1 `$filter` / `$orderby` / `$top`

| OData | Allors |
|---|---|
| `eq` | `AddEquals(roleType, value)` |
| `gt` / `lt` | `AddGreaterThan` / `AddLessThan` |
| `ge` / `le` | combine with `AddNot`, or `AddBetween` |
| `contains` / `startswith` / `endswith` | `AddLike(roleType, "%…%")` |
| `and` / `or` / `not` | `AddAnd` / `AddOr` / `AddNot` (under the top-level AND `Extent.Filter`) |
| `isof(type)` / type cast | `AddInstanceof(composite)` — the linchpin for interface-typed sets |
| `$orderby` | `Extent.AddSort(roleType[, direction])` |
| `$top` / `$skip` | extent paging — **see §7 and the companion RCA** |

### 5.2 `$filter` over navigation paths

Path traversal folds to **sub-extents fed into `AddContainedIn`** — "objects whose role lands in
the set of targets that satisfy the inner predicate."

```csharp
// $filter=Owner/Name eq 'Acme'
var owners = transaction.Extent(ownerRoleType.ObjectType);   // e.g. Party
owners.Filter.AddEquals(nameRoleType, "Acme");
extent.Filter.AddContainedIn(ownerRoleType, owners);
```

- **Reverse direction** (path walks an association/inverse): `AddContainedIn(associationType, …)`.
- **Collection `any`**: inner extent + `AddContainedIn(rolesRoleType, inner)`.
- **Collection `all`**: rewrite as `not any(not P)` — `AddNot()` over an `AddContainedIn` with
  the negated inner predicate.
- **Null/existence**: `Owner ne null` → `AddExists(role)`; `eq null` → `AddNot().AddExists(...)`.
- **Cast in path**: `isof(Owner, Allors.Person)` → `AddInstanceof(ownerRoleType, person)`.
- **Deep chains** `A/B/C eq x`: fold from the **leaf** back to the root, each segment wrapping
  the previous extent in an `AddContainedIn`.

The SQL predicates `RoleContainedInExtent` / `AssociationContainedInExtent` already compile
these to `EXISTS`/`IN` subqueries.

### 5.3 `$expand` → prefetch policies

`$expand` is a tree; `PrefetchPolicy` (`PrefetchPolicyBuilder`) is a tree. Translate recursively:

```csharp
PrefetchPolicy ToPrefetch(IEnumerable<ExpandedNavigationSelectItem> items, IComposite onType)
{
    var b = new PrefetchPolicyBuilder();
    foreach (var item in items)
    {
        var pt = ResolveProperty(item.PathSegment, onType);   // IRoleType or IAssociationType
        var children = item.SelectAndExpand?.SelectedItems.OfType<ExpandedNavigationSelectItem>();
        if (children?.Any() == true)
            b.WithRule(pt, ToPrefetch(children, (IComposite)pt.ObjectType));
        else
            b.WithRule(pt);
    }
    return b.Build();
}
```

- **Kills N+1** — the SQL adapter prefetches each level in batches.
- **Both directions** — `WithRule` takes any `IPropertyType`; `IAssociationType` (the partner)
  prefetches like a forward role.
- **Cast segments** (`Contacts/Allors.Person($select=…)`) — prefetch on the role type is
  unchanged; the cast narrows projection.
- **`$levels` (recursive)** — `PrefetchPolicy` is a finite tree; **cap depth**, expand to N, and
  **log the cap** (no silent truncation).
- **Fold `$select`** — only prefetch roles that are selected or expanded.
- **Cache** — `PrefetchPolicy.AllowCompilation` + `IPrefetchPolicyCache`; Power BI re-runs the
  same folded query on refresh, so key a compiled policy on the normalised `$expand`/`$select`
  shape.

## 6. Security model

Allors enforces per-user, per-object permissions, and `$metadata` is one static document shared
by every consumer. Pruning happens at **two altitudes** — conflating them either leaks schema or
under-shows data.

### Coarse — workspace + (optionally) permission-pruned schema

- A workspace is a **design-time, compiled** projection of the model
  (`[Workspace(...)]` attributes → `WorkspaceNames`). Use a curated **reporting workspace** as
  the analytic allow-list; the exposed surface = `DatabaseComposites` in that workspace.
- Add **multiple workspaces** only when audiences need genuinely different *model shapes* that
  are **stable and few** (and recall §4.1 — different workspaces can carry different spines).
  Do **not** use workspaces as a proxy for dynamic security-group membership: they are
  compiled, don't compose (a user can be in many overlapping groups), and change at the wrong
  speed.
- Optionally drop from `$metadata` any type/property the reporting principal could never read,
  using `IClass.ReadPermissionIdByRelationTypeId` — schema hygiene, not the security boundary.

### Fine — ACL enforced at query time *(authoritative)*

- **Masked objects** (`IAccessControlList.IsMasked()`): security-trim from the result set.
- **Readable object, unreadable role** (`CanRead(roleType)` false): null the property; the row
  appears, the cell is blank.

**Invariant:** the ACL runs unconditionally; the workspace may only ever *narrow* surface, never
grant access. Workspace omission means "not in this schema," never "denied" — the ACL is the
gate on every fetched object/role. Workspace = first line (coarse), ACL = authoritative (fine),
**always both**.

### Whose permissions apply

- **Reporting service principal *(recommended start)*** — one read-only Allors user scopes what
  BI sees. Simple; everyone sees the same slice. **Also makes paging a pure SQL-adapter feature
  with no security-model change** (see §7 and the RCA).
- **Per-user pass-through** — each Power BI user authenticates (OAuth2) and sees only what their
  Allors permissions allow. Honors security fully; needs the visibility-predicate work in the
  RCA and usually an on-prem data gateway.

## 7. Paging — depends on the companion RCA

`$top`/`$skip`/`$count` over an ACL-trimmed result is **not** correct today: the Pull pipeline
fetches the full extent, trims security in memory, then pages in memory, and `SqlExtent.Count`
materialises all ids rather than issuing `COUNT(*)`. This is **not OData-specific** — it is a
property of the current query/security layering and affects the workspace Pull identically.

The fix lives at the query/security layer, not in an OData adapter:

1. Add **`OFFSET/FETCH` + `COUNT(*)` pushdown** to the SQL extent (and `Skip`/`Take` on the
   `Extent` API). Sufficient on its own for the **service-principal** feed (no row masking there).
2. For **per-user pass-through**, add a **foldable object-visibility predicate** built on the
   already-indexed security chain and `Grant.EffectivePermissions`, so the database returns only
   visible rows and paging/count stay exact. Keep cell masking in memory (paging-neutral).

Full root cause analysis, the SQL-adapter findings, and the materialisation ladder are in
[`security-paging-rca.md`](./security-paging-rca.md).

## 8. Where it lives in the repo

- A new **OData controller + a meta-model→EDM builder** under `dotnet/Apps/Database/Server`
  (next to `Custom/`), reusing `ITransactionService`/`IWorkspaceService`/`IPolicyService` and
  the existing JWT/CORS configuration.
- Alternatively a separate `Allors.Reporting` service project if reporting load/dependencies
  should be isolated.
- Layer B is SQL/ETL artefacts plus an export job consuming the domain API.

## 9. Recommendation

Ship **Layer A (OData)** for self-service plus **Layer B (reporting star schema on a replica)**
for dashboards, secured by a **reporting service principal** to start (with **per-user
pass-through** as a later option). Generate the EDM from the meta model with per-workspace
spines; fold query options to `Extent`. Land the **SQL-adapter paging pushdown** first — it
unblocks correct paging for the service-principal feed and benefits all of Pull.

## 10. Open questions

1. Which classes/roles form the reporting workspace(s) — and do any audiences need distinct
   model shapes (and thus distinct spines)?
2. Data volumes / refresh cadence (Import vs DirectQuery; whether Layer B is needed day one)?
3. Is a read replica available, or must reporting target the operational DB?
4. Security: service principal first, or is per-user isolation a hard launch requirement?
5. Hosting: Power BI Service (cloud) vs Report Server (on-prem) — affects whether an on-prem
   data gateway is in scope.

## 11. Suggested next step

Scaffold a minimal **OData spike** — a handful of entity sets generated from the meta model,
`$filter` folded to an `Extent`, behind the reporting principal — to validate folding and the
security model end-to-end before committing to the full surface. Pair it with the SQL-adapter
paging pushdown from the RCA.
