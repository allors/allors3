# Workspace session state

> **Status: Current.**

This page maps the current editing baseline in the .NET and TypeScript workspaces. It records
baseline and merge behavior ahead of the connection/session split; applications must not rely on this
internals page. The tests below are authoritative. The platform boundary is in
[ARCHITECTURE.md](../../ARCHITECTURE.md); domain terminology is in [Domains](../domains.md).

## Ownership

| Owner | State and responsibility |
| --- | --- |
| `DatabaseConnection` | Privately held current database records and permission maps, refreshed by Pull/Sync/Access. Callers must use a connection sequentially. |
| `DatabaseOriginState` for a session object | Retains the last accepted database record and its version. `OnPulled` obtains the connection's current record, checks `CanMerge`, and replaces the session record only when accepted. A merge error leaves this object's entire record in place. |
| `RecordBasedOriginState` | Holds local role changes separately. Reads prefer the local value; otherwise they use the retained record. `Diff` compares local changes with that record. `Reset` clears local changes without fetching or accepting a newer record. |
| Remote `DatabaseOriginState` | Builds Push using the retained version. To-many additions/removals are calculated against the retained record. The .NET Local adapter uses the same workspace implementation as HTTP. |

The tests characterize an editing baseline per object with relation targets loaded. They do not
establish snapshot consistency across an object graph.

## Characterization matrix

The tests use two sessions of one workspace/connection. A remote session changes and pushes an
object, then pulls it to refresh the shared connection. The editing session has local changes
to a string, a many-to-one role, and a many-to-many role. All relation targets are loaded.

| Case | Observed behavior | Authoritative tests |
| --- | --- | --- |
| Only the remote session pulls | The editing session keeps its version, untouched integer value, local values, and all three diff originals. Its Push reports a version error (.NET returns a result; TypeScript throws `ResultError` with that result). Reset returns to the old baseline; a subsequent Pull accepts the remote record. | `DatabaseDiffRetainsBaselineWhenAnotherSessionRefreshes` / `databaseDiffRetainsBaselineWhenAnotherSessionRefreshes` |
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

A successful Push marks the session object's database state as pushed. It does not accept a new
baseline. Its next Pull clears pushed local changes and accepts the connection record without
the ordinary merge check. The existing `ResetUnitAfterPushTest` and `resetUnitAfterPush` tests
remain unchanged: Reset after Push, without Pull, exposes the last pulled value.

A retained data record does **not** pin authorization. The record retains grant/revocation IDs,
but `DatabaseRecord.IsPermitted` / `isPermitted` resolves those IDs through live connection maps.
Refreshing those maps can affect permission checks on an object whose data baseline is older.
The `RefreshesKnownGrantWhenAnObjectIsSynchronized` and
`RefreshesKnownRevocationWhenAnObjectIsSynchronized` tests, and TypeScript
[security specs](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/security.spec.ts),
pin the existing synchronized-object refresh behavior. They do not guarantee permission
freshness on a Pull that needs no Sync, or when the retained record's grant/revocation IDs change
on the server. Data-baseline retention and authorization freshness require separate policies.

## Open decisions

- Decide whether failed Push should keep returning an error result in .NET and throwing
  `ResultError` in TypeScript. The baseline tests now pin both surfaces; aligning them would be
  an API behavior change for callers and needs separate approval.
- Decide whether to preserve the current baseline transitions in the connection/session split,
  including Reset after Push and whole-role conflict detection. The matrix is evidence for that
  decision, not approval to change semantics or to combine to-many membership edits.
- Define authorization freshness independently of data baselines, including changes to grant
  membership and Pulls without Sync. Retaining records alone cannot provide an authorization
  snapshot or a general freshness guarantee.

The connection/session split, Push error-contract changes, protocol envelopes, shared or
persistent caching, and signals remain separate work.
