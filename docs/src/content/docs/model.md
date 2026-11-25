---
title: "The model"
description: "What a repository project declares: classes, interfaces, units, relations and their multiplicity, methods, ids, and the attributes with their defaults."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code
> and the tests.

The model of an application is declared in C#, in its repository project. Generators read the
declarations and produce the metadata and the typed classes that the application compiles. This
page lists what a declaration can say. [Domains](domains.md) explains how the model of an
application builds on Core, and [Objects and relations](objects-and-relations.md) explains how the
generated classes behave.

Unless a section says otherwise, the examples come from the platform's test domain, in
`dotnet/Core/Repository/Domain/Custom`.

## The repository project

A repository project only declares. Its properties hold no logic and its methods have empty
bodies: the generator compiles the project and reads its types, and nothing ever calls them.

- A class or an interface belongs to the model when it is in the namespace `Allors.Repository` and
  carries an `[Id]`.
- A file declares one class or one interface.
- A declaration belongs to the domain whose folder holds its file, as
  [Declare a domain](domains.md#declare-a-domain) describes.
- The properties and the methods of the model are `public`.

## Types

| Type | Declared as | Meaning |
| --- | --- | --- |
| Class | `public partial class Organisation : Deletable, UniquelyIdentifiable` | The type of an object. A class implements interfaces; it does not inherit from another class. |
| Interface | `public partial interface Deletable : Object` | Relations and methods for every class that implements it. An interface can inherit from several interfaces. |
| Unit | One of the C# types of the table below | The type of a value: a text, a number, a date. The platform defines the units; a model does not add any. |

A class or an interface is a **composite**: a type with relations. The eight units are:

| Unit | C# type in a declaration |
| --- | --- |
| `Binary` | `byte[]` |
| `Boolean` | `bool` |
| `DateTime` | `DateTime` |
| `Decimal` | `decimal` |
| `Float` | `double` |
| `Integer` | `int` |
| `String` | `string` |
| `Unique` | `Guid` |

C# requires a class to implement the members of its interfaces. A class therefore states the
properties and the methods of its interfaces again, by convention in regions named
`inherited properties` and `inherited methods`. A copy carries no `[Id]`: the declaration in the
interface is the relation or the method. A copy of a property can carry `[Required]`, which makes
the role required for the objects of that class only; the test domain's `OverrideClass` does this
for `OverrideRequired`.

## Relations

A property of a class or of an interface declares a relation. These are four relations of
`Organisation`, shortened to what matters here:

```csharp
[Id("49b96f79-c33d-4847-8c64-d50a6adb4985")]
[Multiplicity(Multiplicity.OneToMany)]
public Person[] Employees { get; set; }

[Id("dbef262d-7184-4b98-8f1f-cf04e884bb92")]
[Multiplicity(Multiplicity.OneToOne)]
public Person Manager { get; set; }

[Id("845ff004-516f-4ad5-9870-3d0e966a9f7d")]
[Multiplicity(Multiplicity.ManyToOne)]
public Person Owner { get; set; }

[Id("2cc74901-cda5-4185-bcd8-d51c745a8437")]
[Size(256)]
public string Name { get; set; }
```

The property is the [role](terminology.md#relations) of the relation: its name is the name of the
role and its type is the type of the role. The type that declares the property is the type of the
association, the other end.

- A property with a unit type declares a **unit role**, which holds a value.
- A property with a class or an interface as its type declares a **composite role**, which holds
  an object. An array declares a role that holds several objects.

### Multiplicity

The multiplicity of a composite role names the association first and the role second.
`OneToMany` on `Organisation.Employees` says that one organization has many employees, and that a
person is an employee of one organization.

| `Multiplicity` | The object has | An object in the role belongs to | Property type |
| --- | --- | --- | --- |
| `OneToOne` | one object in the role | one object | `Person` |
| `OneToMany` | many objects in the role | one object | `Person[]` |
| `ManyToOne` | one object in the role | many objects | `Person` |
| `ManyToMany` | many objects in the role | many objects | `Person[]` |

Without `[Multiplicity]` a single property is `ManyToOne` and an array is `ManyToMany`. The
generator refuses a multiplicity that contradicts the type of its property.
[Objects and relations](objects-and-relations.md#what-a-multiplicity-maintains) describes what the
platform does to keep each multiplicity true.

### Names

The association of a relation is named after the type that declares it and after the role:
`OrganisationWhereManager` when an object belongs to one object, `OrganisationsWhereOwner`, with
the plural of the type, when it belongs to many.

The generator finds the singular of a role that holds several objects, and the plural of a type,
by the rules of English: `Employees` gives `AddEmployee`, and `Person` gives `People`. Use
`[Singular]` or `[Plural]` where those rules give the wrong word.

## Methods

A method of the model is declared as a method without parameters and without a result:

```csharp
[Id("1869873F-F2F0-4D03-A0F9-7DC73491C117")]
[Workspace(Default)]
public void JustDoIt() { }
```

An interface declares it without a body, as Core's `Deletable` declares `void Delete();`. The
behaviour of a method is written elsewhere, in the parts that the domains add to it;
[The behaviour](domains.md#the-behaviour) describes them.

## Ids

Every class, interface, relation and method has an id, a GUID in `[Id]`.

- Generate a new id for every new declaration, for example with `uuidgen` or with
  `[guid]::NewGuid()` in PowerShell. Do not copy one.
- An id is unique in the model. The generator reports two classes, interfaces or relations that
  share an id. An id that a method shares with another declaration is found later, when the
  metadata is built: that fails with an `ArgumentException` that names the id.
- The id, not the name, identifies a class or a relation in a saved population. Renaming a class
  or a role therefore keeps a saved population readable; changing an id does not.

Many declarations wrap their id in `#region Allors`. The region only folds the id away in an
editor.

## Attributes

The attributes are in the namespace `Allors.Repository.Attributes`.

| Attribute | On | Meaning | Without it |
| --- | --- | --- | --- |
| `[Domain("…")]` | A struct | Declares a domain and its id; see [Declare a domain](domains.md#declare-a-domain). | |
| `[Extends("Core")]` | The struct of a domain | Names the one domain that the domain extends. | The domain extends none, as Core does. |
| `[Id("…")]` | A class, an interface, a property, a method | The [id](#ids) of the declaration. | The generator refuses a relation or a method; a class or an interface is not part of the model. |
| `[Multiplicity(…)]` | A property with a composite type | The [multiplicity](#multiplicity) of the relation. | `ManyToOne`, or `ManyToMany` for an array. |
| `[Required]` | A property | A derivation reports an error when a new or changed object has nothing in the role; see [Required roles](derivations.md#required-roles). On the copy of an inherited property in a class, it holds for that class only. | The role is optional. |
| `[Derived]` | A property | Marks a role that code in the database maintains, usually a [rule](derivations.md). Access control never lets a client write it. Code in the database can still write it, and the attribute computes nothing. | Clients can write the role when their permissions allow it. |
| `[Indexed]` | A property | Asks the SQL adapters for an index on a unit role. A string or binary role without a size limit, or with a size above 4000, gets none. The SQL adapters index a composite role with or without the attribute. | No index on a unit role. |
| `[Unique]` | A property | Marks the role as unique: the platform passes `IsUnique` on to the metadata of the database and of the .NET workspace, and enforces nothing; issue [#404](https://github.com/allors/allors3/issues/404) tracks that. A rule checks uniqueness with `AssertIsUnique`; see [Errors](derivations.md#errors). | Not marked. |
| `[Size(256)]` | A string or binary property | The greatest length of the value. Every database adapter refuses a longer value when the role is set, with an `ArgumentException`, and the SQL adapters size the column with it. `-1` stands for no limit. | 256 for a string, no limit for a binary. |
| `[Precision(19)]` | A decimal property | The number of digits of the value, from 1 to 28. A value with more digits is refused when the role is set; it is not rounded. | 19. |
| `[Scale(2)]` | A decimal property | The number of digits after the decimal point, from 0 to the precision. A value with more decimals is refused, like one with too many digits. | 2. |
| `[MediaType("text/markdown")]` | A property | The media type of the content of the role. The platform passes it on to the metadata of the workspace and does not interpret it. | No media type. |
| `[Workspace]` | A class, a property, a method | Makes the declaration available to the workspaces with the given names, and so to the clients that use them: `[Workspace("X", "Y")]`. Without a name the workspace is `Default`. A relation reaches a workspace only when a class at each of its ends is available there. [The workspace](workspace.md) explains the workspaces. | The declaration exists in the database only. |
| `[Origin(Origin.Session)]` | A property | The role exists in a session of a workspace only. The database has no such role. | `Origin.Database`: the database holds the role. |
| `[Singular("…")]` | A property | The singular [name](#names) of a role that holds several objects. | Derived from the name of the property. |
| `[Plural("…")]` | A class, an interface, a property | The plural name of the type, or of a role that holds one object. | Derived from the singular name. |
| `[Relationship]` | A class, an interface | Marks the type as a relationship; the subtypes of an interface inherit the mark. The platform passes it on to the metadata of the TypeScript workspace and does not use it otherwise. | Not marked. |

The model is refused when a decimal role has a precision or a scale outside the ranges of the
table.

## Add to a type of the domain you extend

A domain can add relations and methods to a class or an interface of the domain that it extends.
Declare the type again as `partial`, in a file below the folder of your own domain, with the new
members only. The platform's repository holds no such declaration; this example shows the form
for a domain that adds a description to Core's class `UserGroup`:

```csharp
namespace Allors.Repository
{
    using Attributes;

    public partial class UserGroup
    {
        [Id("213cad50-c329-46fe-9e68-9c5397ca76f5")]
        [Size(256)]
        public string Description { get; set; }
    }
}
```

The id of the type stays with its first declaration. When you add a member to an interface, every
class that implements the interface states it again, as for any member of an interface.

## What the generator refuses

Generate after every change to a declaration; [Generated code](domains.md#generated-code) says
what that produces. The generator stops with an error for these mistakes:

| Message | Cause |
| --- | --- |
| `Organisation.Employees should be many`, `… should be one` | `[Multiplicity]` contradicts the type of the property: an array with `OneToOne` or `ManyToOne`, or a single object with `OneToMany` or `ManyToMany`. |
| `… has a duplicate id: …` | Two classes, interfaces or relations share an id. |
| `… has a non GUID id: …` | A relation has no `[Id]`, or its id is not a GUID. |
| `… has no Id attribute.` | A method has no `[Id]`. |
| `… should be public` | A property of the model is not `public`. |
| `Sequence contains more than one matching element` | A file declares more than one class or interface. |

The generator of the typed classes then validates the metadata, and so does the `ObjectFactory`
that a program creates for its database. Both refuse two types with one name, and two relations
with one name: the name of a relation is the name of the type that declares it, followed by the
singular name of its role.

## Where this is implemented

- The attributes are in `dotnet/System/Repository/Attributes`. `Multiplicity` and `Origin` are in
  `dotnet/System/Shared`.
- `Repository`, `Property` and `Composite`, in
  `dotnet/System/Repository/Allors.Repository/Domain`, read the declarations: the units, the
  domain of a file, the default multiplicity, the names and the errors of the table above. The
  template `dotnet/System/Repository/Templates/meta.cs.stg` writes the metadata from them.
- `RoleType`, `RelationType` and `Class`, in
  `dotnet/System/Database/Allors.Database.Meta.Props/Props`, hold the defaults of size, precision
  and scale, the check of precision and scale, the workspaces of a relation and the required
  roles of a class.
- `RoleTypeExtensions`, in `dotnet/System/Database/Adapters/Allors.Database.Adapters`, refuses a
  value that does not fit its size, precision or scale. `Indexes`, in
  `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Schema`, decides which relations
  the SQL adapters index. `DatabaseAccessControlList` and `WorkspaceAccessControlList`, below
  `dotnet/Core/Database/Domain/Core/Security/AccessControl`, refuse to write a derived role.
- `MetaBuilderTests`, in `dotnet/Core/Database/Meta.Tests`, builds the metadata of Core with its
  test domain. Among the shared adapter tests in
  `dotnet/System/Database/Adapters/Tests.Static/Static`, `SerializationTest` checks that a wrong
  precision or scale is refused, and `UnitTest` checks the values that a size, a precision or a
  scale refuses. No test runs the repository generator on a wrong declaration, and no test adds
  a member to a type from another domain.
