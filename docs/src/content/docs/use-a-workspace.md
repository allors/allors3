---
title: "Use a workspace"
description: "Connect a .NET workspace to the database in the same process or to a server, pull objects with a filter and an include, read and change them as the user may, push, handle the errors and test on both adapters."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

Use this guide to work with the objects of an application from .NET code through a workspace. It
assumes [The workspace](workspace.md), and an application started from the sample with
[Create an application](create-an-application.md), whose folder `Workspace` holds the projects of
the workspace. The code comes from the sample's tests in `sample/Workspace/Tests`, which run every
step on the Local and on the Remote adapter.

## 1. Choose what the workspace holds

[`[Workspace]`](model.md#attributes) on a class, a role or a method makes it available to the
workspace that it names, `Default` when it names none. The sample makes `WorkItem` and its roles
available, for example `Title`:

```csharp
[Required]
[Size(256)]
[Workspace(Default)]
public string Title { get; set; }
```

A relation reaches a workspace only when a class at each of its ends is available there. After a
change to the model, generate again, as step 5 of
[Create an application](create-an-application.md#5-generate) describes: the sample's generator
writes the metadata and the typed classes of the workspace `Default` to `Workspace/Meta/Generated`,
`Workspace/Meta.Lazy/Generated` and `Workspace/Domain/Generated`.

## 2. Create the configuration

```csharp
var metaPopulation = new MetaBuilder().Build();
var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
this.configuration = new Configuration("Default", metaPopulation, objectFactory, Array.Empty<IRule>());
```

- `MetaBuilder` is the generated builder of `Workspace.Meta.Lazy`, in the namespace
  `Allors.Workspace.Meta.Lazy`. It builds the metadata of the workspace.
- `ReflectionObjectFactory` makes the typed objects. It takes the typed classes in the assembly and
  the namespace of the type that you give it, here the workspace's `Person`, of
  `Allors.Workspace.Domain`.
- `Configuration` belongs to the adapter: `Allors.Workspace.Adapters.Local.Configuration` or
  `Allors.Workspace.Adapters.Remote.Configuration`. Its first argument is the name of the
  workspace, and its last the rules of the sessions; the sample has none.

## 3. Connect

A connection acts for one user. Create one for each user, and give each one of the Remote adapter
an `HttpClient` of its own.

### In the process of the database

The Local adapter works with an `IDatabase` that the program holds. It has no sign-in: the
application finds the user and gives the connection the user's id.

```csharp
using var transaction = this.Database.CreateTransaction();
var users = new Users(transaction);
var user = users.FindBy(users.Meta.UserName, userName);

var connection = new DatabaseConnection(this.configuration, this.Database, () => new WorkspaceServices(), () => new DefaultStructRanges<long>())
{
    UserId = user.Id,
};
```

`connection.CreateWorkspace()` then creates the workspace. `WorkspaceServices` is the class in
`Workspace/Configuration/Custom`, which the application keeps as the platform has it.

### To a server

The Remote adapter calls [the JSON API](json-api.md) of the application's server through an
`HttpClient`, and signs in at the sign-in endpoint:

```csharp
var connection = new DatabaseConnection(this.configuration, () => new WorkspaceServices(), httpClient, new IdGenerator(), new DefaultStructRanges<long>());
if (!await connection.Login(new Uri("Authentication/Token", UriKind.Relative), userName, Population.Password))
{
    throw new InvalidOperationException($"The user {userName} could not sign in.");
}

return connection.CreateWorkspace();
```

- **The base address** of the `HttpClient` is the address of the server followed by `/allors/`,
  with the final slash, such as `http://workspace.example:5000/allors/` in the sample's tests. The
  adapter sends each request to a path relative to it, such as `pull`. Without the final slash the
  requests miss the API, and `Login` throws an `HttpRequestException` for HTTP 404.
- **The host.** The sample's server answers only the hosts in its setting `AllowedHosts`, which is
  `localhost` by default, and answers any other host with HTTP 400. List the host name that the
  client uses; [Settings](configuration.md#settings) describes the setting.
- **The sign-in.** `Login` returns `false` when the user name or the password is wrong, or while
  the user is locked out, and sets the bearer token on the `HttpClient` when it succeeds. Every
  request through that `HttpClient` then carries the token, so connections that share one act for
  the user who signed in last. A request that the server refuses, such as one with an expired
  token, throws an `HttpRequestException` with the status, here HTTP 401: sign in again.
  [Authentication](authentication.md) explains the tokens and the lockout.
- **Retries.** The connection's `Policy` sends a request again when the `HttpClient` throws an
  `HttpRequestException`, as it does when it cannot reach the server, up to five times, after 2, 4,
  8, 16 and 32 seconds. It sends a push or an invoke again as well, which can apply its changes
  twice when only the answer was lost. An answer with an error status, and a request that times
  out, are not sent again. Give `Policy` another Polly policy to change this.

## 4. Pull

```csharp
var workspace = await this.Profile.CreateWorkspace("jane@example.com");
var m = workspace.Services.Get<M>();
var session = workspace.CreateSession();

var result = await session.PullAsync(new Pull
{
    Extent = new Filter(m.WorkItem),
    Results = new[] { new Result { Include = m.WorkItem.Nodes(v => v.Assignee.Node()) } },
});

var workItem = Assert.Single(result.GetCollection<WorkItem>());
```

`m` is the generated metadata of the workspace. The `Filter` finds the `WorkItem` objects, and the
include brings the `Assignee` of each one into the session, so that the code can read
`workItem.Assignee`; without it, reading the role throws. `GetCollection<WorkItem>()` takes the
collection that carries the plural name of the class, `WorkItems`.

A predicate narrows the filter. The sample's tests find Jane this way in a workspace of the
administrator, `admin@example.com`:

```csharp
var result = await session.PullAsync(new Pull
{
    Extent = new Filter(m.Person) { Predicate = new Equals(m.Person.UserName) { Value = "jane@example.com" } },
});

var jane = Assert.Single(result.GetCollection<Person>());
```

[Pulls](pulls.md) lists the predicates, the selects, the includes and the names of the results.

## 5. Read what the user may read

```csharp
Assert.Equal("Write the rule", workItem.Title);
Assert.Equal("Jane", workItem.Assignee.FirstName);

// A worker reads the names of people, and no other role of a person.
Assert.False(workItem.Assignee.CanReadUserName);
Assert.Null(workItem.Assignee.UserName);
```

Check `CanRead…` before displaying a database role. In the sample an unreadable role reads as
empty. The property of a required unit role of a value type, such as an `int`, cannot hold
`null`. Reading `OpenWorkItemCount` therefore throws a `NullReferenceException` for a worker.
Check `CanReadOpenWorkItemCount` first. This check also applies when an active workspace rule
computes a value in the getter, as [The workspace](workspace.md#what-the-user-may-read-and-write)
explains.

## 6. Change an object and push

```csharp
// A worker marks work items done, and changes no other role of a work item.
Assert.True(workItem.CanWriteDone);
Assert.False(workItem.CanWriteTitle);

workItem.Done = true;
var pushResult = await session.PushAsync();

Assert.False(pushResult.HasErrors);
```

Setting `Title` here would do nothing, without an exception: ask `CanWrite…` before you offer a
change to the user. After the push the session keeps the values that it pushed, and `CanWrite…`
stays false for the object until a pull brings its new record. Pull the object before you change
it again.

## 7. Create an object

A worker's push of a new work item fails with access errors: the roles of a new object follow the
grants of the initial `SecurityToken`, as [Creating objects](security.md#creating-objects) explains,
and none of those is the workers'. Use the administrator session that found `jane` in step 4.

```csharp
var workItem = session.Create<WorkItem>();
workItem.Title = "Review the rule";
workItem.Assignee = jane;

var pushResult = await session.PushAsync();

Assert.False(pushResult.HasErrors);

// The push stored the work item, and the database derived the number of open work items of its
// assignee.
await session.PullAsync(new Pull { Object = jane });
```

Read `jane.OpenWorkItemCount` after the pull. If you followed step 6, Jane's first item is
already done and the count is one. The sample's administrator test starts with a fresh population
in which Jane's first item is open, so its count is two.

`jane` comes from a pull of the same session; an object of another session cannot go in a role.
Until the push the new work item has a negative id, and a pull of it throws an `ArgumentException`.
After a push without errors it has its id in the database.

## 8. Handle the errors of a push

```csharp
// A work item requires a title.
session.Create<WorkItem>();
var pushResult = await session.PushAsync();

Assert.True(pushResult.HasErrors);
var error = Assert.Single(pushResult.DerivationErrors);
Assert.Contains(error.Roles, v => v.RelationType == m.WorkItem.Title.RelationType);
```

A push that reports an error stores nothing, and the session keeps its changes. Each kind of error
asks for its own response:

| Result | What it means | What to do |
| --- | --- | --- |
| `DerivationErrors` | The derivation of the push found errors, such as a required role without a value. Each error has a `Message` and `Roles`; the object of a role is `null` when the session does not hold it, as for a new object. | Correct the roles and push again. |
| `VersionErrors` | Another transaction changed the object after the pull. | Pull the object. When the pull reports it in `MergeErrors`, call `Reset()` on its `Strategy`, pull it again and make the change again before you push. |
| `AccessErrors` | The user may not write a role of the object, or its class is outside the workspace. An object can appear once for each role that it refuses. | Leave the change out. `CanWrite…` was false after the last pull, or the access changed since. |
| `MissingErrors` | An object that the session changed has been deleted. On Remote this can also mean that a role refers to a deleted object. | For a deleted changed object, start a fresh session and reapply the changes to the surviving objects. For a missing object in a role, clear or replace that value and push again. |

A failed push keeps every changed object listed for the next push, even after a reset. If a
work item's chosen assignee has been deleted, the Remote adapter reports the missing assignee.
On the Local adapter the push can succeed with the optional `Assignee` role empty.

[The workspace](workspace.md#versions-and-merges) explains versions and merges. With the Remote
adapter, an answer of the server with an error status, such as HTTP 500, throws an
`HttpRequestException` instead of returning a result. In particular, a push that creates no object
and writes a role that the user may no longer write, on an object of the database, gets HTTP 500
instead of an access error; issue [#441](https://github.com/allors/allors3/issues/441) records
this.

## 9. Test on both adapters

The sample runs each test on both adapters. A profile builds the database with the sample's setup
and population, and creates the workspace of a user:

- **The Local profile** builds the database on the Snapshot adapter in the test process and
  connects as step 3 shows.
- **The Remote profile** runs the sample's server in the test process through
  `WebApplicationFactory`, the class `ServerFactory`, with settings that override the
  configuration root:

  ```csharp
  this.factory = new ServerFactory(new Dictionary<string, string>
  {
      ["adapter"] = "snapshot",
      ["AllowedHosts"] = ServerAddress.Host,
      ["JwtToken:Key"] = Key,
  });
  ```

  The client of the factory uses the host `workspace.example`, so the profile lists that host in
  `AllowedHosts`.

The tests are in an abstract class, `WorkItemTests`, that takes the profile. A class for each
adapter derives from it:

```csharp
public class WorkItemTests : Tests.WorkItemTests
{
    public WorkItemTests() : base(new Profile())
    {
    }
}
```

Run them from the root of the application's repository:

```sh
dotnet test Workspace/Tests
```

## Where this is implemented

- `sample/Workspace/Tests` holds `WorkItemTests`, with the four tests that this guide quotes, and
  the profiles in `Local/Profile.cs` and `Remote/Profile.cs`, with `Remote/ServerFactory.cs`. The
  target `SampleTest` of the Nuke build runs them.
- The connection of the Local adapter is
  `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Local/Database/DatabaseConnection.cs`.
  The connection of the Remote adapter with System.Text.Json, with `Login` and `Policy`, is
  `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote.SystemText/Database.cs`.
- `ReflectionObjectFactory` is in `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters`.
- No test sends a pull through a base address without the final slash, or reads a role that the
  user may not read through a property that is not nullable.
