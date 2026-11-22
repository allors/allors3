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

The example is the [sample application](../../../../sample/). An earlier version of the sample
had no permissions and no `Role` Worker; its command `upgrade` loads a population of that version
and adds what the setup of the current version creates.

## How the command works

The command `upgrade` of the sample's [command-line tool](command-line-tool.md), in
`sample/Database/Commands/Custom/Upgrade.cs`, runs these steps:

1. **Load.** It loads the file that the option `-f` names, or else the file of the setting
   `populationFile`, or else `population.xml`. The load [starts with `Init()`](populations.md#saving-and-loading):
   the database loses its population first.
2. **Leave out what the new version removed.** The load leaves out the objects of the classes in
   `ExcludedObjectTypes`, the relations of the relation types in `ExcludedRelationTypes`, the
   relations of the objects that it left out, and those objects in the roles of the objects that
   it loads.
3. **Stop at anything else.** When the load leaves out anything else, the command logs the ids of
   the classes and the relation types concerned, empties the database with `Init()` and exits
   with code 1.
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
| adds a class, a relation type or a method type | Nothing: the new roles start empty, and `Sync` creates the permissions. |
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
an object or a relation that the load leaves out for any other reason, so a mistake in the new
model does not pass for an upgrade.

## 3. Save the population with the running version

Run `save` with the command-line tool of the version that the database runs now. The population
has to outlive the process, so use SQL Server or PostgreSQL. In a clone of the platform's
repository at that version:

```sh
export adapter=npgsql
export ConnectionStrings__DefaultConnection='Host=localhost;Database=sample;Username=…;Password=…'
dotnet run --project sample/Database/Commands -- save -f population.xml
```

The file is the only copy of the population from here on: the upgrade empties the database
before it loads the file. Keep it until the upgraded application runs.

## 4. Upgrade with the new version

Stop the programs that use the database, and run `upgrade` with the command-line tool of the new
version, with the same settings, on the saved file:

```sh
dotnet run --project sample/Database/Commands -- upgrade -f population.xml
```

The load gives the database the tables of the new model. The command logs `Begin`, the file that
it loads and `End`, and exits with code 0. Then start the new version.

## 5. When the upgrade stops

The command exits with code 1 in two cases:

- **The load left out something that is not declared.** The command logs which classes or
  relation types the file holds that the new version could not load, and leaves the database
  empty. Declare what the new version removed on purpose, or correct the model, and upgrade
  again from the same file.
- **The derivation after `Execute` reported an error.** `Derive()` throws, and nothing of the
  transaction of step 4 of [How the command works](#how-the-command-works) is committed: the
  database holds the population as the load left it, without the changes of the upgrade, the new
  permissions or the security setup. Correct `Execute` and upgrade again from the same file.

Start the application only after an upgrade that exits with code 0.

## 6. Test the upgrade

The sample tests its command in `sample/Database/Commands.Tests/UpgradeTests.cs`. A test runs the
tool's commands in one process, on the Snapshot adapter, which it sets in the environment: it
populates a database with one `Program`, removes what an earlier version did not have, saves,
and upgrades with a second `Program`. Then it checks the objects, the setup that the upgrade
added and what a worker may do. Other tests change the saved file: they check that the upgrade
leaves out the objects of a declared class, and that it stops, leaving the database as `Init()`
leaves it, at each kind of omission that is not declared.

```sh
dotnet test sample/Database/Commands.Tests
```

## Where this is implemented

- The command is in `sample/Database/Commands/Custom/Upgrade.cs`, the domain class `Upgrade` in
  `sample/Database/Domain/Custom/Upgrade.cs`, and the tests in
  `sample/Database/Commands.Tests/UpgradeTests.cs`.
- Core's command-line tool for its test domain has an `upgrade` of its own, in
  `dotnet/Core/Database/Commands/Custom/Upgrade.cs`. An application does not include it, and it
  differs from the sample's: issues [#424](https://github.com/allors/allors3/issues/424) and
  [#425](https://github.com/allors/allors3/issues/425) record how.
- The events of the load are described in [Populations](populations.md#a-file-of-an-earlier-model).
