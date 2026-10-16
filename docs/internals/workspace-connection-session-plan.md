# Workspace connection and read protocol

> **Status: Historical.** All four steps, including cross-runtime and SQL-provider integration, are complete.
> Current behavior is in [Complete workspace reads](../workspace-reads.md); the maps below own implementation and tests.
> The later normal lifecycle/caller cutover is recorded in the [caller inventory](workspace-caller-migration-inventory.md).
> Server-endpoint removal, signals and other consumers remain separate work.

This page preserves the decisions and completed scope of the read and scoped PushRead work.
Applications use the current reference linked above. The later caller migration promotes ordinary
scoped Push to that protected path and adds shared mutation results. Protected atomic Invoke now
uses the same result and affected-read owners. Historical PushRead names below describe the
completed steps, not a retained public Session operation.

## Implemented

The complete reader returns selected records and their security information from one SQL
snapshot. Scoped Sessions retain the records they accepted while checking current permissions.
Normal ready factories now use these complete reads and protected mutations. The original
constructor coexistence described below is historical.

| Part | Current ownership and tests |
| --- | --- |
| Population identity, maintenance, protected SQL reads/writes, server PushRead execution | [Read foundations](workspace-read-foundations.md) |
| Messages, including PushRead, and strict decoding in both runtimes | [Read protocol](workspace-read-protocol.md) |
| Immutable records, current indexes, retention and release | [Read storage](workspace-read-store.md) |
| Request capture, cache reuse and complete result publication | [Read coordination](workspace-read-coordinator.md) |
| Editing baselines, merges, permissions and scoped lifetimes | [Session state](workspace-session-state.md) |

Those pages and their named tests hold the current implementation details. This plan does not
repeat them.

## Decisions retained

- Data and authorization must come from the same protected SQL snapshot. A matching object
  version alone cannot prove that permissions or inverse-relation membership are unchanged.
- The server derives population, user, workspace and representation from trusted host inputs.
  A request's expected scope only checks consistency; it cannot select an identity.
- A Session's editing baseline belongs to that Session. Later connection reads must not replace
  it silently. Current permission checks remain separate from historical values.
- A completed immutable result and a Session Pull result serve different purposes: the former
  holds exact records; the latter contains editable Session objects.
- Changing credentials, population or representation requires retiring the old connection and
  creating a replacement. Do not silently move local edits or retry mutations across that change.

## Agreed read design — 2026-10-13

The [current read reference](../workspace-reads.md#requests-and-results) owns the two-step query
flow and its diagram. Every coordinated query discovers, then repeats the captured query with
held inventory. The final response is authoritative; changed data needs no restart or historical
body retrieval. Step 1 implemented this flow in both clients without changing server messages.

## Push with immediate refresh

Steps 2 and 3 implement the server and scoped client API. The public reference owns
[server outcomes](../workspace-reads.md#push-with-immediate-server-refresh) and
[Session save and explicit query completion](../workspace-reads.md#save-and-refresh-a-scoped-session),
including the request-flow diagrams and failure rules. The implementation maps are
[read foundations](workspace-read-foundations.md#protected-pushread-write-and-refresh),
[protocol validation](workspace-read-protocol.md#pushread-messages),
[coordination](workspace-read-coordinator.md), and
[Session save admission](workspace-session-state.md#scoped-save-admission).

The approved design pauses editing only in the saving Session through acknowledgement and
application of the first response. This keeps save acknowledgement independent of concurrent
local edits without adding a new conflict-resolution API. Explicit pending completion lets the
UI use refreshed objects before the final query finishes. Its UI gap owns neither a connection
gate nor cache pins. At completion of this plan, common `ISession` interfaces and existing
constructors kept their contracts; the later caller migration changes ordinary scoped Push.

## Completed implementation steps

Steps 1–4 are complete. Step 4 was approved and validated on 2026-10-13. Its tests extend
controlled client coverage through .NET Local, both .NET HTTP serializers and TypeScript HTTP
on PostgreSQL and SQL Server.

| Step | Files and systems | Evidence |
| --- | --- | --- |
| 1. Mandatory two-step queries — complete | .NET and TypeScript read coordinators/scoped connections, their existing tests and read documentation | Cold, empty and fully cached reads all use two requests; changed membership, data and security between steps finish without restart; cancellation, scope changes and truthful pinned inventory remain correct. |
| 2. Server PushRead — complete | Protocol DTOs/shared fixtures and both serializers; Push/Read executors; SQL transaction tracking/admission; Core controller and Local transport | Reject before writing; capture derived creations/deletions and both relation ends across checkpoints; eligible unavailable IDs only; one post-commit snapshot; known commit survives read/cleanup failures; unknown commit never retries. Both SQL providers. |
| 3. Scoped client PushRead — complete | Both scoped Sessions/connections/transports and origin-state adaptation | One response applies full changed objects and ID mappings; editing pause covers every mutating entry point; independent outcomes; pending read starts only when requested; edits during final refresh use existing merge rules; checkpoint state survives. |
| 4. End-to-end coverage and current docs — complete | Existing .NET Local/HTTP and TypeScript HTTP suites; read/session public and internal pages | One round trip for changed-only saves; two for save plus query; exact reuse of newly returned bodies; concurrent changes, deletion, retirement and failures across real transports. |

The current [Session test map](workspace-session-state.md#authoritative-scoped-tests) names the
client cases and [read foundations](workspace-read-foundations.md) names the SQL/server cases.
[Read protocol](workspace-read-protocol.md) owns the shared wire fixtures. Existing scope,
security, closure, merge and legacy coverage is retained.

## Outside this plan

The completed [caller inventory](workspace-caller-migration-inventory.md) records prepared-declaration,
procedure, standalone result-value, named-route and Memory workspace removal, and the later
protected ordinary Push/shared-result, atomic Invoke and normal lifecycle/caller implementations.
Their approval and validation are recorded there, outside this original plan. Server-endpoint
removal, signals and other consumers still require separate approval. Memory remains a domain-test adapter, not a protected SQL
reader; keep its coverage separate from SQL integration coverage.

Signals remain the intended API in both runtimes, as recorded in [AGENTS.md](../../AGENTS.md).
Their design and other consumers are separate work. Every normal connection now uses protected
reads and mutations; direct legacy server endpoints remain at their separate removal boundary.
Broader mutation migration and exactly-once execution are outside this plan.

For runtime changes, retain merge, diff, Reset, security and unit-role coverage after
[procedure retirement](workspace-caller-migration-inventory.md#decided-remove-procedures); see the
[Session test map](workspace-session-state.md#authoritative-scoped-tests). Validate .NET Local,
both HTTP serializers, TypeScript HTTP and both SQL providers for protected reads. Run affected
build, typecheck, lint and formatting checks. Measure performance for demonstrated problems.

## Completed scope and boundary

Steps 1–3 are complete. The client tests cover immediate application, new-ID mapping, the editing
pause including direct origin-state methods, independent outcomes, reconciliation, cancellation,
retirement, caller-owned checkpoints, and one-use final completion with actual held inventory.
The new client operation is validated with controlled transports, including both .NET HTTP codecs.

Step 4 completed end-to-end scoped PushRead coverage in existing .NET Local/HTTP and TypeScript
HTTP suites on PostgreSQL and SQL Server. The
[Session integration test map](workspace-session-state.md#pushread-integration-tests) names the
added cases. Retained .NET Local and affected server tests, both complete .NET HTTP serializer
suites, and the complete TypeScript HTTP suite pass; affected builds, typechecks, lint and
formatting checks pass. No product behavior change was needed for this integration step.

This four-step plan ends with that validation and the current documentation. Completion does not
authorize Git publication or the later default switch. The caller inventory records the separately
approved lifecycle/default cutover. Signals, other consumers and broader mutation migration remain
separate work requiring their own approval.
