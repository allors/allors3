---
title: "Session and versioned cache design"
description: "Proposed session serialization, immutable versioned caches, security bindings and response publication."
audience: "Maintainers"
kind: "Explanation"
status: "Planned"
---

> **Status: Planned.** Design proposal for maintainers. The direction is session-local
> serialization and shared, complete, immutable cache entries. The detailed contracts below
> are recommendations for review, not implemented behaviour or an approved implementation plan.

The current [session state explanation](workspace-session-state.mdx) remains authoritative for
editing baselines, merge, diff and reset. This proposal replaces repeated security-completeness
checks with explicit ownership and publication rules. It does not provide database-wide
snapshot isolation or change server-side authorization.

Implementation observations were checked on 2026-10-24 against
[`b54fe278e31bbd3f879e7027173c6c9250a509a0`](https://github.com/allors/allors3/commit/b54fe278e31bbd3f879e7027173c6c9250a509a0).
No runtime implementation is included in this design change.

## Problem and existing support

Today, Sync publishes an object before Access and Permission finish. Another session can see
the object version and access IDs in the shared connection cache, skip synchronization, and
return while security dependencies are missing. A failed request can also leave partial state
behind for a later sequential pull. Serializing only within sessions does not prevent either
problem while unfinished state is shared.

Both remote workspace implementations also replace grants and revocations by ID. An older
response arriving late can replace newer information used by another session. A session
retains its editing record, but permission checks consult the connection's live access maps.

The protocol already contains useful version information:

| Response | Current information |
| --- | --- |
| Pull | Each object's ID, version and access IDs; top-level grant and revocation `[id, version]` lists. |
| Sync | Object values and access IDs, but no access-version lists. Roles are filtered using the ACL available to this Sync request. |
| Access | Each returned grant or revocation's ID, version and permission IDs. Requests accept IDs, not historical versions. |
| Permission | Permission ID and its class, operand and operation. |

The current TypeScript and .NET remote adapters do not use Pull's top-level access-version
lists to select their cache entries. Versions are per-object counters, not a common database
revision. A grant at version 4 and a revocation at version 4 need not come from the same time.

## Ownership and guarantees

| Owner | Proposed responsibility |
| --- | --- |
| `DatabaseConnection` | Reuse indexes for complete immutable permission definitions, versioned access entries and object payload variants. Its identity fixes the database, authenticated user, workspace projection and model/configuration epoch. |
| Session | Identity map, local changes, accepted editing records, selected security bindings and a FIFO queue for complete `pull` and `call` workflows. |
| One workflow | Selected result IDs, operation-local references, response staging, completeness validation, retries and final session adoption. |

The guarantees are:

1. Every reusable access entry has all its permission definitions available.
2. Every reusable object payload has an exact binding to complete access entries.
3. Published values never mutate. A new version occupies another cache entry.
4. Another session cannot change an existing session object's selected data or security.
5. A failed loading attempt publishes no partial entries and adopts no partial session state.
6. Session adoption uses the records selected by that workflow, not a later lookup of whichever
   record is current in the shared connection.

These guarantees remove the repeated walk through permission lists on unchanged cache hits.
Checking the requested object and access version keys is still necessary, as is processing
new permission lists once. Ordinary permission evaluation also remains necessary.

Published entries must not expose mutable permission ranges, arrays or maps. A `readonly`
field alone does not make its contents immutable. Construction must take ownership safely
or copy mutable buffers so later staging changes cannot alter a published value.

## Cache identity and security bindings

Within a fixed connection scope, grant and revocation keys are `(ID, version)`, with separate
entry kinds. Permission definitions belong to the same connection/model scope. Authentication
or model changes require a new scope; IDs and versions must not silently cross that boundary.

For example:

| Shared entry | Session A's object | Session B's object |
| --- | --- | --- |
| Grant 11, version 4 | References this entry | |
| Grant 11, version 5 | | References this entry |
| Revocation 21, version 7 | References this entry | References this entry |

Version 4 arriving after version 5 does not overwrite it. A selected binding retains strong
references to exact entries, never an instruction to use the latest access entry by ID.
Version keys identify entries but are not sufficient to retain them after reuse-index eviction.
A separate latest-entry index, if needed for discovery, must not determine existing bindings.

An object payload key must include its ID, data version and canonical grant/revocation version
binding, within the fixed projection scope. Sync filters roles with `acl.CanRead`; therefore
the same object data version can yield different payloads under different security versions.
An access-version change invalidates a payload cache hit even if the data version and access
IDs are unchanged. The loader must Sync that object again.

Bindings are per object, not one mutable session-wide latest-grant map. Updating such a map
would silently alter objects outside the pull. This proposal deliberately permits retained
objects to have different security observations; it does not claim one snapshot for the
whole session or object graph.

## Session scheduling and adoption

The session queue starts before building and dispatching a Pull request and ends after result
binding and session adoption. `pull` and procedure-bearing `call` use the same queue. A rejected
workflow must release its place and leave the queue usable. Other sessions remain concurrent.

This is a pull-workflow guarantee. Ordering with concurrent `push`, `invoke`, synchronous edits
and reset remains a separate contract; the queue must not be advertised as making arbitrary
concurrent session use safe. No cancellation API is added by this proposal.

The TypeScript adoption paths currently reread the connection in
`Session.instantiateDatabaseStrategy`, `DatabaseOriginState.onPulled` and the `Strategy`
constructor used by `Strategy.fromDatabaseRecord`. These paths must receive or retain the
selected record explicitly. The equivalent .NET paths need the same review. Lazy result
collections must resolve to session bindings established before the workflow returns.

Recommended security-adoption policy:

- A successful loading workflow advances security for the objects it covers. Other sessions
  and objects outside that workflow retain their selected bindings.
- Data acceptance continues to follow the existing merge rules.
- A merge conflict keeps the editing baseline and local changes, but still adopts the newly
  loaded security binding for that object. A newly denied permission must not remain allowed
  merely because data could not be merged.
- Network or response-validation failure adopts neither data nor security.

The selected security binding must therefore be separate from the accepted editing record.
This policy changes today's connection-wide live refresh behaviour and requires confirmation
before implementation. It preserves the distinction between editing state and authorization.

For first-time lazy instantiation of an object known only to the connection, retain the
existing discovery capability: choose one complete compatible payload and bind it once into
the session. Existing session objects never follow that discovery index. Define a deterministic
selection order during implementation; an ambiguous projection should require an explicit
Pull instead of mixing fields and security. This is not graph snapshot isolation.

## Loading and the minimal protocol addition

Add grant and revocation `[id, version]` lists to Sync responses, matching Pull's representation.
The server must derive them from the same ACL instances used to filter the returned roles.
Update both protocol models and both JSON serializers. A new client must reject an absent
required manifest instead of guessing; an absent or empty manifest is valid when no access
entries are referenced. Deployment compatibility needs an explicit server/client policy.

Access responses already carry versions. The first implementation can compare those versions
on the client without changing Access requests or introducing historical server reads.

The proposed workflow is:

1. Send the initial Pull request once. Retain its result names, collections, values and pool
   IDs, including any procedure output.
2. Use Pull's version lists to select exact complete cached payloads and access entries.
3. Sync objects whose payload or security binding is missing or different. Sync's payload and
   version lists become authoritative for those objects, as Sync may observe later state.
4. Stage the payloads and resolve their exact access entries. Fetch missing entries through
   Access, then fetch missing permission definitions. Decode and validate before publication.
5. Publish complete entries in a short synchronous section in TypeScript. The .NET implementation
   needs suitable synchronization for concurrent threads. Never await while publishing.
6. Adopt the workflow's selected references and apply the session merge policy.

A successful HTTP response is not enough: every required object, access entry and permission
definition must be present and valid. Never manufacture an empty grant to cover an omission.
Complete entries can remain reusable after a later data merge conflict; shared publication
and editing-baseline acceptance are different boundaries.

## Version mismatch and retry

The server may advance between Sync and Access. For example, Sync can bind grant `11@4`,
while Access returns `11@5`. Store no binding that labels version 5 as version 4.

Recommended policy: discard the affected staged payloads and repeat their read-only Sync
hydration, for at most three attempts in total. Each attempt rebuilds its bindings from its
own Sync manifest. Retry every staged payload depending on a mismatched or missing entry,
not only the first object that revealed the problem. A missing grant can indicate changed
membership and follows the same bounded refresh path. Malformed responses and missing
permission definitions fail explicitly.
Missing or deleted objects must not produce silently incomplete successful results.

Retries preserve the initial Pull's named results, scalar values and selected IDs. They never
resend the initial Pull or its procedure. The existing server Pull policy may itself retry
database exceptions; this proposal does not provide global exactly-once procedure execution.
It follows the existing separation between Pull selection and later Sync materialisation,
rather than promising one transaction snapshot.

On exhaustion, return a distinct hydration failure; the old session state remains usable.
For `call`, the error must distinguish a completed procedure from failed local hydration so
the application does not unknowingly repeat a side effect. The exact public error shape is
an open API decision. A remote procedure failure continues to use its existing result path.

The cost is extra Sync/Access work during security churn. Combining payload and access data
in one server response would reduce that window, but changes server composition and payload
costs more substantially. Historical Access reads would require server history semantics that
do not exist. Neither is required for this proposal's local cache guarantees.

## Retention and freshness

Use bounded shared reuse indexes. Sessions and active workflows keep strong references to
the immutable entries they selected. Evicting an index entry must not invalidate those
references. Values become reclaimable when no session, workflow or cache retains them.
Capacity and workload measurements belong to the implementation review; no production memory
or latency improvement has been measured yet.

A session sees a newer revocation when it successfully refreshes the relevant object; another
session's refresh does not advance it. Explicit invalidation or notifications can later define
other refresh triggers. Server-side authorization remains authoritative. A stable local
binding is neither the newest server state nor a database-wide historical snapshot.

## Validation and delivery boundary

This page proposes the contract; runtime implementation starts after the open decisions below
are resolved.

Recommended implementation scope is the TypeScript JSON workspace, shared protocol/server
response changes, and equivalent .NET remote workspace behaviour before calling the platform
change complete. Preserve the existing editing-baseline tests. Do not include signals,
persistent caching, singleflight, historical server snapshots or unrelated Push API changes.

| Area | Required evidence |
| --- | --- |
| Scheduling | Same-session pull/pull and call/pull run in order; rejection does not poison the queue; different sessions overlap. |
| Complete publication | Delay or fail Access and Permission; another session never observes partial entries. Validate omitted and malformed dependencies; later staging-buffer mutation cannot change published values. |
| Version identity | Older and newer responses coexist in either arrival order; each session keeps its selected binding. |
| Payload projection | Security-only changes force Sync; identical object versions with different security payloads remain distinct. |
| Adoption | All adoption paths use selected references; lazy instantiation and result collections follow the stated policy. |
| Retry | A version mismatch refreshes all staged objects sharing the affected dependency; persistent churn reaches the bound; hydration retries issue no additional procedure-bearing Pull request. |
| Editing and security | Existing merge/diff/reset/push-baseline matrix passes; a merge conflict preserves edits while adopting a new revocation. |
| Retention | Eviction preserves active references and cache growth respects its configured budget. |
| Cost | Complete cache hits do not walk permission lists again; report operation counts and benchmark representative workloads. |
| Protocol parity | Sync manifests match the ACLs used for role filtering; both serializers and remote clients enforce the same rules. |

Before implementation, confirm the proposed per-object security refresh policy, bounded
hydration retry and procedure-failure contract, lazy-instantiation rule, compatibility policy
and the first implementation boundary. These are recommendations, not resolved product facts.

## Research basis and code evidence

- [Zanzibar, section 3.2.5](https://www.usenix.org/system/files/atc19-pang.pdf): authorization
  cache keys include snapshot timestamps; request coalescing is separate. Its global snapshots
  are stronger than the per-object versions proposed here.
- [Datomic best practices](https://docs.datomic.com/reference/best.html): retain a selected
  immutable database value for a unit of work instead of repeatedly consulting current state.
- [PostgreSQL transaction isolation](https://www.postgresql.org/docs/18/transaction-iso.html#XACT-REPEATABLE-READ):
  a common snapshot is an additional contract beyond retaining individual versions.
- [SpiceDB consistency](https://authzed.com/docs/spicedb/concepts/consistency): exact historical
  state and minimum freshness are separate choices.
- [PostgreSQL version reclamation](https://www.postgresql.org/docs/18/routine-vacuuming.html#VACUUM-FOR-SPACE-RECOVERY):
  retain versions while readers need them, then reclaim unused history.
- [Pull protocol](../../../../../typescript/modules/libs/system/common/protocol-json/src/lib/api/pull/pull-response.ts),
  [Sync protocol](../../../../../typescript/modules/libs/system/common/protocol-json/src/lib/api/sync/sync-response.ts)
  and [Access protocol](../../../../../typescript/modules/libs/system/common/protocol-json/src/lib/api/access/access-request.ts).
- [Sync response builder](../../../../../dotnet/System/Database/Allors.Database.Workspace.Json/Sync/SyncResponseBuilder.cs)
  and [Access response builder](../../../../../dotnet/System/Database/Allors.Database.Workspace.Json/Access/AccessResponseBuilder.cs).
- [TypeScript connection](../../../../../typescript/modules/libs/system/workspace/adapters-json/src/lib/database/database-connection.ts)
  and [session editing state](../../../../../typescript/modules/libs/system/workspace/adapters/src/lib/session/originstate/database-origin-state.ts).
