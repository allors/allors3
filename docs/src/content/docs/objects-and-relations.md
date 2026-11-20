---
title: "Objects and relations"
description: "How code in the database creates, relates and deletes objects, what the platform maintains at both ends of a relation, and what a transaction and a change set hold."
audience: "Users"
kind: "Explanation"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code
> and the tests.

The platform owns the objects of an application and the relations between them. Code never
allocates an object of the model with `new`, and never keeps a relation in a field of its own: it
asks the platform, which stores the change and keeps the other end of the relation in step. This
page describes that behaviour for code that runs in the database, such as rules, methods and the
setup of a population. [The model](model.md) lists what the declarations behind it can say.

The examples use `Person` and `Organisation` of the platform's test domain, and a `transaction`
of its database. [Getting started](getting-started.md) shows how to run such code.

## A transaction holds the work

Every read and every change happens in a transaction, an `ITransaction` that the database creates
with `CreateTransaction()`. A transaction sees its own changes at once.

- `Commit()` makes the changes permanent and `Rollback()` discards them. After either one the
  same transaction continues: the objects that it holds stay usable, and an object that a
  rolled-back transaction created is deleted.
- Disposing a transaction rolls back what it did not commit.
- A commit stores the changes as they are. It runs no rules: call `Derive()` first, as
  [Derivations](derivations.md#derive-before-you-commit) explains.
- A transaction does not see the uncommitted work of another one. The Snapshot adapter takes its
  view when the transaction is created and after each commit or rollback. SqlClient and Npgsql
  take theirs at the transaction's first database operation after creation, commit or rollback;
  it can include changes committed by another transaction in the meantime. Once the view is
  taken, later commits by other transactions remain invisible until this transaction commits or
  rolls back.
- On those adapters a commit fails with a `DbException` when another transaction committed a
  change to an object that this transaction changed as well. Roll back and do the work again.
- The Memory adapter has one transaction: `CreateTransaction()` returns the same one every time.

## Objects

An object is an instance of a class of the model. Its class is fixed when it is created, and the
database gives it an `Id`, a positive number.

For every class the generator writes a typed class, a builder and an objects class, here
`Organisation`, `OrganisationBuilder` and `Organisations`. The objects class carries the plural
name of the class and gives access to all its objects; an interface has one as well, such as
`Users` for `User`. A builder is the usual way to create an object:

```csharp
var acme = new OrganisationBuilder(transaction).WithName("Acme").Build();
var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithLastName("Doe").Build();
```

`Build()` creates the object, gives it the values of the builder and then calls the methods
`OnBuild` and `OnPostBuild` of the object. The part of `OnPostBuild` that Core has for every
object gives a default value to every [required](model.md#attributes) unit role that has none:
`false` for a boolean, zero for a number, the current time for a date and a new GUID for a unique.
A required string or binary gets no default.

`transaction.Create<Organisation>()` creates an object without calling those methods, so its
required roles stay empty. `transaction.Build<Organisation>(v => v.Name = "Acme")` does call
them, like a builder.

`transaction.Instantiate(id)` returns the object with that id, or `null` when the database has
none. Two instances that stand for the same object are equal.

## Roles and associations

A relation has two ends. The [role](terminology.md#relations) is the end that code writes, on the
object that declares the relation. The association is the same relation read from the other end.
These are the relations between `Organisation` and `Person` that the examples use:

| Role, on `Organisation` | Association, on `Person` | Multiplicity |
| --- | --- | --- |
| `Employees` | `OrganisationWhereEmployee` | `OneToMany` |
| `Manager` | `OrganisationWhereManager` | `OneToOne` |
| `Owner` | `OrganisationsWhereOwner` | `ManyToOne` |
| `Shareholders` | `OrganisationsWhereShareholder` | `ManyToMany` |

Writing a role changes the association in the same statement:

```csharp
acme.AddEmployee(jane);

var employer = jane.OrganisationWhereEmployee;   // acme
```

An association has no setter. To change it, write the role at the other end.

### The generated members

| Declaration | Members of the typed class |
| --- | --- |
| A unit role, `Name` | `Name`, `ExistName`, `RemoveName()` |
| A role that holds one object, `Manager` | `Manager`, `ExistManager`, `RemoveManager()` |
| A role that holds several objects, `Employees` | `Employees`, `AddEmployee(…)`, `RemoveEmployee(…)`, `ExistEmployees`, `RemoveEmployees()` |
| An association to one object | `OrganisationWhereManager`, `ExistOrganisationWhereManager` |
| An association to several objects | `OrganisationsWhereOwner`, `ExistOrganisationsWhereOwner` |
| A method, `JustDoIt` | `JustDoIt()` |

Each typed class also has `Meta`, the metadata of its class, and `M`, the metadata of the whole
model. `M.Organisation.Name` is the role type of `Name`: the value that extents, rules and
prefetch policies take to name a relation. The typed members call the `Strategy` of the object,
which reads and writes any role by its role type.

### Unit roles

A unit role holds a value or nothing. `ExistName` says which; a role without a value reads as
`null`, and assigning `null` removes the value, as `RemoveName()` does. An empty string is a
value.

The typed property of a number, a boolean, a date or a unique is nullable, such as `int?` for
`Person.Age`, unless the role is required. The property of a required role is not nullable, and
reading it while the role is empty throws: `Build()` gives such a role its default, and
`Exist…` says whether it has a value.

A value has to fit the [size, precision and scale](model.md#attributes) of its role. Setting a
string or a binary that is longer than the size, or a decimal with more digits or more decimals
than the role allows, throws an `ArgumentException`; a decimal is not rounded.

A date is kept in UTC, to the millisecond. A local `DateTime` is converted, and one of the kind
`Unspecified` is refused, apart from `DateTime.MinValue` and `DateTime.MaxValue`.

### What a multiplicity maintains

The multiplicity of a relation is a promise that the platform keeps on every write.

| Multiplicity | Statement | What else changes |
| --- | --- | --- |
| `OneToMany` | `globex.AddEmployee(jane)` | Jane leaves the employees of the organization that had her. |
| `OneToOne` | `globex.Manager = jane` | Jane stops being the manager of the organization that had her, and the manager that `globex` had manages nothing. |
| `ManyToOne` | `globex.Owner = jane` | Nothing: Jane owns one organization more. The owner that `globex` had loses it. |
| `ManyToMany` | `globex.AddShareholder(jane)` | Nothing. |

```csharp
acme.AddEmployee(jane);
globex.AddEmployee(jane);

var employer = jane.OrganisationWhereEmployee;   // globex
var left = acme.ExistEmployees;                  // false
```

A role that holds several objects holds each of them once: adding an object again changes
nothing.

## Deleting

A class that implements Core's interface `Deletable` has the method `Delete()`. It runs the parts
that the domains wrote for the method and then deletes the object. A domain uses such a part to
delete what must not outlive the object. The test domain's `Cascader` deletes its `Cascaded` this
way; its part is shortened here to that statement:

```csharp
public partial class Cascader
{
    public void CustomDelete(DeletableDelete method) => this.Cascaded?.Delete();
}
```

`Strategy.Delete()` deletes any object and runs no parts.

When an object is deleted, the platform removes it from every role and every association, so no
other object refers to it. `Strategy.IsDeleted` is then `true`, and reading a role or an
association of the deleted object throws; for one that holds several objects, it throws when the
result is enumerated. A rollback brings the object and its relations back.

## Change sets

A transaction records what changes. `Checkpoint()` returns the record since the previous
checkpoint as a change set, an `IChangeSet`, and starts a new one.

| Member | Holds |
| --- | --- |
| `Created` | The objects that were created. |
| `Deleted` | The strategies of the objects that were deleted. |
| `Associations` | The objects of which a role changed, without the deleted ones. |
| `Roles` | The objects for which an association changed, without the deleted ones. |
| `RoleTypesByAssociation`, `AssociationTypesByRole` | For each of those objects, the role types or the association types that changed. |
| `AssociationsByRoleType`, `RolesByAssociationType` | The same changes, by role type and by association type. |

For roles and associations a change set holds differences, not statements. Assigning the value
that a role already has is no change, and neither is a change that is undone before the
checkpoint. A binary role is the exception on the Memory adapter and on the SQL adapters, which
compare the array and not its bytes: assigning another array with the same bytes is a change
there. An object that is created and deleted between two checkpoints is in both `Created` and
`Deleted`. A commit and a rollback start a new change set as well.

[Derivations](derivations.md) are built on change sets: each cycle of a derivation takes a
checkpoint and matches the changes against the rules.

## Where this is implemented

- `IDatabase`, `ITransaction`, `IObject`, `IStrategy` and `IChangeSet` are in
  `dotnet/System/Database/Allors.Database`. The adapters under
  `dotnet/System/Database/Adapters` implement the database, the transaction, the strategy and
  the change set; the typed classes implement `IObject`. `RoleTypeExtensions`, in
  `Allors.Database.Adapters` among the adapters, checks the values of unit roles.
- The template `dotnet/Core/Database/Templates/domain.cs.stg` writes the typed classes, the
  builders and the objects classes. `ObjectBuilder`, in `dotnet/Core/Database/Domain/Core`,
  and `ITransactionExtensions`, in its folder `Extensions`, hold `Build`; `ObjectExtensions`, in
  its folder `Common`, holds the defaults of `OnPostBuild`; and `DeletableDelete`, next to it,
  holds `Delete`.
- The shared adapter tests, in `dotnet/System/Database/Adapters/Tests.Static/Static`, run on
  every database adapter. `One2OneTest`, `One2ManyTest`, `Many2OneTest` and `Many2ManyTest`
  check what a multiplicity maintains, `UnitTest` the unit roles, their limits and the dates,
  `LifeCycleTest` creation, deletion, commit and rollback, `ChangesTest` the change sets, and
  `ConcurrencyTest` what transactions see of each other and the commit that fails. The Memory
  adapter takes no part in `ConcurrencyTest`.
- `RequiredTest` and `BuilderTest`, in `dotnet/Core/Database/Domain.Tests`, check the defaults
  of `OnPostBuild` and the builders. `DeletingTest`, next to them, runs `Delete()` on a
  `Cascader`; it does not assert that the `Cascaded` is deleted.
