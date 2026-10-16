# Workspace read foundations

> **Status: Current.**

This is the maintainer map for protected SQL reads, PushRead and InvokeRead. Application behavior and host duties are in
[Complete workspace reads](../workspace-reads.md) and
[Maintain a SQL population](../database-maintenance.md). Applications must not rely on this page.

## Adapter read scope

A protected read needs one committed view for object values, queries and security. The adapters
support it as follows:

| Adapter | Accepted isolation for protected reads |
| --- | --- |
| PostgreSQL/Npgsql | Default, Snapshot or RepeatableRead |
| SQL Server/SqlClient | Default or Snapshot |
| Memory | Unsupported; its transaction handles share uncommitted state for domain tests |

`ReadScopeAcrossCommit` in the [Npgsql](../../dotnet/System/Database/Adapters/Tests.Static/Static/Sql/Npgsql/LifeCycleTest.cs)
and [SqlClient](../../dotnet/System/Database/Adapters/Tests.Static/Static/Sql/SqlClient/LifeCycleTest.cs)
`LifeCycleTest` classes commits a writer between reader operations. Values, query membership,
inverse relations and versions must stay at the earlier snapshot. The shared helper uses separate
reader/writer caches. `TransactionHandlesShareUncommittedRolesQueriesAndInverseRelations` in the
[Memory tests](../../dotnet/System/Database/Adapters/Tests.Static/Static/Memory/LifeCycleTest.cs)
records its different behavior.

An inverse relation can change without advancing the target object's version. Traversal therefore
runs again even when a stored target record can be reused.

## SQL Server isolation policy

SQL Server rejects settings other than unset or Snapshot, including during XML import. Command
applications no longer accept the `-i` override. The
[SqlClient serialization tests](../../dotnet/System/Database/Adapters/Tests.Static/Static/Sql/SqlClient/ObsoleteSerializationTest.cs)
`LoadUsesSnapshotForBulkCopyAndClassInsertion`,
`DirectLoadKeepsCompletedBulkBatchWhenObjectCallbackFails`, and
`DatabaseLoadClearsCompletedBulkBatchWhenObjectCallbackFails` cover isolation and import cleanup.
Import still has batch boundaries; a failed direct import can have completed earlier batches.

## Population identity and maintenance

The [maintenance guide](../database-maintenance.md) owns commands and operational steps.

| Owner | Responsibility |
| --- | --- |
| [PopulationLifecycle](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/PopulationLifecycle.cs) | Enrollment, renewal and the enclosing maintenance workflow. Readiness requires explicit successful completion of every child phase. |
| [PopulationMaintenance](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/PopulationMaintenance.cs) and [PopulationOperation](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/PopulationOperation.cs) | Separate administrative completion from individual Init/Load phases. Disposal alone does not publish readiness. |
| Provider `PopulationMetadataStore` | Persist the schema-local `_population` row, validate it and the schema, and check the incarnation when publishing readiness. XML does not carry this row. |
| Provider `Init`, `Load` and public loaders | Enter or join maintenance before changes. Failed phases remain failed even when an inner SQL exception is caught. |
| [Population command](../../dotnet/Core/Database/Commands/Core/Population.cs), Load, Populate and Upgrade | Keep the whole administrative operation inside maintenance, through its final commits. |

`RequireReady()` is an administrative check. `OpenRead()` instead reads readiness and incarnation
inside the protected transaction. Neither supplies distributed maintenance locking or detects a
native restore automatically.

Authoritative tests are in the provider lifecycle and serialization classes:
`PopulationLifecycleEnrollsWithoutChangingCommittedObjects`,
`PopulationEnrollmentPreservesDataAndIsSharedAcrossInstances`,
`PopulationLifecycleInitRequiresExplicitCompletionAfterSetup`,
`PopulationLifecycleRelationFailureCannotComplete`,
`FailedRelationSqlCannotCompletePopulationMaintenance`, and
`PopulationAbandonedOwnerBlocksRenewalUntilPendingPhasesEnd`.
[RepeatedSetupTests](../../dotnet/Core/Database/Server.Local.Tests/RepeatedSetupTests.cs) covers the commands.

## Protected read scope and trusted inputs

| Owner | Responsibility |
| --- | --- |
| Provider `OpenRead()` | Open the physical snapshot and read ready population metadata before initializing transaction services. Require a supported isolation setting and built-in connection factory. |
| [ReadScope](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/ReadScope.cs) and [ReadGuard](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/ReadGuard.cs) | Own the snapshot and cleanup. Reject writes, transaction restarts, raw commands and replaced services; catching a violation cannot make the attempt valid again. |
| SQL Transaction, Strategy, State and extents | Keep reads on that guarded connection with a private cache. Reject foreign inputs and access after closure; copy mutable binary values. |
| [ReadScopeFactory](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadScopeFactory.cs) | Use copied host configuration, resolve a committed user in this transaction, and compare the derived scope with the request's expectation. |
| [LocalReadIdentity](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/LocalReadIdentity.cs) | Bind a trusted Local user ID to its population before resolving it. |
| [ReadContext](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadContext.cs) | Own the user and fresh workspace ACL. Check cancellation and unchanged identity, then close successfully before results can be returned. |
| [ReadUserIdentity](../../dotnet/Core/Database/Server/Core/Services/User/ReadUserIdentity.cs) | Copy authenticated claims before awaiting the HTTP body and resolve them using the supplied transaction. |

The reader never accepts an ambient editing transaction. Population metadata establishes the
snapshot before any domain read or service callback. Cleanup closes access, disposes services and
attempts native rollback; cleanup failure prevents success. Custom services must respect this
boundary: the guard cannot prevent them from opening unrelated connections or external side effects.

The provider `ProtectedRead*` tests cover snapshot timing, rejected mutations, caught violations,
foreign inputs and cleanup. The `ProtectedRead*` cases in
[PullSecurityTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullSecurityTests.cs)
cover configuration, identity, expected scope, cancellation and untouched ambient edits.
`ReadIdentity*` tests in Core, Identity and Entra resolver suites cover captured claims and read-only
resolution. `ReadRouteOpensItsProtectedSnapshotOnlyAfterCommittedBearerAdmission` in
[EntraOptionsTests](../../dotnet/Entra/Database/Server.Local.Tests/Hosting/EntraOptionsTests.cs)
checks that admission commits before read resolution on both SQL providers.

## Protected PushRead write and refresh

The [application reference](../workspace-reads.md#push-with-immediate-server-refresh) owns the
supported contract and its request-flow diagram.

| Owner | Responsibility |
| --- | --- |
| Provider `OpenWrite()` and [WriteScope](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/WriteScope.cs) | Lock population metadata through commit, validate readiness/incarnation before services initialize, and reject writes until admission. The scope owns the only commit and rollback paths. |
| SQL [ChangeLog](../../dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Changes/ChangeLog.cs) | Capture affected IDs, classes and change kinds as changes occur, independently of checkpoint resets. The scope adds submitted no-ops. Nothing is persisted as a change feed. |
| Provider commit callback | Record physical commit success before connection cleanup. An exception while commit is unacknowledged leaves the outcome unknown; later failures retain known success. |
| [PushResponseBuilder](../../dotnet/System/Database/Allors.Database.Workspace.Json/Push/PushResponseBuilder.cs) | Apply and derive changes without committing for PushRead. Its legacy `Build` wrapper keeps the existing Push commit behavior. |
| [PushReadExecutor](../../dotnet/System/Database/Allors.Database.Workspace.Json/PushRead/PushReadExecutor.cs) | Validate trusted identity/scope and query before admitting writes, own the receipt, then build requested parts in one fresh ReadContext. Shared read failure discards both payloads, never the mutation outcome. |
| [ReadResponseBuilder](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadResponseBuilder.cs) | Project captured eligible identities into full affected records and unavailable IDs, reusing existing role/security/permission construction without query outputs or cache references. |
| [PushReadController](../../dotnet/Core/Database/Server/Core/Api/Json/PushReadController.cs) and [Local PushReadTransport](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Local/PushReadTransport.cs) | Capture inputs, enforce the strict raw boundary and execute once. They do not apply Session state or retry mutations. |

Provider `ProtectedWrite*` cases in the lifecycle tests cover checkpoint-independent capture,
maintenance exclusion through commit/disposal, rejected lifecycle escapes, and known/unknown commit
outcomes. Native fence tests use competing metadata updates; normal population maintenance still
requires stopping every host and writer as described in the maintenance guide.

The `PushRead*` cases in [PushExistingObjectTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Push/PushExistingObjectTests.cs)
and [PushNewObjectTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Push/PushNewObjectTests.cs)
cover scope/query rejection before writes, no-ops, derived creation/deletion across checkpoints,
inverse endpoints, security projection, limits, one shared snapshot, independent failures and
commit preservation across cancellation, tracing and cleanup. Every executor response is decoded
with both .NET serializers. The remote
[DefaultDenyTests](../../dotnet/Core/Database/Server.Remote.Tests/Tests/Api/DefaultDenyTests.cs)
cover authentication, raw decoding, body bounds and a successful combined response over HTTP.

## Protected atomic Invoke

The [application reference](../workspace-reads.md#invoke-and-refresh-a-scoped-session) owns the
supported contract and request flow. Invoke reuses the write fence and affected-read owners above.

| Owner | Responsibility |
| --- | --- |
| [InvokeResponseBuilder](../../dotnet/System/Database/Allors.Database.Workspace.Json/Invoke/InvokeResponseBuilder.cs) | Prepare the entire ordered batch against original versions and exact method workspace membership; execute with a fresh ACL before each method and derive after each success. It owns no transaction boundary. |
| [InvokeReadExecutor](../../dotnet/System/Database/Allors.Database.Workspace.Json/InvokeRead/InvokeReadExecutor.cs) | Resolve trusted committed identity, admit exact scope, include method targets in cumulative affected tracking and own the single commit or rollback. Classify the receipt from observed commit state, then close the write before fresh affected refresh. |
| [InvokeReadController](../../dotnet/Core/Database/Server/Core/Api/Json/InvokeReadController.cs) and [Local transport](../../dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Local/InvokeReadTransport.cs) | Capture host identity/workspace and strict raw input before execution. Enforce the same bounds and one-attempt transport boundary. |
| Legacy [Api](../../dotnet/System/Database/Allors.Database.Workspace.Json/Api.cs) | Temporarily own one ordinary batch commit or rollback using the same preparation/application builder; supply no protected transaction guard, receipt or affected refresh. |

A fresh per-method ACL is required because an earlier method or derivation can change execute
permission and ACL objects cache membership. The write guard makes user changes and caught direct
Commit/Rollback attempts invalidate the batch. These are transaction-local guarantees; custom
methods still own any external side effects they initiate.

[InvokeTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Invoke/InvokeTests.cs) names the
rules: `InvokeReadPreflightsEveryInvocationBeforeEffects`,
`InvokeReadRollsBackTheWholeBatchAfterLaterFailure`, `InvokeReadForbidsMethodTransactionBoundaries`,
`InvokeReadRejectsAnyAdmittedUserChange`, `InvokeReadRefreshesTargetsAndCumulativeAffectedObjects`
and `InvokeReadPreservesCommitClassificationAcrossFailures`. Its controller, Local transport and
population-fence cases cover the same entrypoints. `InvokeReadApiReturnsCommittedReceiptAndAffectedObjects`
in [DefaultDenyTests](../../dotnet/Core/Database/Server.Remote.Tests/Tests/Api/DefaultDenyTests.cs)
checks the actual HTTP route; Identity antiforgery and Entra committed-bearer cases cover plug-in admission.

## Security entries follow the supplied transaction

[Security](../../dotnet/Core/Database/Configuration/Core/Database/Security/Security.cs) reads token membership and
recomputes grant and revocation entries from the supplied transaction. Shared derived caches could
previously pair an unchanged committed version with uncommitted contents and retain them after
rollback. Immutable permission metadata and prefetch policies remain shared.

The `Workspace*FollowTransactionAfterRollback` regressions in
[SecurityAccessControlTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Security/SecurityAccessControlTests.cs)
compare the reused Security instance with a fresh instance after rollback.

## Committed provenance and ACL membership

An object's data version, applicable grant/revocation IDs and security-body versions can change
independently. Recompute the complete applicable set for every read, including delegated membership
and user/workspace filtering. A fresh ACL is needed because it memoizes membership within an attempt.

The `Committed*` tests in [PullSecurityTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullSecurityTests.cs)
cover permission changes, group membership, delegated membership and equivalent role replacement.
`PullSyncAndAccessRetainSnapshotAfterAnotherTransactionCommits` in
[SyncRolesTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Sync/SyncRolesTests.cs)
checks shared caches and Security across two committed snapshots.

## Complete results and transport entrypoints

| Owner | Responsibility |
| --- | --- |
| [ReadCompiler](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadCompiler.cs) and ReadGrammar | Capture inline declarations once, validate grammar, metadata and arguments, and compile built-in declarations before resolving objects. |
| [ReadExecutor](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadExecutor.cs) | Decode, open one protected attempt, build and validate the response, complete cleanup, then return it. Mask private host errors. |
| [ReadResponseBuilder](../../dotnet/System/Database/Allors.Database.Workspace.Json/Read/ReadResponseBuilder.cs) | Evaluate selections/includes/dependencies before deciding reuse. Include all required security and permission data, preserve empty outputs, and reject exceeded limits without truncation. |
| [ReadController](../../dotnet/Core/Database/Server/Core/Api/Json/ReadController.cs) and AddAllorsRead | Require explicit registration and authentication, capture trusted identity/workspace, and call the shared executor. |
| Local and HTTP ReadTransport implementations | Capture requests, strictly decode responses and make one attempt. They own no store or Session state. |

`CompleteReadRejectsInvalidInlineDeclarations`, `ReadTransportRejectsCyclicInlineDeclarations`,
`ReadTransportOwnsInlineExtentSelectIncludeAndArgumentDefinitions`,
`CompleteReadRejectsUnadmittedInlineExecution` and the other `CompleteRead*` tests in
[PullExtentTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullExtentTests.cs) and
[PullSecurityTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullSecurityTests.cs)
cover inline grammar, captured declarations and arguments, graph traversal, cache reuse, snapshot consistency and failure
handling. [PullRetryTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullRetryTests.cs)
`ScopedSessionPreservesInlineArgumentsAndEmptyResults` compares Local and both serializers.
The shared read and PushRead validation fixtures reject removed reference keys, including nulls,
in both serializers and TypeScript. The remote `DefaultDenyTests` and Identity/Entra hosting tests
check route admission. TypeScript protocol and HTTP Pull specs cover its transport.

Cancellation is checked around synchronous SQL work; it cannot cancel an already running SQL
command. Client result assembly belongs to the [coordinator](workspace-read-coordinator.md), retained
values to the [store](workspace-read-store.md), and editing to [Session state](workspace-session-state.md).
