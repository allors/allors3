---
title: "Grant access"
description: "Create the permissions in the setup, give a user group a set of permissions through a grant on a security token, and test what a user may do."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

Use this guide to let a group of users read and change what their work needs. It assumes
[Security](security.md), which explains how Core decides access.

The example is the [sample application](../../../../sample/): the members of the `UserGroup`
Workers have the `Role` Worker, and may read work items, mark them done and read the names of
people. The steps write the setup of the application's domain, in the folder `Custom` of its
domain project.

## 1. Create the permissions

Core's setup creates no permissions, and the security setup can only grant permissions that
exist. Call `Sync` of `IPermissions` in the hook that follows the setup of the objects classes,
`CustomOnPostSetup` of `Setup`, which runs before the security setup. The sample does this in
`sample/Database/Domain/Custom/Setup.cs`:

```csharp
private void CustomOnPostSetup(Config config)
{
    // Core's setup creates no permissions. The security setup that follows can only grant permissions
    // that exist, and the security service of the database cannot be created without them.
    if (config.SetupSecurity)
    {
        this.transaction.Database.Services.Get<IPermissions>().Sync(this.transaction);
    }
}
```

`IPermissions` is in the namespace `Allors.Database.Services`; the file has `using Services;`.

## 2. Create the `Role` and the user group

Give each object that the setup creates a unique id of its own, and merge it in the setup of its
objects class. A merge finds the object by its `UniqueId` and creates it when the database does
not have it; then it applies the action to the object, whether it found the object or created it.
`sample/Database/Domain/Custom/Security/Roles.cs`:

```csharp
public partial class Roles
{
    public static readonly Guid WorkerId = new Guid("de35c400-e90f-47a8-8488-8773b2fb0e33");

    public Role Worker => this.Cache[WorkerId];

    // A merge finds the Role by its unique id, or creates it when the database has none. The setup
    // runs it, and so does the upgrade, for a population that an earlier version saved.
    public Role MergeWorker() => this.Cache.Merger().Action()(WorkerId, v => v.Name = "Worker");

    protected override void CustomSetup(Setup setup) => this.MergeWorker();
}
```

`sample/Database/Domain/Custom/Security/UserGroups.cs` does the same for the user group:

```csharp
public partial class UserGroups
{
    public static readonly Guid WorkersId = new Guid("03910bbe-2843-47b6-bc80-ae1176bbba38");

    public UserGroup Workers => this.Cache[WorkersId];

    // The merge leaves the members of a user group that the database has as they are.
    public UserGroup MergeWorkers() => this.Cache.Merger().Action()(WorkersId, v => v.Name = "Workers");

    protected override void CustomSetup(Setup setup) => this.MergeWorkers();
}
```

`CustomSetup` is the part of the application's domain in the setup of the objects class; it runs
after Core's part. A merge in its own method, such as `MergeWorker`, can serve the
[upgrade](upgrade-a-population.md) as well.

## 3. Create the grant

The `Grant` gives the `Role` to the user group. `sample/Database/Domain/Custom/Security/Grants.cs`:

```csharp
public partial class Grants
{
    public static readonly Guid WorkersId = new Guid("5d168fec-21f6-4b02-a235-b916eca8b7dc");

    public Grant Workers => this.Cache[WorkersId];

    // The Role and the user group of the grant have to exist before the merge.
    public Grant MergeWorkers() => this.Cache.Merger().Action()(WorkersId, v =>
    {
        v.Role = new Roles(this.Transaction).Worker;
        v.AddSubjectGroup(new UserGroups(this.Transaction).Workers);
    });

    // Core makes the setup of grants run after the setup of Roles and user groups.
    protected override void CustomSetup(Setup setup)
    {
        if (setup.Config.SetupSecurity)
        {
            this.MergeWorkers();
        }
    }
}
```

Core's `Grants` declares that dependency in its `CorePrepare`, with `setup.AddDependency`. Add
users who need the `Role` without a user group to the role `Subjects` of a `Grant` instead.

## 4. Put the grant on a security token

A `Grant` applies to the objects that use a security token that holds it. The default security
token applies to every object without security tokens of its own, so the sample adds its grant
there. `sample/Database/Domain/Custom/Security/SecurityTokens.cs`:

```csharp
public partial class SecurityTokens
{
    // An object without security tokens of its own takes its grants from the default security token,
    // and while it is new in a transaction from the initial security token. Core makes the setup of
    // security tokens run after the setup of grants.
    protected override void CustomSetup(Setup setup)
    {
        if (setup.Config.SetupSecurity)
        {
            this.DefaultSecurityToken.AddGrant(new Grants(this.Transaction).Workers);
        }
    }
}
```

To restrict the grant of Workers to selected objects, put it on a `SecurityToken` of your own
instead of the default security token. Give that security token a unique id and a merge, as
step 2 does for the `Role`, so that the setup and the upgrade find the same object. Then change
the two places that add the grant to the default security token: `CustomSetup` above, and
`Upgrade.Execute()`, which
[Upgrade a population](upgrade-a-population.md#1-write-the-changes-of-the-data) shows. In the
upgrade, also call `RemoveGrant(workers)` on the default security token, which holds the grant in
a population that the sample's setup created.

Add your security token to the role `SecurityTokens` of the selected objects, in the upgrade too
for the objects that exist. An object with security tokens of its own no longer uses the default
security token. Add that one as well when the object should keep the default security token's
other grants, such as the grant of the administrators.

## 5. Give the `Role` its permissions

Grant the permissions in the security hook `CustomOnPostSetup` of `Security`.
`sample/Database/Domain/Custom/Security.cs`:

```csharp
// Core grants no permissions: an application gives each Role what its members may do.
private void CustomOnPostSetup()
{
    var m = this.transaction.Database.Services.Get<MetaPopulation>();

    var full = new[] { Operations.Read, Operations.Write, Operations.Execute };
    foreach (ObjectType @class in m.DatabaseClasses)
    {
        this.GrantAdministrator(@class, full);
    }

    // A worker reads work items and marks them done, and reads the names of people.
    this.Grant(Roles.WorkerId, m.WorkItem, Operations.Read);
    this.Grant(Roles.WorkerId, m.WorkItem, m.WorkItem.Done, Operations.Write);
    this.Grant(Roles.WorkerId, m.Person, m.Person.FirstName, Operations.Read);
    this.Grant(Roles.WorkerId, m.Person, m.Person.LastName, Operations.Read);
}
```

| Method | Adds to the `Role` |
| --- | --- |
| `Grant(roleId, class, operations)` | For `Read` or `Write`, the permissions for every role type of the class with `Origin.Database`. For `Execute`, the permissions for every method type of the class. |
| `Grant(roleId, class, roleType or methodType, operations)` | The permission for that role type or method type when the permission exists. |
| `GrantExcept(roleId, class, excepts, operations)` | The permissions of the first form, without those of the role types and method types in `excepts`. |
| `GrantAdministrator`, `GrantCreator`, `GrantGuest`, `GrantGuestCreator` and `GrantOwner` | The first form, for the `Role` of Core with that name. |

- **Name the operations.** A call without operations grants nothing. The operations are `Read`,
  `Write` and `Execute`; for an existing `Role`, `Operations.Create` throws an exception.
- **Check the id of the `Role`.** A call with the id of a `Role` that the database does not have
  does nothing and reports nothing.
- **Grant during the security setup.** Grant in a security hook, such as `CustomOnPostSetup` of
  `Security`, or in `CustomSecure` of an objects class. The security setup starts by removing the
  permissions of every `Role`, so a permission given anywhere else is gone after the next
  `Security.Apply()`.
- **A derived role stays read-only.** The access control list never allows writing it, whatever
  the `Role` has, as the sample's Administrator shows for `OpenWorkItemCount`.

## 6. Test the access

Add a user to the user group with `AddMember`, derive and commit, and ask the access control list
of an object. `sample/Database/Domain.Tests/SecurityTests.cs` holds this test and the method that
builds the list:

```csharp
[Fact]
public void AWorkerCanReadAWorkItemAndMarkItDone()
{
    var john = new PersonBuilder(this.Transaction).WithUserName("john@example.com").Build();
    new UserGroups(this.Transaction).Workers.AddMember(john);
    var workItem = new WorkItemBuilder(this.Transaction).WithTitle("Write the rule").WithAssignee(john).Build();
    this.Transaction.Derive();
    this.Transaction.Commit();

    var onTheWorkItem = this.AccessControl(john)[workItem];
    Assert.True(onTheWorkItem.CanRead(this.M.WorkItem.Title));
    Assert.True(onTheWorkItem.CanWrite(this.M.WorkItem.Done));
    Assert.False(onTheWorkItem.CanWrite(this.M.WorkItem.Title));
    Assert.False(onTheWorkItem.CanWrite(this.M.WorkItem.Assignee));
}
```

```csharp
private DatabaseAccessControl AccessControl(User user) => new DatabaseAccessControl(this.Transaction.Database.Services.Get<ISecurity>(), user);
```

The commit matters for two reasons. Before it, the work item is new in the transaction and takes
its grants from the initial security token, which does not hold the grant of Workers. And the
security service sees changes to grants, user groups and security tokens reliably only once they
are committed, as [Security](security.md#how-access-is-decided) explains. The test class inherits
from `DomainTest`, whose setup runs with `new Config()`, so with security. Run the tests from the
root of the platform's repository; the sample's generated code comes first:

```sh
./build.sh SampleGenerate
dotnet test sample/Database/Domain.Tests --filter "FullyQualifiedName~SecurityTests"
```

## 7. Bring existing databases up to date

The setup runs when a database gets a new population. A database that an earlier version
populated does not have the new `Role`, user group and grant, nor the permissions of new classes.
[Upgrade a population](upgrade-a-population.md) adds them, with the same merges.

## When access is not what you expect

| Check | Why |
| --- | --- |
| The setup calls `Sync` before the security setup. | Without permissions the security setup grants nothing, and the security service cannot be created. |
| The rules list has `GrantEffectiveUsersRule`, `GrantEffectivePermissionsRule` and `SecurityTokenSecurityStampRule`. | They derive the users and the permissions of a `Grant` and renew its security tokens; a rule outside the list never runs. |
| The changes to grants, user groups and security tokens are committed, and the access control list was built after them. | The security service caches them by the version of their objects, which changes at a commit, and a list keeps what it found. |
| A security token that the object uses holds the grant. | A `Grant` applies only through a security token. |
| The object has no security tokens of its own, or they include the default one. | Security tokens of its own replace the default security token. |
| The object is not new in the transaction. | A new object without security tokens of its own uses the initial security token until the transaction commits. |
| The `Grant` call names the operations and an existing `Role`. | Without operations, or with an unknown id, it grants nothing and reports nothing. |
| No revocation of the object denies the permission. | A revocation applies to every user. |
| The role is not derived. | A derived role is never writable. |

## Where this is implemented

- The sample's setup, security and their tests are in `sample/Database/Domain/Custom`,
  `sample/Database/Domain/Custom/Security` and `sample/Database/Domain.Tests/SecurityTests.cs`.
- The methods `Grant` and the security setup, `Security.Apply()`, are in
  `dotnet/Core/Database/Domain/Core/Security/Security.cs`. `Setup.Apply()`, in
  `dotnet/Core/Database/Domain/Core/Setup.cs`, runs the security setup after `OnPostSetup`.
- The merge is `Cache.Merger().Action()`, in `dotnet/Core/Database/Domain/Core/Services/Caches`.
- `Sync` is in `dotnet/Core/Database/Configuration/Core/Database/Permissions/Permissions.cs`.
