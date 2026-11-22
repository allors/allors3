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

`IDatabase.Init()` empties a database: afterwards it holds no objects, and the numbering of the
objects starts again. On SQL Server and PostgreSQL it also checks the tables against the model.
Tables that fit the model are emptied; otherwise the adapter creates the tables of the model
again.

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
| `SetupSecurity` | Whether the setup creates grants and runs the security setup. True by default; Core's own domain tests set it to false. |
| `DataPath` | A folder for the setup of an application to read. Core's setup does not read it. |
| `SetupAccounting` | Read by nothing in this version. |

### Merges

The setup creates the objects that the application refers to by name, such as the `Role`
Administrator, with a merge on their `UniqueId`. A merge finds the object with that id, or
creates it when the database has none, and then applies its action in both cases. The objects
class keeps the id as a constant and gives access to the object, as `Roles.Administrator` does.
Because a merge finds what exists, the code that creates such objects in the setup can bring a
population of an earlier version up to date as well.

## Saving and loading

`IDatabase.Save(XmlWriter)` writes the population to XML, and `IDatabase.Load(XmlReader)` replaces
the population with the one in the XML. The commands `save` and `load` of the
[command-line tool](command-line-tool.md) call them. A file starts like this, here shortened:

```xml
<allors>
  <population version="1">
    <objects>
      <database>
        <ot i="60065f5d-a3c2-4418-880d-1026ab607319">7:3,8:3,9:3,10:3</ot>
        <ot i="8e78ae27-8c39-4bb9-89c5-e5b4f27507c1">240:3</ot>
```

The objects are grouped by the id of their class, as pairs of an object id and its version. The
relations follow in the element `relations`, grouped by the id of their relation type.

- **A load starts with `Init()`.** The database loses its population before the load reads the
  file.
- **A load keeps the ids.** The objects keep the ids that the file gives them, and the next new
  object gets an id above the highest one.
- **A load is all or nothing.** When a load fails, the adapter calls `Init()` again, and the
  database is empty.
- **The adapters share the format.** A file that one adapter saved loads into another.

A load into the same model reproduces the saved population. The known limits of save and load,
such as files that no save writes, are recorded in issues
[#339](https://github.com/allors/allors3/issues/339) and
[#341](https://github.com/allors/allors3/issues/341), and wait for a redesign.

## A file of an earlier model

When the model changes, a file that an earlier version saved can hold objects of a class, or
relations of a relation type, that the model no longer has. The load reports them to two events
of the database:

| Event | Raised for |
| --- | --- |
| `ObjectNotLoaded` | An object that the load cannot load, such as one of a class that the model does not have. Its arguments give the id of the class and the id of the object. |
| `RelationNotLoaded` | A relation that the load cannot load, such as one of a relation type that the model does not have. Its arguments give the id of the relation type, the id of the association and the contents of the role. |

Without a handler for these events the load fails, with an exception such as `Object not loaded:`
followed by the id of the class and the id of the object, and leaves the database empty. With
handlers attached, the load leaves those objects and relations out and loads the rest. The
command `upgrade` of an application decides with such handlers which omissions it accepts;
[Upgrade a population](upgrade-a-population.md) shows how.

## Where a population lives

The Memory and the Snapshot adapters keep the population in the memory of the process: it ends
with the process. Each run of a command-line tool on the Snapshot adapter starts with an empty
database. SQL Server and PostgreSQL keep the population between runs. The setting `adapter` of
the [configuration](configuration.md#settings) selects the Snapshot, SqlClient or Npgsql adapter;
tests create a database on the Memory adapter themselves.

## Where this is implemented

- `Setup` and `Config` are in `dotnet/Core/Database/Domain/Core`, with the order of the setup in
  `ObjectsGraph`. The parts of the objects classes are in `ObjectsBase`, and the application's
  calls to them in its folder `Virtual`, as in `sample/Database/Domain/Virtual`.
- `Init`, `Save` and `Load` are declared on `IDatabase`, in
  `dotnet/System/Database/Allors.Database`, with `ObjectNotLoadedEventArgs` and
  `RelationNotLoadedEventArgs`. Each adapter implements them in
  `dotnet/System/Database/Adapters`.
- `SerializationTest`, in `dotnet/System/Database/Adapters/Tests.Static`, holds the facts of save
  and load: `SchemaChanges` for files that a change of the model explains, and `CorruptFiles` for
  files that a load refuses.
