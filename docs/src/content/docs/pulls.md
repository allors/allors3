---
title: "Pulls"
description: "What a pull of the .NET workspace can ask for: an object or an extent, predicates and sorting, results with a select, an include, paging and a name, arguments, prepared extents and selects, procedures and custom routes, and where the adapters differ."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

A pull finds objects in the database and brings into a session the objects that its results hold,
with the objects that their includes reach. A result without a select holds the objects that the
pull finds; a result with a select holds only the objects at the end of the select.
`session.PullAsync(params Pull[] pulls)` sends several pulls together and returns one
`IPullResult`. Give collections in the same call [distinct names](#names). The classes of a pull
are in the namespace `Allors.Workspace.Data`.

[Extents](extents.md) explains the predicates.
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
| `Results` | What the pull returns. Leave it `null` to return the objects that the pull finds. With `Array.Empty<Result>()`, Local returns no objects and Remote returns the matches. See [Results](#results). |
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

The `Predicate` of a filter holds one predicate; `And`, `Or` and `Not` combine others. The first
argument of a predicate is a role type or an association type of the extent's type. `Like`,
`GreaterThan`, `LessThan` and `Between` take a role type only, and `Equals` and `Instanceof` can do
without either.

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

`Equals` without a `Value`, an `Object` or a `Path` adds no condition. An `And` or an `Or` with
zero operands, or a `Not` without an operand, also adds no condition. If any of these is the
filter's only predicate, the filter matches every object of its type. An `Instanceof` without
`ObjectType` matches nothing. For `Like`, `GreaterThan`, `LessThan`, `Between`, `Contains` and
`ContainedIn`, omitting the value, object, extent or path that the predicate takes makes Local
throw an `ArgumentException` or a `NullReferenceException` and Remote throw an
`HttpRequestException` for HTTP 500.
[Filter](extents.md#filter) states the rules of the predicates and of their patterns.

### Parameters

Instead of a value, a predicate can name an argument in `Parameter`. The pull's `Arguments` give
the value by that name, and a predicate whose argument is missing from them is left out.
`Dependencies` names further arguments; the predicate is left out when one of them is missing. The
adapters differ:

- **Local.** The arguments apply, in the form that the database takes: an object by its id, a
  `long`, several objects as a `long[]`, a class or an interface by its tag, and a unit by its
  value. To compare the object itself, use `new Equals { Object = person }`. With no role type or
  association type, `new Equals { Parameter = "person" }` checks that the id resolves but compares
  against its unset `Object`. Even a valid id then gives no matches on Memory or Snapshot.
  A pull whose `Arguments` is `null` throws a `NullReferenceException` for a predicate with a
  `Parameter` or with `Dependencies`.
- **Remote.** The .NET adapter does not send `Arguments`, so the server leaves out a predicate with
  a `Parameter` or with `Dependencies`. It omits the `Parameter` of an `Instanceof` as well.
  With no explicit `ObjectType` or dependencies, that predicate stays and matches nothing.
  The TypeScript workspace sends the arguments.

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
`OfType` on a step keeps the objects of one class or interface. The Local adapter ignores it
when the workspace supplies the select in `Result.Select`.

The position of a step that holds several objects matters when each step finds objects.

- **Local.** A select can follow a step that holds several objects if its last step holds one
  object. If the last step holds several, Local throws an `Exception` that wraps an
  `InvalidCastException`, even if an earlier step also holds several.
- **Remote.** A pull of one object can follow a step that holds several objects anywhere in the
  path. In a pull of an extent, only the last step can hold several. An earlier such step gives
  HTTP 500. The two-step example above works on Remote because only its last step holds several.

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

On a result with a select, put the include on the last step, `select.End.Include`, and leave
earlier steps without one. Both adapters apply the include to the objects at the end of the
select. Local uses the first step's `Include` if set and otherwise uses the last step's.
Remote uses only the last step's. Both ignore `Include` on the result itself.

### Names

A result without a name is named after what it holds:

| Result | Name |
| --- | --- |
| A pull of an extent | The plural name of the extent's type, such as `WorkItems`. |
| A pull of an object | `ObjectType`, or else the singular name of the object's class, such as `Person`. |
| A select | On the Remote adapter, the plural name of the role or the association at its end, such as `Assignees`, or its singular name on a pull of an object when the select leads to one object. On the Local adapter, a select that ends in a role takes the full name of the role instead, such as `WorkItemAssignees` or `WorkItemAssignee`. |

Name every result that has a select. Give collections in the same call distinct names, even across
pulls. For example, two pulls of `WorkItem` extents both default to `WorkItems`. If both find
matches and the second collection has no include, both adapters add its ids to the existing
collection but skip loading the additional objects. In a fresh session those entries read as `null`.

`result.GetCollection<WorkItem>()` takes the collection with the plural name of the type, and
`GetCollection<Person>("Assignees")` a collection by name;
`GetObject<Person>()` and `GetObject<Person>(name)` do the same for one object, and both return
`null` when the result has no such name. The Remote adapter ignores the case of a name in these
methods; the Local adapter does not. `Collections`, `Objects` and `Values` hold them all, by name;
on the Remote adapter their names are in upper case, such as `WORKITEMS`.

Read a value that a procedure returned with `GetValue<T>(name)` and the type of its unit. The Local
adapter casts the value that the procedure added. The Remote adapter converts the JSON of the
answer, and its `GetValue<T>` takes the types of the units only.

### Paging

`Skip` and `Take` apply to the objects of an extent:

- **With a select.** The Remote adapter pages before the select, so `Take = 1` gives the objects
  that the select reaches from the first object. The Local adapter pages the objects that the
  select reaches.
- **Before or after leaving objects out.** The Remote adapter pages the objects that it may return.
  The Local adapter pages first and then leaves out the objects of a class outside the workspace
  and the objects that a mask hides, so it can return fewer than `Take`.

### Several results

A pull can have several results. In a pull of an extent, each result starts from the objects that
the pull finds on the Remote adapter; on the Local adapter, a result after a result with a select
starts from the objects of that select. In a pull of an object, a result after a select that leads
to one object starts from that object, on both adapters.

## Prepared extents and selects

A prepared extent or select is defined on the side of the database, in the configuration of the
application, and a pull names it by its id in `ExtentRef` or `SelectRef`. The configuration
registers them, by id, in the classes `PreparedExtents` and `PreparedSelects` of
`Database/Configuration/Custom`. The sample registers none.

## Procedures

`session.CallAsync(procedure, pulls)` runs a procedure of the database and the pulls, in one
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
  them. A procedure runs in the transaction of the pull, which the platform does not commit, so
  what the procedure changes is not stored unless the procedure commits it.
- **A missing procedure** gives `Missing procedure <name>` in `ErrorMessage`.
- **When the procedure fails,** with a version error, a missing procedure or an error that it
  adds, the adapters differ. The Remote adapter runs none of the pulls, and its result then has no
  collections, objects or values: `GetCollection` and the others throw. The Local adapter runs the
  pulls anyway, and brings their objects into the session.

`TestValues` is a procedure of Core's test domain; the sample has none.

## Custom routes

`session.CallAsync(args, name)` sends `args` as JSON to the custom route `<name>/pull`, relative to
the base address of the Remote adapter, and reads the answer as the answer of a pull. The
application's server answers with a controller of its own: the server's route
`allors/{controller}/{action}` sends `People/pull` to the action `Pull` of a `PeopleController`,
which builds the answer with Core's `Api.CreatePullResponseBuilder()`. The code marks that method
for deletion, and its builder adds no dependencies. A name without a controller gets HTTP 404,
which throws an `HttpRequestException`. Core's test server has such controllers in
`dotnet/Core/Database/Server/Custom`; the sample's server has none.

The Local adapter has no custom routes: `CallAsync(args, name)` throws a
`NotImplementedException`.

## Where the adapters differ

The Local adapter runs a pull in the process, and the Remote adapter sends it to the server. Most
pulls give the same objects on both, apart from these:

| Pull | Local | Remote |
| --- | --- | --- |
| `Results = Array.Empty<Result>()` | Returns no objects | Returns the matches |
| The name of a result whose select ends in a role, without `Name` | The full name of the role, such as `WorkItemAssignees` | The name of the role, such as `Assignees` |
| The case of a name in `GetCollection` and `GetObject` | Must match | Ignored |
| The names in `Collections`, `Objects` and `Values` | As the result names them | In upper case |
| `GetValue` | The value that the procedure added | The JSON of the answer; `GetValue<T>` takes the types of the units only |
| `Skip` and `Take` with a select | Applied to the selected objects | Applied before the select |
| `Skip` and `Take` and the objects that are left out | Applied before objects are left out | Applied to the objects that may be returned |
| In a pull of an extent, a result after a result with a select | Starts from the objects of that select | Starts from the objects of the extent |
| `OfType` on a step in `Result.Select` | Ignored | Applied |
| A select that reaches a last step holding several objects | Throws an `Exception` that wraps an `InvalidCastException` | Returns the objects. In a pull of an extent, an earlier step that also holds several gives HTTP 500. |
| An extent with a select whose earlier step holds several objects and whose last step reaches one object per path | Returns the objects | `HttpRequestException`, HTTP 500 |
| An include on the first step of a select with several steps | Used in preference to the last step's include | Ignored |
| `Parameter` and `Dependencies` | Apply with the [exception for object equality](#parameters). A `null` arguments dictionary throws. | The server leaves the predicate out when an argument is missing. For `Instanceof` the adapter omits `Parameter` itself. See [Parameters](#parameters). |
| A procedure that fails | The pulls run | The pulls do not run, and the result has no collections, objects or values |
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
- The Local adapter turns a pull into the database's in
  `dotnet/System/Workspace/Allors.Workspace.Protocol.Direct/ToDatabaseVisitor.cs`, which copies no
  `OfType`, with its arguments in `Arguments.cs` next to it, and runs it in
  `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Local/Database/Pull`: `PullExtent`,
  `PullInstantiate` and `Procedure`.
- The predicates of `dotnet/System/Database/Allors.Database/Data` leave out a predicate whose
  argument is missing. `SelectExtension.cs`, in
  `dotnet/System/Database/Allors.Database.Workspace/Data`, takes the steps of a select.
- `IProcedure` is in `dotnet/System/Database/Allors.Database.Workspace/Procedure`, and `Procedures`,
  which finds a procedure by name, in `dotnet/Core/Database/Configuration/Core/Database/Procedures`.
- `PullTests` and `ProcedureTests`, in `dotnet/Core/Workspace/Tests/Tests`, check the predicates,
  the operators, the sorting, a named result, an include on a select without a role type
  (`PullWithInclude`) and the procedures on each adapter; `PagingTests` checks `Take`, and the
  sample's `WorkItemTests` an include on a result. No test of the .NET workspace has a select
  with a step, a parameter or a custom route.
