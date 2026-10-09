# Workspace connection

> **Status: Current.**

The connection is the lowest layer of a workspace: the full contract between the workspace and
the database, reads and writes alike, in ids, versions, meta types and role values, without
objects. A session builds objects and change tracking on it; a transport underneath carries the
wire. The .NET and the TypeScript connection have the same contract; this page lists what an
application can rely on in both. [Workspace layers](internals/workspace-layers.md) maps the parts
and says what is still planned.

## Libraries

| Layer | .NET | TypeScript | Holds |
| --- | --- | --- | --- |
| Connection | `Allors.Workspace.Connection` | `@allors/system/workspace/connection` | `IDatabaseConnection` and `DatabaseConnection`, `ITransport`, the query model, the records, grants, revocations and permissions, the results in ids, the `IdGenerator` and the ranges. |
| Transports | `Allors.Workspace.Connection.Remote.SystemText`, `Allors.Workspace.Connection.Remote.Newtonsoft`, `Allors.Workspace.Connection.Local` | the application's own `ITransport` | How the requests travel: over HTTP with System.Text.Json or with Newtonsoft.Json, or in-process on an `IDatabase`. |
| Session | `Allors.Workspace.Session` | `@allors/system/workspace/session` | `Workspace`, `Session`, `Strategy` and the object factory: objects and change tracking on a connection. |
| Domain | `Allors.Workspace.Domain` | `@allors/system/workspace/domain` | The session API an application programs against: `IWorkspace`, `ISession`, `IObject` and `IStrategy`. The TypeScript library re-exports the query model of the connection. |

Only the connection and its transports see the protocol classes, `Allors.Protocol.Json` and
`@allors/system/common/protocol-json`.

## Bootstrap

A connection serves one user: the user its transport signs the requests as. On sign-out,
discard the connection and its workspace. Signing in as another user means a new transport,
a new connection and a new workspace.

```csharp
var transport = new HttpTransport(httpClient); // or new LocalTransport(database, userId, "Default")
var connection = new DatabaseConnection("Default", metaPopulation, transport, new DefaultStructRanges<long>());
var workspace = new Workspace(connection, objectFactory, rules, services);
```

```ts
const connection = new DatabaseConnection('Default', metaPopulation, transport);
const workspace = new Workspace(connection, objectFactory, rules);
```

The .NET constructor requires the ranges; the TypeScript one takes an optional `{ ranges }`
as its fourth argument and uses `DefaultNumberRanges` when it is absent. The workspace name
and the meta population are the connection's, the object factory and the rules the workspace's,
as is its id generator; `IWorkspace.Configuration` shows the first four.

## The contract

| .NET | TypeScript | Meaning |
| --- | --- | --- |
| `WorkspaceName` | `workspaceName` | The workspace the server serves; it decides which classes and roles the records carry. Every request names it. |
| `MetaPopulation` | `metaPopulation` | The generated workspace meta the records are typed by. |
| `MetaFingerprint` | `metaFingerprint` | The fingerprint of the meta population, see [The envelope](#the-envelope). Every request names it. |
| `Ranges` | `ranges` | The ranges that order the ids of a composites role. |
| `DatabaseId`, `UserId` | `databaseId`, `userId` | The database the server serves and the user it serves the connection as, from the first response; null until then. |
| `RecordChanged` | `recordChanged` | Raised after a pull for every record the pull replaced, once the records, grants and permissions of the pull are in. The TypeScript event is a `Subscribable`: `subscribe(listener)` answers a `Subscription` with `unsubscribe()`. |
| `GetRecord(id)` | `getRecord(id)` | The record of the object, or null when the connection has not received it. |
| `GetPermission(class, operandType, operation)` | `getPermission(cls, operandType, operation)` | The id of the permission, or 0 when the connection has not received it. |
| `PullAsync(pulls, procedure)` | `pull(pulls, { procedure, dependencies, context })` | Pulls by the query model, with a procedure the server runs before the pulls, and brings the records of every object in the answer up to date, see [After a pull](#after-a-pull). The TypeScript `dependencies` name the derived roles whose dependencies the server includes, and `context` is a tracing string the server appends to its events. |
| `PullAsync(name, args)` | | A named pull: a route of the server, `{name}/pull`, with the arguments it takes. The in-process transport throws `NotSupportedException`, since the name is a route. |
| `PushAsync(newObjects, changedObjects)` | `push(newObjects, changedObjects, { context })` | Pushes `PushNewObject(workspaceId, class, roles)` and `PushChangedObject(id, version, roles)`, null for none; a `RoleChange(roleType, value)` carries a unit, the id of a composite role or the ids of a composites role. The server refuses a changed object whose version is not the server's. |
| `InvokeAsync(invocations, options)` | `invoke(invocations, { isolated, continueOnError, context })` | Invokes `Invocation(id, version, methodType)`, every method in one transaction unless isolated, stopping at the first error unless told to continue. |

Deleting is no call of the contract: a session drops a new object itself, and an existing object
is deleted by a method of the domain, through invoke.

Every result says `HasErrors` and `ErrorMessage`, and lists `VersionErrors`, `AccessErrors` and
`MissingErrors` as ids and `DerivationErrors` with a message and the roles, empty when there are
none. A `PullResult` holds `Objects`, `Collections` and `Values` by the name of the pull's
result, case-insensitive in .NET and in upper case in TypeScript, and `Pool`, the ids of every
object the answer names. A `PushResult` holds `DatabaseIdByWorkspaceId` for the new objects. An
`InvokeResult` holds the errors only.

Make the calls of a connection one after the other: start a call after the task or promise of
the previous one has completed. Each connection keeps its own records, grants, revocations
and permissions in memory. Another connection's pulls do not change that state.

## Records and permissions

A record is a database object as the connection received it: one user's view of the object at
the version it had, immutable. The server leaves out the roles the user may not read and sends
the ids of the grants and revocations that apply to the user on the object.

| .NET | TypeScript | Meaning |
| --- | --- | --- |
| `Class`, `Id`, `Version` | `cls`, `id`, `version` | The object. |
| `GrantIds`, `RevocationIds` | `grantIds`, `revocationIds` | The grants and revocations that apply to the user on this object. |
| `GetRole(roleType)` | `getRole(roleType)` | A unit, the id of a composite role, or the sorted ids of a composites role as an `IRange`; null for a role the user may not read. |
| `IsPermitted(permission)` | `isPermitted(permission)` | Whether one of the record's grants holds the permission and none of its revocations denies it, as the connection holds them now. |

A `Grant` and a `Revocation` carry an id, a version and the ids of their permissions; a
`Permission` carries an id, a class, an operand type and an operation. A session answers
`CanRead`, `CanWrite` and `CanExecute` of an object from `GetPermission` and `IsPermitted`.

## After a pull

The answer of a pull names every object of the result with its version, grant ids and revocation
ids, and every grant and revocation with its version. The connection then:

1. Syncs the objects it lacks, or holds at another version, other grant ids or other revocation
   ids, with the server.
2. Requests the grants and revocations the records name that it lacks, or holds at another
   version than the answer advertises, and the permissions those name for the first time.
3. Raises `RecordChanged` for every record the pull replaced, and returns.

A pull whose answer carries errors skips all of this.

## Transports

`ITransport` has the six calls of the protocol, `PullAsync`, `SyncAsync`, `PushAsync`,
`InvokeAsync`, `AccessAsync` and `PermissionAsync`, the .NET named pull, and `ServerMessages`,
the messages a server sends on its own over a transport that keeps a stream open: null for a
request-response transport, and no transport sends one today. The .NET transport also carries
the unit conversion of its serializer.

| Transport | Constructor | Carries the requests |
| --- | --- | --- |
| `HttpTransport` in `Connection.Remote.SystemText` | `HttpTransport(HttpClient)` | As JSON through System.Text.Json, posted to `pull`, `sync`, `push`, `invoke`, `access` and `permission` under the client's base address, with five retries at growing intervals. |
| `HttpTransport` in `Connection.Remote.Newtonsoft` | `HttpTransport(HttpClient)` | The same, through Newtonsoft.Json. |
| `LocalTransport` in `Connection.Local` | `LocalTransport(IDatabase, userId, workspaceName)` | In-process: every call runs the server's `Api` on a transaction of the database, as the user, with the same requests and responses as over HTTP and without a wire. For a program on the server, or a test. |
| TypeScript | the application's class | The application implements `ITransport` with its HTTP client. The test fixture's `FetchTransport` in `adapters-json-tests`, over cross-fetch, is the example. |

## The envelope

Every request carries the client's workspace name and meta fingerprint, `_w` and `_f`; every
response carries the server's database id, user id, workspace name and meta fingerprint, `_db`,
`_u`, `_w` and `_f`.

- The server serves the workspace its host maps to. It refuses a request that names another
  workspace or another fingerprint: the response holds only an error message, which names both
  values and the remedy, to send the request to the host that serves that workspace, or to
  regenerate the client's workspace meta from the server's repository.
- The connection throws on such a response, on a response for another workspace name or
  fingerprint, and on a response without the envelope, before anything is stored.
- The first response sets `DatabaseId` and `UserId`. A later response from another database
  or as another user faults the connection: the call throws with the reason, every later call
  throws, and the connection clears its records, grants, revocations and permissions. The
  user signs in again with a new connection.

The fingerprint is FNV-1a, 64 bits, over the sorted tags of the composites, relation types and
method types of the workspace, as 16 lowercase hexadecimal digits: `MetaFingerprint.Compute` in
`Allors.Shared` and `metaFingerprint` in the TypeScript connection compute it from tags, the
server per workspace in `IMetaCache.GetWorkspaceFingerprint`, and a client from its generated
workspace meta in `IMetaPopulation.Fingerprint()` and `metaPopulationFingerprint`. Both sides
agree without an identity on either meta population.

## What the tests pin

| Rule | .NET | TypeScript |
| --- | --- | --- |
| The contract on the real transports: a pull by extent answers ids and leaves records, roles as values, permissions per record, a push answers database ids and refuses a stale version, an invoke runs the method, `RecordChanged` | `ConnectionTests` in `dotnet/Core/Workspace/Tests`, run by the three test projects | `connection.spec.ts` in `adapters-json-tests`, on the Core test server |
| Private records and access state: independent connections, version guards, permission lookup and events after access information is available | `ConnectionRecordTests` in `Tests.Connection`, and `ConnectionTests` on the three transports | `connection-records.spec.ts` in `adapters-tests`, and `connection.spec.ts` on the Core test server |
| A revocation denies the write, over every transport | `SecurityTests.WithRevocation` | `security.spec.ts` |
| The envelope: refusals, the first response, a fault | `EnvelopeTests` in `Tests.Connection` for the client, in `Server.Local.Tests` for the server, and `ConnectionTests` | `envelope.spec.ts`, and `connection.spec.ts` |
| The fingerprint, with reference values | `MetaFingerprintTests` in `Shared.Tests` | `meta-fingerprint.spec.ts` |
| A grant or a revocation at another version is requested again | `AccessVersionTests`, `ConnectionTests` and `SecurityTests.WithGrantChangedOnTheServer` | `access-version.spec.ts`, `connection.spec.ts` and `security.spec.ts` |
