# Workspace layers

> **Status: Planned.** The connection and the session exist in both workspaces;
> [Which parts exist](#which-parts-exist) says what is still to come.

The .NET and the TypeScript workspace are layers with one contract between them, so that a second
session API, signals, builds on the same way of getting records from the database into the
workspace. [Workspace connection](../connection.md) holds the contract, for users; this page maps
the parts that carry it, says why the cut is where it is, and which parts exist. Depth stays in
the code comments and the tests.

## The parts

| Part | .NET | TypeScript | Owns | Pinned by |
| --- | --- | --- | --- | --- |
| Protocol | `Allors.Protocol.Json`, with `.SystemText` and `.Newtonsoft` for the unit conversion of each serializer, `dotnet/System/Protocol` | `@allors/system/common/protocol-json` | The request and response classes of the wire, the envelope fields included, and nothing of the workspace. Only the connection and its transports see them: `Allors.Workspace.Session` and `Allors.Workspace.Domain` disable transitive project references, and the TypeScript protocol library imports nothing from the domain library. | `EnvelopeTests` in `Server.Local.Tests`, for the server's side of the envelope |
| Server | `Api` in `Allors.Database.Workspace.Json`, `dotnet/System/Database`, namespace `Allors.Database.Protocol.Json` | | The one entry point of the protocol: the six calls on a transaction, as the signed-in user, for the workspace the host maps to. It fills the envelope of every response and refuses a request for another workspace name or meta fingerprint. Core's controllers and the in-process transport call it; it depends on System only, through `IObjectBuilderService`, `IDerivationService` and `IMetaPopulation`. `IMetaCache.GetWorkspaceFingerprint` computes the fingerprint per workspace. | `Server.Local.Tests` and the server's remote tests, `MetaFingerprintTests` in `Shared.Tests` for the hash |
| Transport | `ITransport`: `HttpTransport` in `Connection.Remote.SystemText` and in `Connection.Remote.Newtonsoft`, `LocalTransport` in `Connection.Local` | `ITransport`, implemented by the application | How the requests travel: over HTTP with one of the two serializers, or in-process on an `IDatabase` through `Api`, as one user, without a wire. A transport carries the unit conversion of its serializer and the slot for the messages a server sends on its own. | `ConnectionTests` in `dotnet/Core/Workspace/Tests`, run by the three test projects, one per transport; `connection.spec.ts` in `adapters-json-tests` on the Core test server, over the fixture's `FetchTransport` |
| Connection | `Allors.Workspace.Connection`, `dotnet/System/Workspace` | `@allors/system/workspace/connection` | The contract: the query model, in namespace `Allors.Workspace.Data` as before, typed by `IIdentifiable`; `DatabaseConnection`, which runs the sync, access and permission flow after a pull, checks the envelope of every response and raises `RecordChanged`; the records store, `ICache` with `MemoryCache`, and the immutable `Record`; the translation to the wire, `ToJsonVisitor` and `PushEncoder`; the results in ids; `IPersistenceProvider`; the meta fingerprint; the `IdGenerator`; the ranges. | `Tests.Connection` in `dotnet/Core/Workspace`, over a transport that answers in memory; `adapters-tests`, over a fake server in memory; and the contract tests above on the real transports |
| Session | `Allors.Workspace.Session` | `@allors/system/workspace/session` | Objects and change tracking on a connection: `Workspace`, `Session`, `Strategy`, the database origin state over the records and the session origin state, the change set, the trackers, the results in objects, and the object factory. A workspace owns its object factory, its rules and its id generator instance. | The shared workspace tests in `dotnet/Core/Workspace/Tests`, run by the three test projects; `adapters-json-tests` on the Core test server |
| Domain | `Allors.Workspace.Domain` | `@allors/system/workspace/domain` | The session API an application programs against: `IWorkspace`, `ISession`, `IObject`, which extends `IIdentifiable`, `IStrategy` and `IConfiguration`, with `Node.Resolve` and `IPropertyType.Get(IStrategy)` as extension methods. It references the connection; the TypeScript library re-exports the query model, so that an import of `Pull` from the domain library keeps compiling. The template `workspace.meta.ts.stg` and the meta-json builders import the query types from the connection. | The generated workspace code of the test domains compiles against it |

The layering is protocol, connection, domain, session: each part references the parts below
it and nothing above. The contract in ids is the one place where the server's view of an object
becomes the workspace's; a layer above the connection never sees a request or a response.

## Why the cut is where it is

- **One implementation of the semantics; only the transport is swapped.** The local adapter
  had a second implementation of pulls, pushes and permissions, and it had drifted: it ignored
  revocations and answered records in its own order. It is an in-process transport over the
  server's `Api` now, with the same requests, the same responses and the same unit conversion as
  over HTTP, which fixed the revocations by construction and made the record order the server's.
  The one condition is that the serializer's unit conversion accepts its own output, which the
  System.Text.Json one did not for dates, booleans, doubles and integers.
- **The contract is object-free and carries the whole query model.** Extents are how a
  workspace reads, so they belong to the lowest layer, and their object parameters are typed by
  an identity interface with only an id. The connection answers ids and keeps records; a session
  instantiates. So two session APIs, the mutable one of today and the signals one, and a third
  party's, build on one connection.
- **A connection serves one user, and so does its cache.** A record is one user's view of an
  object: the server leaves out the roles the user may not read and sends the ids of the grants
  and revocations that apply to the user. Sharing a cache between the connections of one user is
  therefore safe and sharing it across users is not. The cache checks the workspace name and the
  meta population when a connection takes it, and the user when the first response binds the
  cache's key.
- **The envelope gives the cache its identity.** Neither meta population had one, so the
  fingerprint is computed on both sides from the sorted tags of the composites, relation types
  and method types of the workspace, with FNV-1a so that TypeScript computes it without a
  dependency. The server refuses a request for another name or fingerprint, and the connection
  refuses such a response before storing anything: a client built for another workspace or
  another version of the domain never fills a cache. A changed database or user faults the
  connection, because what it holds belongs to another key.
- **The sync flow lives in the connection**, so that every layer above receives records the same
  way, and the connection validates the versions of the grants and revocations a pull advertises,
  which the sessions did not. `RecordChanged` is the seam for a layer that watches records: the
  cache raises it at the moment a record is replaced, for every connection on the cache; the
  connection raises its own once the grants and permissions of its pull are in.
- **Persistence stores the wire shape**, so that restoring replays the sync code path instead of
  a second deserializer, and an entry is accepted only at the version, grant ids and revocation
  ids the pull advertises, so that a stale store never shows stale data.
- **One thread at a time per connection**, documented instead of locked; the shared cache is
  the thread-safe part.

## Which parts exist

| Part | Status |
| --- | --- |
| The connection and the session libraries, .NET and TypeScript, with the contract tests on the three .NET transports and on the TypeScript test server | Exists |
| The local adapter as an in-process transport over `Api`; `Allors.Workspace.Protocol.Direct` and the local executors are gone | Exists |
| The envelope and the meta fingerprint; the server serves the workspace its host maps to | Exists. Selecting the workspace by the request's name, with an allow-list per host, comes with gRPC |
| The per-user cache, shared between the connections of one user, with the version guard and the `RemoveRecord` and `Clear` hooks | Exists. Eviction policies come with the providers |
| `IPersistenceProvider` behind the cache | Exists. The platform's providers, on a file or SQLite for .NET and on IndexedDB for TypeScript, each a test profile, are a wave of their own |
| `RecordChanged` on the cache and on the connection | Exists; nothing listens yet. The signals layer is the first listener |
| The `ServerMessages` slot of a transport | Exists, null for HTTP. Server push by server streaming comes with the signals wave |
| The wire schema | Planned, the next wave: one `.proto` generates the C# and the TypeScript types, and JSON over HTTP is derived from it, so that the System.Text.Json and Newtonsoft transports and test projects collapse into one |
| gRPC | Planned, after the wire schema: a .NET client, gRPC-Web for TypeScript and Blazor, the server service, test profiles and CI jobs |
| The signals layer | Planned: a second session library on the same connection, see [ARCHITECTURE.md](../../ARCHITECTURE.md#reactive-workspace-direction) |

The test project and CI job names of the workspace tests are unchanged by the layering, and the
project `dotnet/Core/Workspace/Tests.Connection` is new, with the target
`DotnetCoreWorkspaceConnectionTest` and the CI job `CiDotnetCoreWorkspaceConnectionTest` in the
`memory` job. The TypeScript test projects keep their names, `adapters-tests` for the unit tests
over the fake server and `adapters-json-tests` for the suites on the Core test server.
