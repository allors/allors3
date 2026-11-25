---
title: "Pulls"
description: "What a pull of the .NET workspace can ask for: an object or an extent, predicates and sorting, results with a select, an include, paging and a name, arguments, prepared extents and selects, procedures and custom routes, and where the adapters differ."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

A pull finds objects in the database and brings them into a session, with the objects that its
selects and includes reach. `session.PullAsync(params Pull[] pulls)` sends several pulls together
and returns one `IPullResult`. The classes of a pull are in the namespace `Allors.Workspace.Data`.

[Extents](extents.md) explains the predicates, which mean the same in a pull as in the database.
[Pull](json-api.md#pull) gives the form that the Remote adapter sends, and
[The workspace](workspace.md#what-a-pull-brings-into-a-session) explains what a pull brings into a
session. The examples use the sample's metadata `m`.

## A pull

| Property | Holds |
| --- | --- |
| `Extent` | An extent: a `Filter`, a `Union`, an `Intersect` or an `Except`. |
| `ExtentRef` | The id of a [prepared extent](#prepared-extents-and-selects), instead of `Extent`. |
| `Object`, `ObjectId` | An object of the session, or the id of an object, instead of an extent. |
| `ObjectType` | With an object: a class or an interface. The pull finds nothing when the object is not of that type. |
| `Results` | What the pull returns; see [Results](#results). Without results, a pull returns the objects that it finds. |
| `Arguments` | The values of the [parameters](#parameters) of its predicates, by name. |

A pull of a new object of the session, which has a negative id, throws an `ArgumentException`
with the message `Id is not in the database`. A pull of an id that does not exist in the database
throws: an `HttpRequestException` for HTTP 500 on the Remote adapter, and on the Local adapter an
`Exception` with the message `Either an Extent or an ExtentRef is required.`

## Extents

| Extent | Holds |
| --- | --- |
| `new Filter(m.WorkItem)` | The objects of a class or an interface that match its `Predicate`, or all of them without one. |
| `new Union(first, second)` | The objects of either operand. |
| `new Intersect(first, second)` | The objects of both operands. |
| `new Except(first, second)` | The objects of the first operand that are not in the second. |

Each extent has `Sorting`, a list of `Sort` objects: `new Sort(m.Person.LastName)`, ascending
unless its `SortDirection` is `Descending`. The first sort comes first.

## Predicates

The `Predicate` of a filter holds one predicate; `And`, `Or` and `Not` combine others. A predicate
takes the role type or the association type of the extent's type as its first argument.

| Predicate | Takes | An object matches when |
| --- | --- | --- |
| `Equals` | `Value`, `Object` or `Path` | The role has the value or holds the object, the association is the object, or the unit role equals the unit role in `Path` of the same object. Without a role type or an association type: the object is `Object`. |
| `Like` | `Value` | The string role matches the pattern; `%` stands for any run of characters and `_` for one character. |
| `GreaterThan`, `LessThan` | `Value` or `Path` | The role is greater than, or less than, the value or the role in `Path`. |
| `Between` | `Values`, two values, or `Paths`, two role types | The role lies between the two, both included. |
| `Exists` | Nothing | The role holds something, or the association exists. |
| `Contains` | `Object` | The role or the association, one that holds several objects, holds the object. |
| `ContainedIn` | `Extent` or `Objects` | An object of the role or the association is among those of the extent or the list. |
| `Instanceof` | `ObjectType` | The object in the role or the association is of that class or interface; without a role type or an association type, the object itself is. |
| `And`, `Or` | `Operands` | Every operand holds, or one does. |
| `Not` | `Operand` | The operand does not hold. |

```csharp
var result = await session.PullAsync(new Pull
{
    Extent = new Filter(m.Person) { Predicate = new Equals(m.Person.UserName) { Value = "jane@example.com" } },
});
```

A predicate without a value adds no condition: `Equals` with a `Value` of `null` matches every
object. [Filter](extents.md#filter) states the rules of the predicates and of their patterns.

### Parameters

Instead of a value, a predicate can name an argument in `Parameter`. The pull's `Arguments` give
the value by that name, and a predicate whose argument is missing from them is left out.
`Dependencies` names further arguments; the predicate is left out when one of them is missing. The
adapters differ:

- **Local.** The arguments apply. A pull whose `Arguments` is `null` throws a
  `NullReferenceException` for a predicate with a `Parameter` or with `Dependencies`.
- **Remote.** The .NET adapter does not send `Arguments`, so the server leaves out every predicate
  with a `Parameter` or with `Dependencies`. The TypeScript workspace sends them.

## Results

| Property | Holds |
| --- | --- |
| `Name` | The name of the result. |
| `Select` | A path along roles and associations, from each object that the pull finds. The result holds the objects at its end. |
| `SelectRef` | The id of a [prepared select](#prepared-extents-and-selects), instead of `Select`. |
| `Include` | The objects to bring into the session along the roles and associations of the result's objects, when the result has no select. |
| `Skip`, `Take` | Skip some objects of an extent and take some; a pull of an object ignores both. |

### Select

`new Select(m.WorkItem.Assignee)` holds the assignee of each work item that the pull finds; the
result holds each object once. `new Select(m.WorkItem.Assignee, m.Person.WorkItemsWhereAssignee)`
takes two steps, each in a `Select` of its own, linked through `Next`; `End` is the last step.
`OfType` on a step keeps the objects of one class or interface.

A select and an include follow a role only when the user may read it, as [Pull](json-api.md#pull)
describes.

### Include

An include is a tree of `Node` objects. The generated metadata builds one:

```csharp
var result = await session.PullAsync(new Pull
{
    Extent = new Filter(m.WorkItem),
    Results = new[] { new Result { Include = m.WorkItem.Nodes(v => v.Assignee.Node()) } },
});
```

`Node` takes a further step from the type at the end of a role, through `ObjectType`:
`m.WorkItem.Nodes(v => v.Assignee.Node(w => w.ObjectType.WorkItemsWhereAssignee.Node()))` brings
the assignee of each work item and the work items of that assignee.

On a result with a select, set the include on the last step, `select.End.Include`: both adapters
ignore the result's own `Include` when it has a select.

### Names

A result without a name is named after what it holds:

| Result | Name |
| --- | --- |
| A pull of an extent | The plural name of the extent's type, such as `WorkItems`. |
| A pull of an object | `ObjectType`, or else the singular name of the object's class, such as `Person`. |
| A select | On the Remote adapter, the plural name of the role or the association at its end, such as `Assignees`, or its singular name on a pull of an object when the select leads to one object. On the Local adapter, the full name instead, such as `WorkItemAssignees` or `WorkItemAssignee`. |

Name every result that has a select. `result.GetCollection<WorkItem>()` takes the collection with
the plural name of the type, and `GetCollection<Person>("Assignees")` a collection by name;
`GetObject<Person>()` and `GetObject<Person>(name)` do the same for one object, and both return
`null` when the result has no such name. `GetValue(name)` reads a value that a procedure returned.
`Collections`, `Objects` and `Values` hold them all. The Remote adapter ignores the case of a name;
the Local adapter does not.

### Paging

`Skip` and `Take` apply to the objects of an extent. On the Remote adapter they apply before the
select, so `Take = 1` with a select gives the objects that the select reaches from the first
object. On the Local adapter they apply to the objects that the select reaches.

## Prepared extents and selects

A prepared extent or select is defined in the database, and a pull names it by its id in
`ExtentRef` or `SelectRef`. The application's configuration registers them, by id, in the
classes `PreparedExtents` and `PreparedSelects` of `Database/Configuration/Custom`. The sample
registers none.

## Procedures

`session.CallAsync(procedure, pulls)` runs a procedure of the database, and then the pulls, in one
request:

```csharp
var procedure = new Procedure("TestValues");

var result = await session.CallAsync(procedure);

Assert.False(result.HasErrors);
Assert.True(result.GetValue<bool>("allorsBoolean"));
```

- **The procedure** is a class of the domain that implements `IProcedure`, with a constructor
  without parameters. The database finds it by the name of its class, ignoring case. Its
  `Execute` takes a context, with the transaction and the access control of the pull, the input
  and the output.
- **The input.** `Procedure` takes `Collections`, `Objects` and `Values` by name, and `Pool`, the
  objects whose versions the database checks first. A version that has changed is a version error,
  and the procedure does not run.
- **The output.** The procedure adds collections, objects and values by name; the pull result holds
  them. A procedure runs in the transaction of the pull, which the database never commits, so
  what the procedure changes is not stored.
- **A missing procedure** gives `Missing procedure <name>` in `ErrorMessage`.

`TestValues` is a procedure of Core's test domain; the sample has none.

## Custom routes

`session.CallAsync(args, name)` sends `args` as JSON to the custom route `<name>/pull`, relative to
the base address of the Remote adapter, and reads the answer as the answer of a pull. The
application's server answers with a controller of its own: the server's route
`allors/{controller}/{action}` sends `People/pull` to the action `Pull` of a `PeopleController`,
which builds the answer with the pull response builder of Core's `Api`. A name without a controller
gets HTTP 404, which throws an `HttpRequestException`. Core's test server has such controllers in
`dotnet/Core/Database/Server/Custom`; the sample's server has none.

The Local adapter has no custom routes: `CallAsync(args, name)` throws a
`NotImplementedException`.

## Where the adapters differ

The Local adapter runs a pull in the process, and the Remote adapter sends it to the server. Most
pulls give the same objects on both, apart from these:

| Pull | Local | Remote |
| --- | --- | --- |
| The name of a result with a select and without `Name` | The full name of the role, such as `WorkItemAssignees` | The name of the role, such as `Assignees` |
| The case of a name in `GetCollection` | Must match | Ignored |
| `Skip` and `Take` with a select | Applied to the selected objects | Applied before the select |
| A result after a result with a select, in one pull | Starts from the objects of that select | Starts from the objects of the extent |
| A select that ends in an association that holds several objects | Throws an `Exception` that wraps an `InvalidCastException` | Returns the objects |
| `Parameter` and `Dependencies` | Apply; `null` arguments throw | Left out, because the arguments are not sent |
| An id that does not exist | `Exception` | `HttpRequestException`, HTTP 500 |
| A pull to a custom route | `NotImplementedException` | Sent to the server |

## Where this is implemented

- `Pull`, `Result`, `Select`, `Node`, the extents, the predicates, `Sort` and `Procedure` are in
  `dotnet/System/Workspace/Allors.Workspace.Domain/Data`, with the builders of an include in
  `Data/Extensions`.
- The Remote adapter turns a pull into JSON in
  `dotnet/System/Workspace/Allors.Workspace.Protocol.Json/Data/ToJsonVisitor.cs`; `VisitPull`
  copies no arguments. The server answers in
  `dotnet/System/Database/Allors.Database.Workspace.Json/Pull`.
- The Local adapter runs a pull in
  `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Local/Database/Pull`: `PullExtent`,
  `PullInstantiate` and `Procedure`. Both run the predicates of
  `dotnet/System/Database/Allors.Database/Data`, which leave out a predicate whose argument is
  missing.
- `IProcedure` is in `dotnet/System/Database/Allors.Database.Workspace/Procedure`, and `Procedures`,
  which finds a procedure by name, in `dotnet/Core/Database/Configuration/Core/Database/Procedures`.
- `PullTests` and `ProcedureTests`, in `dotnet/Core/Workspace/Tests/Tests`, check the predicates,
  the operators, the sorting, a named result, an include and the procedures on each adapter;
  `PagingTests` checks `Take`. No test of the workspace selects along a role or an association, or
  uses a parameter or a custom route.
