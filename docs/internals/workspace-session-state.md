# Workspace session state

> **Status: Current.**

This page maps the current editing baseline in the .NET and TypeScript workspaces. It records
baseline, merge behavior and connection ownership; applications must not rely on this
internals page. The tests below are authoritative. The platform boundary is in
[ARCHITECTURE.md](../../ARCHITECTURE.md); domain terminology is in [Domains](../domains.md).

## Connection ownership

The [public lifecycle](../workspace-reads.md#scoped-sessions) owns ready creation and cleanup rules.
Normal Pull captures its declarations and completes discovery/read before adapting objects.

| Owner | State and responsibility |
| --- | --- |
| `DatabaseConnection` | Readiness, workspace admission, registered Sessions, the internal cache/read assembler and operation gate. Complete reads publish current authority; each Session accepts its own records. |
| `DatabaseOriginState` | Last accepted record and version. Adaptation checks `CanMerge` before replacing it; conflict leaves the entire old record in place. |
| `RecordBasedOriginState` | Local role changes, Diff and Reset against that accepted record. Reset does not fetch newer state. |
| Remote `DatabaseOriginState` | Push versions and to-many changes against the retained record. Local uses the same workspace implementation as HTTP. |

TypeScript's `an unchanged complete read advertises exact cached bodies without legacy requests`
and `pull forwards the session context and completes permission refresh before returning objects`
check exact reuse, fixed context and permission closure before object publication in the
[response-context spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/database/security/response-context.spec.ts).

## Characterization matrix

These retained tests use normal construction and two sessions of one workspace/connection. A remote session changes and pushes an
object, then pulls it to refresh the shared connection. The editing session has local changes
to a string, a many-to-one role, and a many-to-many role. All relation targets are loaded.

| Case | Observed behavior | Authoritative tests |
| --- | --- | --- |
| Only the remote session pulls | The editing session keeps its version, untouched integer value, local values, and all three diff originals. Its Push returns a rejected result with version diagnostics in both runtimes. Reset returns to the old baseline; a subsequent Pull accepts the remote record. | `DatabaseDiffRetainsBaselineWhenAnotherSessionRefreshes` / `databaseDiffRetainsBaselineWhenAnotherSessionRefreshes` |
| Remote changes only the integer | Editing Pull accepts the newer version and integer while preserving all three local edits and their diff originals. A subsequent Push succeeds and preserves both sides' changes. | `DatabaseEditingBaselineAfterPull("disjoint")` / `databaseEditingBaselineAfterPull: disjoint` |
| Remote changes one locally edited role plus the integer | Editing Pull reports one merge error for the object. Its version, integer, all local edits, and all diff originals remain unchanged. Reset clears edits but keeps the old record; Pull then accepts the remote record. | The `string`, `to-one`, and `to-many` rows of the same tests |

The to-many conflict deliberately uses additions of different members. Merge compares the whole
role against its baseline; it does not combine independent membership changes within that role.
The representative relation cases do not establish every cardinality or inverse-association
behavior.

Tests live in [the .NET MergeTests](../../dotnet/Core/Workspace/Tests/Tests/MergeTests.cs),
[DiffTests](../../dotnet/Core/Workspace/Tests/Tests/DiffTests.cs),
[TypeScript merge specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/merge.spec.ts),
and [diff specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/diff.spec.ts).
The .NET cases run through Local, System.Text.Json HTTP, and Newtonsoft.Json HTTP; TypeScript
uses HTTP.

## Unit merge equality

Merge compares the retained baseline with the refreshed remote value for each locally edited
role. It compares DateTime and binary values by content in both runtimes; the local value does
not decide whether the remote baseline changed.

| Locally edited role | .NET (all three adapters) | TypeScript HTTP |
| --- | --- | --- |
| DateTime | `Equals` compares DateTime values. | Date timestamps are compared with `getTime()`. |
| Binary | `SequenceEqual` compares byte-array contents, including length. | Binary is a base64 string, compared by value. |

`DatabaseUnitEqualityDuringDisjointPull` / `databaseUnitEqualityDuringDisjointPull` start with a
non-null unit value, edit it locally, and change only an integer remotely. Pull accepts the new
record/version while preserving the local edit and its diff original. A newly decoded array or
Date instance representing the same value does not cause a conflict.

`DatabaseUnitValueEqualityDuringPull` / `databaseUnitValueEqualityDuringPull` cover both units
with an unchanged null baseline, null-to-value and value-to-null transitions, different remote
values, and local removal with an unchanged remote value. Binary also covers a length change
with the same starting bytes. Equal baselines accept the refreshed record; actual changes report
a conflict and retain the old record/local edit until Reset followed by Pull. Null remains
distinct from a non-null value. Other unit types and relation comparisons keep their existing
rules. Binary comparison can scan the byte contents; DateTime comparison is constant-time.

## Push and permissions are separate

Default Push accepts the affected refresh as its new baseline. `ResetUnitAfterPushTest` and
`resetUnitAfterPush` therefore retain the saved value after Reset. Explicitly skipped refresh
acknowledges the save while retaining the old baseline and write restriction until an accepted read.
`ResetAfterPushWithoutRefreshUsesPriorBaseline` and TypeScript's
`Reset after skipped Push retains the accepted %s baseline` preserve that separate contract.

Workspace object masking is removed. Within the workspace's configured classes, the Pull and
Sync builders retain object identities, including targets of readable relations. A target's
unreadable roles do not remove it from a relation. Sync still checks `CanRead` for each role
before serializing its value; retaining an object identity does not grant access to its roles.
The .NET [PullSecurityTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullSecurityTests.cs)
`PullRetainsObjectsWithUnreadableNames`, `PullRetainsRelatedObjectsWithUnreadableNames`, and
`PullDoesNotTraverseUnreadableRoleIncludes` pin Pull selection and traversal.
`SyncRetainsRelationTargetsWhileFilteringUnreadableRoles` in
[SyncRolesTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Sync/SyncRolesTests.cs) pins Sync.
`ObjectsWithUnreadableNamesRemainInResultsAndRelations` in the shared
[workspace SecurityTests](../../dotnet/Core/Workspace/Tests/Tests/SecurityTests.cs) covers .NET
Local and both JSON HTTP adapters.
The TypeScript [security specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/security.spec.ts)
`retains objects and readable relations when a role is unreadable` and
`directly pulls an object with an unreadable role` pin retained identities and relations while
denied role values remain omitted.

Those direct Pull/Sync server tests and the legacy record decoder remain at the separate server
boundary. Normal Session authority follows the current connection membership/security closure,
independently of retained baselines; [live authority](#scoped-live-authority) names that owner.

## Scoped Session ownership

[Complete workspace reads](../workspace-reads.md#scoped-sessions) owns normal construction,
supported operations, results and host obligations. The following ownership applies to every
workspace; the characterization matrix above describes the retained baseline transitions.

| Owner | Scoped responsibility |
| --- | --- |
| Connection | Own the internal read assembler/cache and one operation gate. Hold admission through transport, read or mutation application and notifications. Bind captured metadata, revision, workspace, context and authenticated identity. |
| Declaration converter | Capture strict declarations before queueing, preserving arguments, `Instanceof` parameters and explicit empty results. Reject excluded call shapes without dispatch. |
| Completed-read adapter | Convert exact decoded records into the existing record/range forms, stage object factories and merge decisions, then check connection admission before applying state. Never choose a baseline by looking up the latest connection record. |
| Origin state | Hold direct immutable references to the accepted baseline and previous checkpoint record. Compare exact record identity for checkpoint changes, including changed security payloads at one data version. |
| Scoped Pull result | Eagerly own named memberships of live Session objects and copied input outcomes/scope/diagnostics. It needs no release and adds no required member to `IPullResult`. |
| Scoped Session | Keep held values/diffs inspectable after close; active operations and permission queries reject closed, inconsistent or retired state. |

The .NET owner is
[`ScopedConnection`](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ScopedConnection.cs).
[`Session`](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Session/Session.cs)
stages strategies, applies exact records and exposes disposal. Normal
[`DatabaseConnection`](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Database/DatabaseConnection.cs)
owns public admission, construction reservations and services; Local and both serializers use its
ready initialization. The
[`ReadToJsonVisitor`](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ReadToJsonVisitor.cs)
extends the legacy converter without changing its existing output behavior.

TypeScript uses
[`DatabaseConnection`](../../typescript/modules/libs/system/workspace/adapters-json/src/lib/database/database-connection.ts)
and [`Session`](../../typescript/modules/libs/system/workspace/adapters-json/src/lib/session/session.ts).
Direct Workspace/Session constructors use the connection's readiness and registration boundary.
The Strategy factory passes its supplied exact record into origin state; Sessions accept only
their own completed pool and prior baselines.

In .NET, successful construction calls services `OnInit` exactly once after registration. Factories transfer fresh services
to connection ownership. A construction reservation covers user factory/callback execution and
the final admission check, including throws, reentrant close and late returned services. Failed
captured objects remain unusable; ordinary construction failure leaves ready siblings usable.
Early Session disposal retains immutable state and service cleanup registration until connection
drain. Cleanup attempts all Session services before workspace services, even after failures.

`NormalSessionInitializationRunsOnceAfterRegistration`,
`NormalConstructionCallbacksCannotPublishAfterShutdown`,
`EarlySessionCloseRetainsOwnedServicesUntilConnectionDrain` and
`NormalConnectionCleanupContinuesAfterServiceDisposalFailure` own these .NET rules.
`NormalQueuedOperationsCannotTouchResourcesAfterClose` checks late queue continuations: draining
dispatched response handling does not promise to join every outer public operation task.

Internal completed data holds all records and security data directly during adaptation. Session
baseline and checkpoint references remain independent; a conflict keeps the old record and edits.
The connection retains historical cache lookup while live and clears it after dispatched work
drains. There is no public pin, eviction or completed-result lifetime API; see
[cache ownership](workspace-read-store.md).

Factories, conversion and tracker copies run before the final active-state check. State application
shares a lock with retirement in .NET and has no await or host callback; notifications follow it.
This prevents retirement during a factory callback from installing stale records. An unexpected
adaptation failure does not undo an already published read. .NET conservatively faults the Session;
TypeScript faults it when application may be incomplete, while a staging failure before any Session
application leaves the Session usable. For protected Push and Invoke, notification failure after complete application does not
fault the Session.

### Scoped live authority

Permission checks use current object membership and current security/permission bodies, separate
from the accepted data record. Revocations win. .NET checks the related indexes under one store
lock. Immutable baseline references preserve historical values, not authority.

Explicit unavailability clears current authority without losing edits. Query omission does not.
A complete new-ID mapping is validated before Push changes any object IDs or creation tracking.
A failed response after dispatch can mean the mutation committed; it cannot be replayed safely.
The public reference owns [mutation and retirement rules](../workspace-reads.md#scoped-sessions).

### Scoped save admission

The public [save and refresh contract](../workspace-reads.md#save-and-refresh-a-scoped-session)
owns ordinary scoped Push behavior. Common `ISession`, `PushOptions` and `IPushResult` expose it;
`IMutationResult` owns shared mutation evidence for both Push and [Invoke](#scoped-invoke-admission).

| Owner | Responsibility |
| --- | --- |
| Scoped Session | Count in-flight operations, acquire the saving guard before capture, validate every remap, acknowledge submitted states, and adapt usable changed records. Keep temporary busy state separate from reconciliation state. |
| Strategy and origin state | Guard public editing methods, including direct origin-state entrypoints. Internal acknowledgement marks submitted states as pushed without replacing their baseline or checkpoint. Inspection uses the active-state check independently. |
| Connection and transport | Admit one mutation under the connection gate and preserve raw response data for independent receipt/read decoding. Every transport capability is required before bootstrap. |
| Pending query | Own only the captured query, discovery and scope until its single explicit completion. Reacquire the connection gate, obtain actual held inventory and apply through the ordinary scoped Pull path. |

The .NET implementation is in [Session.PushRead](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Session/Session.PushRead.cs)
and [ScopedConnection.PushRead](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ScopedConnection.PushRead.cs).
TypeScript keeps these responsibilities in `Session` and `DatabaseConnection`, linked
above. Submitted states are acknowledged before merge staging; derivation-changed objects outside
the submitted set retain ordinary merge checks. Failed refresh cannot turn that acknowledgement
into a rollback. Independent read validation precedes local lifetime/application gates, so failed
acknowledgement does not erase valid remote read evidence. Protected result adapters own detached
raw diagnostics and mappings; convenience projections resolve only held objects. The retained legacy decoder serves direct protocol coverage only.

### Scoped Invoke admission

The public [Invoke contract](../workspace-reads.md#invoke-and-refresh-a-scoped-session) owns options,
outcomes and caller duties. [Session](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Session/Session.cs)
captures method identity/version and reuses the exclusive guard in `Session.PushRead`.
[ScopedConnection.InvokeRead](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote/Read/ScopedConnection.InvokeRead.cs)
owns one transport attempt, receipt classification and shared affected import. TypeScript keeps
these responsibilities in `Session` and `DatabaseConnection`.

The guard spans queueing, merge staging, factories and notifications: callback edits cannot invalidate
an earlier staged merge decision. No Invoke path captures Push changes, acknowledges trackers or
calls Reset/Checkpoint. `CanAcceptPulledRecord` therefore keeps ordinary whole-record merge rules;
a prior Push restriction clears only when a usable record is accepted. Remote read classification
precedes local lifetime/cancellation gates, without treating skipped refresh as failed acknowledgement.

| Contract | Authoritative .NET tests |
| --- | --- |
| Common result/options and capture | `ProtectedInvokePublicContractUsesSharedResultAndRefreshOnly`; `ProtectedInvokeOwnsMethodsOptionsAndRawDiagnostics` |
| Unsaved edits, creations, checkpoints and current permission | `ProtectedInvokeDefaultsToRefreshAndExplicitSkipPreservesEdits`; `ProtectedInvokeRefreshPreservesEditsAndCheckpointOrReportsConflict` |
| Editing, callback exclusion and queue cancellation | `ProtectedInvokePausesEditingAndSameSessionOperations`; `ProtectedInvokeQueuedCancellationReleasesPauseWithoutDispatch` |
| Independent receipt/read/lifetime evidence | `ProtectedInvokeClassifiesIndependentReceiptAndRefresh`; `ProtectedInvokeUntrustedReceiptRequiresReconciliation`; `ProtectedInvokePostDispatchLifecycleKeepsKnownOutcome` |
| Application versus notification failure | `ProtectedInvokeApplicationAndNotificationFailuresRemainDistinct` |
| Real SQL and HTTP | `ProtectedInvokeRealAtomicBatchReturnsOneOutcome`; `ProtectedInvokeRefreshAndSkipUseOneRequest`; both serializers' `ProtectedInvokeHttp*` cases |

The existing response-context and runtime method specs contain TypeScript's `protected Invoke` cases,
including `protected Invoke reports conflicting refresh without erasing the old baseline or unsaved edits`
and `protected Invoke refresh preserves unsaved edits creations and checkpoint ownership without acknowledging Push`.
They use the same distinction between committed outcome, remote refresh and local application.

### Authoritative scoped tests

The additive cases in the existing
[.NET Local PullTests](../../dotnet/Core/Workspace/Tests.Local/Tests/PullTests.cs) use controlled
transports for deterministic interleavings. The dedicated
[TypeScript response-context spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/database/security/response-context.spec.ts)
retains direct decoder coverage and checks the matching normal contracts.

| Contract | .NET test | TypeScript test |
| --- | --- | --- |
| Common Push surface, default refresh and explicit skip | `ProtectedPushPublicContractHasOneSaveOperation`; `ProtectedPushDefaultRefreshesAndExplicitSkipPreservesBaseline` | `ordinary Push defaults to affected refresh and Reset uses its accepted baseline` |
| Raw diagnostics and mappings remain owned after closure | `ProtectedPushDiagnosticsRemainInspectableAfterClosure` | `ordinary Push returns expected %s rejection diagnostics without ResultError`; `Push result diagnostics and mappings remain owned after closure` |
| Remote evidence survives local application failure | `ProtectedPushSeparatesRefreshReceiptFromApplicationFailure`; `ScopedPushReadKeepsCommitAcrossPostDispatchLifecycleChanges` | `committed Push preserves independent read evidence when acknowledgement cannot apply` |
| Untrusted receipt blocks replay; read failure permits unrelated work | `ProtectedPushUntrustedHeaderReturnsUnknown`; `ProtectedPushRefreshFailureAllowsUnrelatedPermittedMutation` | `refresh failure permits an unrelated explicit mutation without replay` and the retained unknown-outcome cases below |
| Ready admission, supported options and mappings before remap | `NormalPushReadinessAndResultsUseOneProtectedPath`; `ProtectedPushMappingFailuresPreserveAllLocalIdentities` | `normal Push accepts supported %s arguments without a legacy endpoint`; `normal Push rejects %s mappings before any local remap` |
| Exact baseline, whole-record conflict and Session-owned instantiation | `ScopedSessionOwnsBaselinesAndRejectsCrossSessionInstantiation` | `discovers then completes one protected read, owns its baseline, and forbids cross-Session cache instantiation` and `keeps conflicts and reports unavailable inputs without losing local edits` |
| Structured failures, factory admission and unsupported calls | `ScopedSessionReportsRawFailureAndRejectsUnsupportedCallsWithoutDispatch` | `returns structured immutable failures without instantiating diagnostic IDs` and `requires successful factory bootstrap for mutations and never retries transport failure` |
| Same-version readable payload and availability | `ScopedSessionCheckpointsSecurityOnlyChangesAndKeepsUnavailableEdits` | `observes readable-role changes at the same version and retains immutable checkpoints until owner cleanup` |
| Independent baseline/checkpoint retention | `ScopedSessionKeepsAcceptedAndPreviousCheckpointRecordsIndependently` | The same immutable-checkpoint case above |
| Captured declarations and excluded grammar | `ScopedSessionCapturesDeclarationsBeforeQueueingAndRejectsExistsParameters` | `captures declarations and dependencies before waiting and orders mutations through response application` and `rejects unsupported Exists parameters and custom extents before any transport` |
| Final admission after object-factory callbacks | `ScopedSessionRetirementOrCloseDuringFactoryPreventsApplication` | `stages factories before application and rejects reentrant retirement without importing objects` |
| Complete Push mappings and uncertain malformed responses | `ScopedSessionMalformedPushCannotPartiallyRemapOrReplay` | `keeps new-object tracking when a successful Push omits assigned ID mappings` |
| Rejected Push retains negative new-object access diagnostics | `ScopedSessionPushPreservesNegativeAccessErrorsAndAppliesWithoutTrackerCallbacks` | `preserves negative new-object access diagnostics and created tracking as structured results` |
| Close and retirement before shortcuts | `ScopedSessionCloseRejectsLocalOperationsAndPermissions` | `rejects new-object permission shortcuts after retirement while keeping values inspectable` |
| Memory workspace construction is rejected | `ScopedSessionLocalFactoryRejectsMemory`; `LocalConnectionRejectsMemoryBeforeCreatingWorkspace` | SQL capability is enforced by the remote endpoint |
| Immediate changed refresh and caller-owned checkpoints | `ScopedPushReadAppliesChangedBeforeDeferredFinalQuery`; `ScopedPushReadNewIdsPreserveObjectIdentityAndCheckpointOwnership` | `protected Push applies changed values before return and leaves checkpoints owned by the caller`; `protected Push maps creations in place and applies refreshed security while preserving their prior checkpoint` |
| Saving pause, direct origin-state methods and inspection | `ScopedPushReadPausesEditingAndOverlappingOperationsButKeepsInspection`; `ScopedPushReadEditingPauseCoversStrategyAndBothDirectOriginStateSurfaces` | `protected Push pauses editing and overlapping operations while inspection and another Session remain usable`; `protected Push guards every direct origin and Strategy editing entrypoint including Session-origin roles` |
| Independent malformed refresh and mutation certainty | `ScopedPushReadKeepsCommittedReceiptAndIndependentValidRead`; `ScopedPushReadSeparatesMutationCertaintyAndReconciliation` | `protected Push preserves independent outcomes when raw %s JSON has duplicate properties`; `protected Push retains write blocks after malformed refresh and requires reconciliation after unknown transport outcome` |
| Captured pending query, one-use lifetime and intervening edits | `ScopedPushReadPendingCompletionMergesInterveningEditsAndOwnsCapturedQuery`; `ScopedPushReadPendingReadRejectsInvalidLifetimeAndIsOneUse` | `protected Push completes its captured query only on demand and once with merge conflicts preserved`; `protected Push leaves no gate during the UI gap and advertises inventory obtained from another Session at completion` |
| Rejection before dispatch and pause release | `ScopedPushReadRejectsEarlierOperationAndRecoversAfterPredispatchFailure`; `ScopedPushReadRejectsUnsavedObjectArgumentsBeforeDispatch` | `protected Push rejects unsupported transports and earlier in-flight operations before dispatch`; `protected Push rejects a new object passed as a bundled query argument before transport` |
| Cancellation with a usable receipt | `ScopedPushReadAcknowledgesCompleteReceiptEvenAfterCancellation` | `protected Push rejects cancellation before dispatch but preserves a received committed acknowledgement after cancellation` |

SQL-backed scoped Session cases live in the existing
[`PullRetryTests`](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullRetryTests.cs),
both .NET HTTP PullTests classes
([System.Text.Json](../../dotnet/Core/Workspace/Tests.Remote.Json.SystemText/Tests/PullTests.cs),
[Newtonsoft.Json](../../dotnet/Core/Workspace/Tests.Remote.Json.Newtonsoft/Tests/PullTests.cs)), and
[TypeScript pull specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/pull.spec.ts).
They use normal ready construction. Run protected cases against both SQL providers and both
HTTP serializers.

The ordinary Local workspace Profile lazily creates a fresh SQL database object on first database
access or login, using the existing `core/commands` configuration and a runner-owned database.
Each test that accesses SQL initializes and populates its database under a maintenance lease,
then captures committed `LocalReadIdentity` for ready Local connections. Login drains every old
Profile-owned connection before publishing a replacement; independent workspace creation is awaited.

Local protocol, internal cache/assembly, controlled-transport and conversion cases do not initialize
SQL. The conversion methods live in Local PullTests to avoid inherited login. Local workspace tests
construct Memory explicitly only for rejection. `WorkspaceProtocolConstructorsRejectMemory` in
server PullSecurityTests covers the JSON API and its six direct builders; its SQL fixture means
those seven rows remain integration coverage. Memory database/domain and authentication scaffolding
remain separate from workspace integration.

### PushRead integration tests

The Local cases use protected SQL execution. Both HTTP classes forward requests to the running
Core test server; the TypeScript cases are in `scoped protected Push HTTP integration`. Response-loss and
damaged-part cases inject faults after receiving a real server response, so their committed writes
can be checked independently.

| Behavior | .NET cases | TypeScript HTTP cases |
| --- | --- | --- |
| Ordinary Push default refresh and explicit skip | `ProtectedPushRefreshAndSkipUseOneRequest` in Local server and both HTTP serializer PullTests. | `ordinary Push default and explicit skip preserve their baseline contract` |
| Immediate refresh and in-place new IDs | HTTP `ScopedPushReadHttpChangedOnlyUsesOneRequestAndPreservesNewObjectIdentity`; Local `ScopedPushReadLocalRefreshesMappedRelationsAndPreservesCheckpoint` also checks relations and checkpoint ownership. | `changed-only save uses one HTTP request and remaps a new object in place` |
| Two requests and exact refreshed-body reuse | HTTP `ScopedPushReadHttpDeferredQueryUsesTwoRequestsAndReusesRefreshedBody` | `bundled query completes on demand in a second HTTP request using exact refreshed record bodies` |
| Concurrent membership, data and permissions | HTTP `ScopedPushReadHttpDeferredQueryObservesConcurrentDataAndMembership` and `ScopedPushReadHttpDeferredQueryRefreshesConcurrentPermissionChanges`; Local `ScopedPushReadLocalFinalQueryRefreshesConcurrentSecurityAndMembership` and `ScopedPushReadLocalFinalQueryRefreshesSecurityAtTheSameObjectVersion`. | `final HTTP read observes concurrent membership and data while preserving a local merge conflict`; `final HTTP read refreshes concurrent %s permission changes` |
| Deletion and unavailable targets | HTTP `ScopedPushReadHttpDeferredQueryMarksDeletedObjectsUnavailable`; Local `ScopedPushReadLocalPendingQueryReportsConcurrentDeletion`. | `final HTTP read reports concurrent deletion as unavailable and removes query membership` |
| Independent refresh failure and unknown mutation | HTTP `ScopedPushReadHttpKeepsCommitWhenOneReadPartFails` and `ScopedPushReadHttpLostCommittedResponseRequiresReconciliationWithoutReplay`; Local `ScopedPushReadLocalKeepsCommitWhenCompletionCannotRefresh` covers a post-commit server failure. | `a failed %s part after real HTTP commit preserves the independent outcome`; `lost HTTP receipt after server commit leaves an unknown outcome without replay` |
| Retirement before application or completion | HTTP `ScopedPushReadHttpPendingCompletionRejectsRetiredConnection`; Local `ScopedPushReadLocalKeepsCommitWhenCompletionCannotRefresh`. | `pending HTTP completion rejects after %s without another request`; `retirement after server HTTP commitment retains the receipt and prevents local application` |

Set `ALLORS_TEST_DATABASE_PREFIX` for isolated Core server Local test databases. The
[fixture](../../dotnet/Core/Database/Server.Local.Tests/Json/Fixture.cs) prepends it to each test
class name before provisioning; use lowercase letters, digits and underscores, starting with a
letter, with at most 63 characters for the combined name. The harness owns cleanup of those
databases. Point each HTTP server at its own database through `ConnectionStrings__DefaultConnection`.
Both .NET HTTP profiles and the TypeScript fixture accept `ALLORS_TEST_URL`, including the trailing
`/allors/`, for example `http://localhost:4200/allors/`. When unset, they keep the existing
`http://localhost:5000/allors/` default. The server's `AllorsServerOptions.WorkspaceNameByHost` must
map the requested host and port to `Default`; both example endpoints already have that mapping.
Use distinct servers and databases for parallel provider runs.

### Scoped test owners

The former owner wrappers are replaced by the normal connection lifecycle. Test helpers retain
only transport-resource ownership; the [public lifetime rules](../workspace-reads.md#scoped-sessions)
apply to every caller.

| Harness | Ownership |
| --- | --- |
| .NET HTTP | Each Profile owns its root and independent ready connections with dedicated captured-credential clients. `CreateHttpTestResourcesAsync` creates explicitly separate test-owned resources. `HttpTestResources.DisposeAsync` drains the connection before disposing its client. |
| Local SQL | Profile remains lazy for service-free tests. It owns ready connections and borrows the harness database. Server.Local tests use the normal Local factory or the protected adapter test subclass with real Local transports. |
| TypeScript | Fixture owns captured transports and connections. `createExclusiveWorkspace` shares its root; awaited `createWorkspace` owns an independent connection. Every runtime spec awaits partial-safe teardown. |

Login drains managed connections before replacing identity, closes old Sessions and retains their
inspectable values/diffs. Explicitly separate test-owned connections retain their identity until
closed by their owner. Four TypeScript relation contexts retain three connections per case, with
six initial HTTP read requests. Operation request counts exclude these explicitly checked bootstraps.

| Contract | Authoritative tests |
| --- | --- |
| Ready bootstrap without application callbacks | `NormalHttpFactoryBootstrapsOnceWithoutApplicationServices`; `NormalLocalFactoryBootstrapsWithoutApplicationServices`; `normal factory captures configuration options and all transport functions before awaiting bootstrap` |
| All-workspace closure and borrowed resources | `NormalConnectionTracksAllWorkspaceSessionsAndIdempotentClosure`; `NormalLocalConnectionClosesSessionsAcrossAllWorkspaces`; `normal close and failed creation never dispose borrowed transports` |
| Profile identity replacement and failed creation | `NormalLocalProfileLoginDrainsAllOwnedConnections`; both HTTP codecs' `NormalProfileLoginDrainsBeforeReplacingCapturedIdentity` and `NormalProfileFailedReplacementPublishesNoWorkspace` |
| TypeScript identity replacement and committed Invoke drain | `normal Fixture identity replacement drains managed connections and keeps explicit independent ownership`; `normal close drains an actually committed %s Invoke receipt before releasing the owned HTTP transport` |

## Remaining decisions

- Retained direct server endpoints, shared DTOs and decoder tests have a separate removal boundary.
- Whole-role merge conflict semantics and merging independent to-many edits require a separate decision.
- Persistent caches, other consumers and signals remain separate work. The historical
  [connection plan](workspace-connection-session-plan.md) records its original scope.
