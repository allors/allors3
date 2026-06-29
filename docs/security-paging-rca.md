# Access Control & Paging — Root Cause Analysis

> Status: Draft / investigation. Branch: `claude/power-bi-integration-design-7uer34`.
> Companion doc: [`power-bi-odata-integration.md`](./power-bi-odata-integration.md), whose
> paging story depends on this analysis.

## 1. Summary

Reconciling Allors' object-level access control with **paging** (`$top`/`$skip`/`$count`, or the
protocol's `Result.Skip`/`Take`) is broken in a specific, structural way: security and paging are
both applied **in application memory, after** the SQL query has materialised the full extent.

This is **not** an OData problem. The **workspace Pull** pipeline exhibits the identical
behaviour; an OData feed would merely inherit it. The fix therefore belongs at the
query/security layer, where it benefits both surfaces.

Encouragingly, the underlying security state is already stored, indexed relational data
(`Grant.EffectivePermissions` is even derived + indexed), so object visibility **can** be
expressed as a foldable predicate. The only genuinely missing capability is SQL-side
paging/count pushdown on the extent.

## 2. What the code actually does

### 2.1 Pull executes in three phases

`dotnet/System/Database/Allors.Database.Workspace.Json/Pull/PullExtent.cs`, `WithResults`:

```csharp
var extent = dataExtent.Build(this.transaction, this.pull.Arguments);     // (1) SQL: data predicate only
this.transaction.Prefetch(this.prefetchPolicyCache.Security, extent);     //     prefetch security for ALL rows
...
trimmed = extent.Where(response.Include).Take(resultsRequired).ToArray(); // (2) in-memory security trim
...
var paged = objects.Skip(result.Skip.Value).Take(result.Take.Value);      // (3) in-memory paging
```

1. `extent.Build(...)` folds only the **data** predicate to SQL and returns object ids.
   Security is not part of this query.
2. `.Where(response.Include)` trims **in application memory**.
3. `.Skip().Take()` pages the already-trimmed, in-memory sequence. No total/count is computed
   or returned anywhere in the protocol.

The `resultsRequired = max(Skip + Take)` over-fetch in branch (2) is a heuristic, not a fold —
its existence is evidence the authors already feel this friction.

### 2.2 What "Include" checks

`PullResponseBuilder.Include` keeps a row iff
`AllowedClasses.Contains(class)` **and** `!AccessControl[obj].IsMasked()`. The two
`IAccessControl` implementations differ:

- **`DatabaseAccessControl.IsMasked() => false`** (trusted server context) — **no row hiding at
  all**. Row inclusion reduces to the `AllowedClasses` (subtype) check.
- **`WorkspaceAccessControl.IsMasked()`** — *real* row masking: per class there is a designated
  **mask role** (`IDictionary<IClass, IRoleType>` from `IWorkspaceMask`); the object is hidden
  iff `!CanRead(maskRole)`.

So real row-level masking is a **workspace-client** concern (≈ per-user pass-through); the
trusted server does class-level + cell-level only.

### 2.3 How masking is computed

`DatabaseAccessControlList.CanRead(roleType)` =
`readPermissionId ∈ grants.PermissionSet && ∉ revocations`, where the grants are gathered
**per object in C#** (`DatabaseAccessControl.GetAccessControlList`): collect the object's
`SecurityTokens` (+ `DelegatedAccess`), call `security.GetVersionedGrants(transaction, user,
tokens)`; same for revocations. Cell masking happens during serialisation
(`tree.Resolve(..., acls, ...)`, `select.Get(v, acls)`) and **does not remove rows**.

### 2.4 The SQL extent cannot page or count

`dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Extents/SqlExtent.cs`:

```csharp
public override int Count => this.ObjectIds.Count;          // list length, NOT SELECT COUNT(*)
private IList<long> ObjectIds => this.objectIds ??= this.GetObjectIds();  // SQL returns ALL ids
```

Every access path (`GetEnumerator`, `ToArray`, `GetItem`, `Contains`, `Count`) funnels through
the fully-materialised id list. Only `ORDER BY` folds (`ExtentSort`); there is **no**
`OFFSET`/`FETCH`/`LIMIT`. The `Extent` API itself exposes only `Count`, `Filter`, `AddSort` — no
`Skip`/`Take`.

## 3. Root cause

> Allors models access control as an **evaluable function over a materialised object**
> (`IAccessControl[obj] → bool`), not as a **reifiable predicate** the query layer can compose
> and push to the database. Data selection (the Extent) folds to SQL; security is a separate,
> post-materialisation pass — and so is paging.

Consequences, all mechanical and none OData-specific:

1. The SQL query is **never bounded by page size** — the full extent materialises every request.
2. **`$count`/totals require a full scan + trim** — visible rows can't be counted without
   producing them all.
3. **Deep paging rescans from row 0** every request — the offset is applied after the in-memory
   trim; there is no cursor.
4. **Pages can come back short** when masking is real (workspace clients): ask for 50, masking
   removes some, get fewer.

A secondary amplifier: `PullExtent` prefetches security over the **whole** extent
(O(total), not O(page)) before trimming.

## 4. The state needed to fix it already exists

Every hop required to express "can this principal see this row?" is a stored, indexed relation,
and one is already derived:

| Hop | Status |
|---|---|
| `Object.SecurityTokens` | stored, **indexed** M2M |
| `SecurityToken.Grants` | stored, **indexed** M2M |
| `Grant.Subjects` / `Grant.SubjectGroups` | stored, **indexed** |
| `Grant.EffectivePermissions` | **derived + indexed** — `GrantEffectivePermissionsRule` sets it to `Role.Permissions` |
| `Grant.Revocations` / `Object.Revocations` | stored |

The containment fold already exists as a SQL predicate
(`Predicates/RoleContainedInExtent.cs`, `AssociationContainedInExtent.cs`), so
`AddContainedIn(role, subExtent)` compiles to a SQL `EXISTS`/`IN` subquery.

**Consistency-by-construction:** the evaluator's `VersionedGrant.PermissionSet` derives from the
same `Role.Permissions` → `EffectivePermissions`. A predicate built on `EffectivePermissions`
therefore shares a source of truth with the in-memory ACL — the answer to the
dual-implementation divergence risk is *don't write a second security algorithm; build the
predicate on the same derived data the evaluator consumes.*

## 5. The materialisation ladder

**Level 0 (today).** `Grant.EffectivePermissions` — `Role`→`Permission` flattened onto `Grant`,
derived + indexed. Grant↔SecurityToken↔Object all indexed.

**Level 1 — per-request granting-token set. *Recommended default; no schema change.***
Once per `(user, readPermission)`, compute `grantingTokens` = tokens whose grants apply to the
user and contain the permission, minus revocations — a single query over the indexed Grant
relations. The per-row predicate is then one indexed containment:

```
obj.SecurityTokens ∩ grantingTokens ≠ ∅      // AddContainedIn(SecurityTokens, grantingTokenExtent)
```

folded via the existing `RoleContainedInExtent`. Zero new persistence, reuses existing indexes,
consistent-by-construction. Handle `DelegatedAccess` by also traversing the delegated tokens
(the §5.2-style sub-extent chaining in the companion doc).

**Level 2a — subject-centric persisted relation.** Persist a derived
`Subject → (Permission, SecurityToken)` map so `grantingTokens(U,P)` is a direct lookup,
skipping Grant traversal. Recomputed on security-config changes (grants/roles/memberships —
infrequent vs. reads). Moderate size. Escalate here only if the Level-1 precompute is a measured
bottleneck.

**Level 2b — object-centric "readers" materialisation.** Persist `Object → Subject` (fully
denormalised "who can read this row"). Visibility becomes a single indexed join — best query
performance, trivially correct paging/count, DirectQuery-friendly. Costs:

- **Write amplification** — a token change on a shared object, a `Grant` edit, a
  `Role.Permissions` change, or a group-membership change fans out to many `(object, subject)`
  rows.
- **Broad derivation dependency graph** — must be airtight or you get stale-visibility
  *security* bugs.
- **Storage** ~ O(objects × subjects-with-access).

**Convergence with reporting (Layer B).** Level 2b is essentially "apply security once at
projection time" — the same idea as the reporting star-schema. If you need object-centric
materialised visibility, build it on the **replica/projection during ETL**, not the operational
DB, so the write amplification taxes the reporting refresh rather than transactional writes.

## 6. What each scenario actually requires

The most useful result of the investigation, tied to the service-principal vs. per-user
decision:

- **Trusted server / OData with a reporting service principal** — `IsMasked() => false`, so
  there is **no row masking**. Visibility reduces to the `AllowedClasses` restriction, foldable
  as an `instanceof`. The paging fix is therefore **purely the SQL-adapter capability addition**
  — *no security-model change at all.*
- **Per-user pass-through (and the existing workspace Pull)** — `WorkspaceAccessControl`
  masking is real, so this is the only case that needs the **Level-1 visibility predicate**.

This reinforces "start with a service principal": it makes paging a contained adapter feature
and defers the security-predicate work until per-user pass-through is committed to.

## 7. Recommendations

1. **Add SQL-side paging/count pushdown to the extent** — `OFFSET/FETCH` (or `LIMIT`) generation
   in `ExtentStatement`/`BuildSql`, a real `SELECT COUNT(*)` path for `Count`, and `Skip`/`Take`
   on the `Extent` API. Necessary for everyone; sufficient on its own for the service-principal
   OData feed; a general scalability win for all of Pull (removes the full-extent materialisation
   and the O(total) security prefetch).
2. **For per-user pass-through, add a foldable object-visibility predicate (Level 1)** built on
   `Grant.EffectivePermissions` so it shares the evaluator's source of truth, folded via the
   existing `AddContainedIn` machinery. Keep the per-object ACL as authority for **cell masking**
   (paging-neutral). Also move the `AllowedClasses` trim into the extent as an `instanceof`
   restriction. Property-test that `folded-set ≡ evaluator-trimmed-set` over random
   principals/objects — a too-loose predicate leaks row *existence* (ids), which cell masking
   does not catch.
3. **Treat Level 2a/2b as scale escalations only** — and if you reach 2b, materialise it in the
   Layer B projection, not the operational store.

## 8. Fallback if security is not changed

OData-local mitigation: server-driven keyset paging (`@odata.nextLink`, seek cursor with object
`Id` as the total-order tiebreaker) + trim-and-fill + best-effort/omitted `$count`. This makes
OData tolerable for Import-mode Power BI but leaves the Pull root cause in place and gives up
exact counts. Because the problem is shared, fixing it at the query/security layer is preferred.

## 9. Key files

- `dotnet/System/Database/Allors.Database.Workspace.Json/Pull/PullExtent.cs` — the three-phase
  fetch/trim/page.
- `dotnet/System/Database/Allors.Database.Workspace.Json/Pull/PullResponseBuilder.cs` —
  `Include`, masking, serialisation.
- `dotnet/Core/Database/Domain/Core/Security/AccessControl/Database/DatabaseAccessControl*.cs`,
  `.../Workspace/WorkspaceAccessControl*.cs` — the two ACL implementations and `IsMasked`.
- `dotnet/Core/Database/Domain/Core/Rules/Grant/GrantEffectivePermissionsRule.cs` — the derived,
  indexed `EffectivePermissions`.
- `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Extents/SqlExtent.cs` — `Count`
  materialises all ids; no `OFFSET/FETCH`.
- `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Predicates/RoleContainedInExtent.cs`
  — the existing containment→SQL fold the visibility predicate would reuse.
