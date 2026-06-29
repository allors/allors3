# External Data Access & Reporting Integration — Architecture & Design

> Status: Draft / proposal. Branch: `claude/power-bi-integration-design-7uer34`.
> Scope: how external clients (Power BI first, but not only) should read Allors data — the
> protocol-agnostic core that makes this possible, an OData v4 projection over it, the
> access-control/paging issue both must solve, the consumer/pipeline ecosystem, and the
> alternatives (GraphQL, SQL, REST).

---

## 1. Problem & principles

We want external systems — BI tools, spreadsheets, ETL pipelines, partner apps — to read
Allors data. The obstacle is architectural:

- The Apps server (`dotnet/Apps/Database/Server`) exposes a **bespoke JSON protocol**
  (`allors/pull`, `allors/push`, `allors/sync`, `allors/invoke`) behind **JWT bearer** auth
  (`Startup.cs`). External tools cannot speak it natively.
- Data persists to **SQL Server** in an **Allors-managed schema** (object + relation tables),
  normalised for the runtime, not for analytics. Querying it directly bypasses access control.
- Allors enforces **fine-grained access control** (`IPolicyService`, permissions, workspaces).
- A runtime **meta model** (`IMetaPopulation`, `IComposite`, `IInterface`, `IClass`,
  `IRoleType`, `IRelationType`, `IUnit`) fully describes every type and relation — so the
  exposed surface can be *generated*, not hand-written.

**Guiding principle (developed in §2):** the integration is a *projection* over a
protocol-agnostic core. The hard parts — generating a surface from the meta model, folding
queries to the database, and enforcing security/paging — belong to that core, shared by every
protocol. OData, GraphQL, REST become thin front-ends.

---

## 2. The protocol-agnostic core

Most of the core **already exists** in Allors; it is merely fused to the JSON protocol. Three
layers:

1. **Meta** (`Allors.Database.Meta`) — `IMetaPopulation` etc. The schema source of truth.
2. **A query IR** (`Allors.Database.Data`) — `Pull` (query root); `IExtent`/`Extent` carrying a
   **visitable predicate AST** (`Equals`, `Like`, `Not`, `Or`, `Exists`, `Union`/`Except`, the
   `ICompositePredicate` tree); `Result` (paging `Skip`/`Take`, a `Select` projection, an
   `Include` tree); `Node` (the prefetch/include tree); `Sort`; `IArguments`. It is an
   **`IVisitable` AST with an `IVisitor`** — a real intermediate representation.
3. **Execution + security + prefetch** — compiles the IR to SQL (`Extent.Build`), applies
   `PrefetchPolicy`, enforces `IAccessControl`, shapes results.

`Allors.Database.Workspace.Json` is **one projection** over this: it deserialises a wire format
into `Data.Pull` (`pullRequest.l.FromJson(...)`) and serialises results back. OData and GraphQL
should be **peer front-ends that compile their query into the same `Data.Pull`/`IExtent` IR** and
reuse one execution path.

```
            ┌────────────────────────────────────────────────────────────┐
  per       │  OData front-end   │  GraphQL front-end  │  JSON Pull (today) │
  protocol  │  • EDM from meta   │  • SDL from meta    │  • wire ⇄ Data     │
  (thin)    │  • $filter→AST     │  • selection→Node   │                    │
            │  • $expand→Node    │  • args→AST         │                    │
            │  • → OData JSON     │  • → GraphQL resp   │  • → JSON envelope  │
            └─────────┬──────────┴──────────┬──────────┴─────────┬──────────┘
                      │   all compile into   │                    │
            ┌─────────▼──────────────────────▼────────────────────▼─────────┐
  SHARED    │  Query IR  —  Allors.Database.Data (Pull / IExtent / predicate │
  CORE      │  AST / Result / Select / Node / Sort), IVisitable              │
            ├───────────────────────────────────────────────────────────────┤
            │  Execution service (protocol-neutral)                          │
            │   • compile IR → SQL via adapter (Extent.Build)                │
            │   • inject visibility predicate (§4 RCA fix) — as an IVisitor   │
            │   • PrefetchPolicy + IPrefetchPolicyCache                       │
            │   • ACL cell-masking; paging + COUNT (§4 pushdown)             │
            │   • guardrails (max page, max expand depth, capability checks)  │
            │   → neutral result: ids + cell-masked role values + count/cursor│
            ├───────────────────────────────────────────────────────────────┤
            │  Meta (IMetaPopulation)  +  Surface descriptor                 │
            │   (reporting workspace allow-list, naming, capability flags)    │
            └───────────────────────────────────────────────────────────────┘
```

Each protocol does exactly three things: **render the schema** from meta, **compile its query
into the IR**, and **serialise the neutral result**. Everything between is shared.

### 2.1 The seam to break

Execution + security + response-shaping currently lives *inside* the JSON project
(`PullExtent`, `PullResponseBuilder`) and bakes in post-fetch trimming and in-memory paging.
Refactor:

- Extract a **protocol-neutral execution service** — input: a `Data.Pull` (or list) + an access
  context; output: a **neutral result** (object ids + materialised, cell-masked role values +
  total count + a paging cursor/continuation). This is the reusable guts of
  `PullExtent`/`PullResponseBuilder` minus JSON.
- Refactor JSON Pull into a thin front-end over the service (it nearly is already).
- Add OData and later GraphQL as peer front-ends.

Nuance: the JSON Pull response also returns **grants/revocations/versions** so the TS workspace
can maintain its own client-side ACL cache — a workspace-Pull concern, not a BI one. The neutral
result exposes secured data + paging; the Pull front-end appends that access-metadata tail.

### 2.2 Shared vs. per-protocol

| Concern | Where it lives |
|---|---|
| Schema source | **Core** (meta + surface descriptor) |
| Query → IR compilation | **Per protocol** (parsing differs) |
| Predicate / paging / projection / prefetch semantics | **Core** (`Data` IR + execution) |
| Object **visibility predicate** (§4) | **Core** — an `IVisitor` pass over the IR before `Extent.Build` |
| Cell masking, COUNT, OFFSET/FETCH pushdown | **Core** (execution + adapter) |
| Guardrails (max page, max expand depth) | **Core** — declared in surface descriptor, advertised per protocol |
| Type-system mapping incl. **spine selection** (§3.1) | **Per protocol** — EDM needs a spine; GraphQL emits multi-interface types directly |
| Response serialisation + protocol metadata (`@odata.nextLink`, Relay `pageInfo`) | **Per protocol** |

Two rows are the payoff:

- **The §4 fix is done once.** The foldable visibility predicate is a node injected into the IR
  (clean, because it is a visitable AST); the OFFSET/FETCH/COUNT pushdown is in the adapter
  beneath execution. OData, GraphQL, *and the existing Pull* all inherit correct, scalable paging.
- **Spine selection is explicitly *not* core** — it is an artifact of EDM single inheritance and
  lives in the OData schema generator. GraphQL reads the same meta and emits multi-interface
  types with no spine. That is the clean test of the boundary: meta + surface descriptor are
  shared; the *type-system projection* is per-protocol.

### 2.3 Project layout

- `Allors.Database` — `Data` IR + `Meta` + `Prefetch`. *(exists)*
- `Allors.Database.Api` (new) — execution service + surface descriptor + guardrails + visibility
  visitor. *(extracted from the JSON project)*
- `Allors.Database.Workspace.Json` — refactored to a thin front-end. *(exists, slimmed)*
- `Allors.Database.Protocol.OData` (new) — EDM generator + OData-query→IR compiler + serialiser.
- `Allors.Database.Protocol.GraphQL` (later) — SDL generator + selection→IR compiler + resolver.

The discipline that makes this pay off: **nothing protocol-specific below the IR line; no
re-implementation of query/security/paging above it.** Each time OData "needs" something, decide
whether it is a projection concern (schema rendering, serialisation, parsing) or a core concern
in disguise (filtering, paging, security) — the latter goes in the core so the next protocol
gets it free.

---

## 3. The OData projection (primary, for Power BI & friends)

OData v4 is the right *first* projection: BI tools and pipelines fold its query options
natively, and the surface is discoverable via `$metadata`. Mounted at `allors/odata` on the Apps
server, reusing `ITransactionService`/`IWorkspaceService`/`IPolicyService` and the existing
JWT/CORS setup.

### 3.1 Mapping the meta model to an EDM

Walk `IMetaPopulation`, using the **database-scoped** collections (`DatabaseComposites`,
`DatabaseInterfaces`, `DatabaseClasses`, `DatabaseRelationTypes`) to exclude workspace-only
types. Key the EDM off each meta object's stable `Tag` (`IMetaIdentifiableObject.Tag`), **not**
the CLR name — names get refactored and would silently break published reports.

| Allors meta | OData EDM | Notes |
|---|---|---|
| `IUnit` | EDM primitive | `IsString`→`Edm.String`, `IsInteger`→`Edm.Int32`, `IsDecimal`→`Edm.Decimal`, `IsFloat`→`Edm.Double`, `IsBoolean`→`Edm.Boolean`, `IsDateTime`→`Edm.DateTimeOffset`, `IsBinary`→`Edm.Binary`, unique unit→`Edm.Guid` |
| object id (`long`) | `Edm.Int64` **key** | Positive for DB objects; the natural entity key |
| `IClass` | **concrete** entity type | `SingularName`→type name, `PluralName`→entity set name |
| `IInterface` | **abstract** entity type (`IsAbstract`) | polymorphism anchor — see below |
| `IRoleType` (unit, `IsOne`) | structural property | `Nullable=!IsRequired`; `MaxLength=Size`; `Precision`/`Scale` |
| `IRoleType` (composite, `IsOne`) | navigation property (single) | target may be an interface → polymorphic |
| `IRoleType` (composite, `IsMany`) | collection navigation property | |
| `IAssociationType` | partner navigation property (readonly) | relations are bidirectional; expose the inverse so clients can `$expand` both ways — readonly is fine, the feed is read-only |

`IRelationType.Multiplicity` gives cardinality on both ends; `IPropertyType.IsOne`/`IsMany`
picks single vs. collection.

**Derived roles need no special handling.** `IRelationType.IsDerived` appears nowhere in the SQL
adapters/schema — it only affects *write* semantics (rule-populated vs. client-writable). A
derived role is a normal stored, indexed column (e.g. `Party.BillingInquiriesFax` is `[Derived]`
*and* `[Indexed]`). For a read-only feed they are ordinary filterable / sortable / expandable
properties.

#### Interfaces — the impedance mismatch and spine selection

EDM entity types support inheritance (`BaseType`, `Abstract`): interfaces → abstract entity
types, classes → concrete types deriving from a supertype, and **entity sets typed at an
interface** return all subtypes with polymorphism (`~/Parties/Allors.Person`,
`$filter=isof(...)`, cast-scoped `$select`/`$expand`).

The mismatch: Allors composites can have **multiple** `Supertypes`; EDM allows exactly **one**
`BaseType`. Linearise the DAG into a tree. Keep two decisions separate:

- **Declaration** — *every* database interface becomes an abstract entity type (unconditional;
  lets navigation properties target it).
- **BaseType** — each class/interface picks *exactly one* supertype as its EDM base (the
  "spine"); the other supertypes' members are flattened in.

Example: `Person : Party, Deletable, Versioned`, `Party : Localised, Auditable,
UniquelyIdentifiable, Commentable, IDisplayName, Searchable`. `Party` is the substantive spine;
the rest are cross-cutting capability mixins.

```
Spine-selection algorithm
1. Build the Hasse diagram (direct supertypes) from IComposite.Supertypes (transitive set):
     A is a direct supertype of X  ⟺  A ∈ X.Supertypes ∧ ∄ Y ∈ X.Supertypes : A ∈ Y.Supertypes
2. For each composite X, score each direct supertype and pick the primary base:
     (a) Explicit override wins — a (workspace, Tag) → preferred-base map / [ReportingBase].
     (b) Substance over mixin — score by subtype-closure size + exclusive-role count, weighted
         by depth (length of the candidate's own supertype chain). A deep, structure-owning
         interface (Party) beats a shallow universal mixin (Deletable/Versioned).
     (c) Deterministic tie-break on Tag (never name/order — the EDM must be reproducible).
3. Tree-validate: each node now has ≤1 chosen parent → forest (supertype graph is acyclic).
4. Flatten orphans: for class X with chosen base B,
     inherited = roles via B's chain
     flattened = ExclusiveDatabaseRoleTypes of each non-chosen supertype, deduped by
                 RelationType.Tag, minus anything on the spine.
     Name collisions (same SingularName, different Tag) → disambiguate via SingularFullName.
```

- **Per-workspace spines are a feature.** The useful polymorphic axis is audience-dependent — a
  CRM workspace wants `Party` as the spine; an audit workspace may want `Auditable`/`Versioned`
  and flatten `Party`. Hence the override map is keyed by `(workspace, Tag)`. A smaller
  per-workspace DAG also yields fewer orphans/collisions and a less ambiguous spine.
- **Limitation:** EDM single inheritance cannot express "`Person` is-a both `Party` and
  `Deletable`," so a navigation property *typed to a non-spine interface* can't polymorphically
  return a `Person`. Mitigation: choose spines so interfaces used as **relation endpoints** sit
  on a spine. In practice relations point at substantive domain types, never at capability
  mixins, so the flattened interfaces are exactly the ones nobody navigates *to*.
- **Cross-feed caveat:** the same class has a different EDM shape across workspaces. Fine within a
  feed; it only bites a Power BI **composite model joining two feeds**. Saving grace: the
  **entity key (object Id) is stable across workspaces**, so a cross-feed join *on Id* still
  lines up row-for-row.

### 3.2 Folding OData query options to the IR / Extent

| OData | Allors |
|---|---|
| `eq` | `AddEquals(roleType, value)` |
| `gt` / `lt` | `AddGreaterThan` / `AddLessThan` |
| `ge` / `le` | combine with `AddNot`, or `AddBetween` |
| `contains` / `startswith` / `endswith` | `AddLike(roleType, "%…%")` |
| `and` / `or` / `not` | `AddAnd` / `AddOr` / `AddNot` (under the top-level AND `Extent.Filter`) |
| `isof(type)` / type cast | `AddInstanceof(composite)` — the linchpin for interface-typed sets |
| `$orderby` | `Extent.AddSort(roleType[, direction])` |
| `$top` / `$skip` | paging — see §4 |

**`$filter` over navigation paths** folds to **sub-extents fed into `AddContainedIn`**:

```csharp
// $filter=Owner/Name eq 'Acme'
var owners = transaction.Extent(ownerRoleType.ObjectType);
owners.Filter.AddEquals(nameRoleType, "Acme");
extent.Filter.AddContainedIn(ownerRoleType, owners);
```

- Reverse direction (association): `AddContainedIn(associationType, …)`.
- Collection `any`: inner extent + `AddContainedIn`. Collection `all`: rewrite `not any(not P)`.
- `Owner ne null` → `AddExists(role)`; `eq null` → `AddNot().AddExists(...)`.
- Cast in path: `isof(Owner, Allors.Person)` → `AddInstanceof(ownerRoleType, person)`.
- Deep chains `A/B/C eq x`: fold from the **leaf** back to the root, each segment wrapping the
  previous extent in an `AddContainedIn`.

The SQL predicates `RoleContainedInExtent` / `AssociationContainedInExtent` already compile these
to `EXISTS`/`IN` subqueries.

**`$expand` → prefetch policies.** `$expand` is a tree; `PrefetchPolicy` is a tree:

```csharp
PrefetchPolicy ToPrefetch(IEnumerable<ExpandedNavigationSelectItem> items, IComposite onType)
{
    var b = new PrefetchPolicyBuilder();
    foreach (var item in items)
    {
        var pt = ResolveProperty(item.PathSegment, onType);   // IRoleType or IAssociationType
        var children = item.SelectAndExpand?.SelectedItems.OfType<ExpandedNavigationSelectItem>();
        b = children?.Any() == true
            ? b.WithRule(pt, ToPrefetch(children, (IComposite)pt.ObjectType))
            : b.WithRule(pt);
    }
    return b.Build();
}
```

- Kills N+1 (batched per level). Both directions expand (`WithRule` takes any `IPropertyType`).
- Cast segments narrow projection only. `$levels` (recursive) — `PrefetchPolicy` is finite, so
  **cap depth, expand to N, and log the cap** (no silent truncation).
- Fold `$select` (only prefetch selected/expanded roles). **Cache** compiled policies
  (`PrefetchPolicy.AllowCompilation` + `IPrefetchPolicyCache`), keyed on the normalised
  `$expand`/`$select` shape — Power BI re-runs the same folded query on refresh.

### 3.3 Security — two altitudes

`$metadata` is one static document shared by every consumer, but Allors permissions are resolved
per object at runtime. Prune at two altitudes:

- **Coarse — workspace (+ optional permission-pruned schema).** A workspace is a *design-time,
  compiled* projection (`[Workspace(...)]` → `WorkspaceNames`). Use a curated **reporting
  workspace** as the analytic allow-list. Add multiple workspaces only when audiences need
  genuinely different *model shapes* that are **stable and few** (recall §3.1: different
  workspaces can carry different spines). Do **not** use workspaces as a proxy for dynamic
  security-group membership — they are compiled, don't compose (a user is in many overlapping
  groups), and change at the wrong speed. Optionally drop from `$metadata` anything the reporting
  principal could never read (via `IClass.ReadPermissionIdByRelationTypeId`) — hygiene, not the
  boundary.
- **Fine — ACL at query time *(authoritative)*.** Masked objects (`IsMasked()`) are
  security-trimmed; unreadable roles (`CanRead(roleType)` false) are nulled (row appears, cell
  blank).

**Invariant:** the ACL runs unconditionally; the workspace may only ever *narrow* surface, never
grant access. Workspace omission means "not in this schema," never "denied." Workspace = first
line (coarse), ACL = authoritative (fine), **always both**.

**Whose permissions apply:**
- **Reporting service principal *(recommended start)*** — one read-only user scopes what BI
  sees. Simple; everyone sees the same slice. Also makes paging a pure SQL-adapter feature with
  **no security-model change** (§4).
- **Per-user pass-through** — each user authenticates (OAuth2) and sees only their permitted
  data. Honors security fully; needs the §4 visibility predicate and usually an on-prem gateway.

---

## 4. Access control & paging — root cause analysis

Reconciling object-level access control with **paging** (`$top`/`$skip`/`$count`, or the
protocol's `Result.Skip`/`Take`) is broken in a structural way — and **not** specific to OData.
The **workspace Pull** has the identical behaviour; OData would inherit it. So the fix belongs in
the core (§2), where both surfaces benefit.

### 4.1 What the code does

`PullExtent.WithResults` runs three phases:

```csharp
var extent = dataExtent.Build(this.transaction, this.pull.Arguments);     // (1) SQL: data predicate only
this.transaction.Prefetch(this.prefetchPolicyCache.Security, extent);     //     prefetch security for ALL rows
trimmed = extent.Where(response.Include).Take(resultsRequired).ToArray(); // (2) in-memory security trim
var paged = objects.Skip(result.Skip.Value).Take(result.Take.Value);      // (3) in-memory paging
```

1. `extent.Build` folds only the **data** predicate to SQL; security is not in the query.
2. `.Where(response.Include)` trims **in memory** (`AllowedClasses.Contains(class)` and
   `!AccessControl[obj].IsMasked()`).
3. `.Skip().Take()` pages the already-trimmed in-memory sequence. No total/count is ever
   computed. (`resultsRequired = max(Skip+Take)` is a heuristic over-fetch, not a fold.)

The two `IAccessControl` implementations differ:
- **`DatabaseAccessControl.IsMasked() => false`** (trusted server) — **no row hiding**; inclusion
  reduces to the `AllowedClasses` (subtype) check.
- **`WorkspaceAccessControl.IsMasked()`** — *real* row masking via a per-class **mask role**
  (`IWorkspaceMask`): object hidden iff `!CanRead(maskRole)`.

`CanRead` = `readPermissionId ∈ grants.PermissionSet && ∉ revocations`, with grants gathered
**per object in C#** (`GetVersionedGrants` over the object's `SecurityTokens` + `DelegatedAccess`).
Cell masking happens during serialisation and **does not remove rows**.

**The SQL extent cannot page or count.** In `SqlExtent`:

```csharp
public override int Count => this.ObjectIds.Count;          // list length, NOT SELECT COUNT(*)
private IList<long> ObjectIds => this.objectIds ??= this.GetObjectIds();  // SQL returns ALL ids
```

Every access path funnels through the fully-materialised id list. Only `ORDER BY` folds
(`ExtentSort`); there is **no** `OFFSET`/`FETCH`/`LIMIT`, and the `Extent` API exposes only
`Count`/`Filter`/`AddSort` — no `Skip`/`Take`.

### 4.2 Root cause

> Allors models access control as an **evaluable function over a materialised object**
> (`IAccessControl[obj] → bool`), not as a **reifiable predicate** the query layer can push to
> the database. Data selection folds to SQL; security is a separate post-materialisation pass —
> and so is paging.

Consequences (mechanical, none OData-specific): the SQL query is never bounded by page size;
`$count`/totals need a full scan + trim; deep paging rescans from row 0 (no cursor); pages come
back short when masking is real. A secondary amplifier: security is prefetched over the **whole**
extent (O(total), not O(page)).

### 4.3 The state needed to fix it already exists

Every hop to express "can this principal see this row?" is a stored, indexed relation, and one is
derived:

| Hop | Status |
|---|---|
| `Object.SecurityTokens` | stored, **indexed** M2M |
| `SecurityToken.Grants` | stored, **indexed** M2M |
| `Grant.Subjects` / `Grant.SubjectGroups` | stored, **indexed** |
| `Grant.EffectivePermissions` | **derived + indexed** — `GrantEffectivePermissionsRule` sets it to `Role.Permissions` |
| `Grant.Revocations` / `Object.Revocations` | stored |

The containment fold already exists (`RoleContainedInExtent` / `AssociationContainedInExtent`).
**Consistency-by-construction:** the evaluator's `VersionedGrant.PermissionSet` derives from the
same `Role.Permissions` → `EffectivePermissions`, so a predicate built on `EffectivePermissions`
shares a source of truth with the in-memory ACL. The answer to dual-implementation drift is
*don't write a second security algorithm; build the predicate on the same derived data.*

### 4.4 Materialisation ladder

- **Level 0 (today):** `Grant.EffectivePermissions` — `Role`→`Permission` flattened, derived +
  indexed.
- **Level 1 — per-request granting-token set. *Recommended default; no schema change.*** Once per
  `(user, readPermission)`, compute `grantingTokens` (a single query over indexed Grant
  relations). Per-row predicate is one indexed containment:
  `AddContainedIn(SecurityTokens, grantingTokenExtent)` (+ `DelegatedAccess` traversal), folded
  via `RoleContainedInExtent`. Reuses existing indexes; consistent-by-construction.
- **Level 2a — subject-centric persisted relation** (`Subject → (Permission, SecurityToken)`):
  direct lookup, recomputed on infrequent security-config changes. Escalate only if Level 1 is a
  measured bottleneck.
- **Level 2b — object-centric "readers" materialisation** (`Object → Subject`): single indexed
  join, trivially correct paging/count, DirectQuery-friendly. Costs: write amplification, broad
  derivation dependency graph (stale visibility = security bug), storage ~ O(objects × subjects).
  **Convergence:** Level 2b ≈ "apply security at projection time" (the reporting star-schema of
  §5.2). If you reach it, materialise it on the **replica/projection during ETL**, not the
  operational DB.

### 4.5 What each scenario requires

- **Service principal:** `IsMasked() => false`, no row masking — the paging fix is **purely the
  SQL-adapter pushdown**, no security-model change.
- **Per-user pass-through (and the existing workspace Pull):** needs the **Level-1 visibility
  predicate**.

This reinforces "start with a service principal": it makes paging a contained adapter feature and
defers the security-predicate work.

### 4.6 Recommendations

1. **Add SQL-side paging/count pushdown to the extent** — `OFFSET/FETCH` + a real
   `SELECT COUNT(*)` path + `Skip`/`Take` on the `Extent` API. Necessary for everyone; sufficient
   alone for the service-principal feed; a general scalability win for all of Pull.
2. **For per-user pass-through, add the foldable visibility predicate (Level 1)** built on
   `Grant.EffectivePermissions`, folded via `AddContainedIn`, with the `AllowedClasses` trim moved
   into the extent as an `instanceof`. Keep cell masking in memory (paging-neutral). Property-test
   `folded-set ≡ evaluator-trimmed-set` (a too-loose predicate leaks row *existence*, which cell
   masking does not catch).
3. **Treat Level 2a/2b as scale escalations only**; if 2b, materialise in the Layer-B projection.

**Fallback if security is untouched:** OData-local server-driven keyset paging (`@odata.nextLink`,
seek cursor with object `Id` as tiebreaker) + trim-and-fill + best-effort `$count`. Tolerable for
Import-mode BI but leaves the Pull root cause in place.

---

## 5. Companion: the reporting projection (Layer B)

For curated dashboards and high-volume analytics where Import/DirectQuery performance matters,
OData-over-the-domain is not always the best technology.

- Build a **denormalised reporting star schema** (fact/dimension tables), populated **through the
  domain API** (scheduled export or derivations) so business rules and access scoping apply at
  projection time. Host on a **read replica** so BI load never touches the operational DB. Power
  BI uses the native **SQL Server** connector.
- This is the same idea as §4.4 Level 2b ("apply security once at projection time").
- Two ways to build it: Allors maintains it in-domain (rules/security closest to the data), **or**
  an external pipeline pulls via OData and builds the warehouse (decoupled, but only sees what the
  feed exposes and must trust the service-principal scope) — see §6.2.

A real-time **push/streaming** path (Power BI push datasets via the REST API) exists for KPI
tiles only; not a substitute for the above.

---

## 6. Ecosystem — who consumes this, and pipelines

### 6.1 OData clients

OData is an OASIS/ISO standard with discoverable `$metadata`, so one endpoint serves many
consumers with no per-client adapter:

- **Excel** (native *From OData feed* — often the real killer feature), and the whole **Power
  Query** family (Power BI, Power Automate, Power Apps, Analysis Services, **Microsoft Fabric**
  dataflows).
- **Tableau** (OData connector).
- **Salesforce Connect** — surfaces OData as live **External Objects** (no copy).
- **SAP** (Gateway/Fiori/Analytics Cloud), **Dynamics/Dataverse** (virtual tables).
- **ETL/integration** — Azure Data Factory, Synapse, SSIS, Logic Apps, iPaaS tools.
- **SDKs** — .NET (`Microsoft.OData.Client` typed proxies from `$metadata`), Python (`pyodata`),
  Java (Apache Olingo), JS/TS, R; and any HTTP client (it's REST+JSON).
- For context, **Microsoft Graph is itself OData** — the pattern is proven at scale.

Caveat: clients vary in fold fidelity, so the server must publish **capability annotations**
truthfully (mark computed props / unsupported operators) rather than let a client silently filter
client-side.

### 6.2 OData as the first step in a pipeline

Yes — arguably its strongest use:

```
Allors OData feed → ETL/ELT (ADF / Fabric dataflow / Synapse / Airbyte / Fivetran)
                  → warehouse/lakehouse (Fabric, Snowflake, Synapse, BigQuery)
                  → dbt / semantic layer → many reports
```

The feed is the **governed extract boundary** — no DB credentials handed out; everything
downstream inherits the service-principal scope. Two make-or-break concerns:

- **Incremental extraction.** Pipelines must not full-reload. OData v4 supports **delta /
  change-tracking** (`Preference: odata.track-changes` → delta links with `$deltatoken`). Allors
  has the backing state: `Strategy.ObjectVersion`, the Sync protocol's version tracking, and the
  `Auditable`/`Versioned` interfaces (modified timestamps). Implement either true OData delta or
  the simpler watermark (`$filter=ModifiedDate gt <last>`).
- **§4 matters more, not less.** ELT pulls large volumes, so the full-extent-materialise problem
  bites harder; the OFFSET/FETCH/COUNT pushdown is effectively a prerequisite, and delta is the
  other half.

Keep the feed **read-only**: writes must go through the domain (derivations/validation/security),
not OData PATCH.

---

## 7. Alternatives to OData

The protocol is a projection (§2); the choice is by audience.

### GraphQL

Strong, and in one respect a *better* fit than OData:

- **Multiple inheritance just works** — a type can `implements Party & Deletable & Versioned`
  (interfaces *and* unions are native), which **dissolves the entire spine-selection problem**
  (§3.1). The meta→SDL mapping is more faithful than meta→EDM.
- **Selection sets ≈ prefetch trees**; **Relay cursor connections are keyset-based** (aligns with
  the §4.6 fallback); **introspection ≈ `$metadata`**.

But for BI it has a decisive weakness: **BI tools don't speak GraphQL** (you'd write a custom,
non-folding connector — the bespoke-protocol problem we reject), and it **doesn't standardise
filter/aggregate/paging** (every schema invents its own), so there is no universal fold.

> **OData for analysts/BI/integration; GraphQL for application developers.** Not mutually
> exclusive — both generated from the same meta model over the same core (§2).

### Others

- **REST + OpenAPI** — universal, codegen-friendly, but no standardised query semantics; weak for
  ad-hoc/self-service, fine for fixed reports/integration.
- **SQL via ODBC/JDBC (= Layer B, §5)** — the most universal *analytics* interface; folds
  aggressively; often the *best* technology for heavy dashboards. Bypasses domain/ACL unless via
  secured views / DB row-level security.
- **Driver bridges (e.g. CData)** — ODBC/JDBC drivers that wrap an OData/REST endpoint, so
  SQL-only tools see a database without building both servers.
- **Semantic/headless-BI layers (Cube, dbt semantic layer, Analysis Services tabular)** — model
  once, expose SQL/REST/GraphQL/MDX. The natural endpoint if "serve every protocol from one
  model" becomes a goal.
- **MDX/XMLA** (OLAP cubes; Excel PivotTables) — only if going full cube.
- **Parquet/lakehouse exports** and **streaming/CDC** — for very large scale and real-time;
  complementary.

---

## 8. Recommendation & roadmap

Ship an **OData feed** for self-service plus a **reporting star schema on a replica** for
dashboards, secured by a **reporting service principal** to start (per-user pass-through later).
Build it as a thin projection over a properly-factored protocol-agnostic core, so the §4 fix is
done once and a future GraphQL endpoint is cheap.

Incremental, low-risk path:

1. **Extract the execution service** behind the existing JSON Pull; prove byte-for-byte parity
   (pure refactor, covered by existing tests).
2. **Land the §4 fixes in the core** — `OFFSET/FETCH`/`COUNT` pushdown (everyone benefits) and,
   for per-user, the Level-1 visibility predicate. Assert `folded-set ≡ evaluator-trimmed-set`.
3. **Add the OData front-end** over the clean core (EDM generator + query compiler + serialiser)
   — the spike below.
4. **Add GraphQL later** if app-developer demand appears — a new front-end, zero core changes.

**Spike:** a handful of entity sets generated from the meta model, `$filter` folded to an
`Extent`, behind the reporting principal — to validate folding + security end-to-end. Pair it
with the SQL-adapter paging pushdown.

---

## 9. Open questions

1. Which classes/roles form the reporting workspace(s) — and do any audiences need distinct model
   shapes (and thus distinct spines)?
2. Data volumes / refresh cadence (Import vs DirectQuery; whether Layer B / a pipeline is needed
   day one; whether delta extraction is required)?
3. Is a read replica available, or must reporting target the operational DB?
4. Security: service principal first, or is per-user isolation a hard launch requirement?
5. Hosting: Power BI Service (cloud) vs Report Server (on-prem) — does an on-prem data gateway
   come into scope?
6. Is a GraphQL (app-developer) projection anticipated, i.e. how hard to insist on the
   protocol-agnostic core up front?

---

## 10. Key files

- `dotnet/System/Database/Allors.Database/Data/` — the query IR (`Pull`, `IExtent`, predicate AST,
  `Result`, `Select`, `Node`, `Sort`, `IVisitor`).
- `dotnet/System/Database/Allors.Database.Workspace.Json/Pull/PullExtent.cs`,
  `PullResponseBuilder.cs` — current execution/trim/page + JSON shaping (the seam to extract).
- `dotnet/Core/Database/Domain/Core/Security/AccessControl/{Database,Workspace}/` — the two ACL
  implementations and `IsMasked`.
- `dotnet/Core/Database/Domain/Core/Rules/Grant/GrantEffectivePermissionsRule.cs` — derived,
  indexed `EffectivePermissions`.
- `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Extents/SqlExtent.cs` — `Count`
  materialises all ids; no `OFFSET/FETCH`.
- `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Predicates/RoleContainedInExtent.cs`
  — the existing containment→SQL fold the visibility predicate reuses.
- `dotnet/Apps/Database/Server/Startup.cs`, `Custom/` — Apps server wiring; where an OData
  controller would mount.
