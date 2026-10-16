# Workspace read cache

> **Status: Current.** Ownership map for immutable cached contents and publication.

[Connection cache](../workspace-reads.md#connection-cache) is the application reference.
This page is for maintainers; applications must not rely on it. The
[read assembler](workspace-read-coordinator.md) resolves responses, and
[Session state](workspace-session-state.md) maps editing baselines.

## Code ownership

The internal [.NET cache](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ReadStore.cs)
is shared by Local and HTTP adapters. The internal
[TypeScript cache](../../typescript/modules/libs/system/workspace/adapters-json/src/lib/read/read-store.ts)
lives in the JSON adapter. Each belongs to one normal connection; neither owns a queue or an
independent lifecycle. Public immutable scope values remain available in result diagnostics.

| Owner | Responsibility |
| --- | --- |
| Connection | Own admission, operation ordering, exact scope and cache lookup lifetime. |
| Stored values | Copy mutable values on entry and exposure. Record identity includes class, data version and complete grant/revocation keys; security identity includes kind, ID and version. Permission encoding belongs to the representation revision. |
| Cache | Intern identical bodies, reject conflicting bodies for one key and publish current indexes atomically. Retaining a historical body does not make it current. |
| Request inventory | Hold direct references to exactly the advertised bodies until response assembly finishes. Resolve reuse indexes against this inventory. |
| Completed data | Hold immutable records, complete security/permission closure and copied outputs during adaptation. No disposal or release protocol is required. |
| Session origin state | Hold its accepted and checkpoint records directly, independently of later cache publication. |

A record's grant/revocation keys are provenance. They do not imply that a corresponding security
body is available. Hint construction still distinguishes those cases; only an actually held body
can satisfy a response reuse index. Historical record contents never supply current authority.

## Publication and lifetime

Publication validates exact scope, metadata, conflicting identities, explicit unavailability and
the complete closure before advancing current indexes. Missing or unrelated bodies reject the
whole publication. Entries omitted from a query keep their current membership. Permission checks
use current membership and current security/permission bodies; revocations win.

.NET imports and publishes under the connection's state lock. TypeScript preflights conflicts
before synchronous publication. Input copying and any external callbacks finish before the final
admission check; the commit uses owned values and private collections.

Historical exact-key maps retain accepted records, security and permissions while the connection
is live, including bodies no longer used by a Session baseline or checkpoint. This preserves
cross-Session and historical cache hits. There is no eviction API, automatic pruning or memory
bound. Long-lived history growth remains a separate policy decision.

Retirement stops admission and invalidates current authority immediately. Connection close drains
dispatched handling before clearing cache lookup. Held immutable records, completed data and diffs
remain inspectable through ordinary references. Clearing lookup does not free values still held by
applications, and it cannot make a closed Session active again.

## Authoritative tests

Cache and immutable-value checks live in
[.NET Local PullTests](../../dotnet/Core/Workspace/Tests.Local/Tests/PullTests.cs) and the
[TypeScript protocol spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/read-contract.spec.ts).
They cover exact identities, copied nested values, complete closure, conflicting bodies and
all-or-nothing publication. The [Session tests](workspace-session-state.md#authoritative-scoped-tests)
cover independent accepted/checkpoint records, current authority, historical reuse and close.
