---
title: "The workspace"
description: "How a client works with the objects of the database through a workspace: connections, workspaces and sessions, the Local and Remote adapters, what a pull brings into a session, what the user may read and write there, and what a push stores."
audience: "Users"
kind: "Explanation"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

A workspace is how code outside the database works with its objects: a user interface, a client
of a server, a test. It works for one user. It brings the objects that the user's pulls find into
a session, keeps the user's changes there, and sends them to the database, which checks them,
derives and commits. Code in the database works in a transaction instead, as
[Objects and relations](objects-and-relations.md) describes.

This page explains the .NET workspace. [Use a workspace](use-a-workspace.md) gives the steps,
[Pulls](pulls.md) lists what a pull can ask for, and
[The TypeScript workspace](typescript-workspace.md) describes the TypeScript libraries. The
examples use the [sample application](create-an-application.md), in which a worker may read
work items, mark them done and read the names of people.

## Connection, workspace and session

```mermaid
flowchart TD
    accTitle: The parts of a workspace
    accDescr: The application creates a database connection, which acts for one user. The connection creates a workspace, and the workspace creates sessions. The connection of the Local adapter calls the database in the same process. The connection of the Remote adapter calls the JSON API of a server, which calls the database.
    C["DatabaseConnection, for one user"] -- "CreateWorkspace()" --> W["IWorkspace"]
    W -- "CreateSession()" --> S["ISession"]
    C -- "Local" --> D[("Database")]
    C -- "Remote" --> A["JSON API of a server"]
    A --> D
```

An arrow points from a part to what it creates or calls. A workspace creates as many sessions as
the application needs. The connection of the Local adapter calls the database in the same process,
and the connection of the Remote adapter calls the JSON API of a server over HTTP.

| Part | What it holds |
| --- | --- |
| `DatabaseConnection` | The configuration of the workspace, the user, and the records of the objects that its sessions have pulled. A workspace adapter provides the class. |
| `IWorkspace` | The configuration and the services of the workspace, such as the metadata, `workspace.Services.Get<M>()`. |
| `ISession` | Objects and their changes, and the methods that pull, push and invoke. |

- **The configuration** names the workspace, gives its metadata and the factory of its typed
  objects, and lists the rules of its sessions. [`[Workspace]`](model.md#attributes) decides which
  classes, roles and methods a workspace of a name has, and the generator writes the metadata and
  the typed classes of one, `Default` in the sample. Give the configuration the name of that
  workspace. The Local adapter works with the database as that workspace; for the Remote adapter
  the server decides the workspace of each request, as [The workspace](json-api.md#the-workspace)
  of the JSON API describes.
- **A connection acts for one user:** for the Local adapter the user whose id the application
  gives it, for the Remote adapter the user who signed in. Create a connection for each user, and
  give each connection of the Remote adapter an `HttpClient` of its own: the sign-in puts the token
  on the `HttpClient`, so connections that share one act for the user who signed in last.
- **A session is a unit of work,** such as a screen. Each session has objects of its own: two
  sessions that pull the same object each hold an object for it, and putting an object of one
  session in a role of an object of another throws an `ArgumentException`. The records belong
  to the connection, and its sessions share them.

## The adapters

| Adapter | Project | Where it runs | The user |
| --- | --- | --- | --- |
| Local | `Allors.Workspace.Adapters.Local`, for .NET 10 | In the process of the database. Each pull, push and invoke runs in a transaction of its own. | The id in `UserId`; there is no sign-in. |
| Remote, with System.Text.Json | `Allors.Workspace.Adapters.Remote.SystemText`, for .NET Standard 2.0 | Anywhere: it calls [the JSON API](json-api.md) of a server. | The user who signed in with `Login`; the bearer token goes with every request. |
| Remote, with Newtonsoft.Json | `Allors.Workspace.Adapters.Remote.Newtonsoft`, for .NET Framework 4.8 | As the System.Text.Json variant. | As the System.Text.Json variant. |

Both adapters offer the same interfaces. [Where the adapters differ](pulls.md#where-the-adapters-differ)
lists the pulls that give different results.

## What a pull brings into a session

A pull names objects of the database. The adapter brings their records into the connection, each
with the version of its object, its roles and what the user may do with it, and the session gets
an object for each:

- **Only the objects that a pull named are in the session.** `session.Instantiate<T>(id)` finds an
  object that a pull brought into this session, or one that the session created, and returns
  `null` for any other.
- **A role names its objects by id.** Reading a role that holds an object that is not in the session
  throws an `Exception` with the message `Strategy is not in Session.` A pull brings the objects
  that you read through a role when it includes them; [Pulls](pulls.md#include) shows how.
- **An association comes from the objects in the session.** `jane.WorkItemsWhereAssignee` holds the
  `WorkItem` objects of the session whose `Assignee` is Jane and whose role `Assignee` the user
  may read. Pull the objects at the other end first.
- **A record changes only with a pull.** The session does not see what others change in the
  database until it pulls the object again.

## What the user may read and write

The generated classes have `CanRead…` and `CanWrite…` properties for roles with
`Origin.Database`, and `CanExecute…` for methods with that origin. The session answers from the
object's state and its last pulled record. Code that holds an object through a generated interface
can check a role with `Strategy.CanRead(roleType)` or `Strategy.CanWrite(roleType)`.
[Security](security.md) explains how the database decides.

- **An unreadable role reads as empty from the record** (`null`, or no objects). The typed
  property of a required unit role of a value type, such as `Done` of `WorkItem`, a `bool`, is not
  nullable, so reading it then throws a `NullReferenceException`. Ask `CanRead…` first, as
  `CanReadDone`. The property throws as well on a new object whose role has no value yet. A
  required `String` or `Binary` reads as `null`. When a derived database role has an active
  [workspace rule](#rules-in-a-session), the getter returns the value of the rule before checking
  permission to read the record. Check `CanRead…` before displaying that value.
- **Writing a role does nothing when `CanWrite…` is false for it.** The setter throws nothing, the
  role keeps its value and the session records no change. Ask `CanWrite…` first.
- **A new object** has every role readable and writable in the session until it is pushed. The
  database decides at the push, as [Creating objects](security.md#creating-objects) explains.
- **The database checks again.** A push writes a role, and an invoke runs a method, only when the
  user may. What the session answers comes from the record and serves the user interface.
- **A role with `Origin.Session`** exists only in the session. The user may always read and write
  it, and a push does not send it.
- **A mask** leaves an object out of a pull when the user may not read the role that masks its
  class, as [Where access is checked](security.md#where-access-is-checked) describes.

## Changes and the push

Only edits to roles with `Origin.Database` set `session.HasChanges`. A session with edits only
to roles with `Origin.Session` therefore reports `false`. `session.Create<WorkItem>()` gives
the new object a negative id. Creating an object alone leaves `HasChanges` false, but the next
push still sends the object.

`object.Strategy.Reset()` discards all edits to database roles on one object. `session.Reset()`
does the same for the objects in the change set since the last `session.Checkpoint()`, or since the
session began. Both forms of reset preserve the values of session roles. New objects stay in the
session and the next push still sends them. A changed object of the database also stays listed
for the next push after a reset. Over the Remote adapter that push fails with HTTP 500 unless
the object has another edit to a database role.

`session.PushAsync()` sends the new objects and the changed database roles. The database checks
versions and access, derives, and commits when nothing failed, as [Push](json-api.md#push)
describes for the server. The Local adapter takes similar steps in the process. The result reports
what failed in `VersionErrors`, `AccessErrors`, `MissingErrors`, `DerivationErrors` and
`ErrorMessage`, and `HasErrors` is true when one of them holds something.

- **A push that reports errors stores nothing,** and the session keeps its changes. See
  [Handle the errors of a push](use-a-workspace.md#8-handle-the-errors-of-a-push) for how to
  recover.
- **A push that succeeds gives each new object its id** in the database, and the object keeps it.
- **After a push the session waits for a pull.** The objects keep the values that were pushed,
  and `HasChanges` keeps its value. On each pushed object, `CanWrite…` is false for roles with
  `Origin.Database`, so a write does nothing. Pull those objects after every push to get their
  new versions and any values that the database derived.
- **A derivation error** has its message and the roles that it concerns, each with an object and a
  relation type. The object is `null` when the session does not hold it, as for a new object: the
  database gave the new object an id for the derivation and discarded it with the failed push.

## Versions and merges

Each record holds the version of its object, which changes when a transaction commits a change to
the object. A push sends the version of the record:

- **A version error.** When another transaction changed the object after the pull, the push
  reports the object in `VersionErrors` and stores nothing.
- **A merge.** A pull brings the new record. When the session changed a role that the new version
  changed as well, the pull reports the object in `MergeErrors`, `HasErrors` is true, and the
  session keeps its change and the earlier record. Otherwise the session keeps its changes on top
  of the new record, and the next push stores them.
- **To take the database's value,** call `Reset()` on the `Strategy` of each object in
  `MergeErrors` and pull again. Change the object again before the next push, as
  [Changes and the push](#changes-and-the-push) explains.

## Methods

A database method that `[Workspace]` makes available gives the typed class a property that returns a
`Method`, such as `person.Delete`, and a property `CanExecute…`, such as `CanExecuteDelete`.
`session.InvokeAsync(person.Delete)` runs the method in the database for the user. The database
checks the version and the permission, runs the method, derives, and commits when nothing failed.
With `Isolated` in `InvokeOptions`, each method runs in a transaction of its own, and then
`ContinueOnError` goes on after one that fails, as [Invoke](json-api.md#invoke) describes. Pull
afterwards to see what the method changed.

## Rules in a session

A derived role can have a rule in the workspace: a class that implements `IRule` of the workspace,
usually through the base class `Rule` of Core's workspace domain. The rule names the class and the
role, and computes the value in `Derive`. The configuration lists the rules, and
`session.Activate(workspace.Configuration.Rules)` turns them on in a session; a rule counts only
as the instance that the configuration holds. Reading the role then calls `Derive`. Core's
test domain has `PersonSessionFullNameRule`, which derives `SessionFullName`, a role with
`Origin.Session`. The sample has no rule in its workspace.

## Where this is implemented

- `dotnet/System/Workspace/Allors.Workspace.Domain` holds `IWorkspace`, `ISession`, `IObject`,
  `IStrategy`, the results in `Api`, and the pulls in `Data`.
- `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters` holds what the adapters share. Its
  `Strategy`, in `Session`, answers `CanRead`, `CanWrite` and `CanExecute`, reads the roles and
  leaves out a write without permission. `Session/OriginState` holds the records of an
  object, its changes and the merge. The Local adapter, the Remote adapter and its two variants
  are in the folders next to it.
- The templates of the workspace are in `dotnet/Core/Workspace/Templates`; `domain.cs.stg` writes
  the typed classes, with their `CanRead…`, `CanWrite…` and `CanExecute…` properties.
- `dotnet/Core/Workspace/Tests` holds tests that `Tests.Local`, `Tests.Remote.Json.SystemText` and
  `Tests.Remote.Json.Newtonsoft` run on each adapter against Core's test domain, among them
  `SecurityTests`, `PushTests` (`PushShouldUpdateId`), `MergeTests`, `MethodTests` and
  `DerivationTests`. `ManyToManyTests.SetRoleOld` checks that `CanWrite…` is false after the push
  of a new object, and that a write then does nothing. `WorkspaceResetTests.ResetOne2OneWithoutPush`
  checks that reading a role after a reset throws when its object was never pulled.
  `WorkItemTests`, in `sample/Workspace/Tests`, runs the sample's workspace on both adapters.
  No test reads an unreadable required role, writes a role that the grants of the user do not
  allow, calls `session.Reset()`, or pushes after a reset.
