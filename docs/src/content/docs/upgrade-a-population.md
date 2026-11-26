---
title: "Upgrade a population"
description: "Bring the population of an earlier version up to date with a new version of an application: write the changes, declare what the new version removed, save, upgrade and test."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

Use this guide when a new version of an application changes its model or its setup, and a
database that an earlier version populated has to follow. It assumes
[Populations](populations.md), which explains saving, loading and the setup.

The example is the [sample application](../../../../sample/). Its setup creates permissions and
the `Role` Worker, which an earlier version of the sample did not; its command `upgrade` adds them
to a population without them. The sample's test of the command builds such a population by
deleting them before it saves.

## How the command works

The command `upgrade` of the sample's [command-line tool](command-line-tool.md), in
`sample/Database/Commands/Custom/Upgrade.cs`, runs these steps:

1. **Load.** It loads the file that the option `-f` names, or else the file of the setting
   `populationFile`, or else `population.xml`. The load [starts with `Init()`](populations.md#saving-and-loading):
   the database loses its population first.
2. **Accept what the new version removed.** The load leaves out the objects of the classes, and
   the relations of the relation types, that the new model does not have, and reports them. The
   command accepts the omissions that concern the classes in `ExcludedObjectTypes` and the
   relation types in `ExcludedRelationTypes`: the objects of those classes, their relations, and
   those objects in the roles of the objects that the load keeps. A class or a relation type that
   the new model still has loads as usual, whether it is in these sets or not.
3. **Stop at any other omission that the load reports.** The command logs the ids of the classes
   and the relation types concerned, empties the database with `Init()` and exits with code 1.
4. **Change the data.** In one transaction it creates the permissions of the new model with
   `Sync`, runs `Upgrade.Execute()` of the domain, derives and commits.
5. **Grant again.** It runs the [security setup](security.md#permissions), so that every `Role`
   gets its permissions again, the new ones included, and commits.

## 1. Write the changes of the data

Write what the new version needs in `Execute` of the domain class `Upgrade`, in the folder
`Custom` of the domain project. For the setup's objects, call the merges of the setup, which
find what exists and create what is missing.
`sample/Database/Domain/Custom/Upgrade.cs`:

```csharp
public void Execute()
{
    // A population that an earlier version saved can lack the Role Worker, the user group Workers
    // and their grant on the default security token, which the setup creates. The merges of the setup create
    // what is missing, and leave the members of the user group as they are.
    new Roles(this.Transaction).MergeWorker();
    new UserGroups(this.Transaction).MergeWorkers();
    var workers = new Grants(this.Transaction).MergeWorkers();
    new SecurityTokens(this.Transaction).DefaultSecurityToken.AddGrant(workers);
}
```

| The new version | What to do |
| --- | --- |
| adds a class, a method type or a role that can start empty | No data change is needed in `Execute`. `Sync` creates the permissions. |
| adds a derived role whose value depends on existing data, or changes how a rule derives one | Compute its value for each affected object in `Execute`. The load finishes before the upgrade transaction starts, so unchanged inputs in the saved population do not trigger the rule. |
| requires a role that existing objects have no value for | Give every existing object a value in `Execute`. The derivation checks the required roles only of the objects that the upgrade changes. |
| creates an object in its setup | Merge the object in `Execute`, with the merge of the setup; [Grant access](grant-access.md#2-create-the-role-and-the-user-group) shows such merges. |
| gives a `Role` other permissions | Nothing: the command runs the security setup. |
| removes a class or a relation type | Declare it, as the next step shows. |

## 2. Declare what the new version removed

Add the id of every class and every relation type that the new version removed on purpose to
the command, in `sample/Database/Commands/Custom/Upgrade.cs`:

```csharp
public HashSet<Guid> ExcludedObjectTypes { get; } = new HashSet<Guid>
{
};

public HashSet<Guid> ExcludedRelationTypes { get; } = new HashSet<Guid>
{
};
```

The ids are those of the `[Id]` attributes that the earlier model declared. The command stops at
any other omission that the load reports, so a class or a relation type that the new model lost
by mistake does not pass for an upgrade. The load does not report every change, though:
[Populations](populations.md#a-file-of-an-earlier-model) says what it lets pass.

## 3. Save the population with the running version

Before the final save, stop accepting writes and let active transactions finish. Keep all
programs that write to the database stopped until the upgrade succeeds. Any change committed
after the save will be lost when the upgrade loads the file.

Use the command-line tool of the deployed version. The population has to outlive the process, so
use SQL Server or PostgreSQL. For the sample, generate that version in its own clone and save to
a new file outside the clone, here on PostgreSQL:

```sh
./build.sh SampleGenerate
export adapter=npgsql
export ConnectionStrings__DefaultConnection='Host=localhost;Database=sample;Username=…;Password=…'
dotnet run --project sample/Database/Commands -- save -f ~/population.xml
```

Check that `save` exits with code 0 before running `upgrade`. A failed save can leave a partial
file, and the upgrade empties the database before reading it. Keep the file until the upgraded
application runs.

## 4. Upgrade with the new version

Stop any remaining programs that read the database. In a clone at the new version, generate the
sample again and run `upgrade` with the same settings on the saved file:

```sh
./build.sh SampleGenerate
dotnet run --project sample/Database/Commands -- upgrade -f ~/population.xml
```

The load gives the database the tables of the new model. The command logs `Begin`, the file that
it loads and `End`, and exits with code 0. Then start the new version.

### A database with the table names of an earlier version

On SQL Server and PostgreSQL the adapter names its tables, columns, keys and indexes by the ids of
the model: the table of a class is `c_` followed by the id of the class, the table of a relation
type is `r_` followed by the id of the relation type, and a column is `col_` followed by the id of
its relation type. A version of the platform from before this naming built its tables under the
names of the classes and the roles, such as `person`.

The new version does not use those tables. Of its own tables it finds only the objects table in
such a database, which both versions name `_o`, and a program that opens a transaction fails with
`Missing Tables:`, followed by the other tables that it looks for, each with its class or its
relation type. Upgrade into a new database instead:

1. Save the population with the running version, as
   [step 3](#3-save-the-population-with-the-running-version) describes.
2. Create a new, empty database on the server.
3. Set `ConnectionStrings__DefaultConnection` to the new database and run `upgrade` with the new
   version on the saved file.

The database of the earlier version stays as it was, so the earlier version can still use it.

Do not run `upgrade` on the database of the earlier version. The load succeeds there, and leaves
the database with the tables of both versions: `Init()` builds the tables of the new version next
to the earlier ones, which keep their rows, and it drops and creates the objects table, which
both versions use. Each version then finds its tables in that database, and the earlier version
would read the rows of its own tables with the object versions that the new version writes.

## 5. When the upgrade stops

The command exits with code 1 whenever it fails, and logs the message of the error. What the
database then holds depends on where the command stopped:

- **Before the load,** at a missing file or a missing setting: the database is unchanged.
- **During the load,** at a file that the load refuses: the database is empty. On SQL Server and
  PostgreSQL a failure to build the indexes after the load leaves the loaded rows.
- **At an omission that is not declared:** the command logs which classes or relation types the
  file holds that the new version could not load, and leaves the database empty. Declare what the
  new version removed on purpose, or correct the model.
- **At an error of the derivation after `Execute`:** `Derive()` throws, and nothing of the
  transaction of step 4 of [How the command works](#how-the-command-works) is committed. The
  database holds the population as the load left it, without the changes of the upgrade, the new
  permissions or the security setup. Correct `Execute`.
- **In the security setup:** the changes of `Execute` and the new permissions are committed, and
  the security setup is not.

In each case, correct the cause and upgrade again from the same file: the upgrade starts by
emptying the database. Start the application only after an upgrade that exits with code 0.

## 6. Test the upgrade

The sample tests its command in `sample/Database/Commands.Tests/UpgradeTests.cs`. The tests run
the tool's commands in one process, on the Snapshot adapter, which they set in the environment.
One populates a database with one `Program`, removes what an earlier version did not have, saves,
and upgrades with a second `Program`; then it checks the objects, the setup that the upgrade
added and what a worker may do. Another checks that the upgrade keeps the members of Workers. The
others change the saved file: they check that the upgrade leaves out the objects of a declared
class, and that it stops, leaving the database as `Init()` leaves it, at four kinds of omission
that are not declared.

```sh
./build.sh SampleGenerate
dotnet test sample/Database/Commands.Tests
```

## Where this is implemented

- The command is in `sample/Database/Commands/Custom/Upgrade.cs`, the domain class `Upgrade` in
  `sample/Database/Domain/Custom/Upgrade.cs`, and the tests in
  `sample/Database/Commands.Tests/UpgradeTests.cs`.
- Core's command-line tool for its test domain has an `upgrade` of its own, in
  `dotnet/Core/Database/Commands/Custom/Upgrade.cs`. An application does not include it, and it
  differs from the sample's: issues [#424](https://github.com/allors/allors3/issues/424) and
  [#425](https://github.com/allors/allors3/issues/425) record two of the differences. It also
  commits the changes of its `Upgrade` without deriving them, and does not read the setting
  `populationFile`.
- The events of the load are described in [Populations](populations.md#a-file-of-an-earlier-model).
- The SQL adapters take the names of their tables, columns, keys and indexes from `Mapping`, in
  `dotnet/System/Database/Adapters/Allors.Database.Adapters.Sql/Schema`. The fact
  `TheNamesOfTheSchemaCarryTheIdsOfTheModel` of `IndexTest`, in
  `dotnet/System/Database/Adapters/Tests.Static`, writes the names out.
