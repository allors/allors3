# Archived spike: protected workspace reads

> **Status: Historical.** This branch is a frozen learning experiment, snapshotted on
> 16 October 2026. It is not meant to be merged. The current platform is on `main`.

## What this branch is

One commit, `99e2bde764`, above `main` at `d034e53d70` (#282). It squashes the 16 commits of
`refactor/workspace-pull-refresh`, from `0118ea819e` to `dd34ff56f2`, together with the 320
uncommitted paths of that checkout, into one snapshot; the commits above it only describe the
archive. The branch was archived as `chore/archive-workspace-pull-refresh` and renamed on
19 October 2026. The source branch was afterwards rebuilt from `main` as the Local adapter
removal of #283, so the experiment's history survives only here;
[prior-commits.txt](docs/internals/workspace-experiment-archive-2026-10-16/prior-commits.txt)
lists the original commits.

## Why it was set aside

The [archive README](docs/internals/workspace-experiment-archive-2026-10-16/README.md) records
the decision: the snapshot was "preserved for reference before starting a narrower Local-adapter
removal from `main`". The spike had grown into a replacement of the workspace's read path on both
runtimes and both SQL providers, with a validation record of selected runs but no full provider
matrix claimed green and expectation failures still pending. Its last proposal, a
test-consolidation map, was never implemented. The narrower change became #283.

## What was tried

- Complete reads in two steps: discover, then complete the original query with the held cache
  inventory, so that one PostgreSQL or SQL Server snapshot answers the selected records together
  with their security and permissions. Memory was rejected for workspaces.
- New protocol messages `Read`, `PushRead` and `InvokeRead`, strictly validated in .NET and
  TypeScript, with server controllers and a host registration `AddAllorsRead` that names a
  representation revision per workspace.
- Protected SQL read and write scopes in the adapters, with population identity and lifecycle
  metadata for both providers, snapshot isolation enforced for SQL Server, and security entries
  recomputed per transaction.
- On the client, a read coordinator and a read store under the connection, scoped sessions with
  immutable editing baselines and checkpoints, protected push and atomic invoke with
  affected-object refresh, and connection-owned caches without public pin or lifetime APIs.
- Removal of object masking, prepared declarations, procedures, named routes and Memory-backed
  workspaces; PostgreSQL provisioned in CI for Entra's read-admission tests; and rule additions to
  `AGENTS.md` on code style and diagrams.
- Six pages of documentation and an evidence directory with the handoff, the prior commits and
  the validation totals.

## Where to read

- `docs/workspace-reads.md`: the application reference the spike wrote.
- `docs/internals/workspace-read-foundations.md`, `workspace-read-protocol.md`,
  `workspace-read-store.md` and `workspace-read-coordinator.md`: maintainer maps of code and tests.
- `docs/internals/workspace-connection-session-plan.md`: the decisions retained.
- `docs/internals/workspace-caller-migration-inventory.md`: the implementation history, step by
  step.
- `docs/internals/workspace-experiment-archive-2026-10-16/`: the archive README, the handoff
  before archiving, the prior commits and the validation records.
- `CHANGELOG.md`, under Unreleased: the entries the spike wrote for itself.

## Working with the archive

Check it out with `git switch -c spike archive/workspace-protected-reads`. Regenerate outputs
through the normal build; the snapshot keeps no binaries. Do not merge it, and do not use it as
the implementation map for `main`, as the archive README explains.
