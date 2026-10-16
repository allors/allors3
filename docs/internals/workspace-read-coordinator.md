# Workspace read assembly

> **Status: Current.** Ownership map for assembling and publishing complete reads.

[Complete workspace reads](../workspace-reads.md) is the application reference.
This page is for maintainers; applications must not rely on it. See
[protocol validation](workspace-read-protocol.md), [cache ownership](workspace-read-store.md), and
[Session state](workspace-session-state.md) for those owners.

## Code ownership

The internal [.NET ReadCoordinator](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ReadCoordinator.cs)
is shared by Local and both serializers. The internal
[TypeScript ReadCoordinator](../../typescript/modules/libs/system/workspace/adapters-json/src/lib/read/read-coordinator.ts)
lives beside its transport and cache. These helpers assemble reads under the normal connection;
they expose no standalone construction, queue, retirement or result-lifetime API.

| Owner | Responsibility |
| --- | --- |
| Connection | Own one operation gate, admission and retirement. Hold the gate through transport, publication, Session adaptation and notifications. |
| Read assembler | Discover then complete the captured original query, select exact held hints, validate scope/metadata/closure and publish a complete result. |
| Captured metadata | Check class, role, multiplicity, unit encoding and permission operands without depending on later caller changes. |
| Transport | Perform one attempt with captured messages and strict decoding; retain no cache contents. |
| Request inventory and completed data | Hold immutable references and copied outputs directly. Reuse indexes refer to the captured inventory, never a current-by-ID lookup. |
| Host | Keep authentication/configuration stable and close the connection before replacement. |

```mermaid
sequenceDiagram
    participant S as Session
    participant C as Connection owner
    participant R as Read assembler and cache
    participant T as Transport
    S->>C: Captured Pull
    C->>C: Enter operation gate; check admission
    C->>R: Assemble original query
    R->>T: Discover
    T-->>R: Scope and candidate headers
    R->>R: Capture exact held body references
    R->>T: Complete original query with hints
    T-->>R: Complete response
    R->>R: Validate closure; publish current indexes
    R-->>C: Immutable completed data
    C->>C: Stage; check admission; apply; notify
    C-->>S: Pull result; release operation gate
```

## Admission and publication

Discovery establishes neither current authority nor a completed result. Every ordinary Pull,
including cold, empty and fully cached reads, completes the same original declarations in a fresh
snapshot. Only final publication binds the initial population/user scope. Changed data, security
or membership between requests requires no restart.

Hint selection uses retained historical identities and the existing entry/byte bounds. Security
provenance can be advertised without a body; body reuse requires an actual reference in the request
inventory. Assembly validates every record's security closure and required permission descriptors.
A relation ID outside the returned pool stays unloaded even if the cache holds an old record.

After the final cancellation/admission check, publication updates current indexes together. .NET
uses the connection state lock; TypeScript publishes synchronously. Validation or transport failure
leaves previous current knowledge intact. A validated response proving scope changed invalidates
the connection; malformed messages, generic failures and message text do not establish that.

Protected mutations use the same full affected import, metadata checks and atomic publication.
Independent receipt/read validation precedes local application, so close or application failure
cannot erase valid remote commitment evidence. Invoke has no query discovery or pending completion.

Push may return a captured pending query. During the UI gap it owns only declarations, discovery
and scope, with no operation gate or body inventory. Explicit one-use completion enters the same
connection gate, obtains then-current held bodies and sends only the final read. It does not repeat
discovery. Ordinary merge, notification and lifetime checks apply.

## Initialization and drain

The normal factory performs an empty discover/complete read under the same operation owner before
publishing readiness, without creating Workspace/Session services or invoking their callbacks.
There is no separate cold-binding caller API.

Close stops admission and invalidates current authority before draining dispatched response
classification, adaptation and notifications. It then clears cache lookup and releases owned
resources. This is not a join of every queued public Task/Promise: rejected continuations can
unwind later but cannot use released services. .NET also drains construction reservations through
service factories and `OnInit`. The [Session ownership map](workspace-session-state.md#scoped-session-ownership)
owns registration and cleanup order.

## Authoritative tests

Controlled-response coverage lives in
[.NET Local PullTests](../../dotnet/Core/Workspace/Tests.Local/Tests/PullTests.cs) and the
[TypeScript protocol spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/read-contract.spec.ts).
`ReadCoordinatorAlwaysDiscoversBeforeCompleting`,
`ReadCoordinatorDiscoveryRepeatsTheOriginalSelectionAndBoundsHints` and
`ReadCoordinatorCompletesChangedDiscoveryWithoutRestarting` cover the unchanged two-request flow.
`ReadCoordinatorRejectsInvalidCompletionWithoutPublishing` covers failed publication.

`NormalConnectionCloseDistinguishesPendingQueryGapFromCompletion` and
`NormalQueuedOperationsCannotTouchResourcesAfterClose` cover the operation owner's drain boundary.
The [Session map](workspace-session-state.md) also names mutation, callback and service-lifetime tests.

SQL integration lives in
[Server.Local PullRetryTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullRetryTests.cs),
both HTTP PullTests classes
([System.Text.Json](../../dotnet/Core/Workspace/Tests.Remote.Json.SystemText/Tests/PullTests.cs),
[Newtonsoft.Json](../../dotnet/Core/Workspace/Tests.Remote.Json.Newtonsoft/Tests/PullTests.cs)), and
[TypeScript Pull specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/pull.spec.ts).
These check real complete closures, exact cache reuse and reselected data/security; they are not
performance measurements.
