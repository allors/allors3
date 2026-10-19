# Archived spike: workspace connection and session layers

> **Status: Historical.** This branch is a frozen learning experiment of 8 and 9 October 2026.
> It is not meant to be merged. The current platform is on `main`.

## What this branch is

The branch keeps the 18 commits of `refactor/workspace-layers` as they stood on 9 October 2026,
above `main` at `5b3d93025c` (#278, a domain extends several domains). The experiment ends at
`f1d42c25bd`; the commits above it only describe the archive. The branch was archived as
`archive/pr279-workspace-layers` and renamed on 19 October 2026.

## Why it was set aside

The spike was a learning experiment. It explored how the .NET and the TypeScript workspace split
into a connection and a session, with one contract in ids between them, so that a second session
API, signals, could build on the same connection. The parts that were ready reached `main`
through pull requests of their own. The layering itself was not continued and stays here for
its reasoning.

## What was tried

- Split both workspaces into a connection library and a session library:
  `Allors.Workspace.Connection` and `Allors.Workspace.Session` in .NET,
  `@allors/system/workspace/connection` and `@allors/system/workspace/session` in TypeScript, in
  place of the five `Allors.Workspace.Adapters*` projects and `Allors.Workspace.Protocol.Json`. The
  connection is the full contract in ids, versions, meta types and role values, without objects;
  the session builds objects and change tracking on it. The query model moved into the connection,
  typed by `IIdentifiable`.
- Put the wire behind one `ITransport`: HTTP with System.Text.Json or Newtonsoft.Json, and an
  in-process `LocalTransport` over the server's `Api`, so that the local adapter stopped being a
  second implementation of pulls, pushes and permissions.
- Carry an envelope on every JSON request and response: database, user, workspace name and a meta
  fingerprint, computed on both sides with FNV-1a over the sorted tags of the workspace's
  composites, relation types and method types. Server and connection refuse a mismatch; a changed
  database or user faults the connection.
- Move the sync, access and permission flow after a pull into the connection, and request grants
  and revocations again when a pull advertises another version.
- Let the connections of one user share a thread-safe cache, later walked back to private state
  per connection, and add a persistence provider behind the connection's cache.
- Pin the connection's contract with tests on the three .NET transports, in the new project
  `dotnet/Core/Workspace/Tests.Connection`, and on the TypeScript test server.
- Describe the result: `docs/connection.md` for application developers and
  `docs/internals/workspace-layers.md` for maintainers, with the waves that never started here:
  a wire schema in `.proto`, gRPC, and the signals layer as a second session library.

## What reached main

| Commit here | Pull request on `main` |
| --- | --- |
| `7720780113` move Api into Allors.Database.Workspace.Json | #279, as it is |
| `49f1ec4540` run the local adapter in-process over the JSON Api | #280, redone against the layout of `main` |
| `bc4ecd6430` request grants and revocations again when a TypeScript pull advertises another version | #281, redone against the layout of `main` |

Everything else in the list above stayed on this branch. `main` keeps the adapters layout, and
#283 removes the Local adapter rather than turning it into a transport.

## Where to read

- `docs/connection.md`: the contract, the bootstrap, the records and permissions, the envelope.
- `docs/internals/workspace-layers.md`: the parts, why the cut is where it is, which parts existed.
- `CHANGELOG.md`, under Unreleased: the entries the spike wrote for itself.
- `git log --oneline 5b3d93025c..f1d42c25bd`: the 18 commits in order.

## Working with the archive

Check it out with `git switch -c spike archive/workspace-connection-session-layers` and build as
`main` was built at that time. Do not merge it, and do not rebase it onto `main`; cherry-pick what
is worth keeping into a branch of its own.
