---
title: "Populations"
description: "How a database gets its population: initialization, the setup and its hooks, saving and loading a population, and what a load does with a file of an earlier model."
audience: "Users"
kind: "Explanation"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

A population is the objects and relations that a database holds. A database gets its first
population from the setup, which creates the objects that every database of the application
needs. Saving writes a population to a file, and loading replaces the population of a database
with the one in a file. This page explains these steps and what an application can rely on;
[Upgrade a population](upgrade-a-population.md) applies them to a new version of the model.

## Initialization

`IDatabase.Init()` empties a database and restarts object numbering. On SQL Server and PostgreSQL
it checks the tables against the model. On SQL Server it also checks the user-defined table types
that the adapter requires. If these checks pass, `Init()` empties the model's tables. If a required
table or table type is missing or invalid, it drops and recreates the model's tables. Tables that
the model no longer has are left as they are.

On SQL Server the rebuild first drops every user-defined table type in the adapter's schema,
`allors` by default, including any that the application created. If a stored procedure uses one
of these types, SQL Server refuses to drop it and the rebuild stops before it reaches the tables.
After rebuilding the tables, the adapter creates only the table types it requires.

## A new population

The setup builds a new population in an empty database. The sample's domain tests and the command
`populate` of its [command-line tool](command-line-tool.md) run it this way:

```csharp
database.Init();
new Setup(database, new Config()).Apply();
```

`Setup.Apply()` works in one transaction. The steps that name a hook call the parts of the
domains, Core's part first:

1. The hook `OnPrePrepare`.
2. The preparation of every objects class, such as `Roles` or `People`. By default an objects
   class registers itself for the setup. It can also declare that its setup needs that of another
   class first, with `setup.AddDependency`: Core's `Grants` needs `Role` and `UserGroup`, and
   Core's `SecurityTokens` needs `Grant`.
3. The hooks `OnPostPrepare` and `OnPreSetup`.
4. The setup of every registered objects class, each after the ones it depends on. An objects
   class runs Core's part, `CoreSetup`, then the application's part, `CustomSetup`, and derives.
5. The hook `OnPostSetup`.
6. A derivation, then the security setup when `Config.SetupSecurity` is true.
7. A last derivation, and the commit.

The hooks and the parts of the objects classes are methods named after the domain, such as
`CustomOnPostSetup` and `CustomSetup`; [Domains](domains.md#setup-and-security) explains how an
application calls them. [Security](security.md#what-cores-setup-creates) lists what Core's setup
creates, and [Grant access](grant-access.md) shows the setup of an application.

`Config` holds the choices of a setup:

| Member | Meaning |
| --- | --- |
| `SetupSecurity` | Whether the setup creates grants and runs the security setup. True by default; the base class of Core's domain tests sets it to false, and Core's tests of security set it back to true. |
| `DataPath` | A folder for the setup of an application to read. Core's setup does not read it. |
| `SetupAccounting` | Read by nothing in this version. |

### Merges

The setup creates the objects that the application refers to by name, such as the `Role`
Administrator, with a merge on their `UniqueId`. A merge finds the object with that id, or
creates it when the database has none, and then applies its action in both cases. The objects
class keeps the id in a static field, such as `Roles.AdministratorId`, and gives access to the
object, as the property `Administrator` of `Roles` does.
Because a merge finds what exists, the code that creates such objects in the setup can bring a
population of an earlier version up to date as well.

## Saving and loading

`IDatabase.Save(XmlWriter)` writes the population to XML, and `IDatabase.Load(XmlReader)` replaces
the population with the one in the XML. The commands `save` and `load` of the
[command-line tool](command-line-tool.md) call them. A file is written as one line. This is a
file of the sample application with one work item, indented and shortened:

```xml
<?xml version="1.0" encoding="utf-8"?>
<allors>
  <population version="1">
    <objects>
      <database>
        <!-- … -->
        <ot i="60065f5d-a3c2-4418-880d-1026ab607319">7:3,8:3,9:3,10:3</ot>
        <ot i="8e78ae27-8c39-4bb9-89c5-e5b4f27507c1">240:3</ot>
        <!-- … -->
      </database>
    </objects>
    <relations>
      <database>
        <rtc i="0dbbff5c-3dca-4257-b2da-442d263dcd86"><r a="11">8</r><!-- … --></rtc>
        <!-- … -->
        <rtu i="6992a861-6b69-4715-973a-366273674bfa"><r a="240">V3JpdGUgdGhlIHJ1bGU=</r></rtu>
        <!-- … -->
      </database>
    </relations>
  </population>
</allors>
```

The objects are grouped by the id of their class, as pairs of an object id and its version: here
the four `UserGroup` objects and the `WorkItem`. The relations are grouped by the id of their
relation type, in `rtc` for a composite role and `rtu` for a unit role. Each `r` gives the id of
the association and the role: the `Grant` 11 holds the `UserGroup` 8 in its role `SubjectGroups`,
and the title of the work item is the Base64 of the UTF-8 of "Write the rule".

- **A load starts with `Init()`.** The database loses its population before the load reads the
  file.
- **A load keeps the ids.** The objects keep the ids that the file gives them, and the next new
  object gets an id above the highest one that the load loaded.
- **A load that throws leaves the database empty.** The adapter calls `Init()` again. On SQL
  Server and PostgreSQL a load commits as it goes, and builds the indexes after the rows: a
  process that ends during a load leaves part of the file, and a failure to build the indexes
  leaves the rows.
- **The adapters share the format.** A file that one adapter saved loads into another.

A load into the same model reproduces the saved population, with the exceptions that issues
[#339](https://github.com/allors/allors3/issues/339) and
[#341](https://github.com/allors/allors3/issues/341) record, such as a role that two
transactions gave to two associations where the model allows one. Issue #339 also lists files
that the adapters load differently, and its findings wait for a redesign of save and load.

## A file of an earlier model

When the model changes, a file that an earlier version saved can hold objects of a class, or
relations of a relation type, that the model no longer has. The load reports them to two events
of the database:

| Event | Raised for |
| --- | --- |
| `ObjectNotLoaded` | An object that the load cannot load, such as one of a class that the model does not have. Its arguments give the id of the class and the id of the object. |
| `RelationNotLoaded` | A relation that the load cannot load, such as one of a relation type that the model does not have. Its arguments give the id of the relation type, the id of the association and the contents of the role. For a composite role the load raises one event for each object of the role, with the id of that object. |

Without a handler for these events the load fails, with an exception such as `Object not loaded:`
followed by the id of the class and the id of the object, and leaves the database empty. With
handlers attached, the load leaves those objects and relations out and loads the rest. The
command `upgrade` of an application decides with such handlers which omissions it accepts;
[Upgrade a population](upgrade-a-population.md) shows how.

A load does not report every change. Issue [#339](https://github.com/allors/allors3/issues/339)
lists what it lets pass without an event, such as a role with two associations where a changed
multiplicity allows one; which associations keep the role then differs between the adapters.

## Where a population lives

The Memory and the Snapshot adapters keep the population in the memory of the process: it ends
with the process. Each run of a command-line tool on the Snapshot adapter starts with an empty
database. SQL Server and PostgreSQL keep the population between runs. The setting `adapter` of
the [configuration](configuration.md#settings) selects the Snapshot, SqlClient or Npgsql adapter;
the sample's domain tests create a database on the Memory adapter themselves.

## Where this is implemented

- `Setup` and `Config` are in `dotnet/Core/Database/Domain/Core`, with the order of the setup in
  `ObjectsGraph`. The parts of the objects classes are in `ObjectsBase`, and the application's
  calls to them in its folder `Virtual`, as in `sample/Database/Domain/Virtual`.
- `Init`, `Save` and `Load` are declared on `IDatabase`, in
  `dotnet/System/Database/Allors.Database`, with `ObjectNotLoadedEventArgs` and
  `RelationNotLoadedEventArgs`. Each adapter implements them in
  `dotnet/System/Database/Adapters`. In the folder `Schema` of the SqlClient and Npgsql adapters,
  `Validation` checks the tables, and on SQL Server the table types, and `Initialization` empties
  or rebuilds them.
- `SerializationTest`, in `dotnet/System/Database/Adapters/Tests.Static`, holds the facts of save
  and load: `SchemaChanges` for files that a change of the model explains, and `CorruptFiles` for
  files that a load refuses.
