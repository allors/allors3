---
title: "Extents"
description: "Query the objects of a class or an interface: filter them, follow relations in the filter, sort, combine extents and prefetch."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code
> and the tests.

An extent is the set of objects of one class or interface, narrowed by a filter. Use an extent to
find objects that you cannot reach by following a relation from an object that you already hold.
This guide assumes [Objects and relations](objects-and-relations.md). A workspace finds objects
with the same predicates in a pull; [Pulls](pulls.md) describes it.

The examples use `Person` and `Organisation` of the platform's test domain, a `transaction`, and
`m`, the metadata of the model. Every typed object and every objects class has it as `M`; a
transaction gives it as `transaction.Database.Services.Get<MetaPopulation>()`.

## Get an extent

```csharp
var people = transaction.Extent<Person>();        // an Extent<Person>
var same = new People(transaction).Extent();      // the same, through the objects class
Extent untyped = transaction.Extent(m.Person);    // by metadata, for a type that is known at run time
```

Without a filter an extent holds every object of its type. The extent of an interface holds the
objects of every class that implements it: `transaction.Extent<Deletable>()`.

## Filter

Add predicates to `Filter`. An object is in the extent when all of them hold:

```csharp
var people = transaction.Extent<Person>();
people.Filter.AddEquals(m.Person.LastName, "Doe");
people.Filter.AddExists(m.Person.OrganisationWhereEmployee);
```

This extent holds the people with the last name Doe who are an employee.

| Predicate | An object matches when |
| --- | --- |
| `AddEquals(roleType, value)` | The role has that value, or holds that object. |
| `AddEquals(associationType, object)` | The association is that object. |
| `AddLike(roleType, pattern)` | The string role matches the pattern. `%` stands for any run of characters and `_` for one character; without them the whole value has to equal the pattern. Square brackets have no special meaning. |
| `AddGreaterThan(roleType, value)`, `AddLessThan(roleType, value)` | The role is greater than, or less than, the value. |
| `AddBetween(roleType, first, second)` | The role lies from `first` to `second`, both included. |
| `AddExists(roleType)`, `AddExists(associationType)` | The role holds something, or the association exists. |
| `AddContains(roleType, object)`, `AddContains(associationType, object)` | The role or the association, one that holds several objects, holds that object. |
| `AddContainedIn(roleType, objects)`, `AddContainedIn(associationType, objects)` | An object of the role or the association is among `objects`, which is another extent or a collection of objects. |
| `AddInstanceof(type)` | The object is of that class or interface. With a role type or an association type as the first argument, the object in that role or association is. |

Three rules hold for these predicates:

- **Compare two unit roles of one object.** `AddEquals`, `AddGreaterThan`, `AddLessThan` and
  `AddBetween` take the role type of a unit role where a value is expected:
  `AddEquals(m.Person.FirstName, m.Person.LastName)` matches the people whose first name is their
  last name. In `AddBetween`, give both bounds as values or both as role types. A composite role
  type in that place throws an `ArgumentException`.
- **The first argument belongs to the type of the extent.** A predicate on a role type or an
  association type that the type of the extent does not have throws an `ArgumentException`, as
  `transaction.Extent<Person>().Filter.AddEquals(m.Organisation.Name, "Acme")` does.
- **Keep the patterns of `AddLike` simple.** The adapters agree on patterns with `%` and `_` for
  values of one line. They differ for an empty pattern, for a backslash in a pattern and for a
  value with a line break: do not rely on those.

### Combine predicates

`AddOr()` and `AddAnd()` return a predicate that takes predicates of its own. `AddNot()` returns
one that takes a single predicate; a second one throws an `ArgumentException`. To negate several
predicates, add an `AddOr()` or an `AddAnd()` to it first.

```csharp
var people = transaction.Extent<Person>();

var either = people.Filter.AddOr();
either.AddEquals(m.Person.FirstName, "Jane");
either.AddEquals(m.Person.FirstName, "John");

people.Filter.AddNot().AddExists(m.Person.OrganisationWhereEmployee);
```

This extent holds the people named Jane or John who are not an employee.

### Follow a relation

A predicate on a role or an association reaches the objects next to the ones that you query, and
`AddContainedIn` takes another extent to go a step further. The employees of the organizations
that John owns:

```csharp
var owned = transaction.Extent<Organisation>();
owned.Filter.AddEquals(m.Organisation.Owner, john);

var employees = transaction.Extent<Person>();
employees.Filter.AddContainedIn(m.Person.OrganisationWhereEmployee, owned);
```

## Sort

```csharp
people.AddSort(m.Person.LastName);
people.AddSort(m.Person.FirstName, SortDirection.Descending);
```

The first sort comes first; a later one orders the objects that the earlier ones leave equal.
Without a sort, do not rely on the order of an extent.

The adapters differ on the objects that have nothing in the role of a sort. In an ascending sort
the Memory and Snapshot adapters put them last and the SQL adapters put them first; a descending
sort reverses both.

## Combine extents

`Union`, `Intersect` and `Except` of a transaction combine two extents of the same type:

```csharp
Extent does = transaction.Extent(m.Person);
does.Filter.AddEquals(m.Person.LastName, "Doe");

Extent employees = transaction.Extent(m.Person);
employees.Filter.AddExists(m.Person.OrganisationWhereEmployee);

var either = transaction.Union(does, employees);
```

- Two extents of different types throw an `ArgumentException`.
- The result is an extent: it can be sorted, and it can be an operand of a further operation. It
  has no `Filter`: filter the operands.
- Use an extent as an operand once. The Memory and Snapshot adapters refuse an extent that is
  already the operand of another operation: build the operands again for each operation.

## Read the result

An extent is a read-only list. Read `Count`, take an object by its index, enumerate it or call
`ToArray()`; a typed extent also has `First`, which is `null` when the extent is empty.

- An extent sees the changes of its own transaction, also the ones that are not committed.
- An extent runs its query when it is first read. Adding a predicate or a sort afterwards makes it
  run again.
- Do not expect an extent that has been read to follow later changes to the objects. Build a new
  extent after a change.

## Find one object

The objects class finds the first object with a value in a role, or `null`:

```csharp
var jane = new People(transaction).FindBy(m.Person.UserName, "jane@example.com");
```

## Prefetch

Code that reads the same relations of many objects can name those relations in a prefetch
policy, so that the adapter loads them together:

```csharp
var policy = new PrefetchPolicyBuilder()
    .WithRule(m.Organisation.Employees)
    .WithRule(m.Organisation.Owner)
    .Build();

var organisations = transaction.Extent<Organisation>().ToArray();
transaction.Prefetch(policy, organisations);
```

`WithRule` takes a role type or an association type and, when you give one, a second policy for
the objects that the relation leads to. A prefetch changes no result: it changes when the data is
read, which matters on a database server.

## Where this is implemented

- `Extent`, `Extent<T>`, `ICompositePredicate` and the prefetch classes are in
  `dotnet/System/Database/Allors.Database`; `ITransaction` has `Extent`, `Union`, `Intersect`,
  `Except` and `Prefetch`. `ObjectsBase`, in `dotnet/Core/Database/Domain/Core`, holds `Extent()`
  and `FindBy` of the objects classes.
- `ExtentTest`, in `dotnet/System/Database/Adapters/Tests.Static/Static`, checks the predicates,
  the sorts and the operations on every database adapter; the SQL adapters have expectations of
  their own for the place of an empty role in a sort. `LifeCycleTest`, next to it, checks
  prefetching. No test uses an extent as the operand of two operations.
