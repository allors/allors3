---
title: "Write a rule"
description: "Declare a derived role, write the rule that maintains it, register the rule and test it."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code
> and the tests.

Use this guide to let the platform maintain a role that follows from other roles. It assumes
[Derivations](derivations.md), which explains when a rule runs.

The example is a rule of the platform's test domain: the full name of a `Person`,
`DomainFullName`, follows from `FirstName` and `LastName`.

## 1. Declare the derived role

Declare the role in the repository project, on the class or the interface that has it, with an
[id](model.md#ids) of its own and with `[Derived]`:

```csharp
#region Allors
[Id("105CF367-F076-45F8-8E2A-2431BB2D65C7")]
[Size(256)]
#endregion
[Workspace(Default)]
[Derived]
public string DomainFullName { get; set; }
```

Generate, so that the typed class and the metadata have the role. In the platform's repository
that is `./build.sh Generate`.

## 2. Write the rule

Write a class that inherits from `Rule`, in the folder of your domain in the domain project:

```csharp
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class PersonFullNameRule : Rule
    {
        public PersonFullNameRule(MetaPopulation m) : base(m, new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern<Meta.Person>(m.Person, v => v.FirstName),
                m.Person.RolePattern(v => v.LastName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainFullName = $"{person.FirstName} {person.LastName}";
            }
        }
    }
}
```

- **The constructor** passes the metadata and an id to `Rule`. Give every rule a new GUID.
- **The patterns** name every role and every association that `Derive` reads. Here that is the
  first name and the last name of a person; [Patterns](derivations.md#patterns) lists the kinds.
- **`Derive`** receives the objects that matched and writes the derived role of each. Compute the
  value from what the object holds now, and write it every time: writing the value that a role
  already has is [no change](objects-and-relations.md#change-sets).

A rule is created once and serves every transaction. Keep no state in its fields.

## 3. Register the rule

Add the rule to the list of rules that the programs give to the derivation engine. In the
platform's repository that list is `Rules.Create`, in `dotnet/Core/Database/Domain/Rules.cs`:

```csharp
public static Rule[] Create(MetaPopulation m) =>
    new Rule[]
    {
        // Core
        new UserNormalizedUserNameRule(m),
        new UserNormalizedUserEmailRule(m),
        new UserInUserPasswordRule(m),
        new GrantEffectiveUsersRule(m),
        new GrantEffectivePermissionsRule(m),
        new SecurityTokenSecurityStampRule(m),

        // Custom
        new PersonFullNameRule(m),
        // …
    };
```

A rule that is not in the list never runs, and nothing reports that it is missing.

`Rules.cs` is at the root of the domain project, outside the folders `Core` and `Custom`, so the
projects of an application do not [include it](domains.md#how-an-application-includes-core). An
application has a list of its own. That list names the six rules of Core, which are in
`dotnet/Core/Database/Domain/Core/Rules`, next to the rules of the application.

## 4. Test the rule

Create the objects, derive and compare. This is the test of the example, a domain test of the
platform:

```csharp
public class DomainDerivationTest : DomainTest, IClassFixture<Fixture>
{
    public DomainDerivationTest(Fixture fixture) : base(fixture) { }

    [Fact]
    public void UnitRoles()
    {
        var person = new PersonBuilder(this.Transaction)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .Build();

        this.Transaction.Derive();

        Assert.Equal("Jane Doe", person.DomainFullName);
        Assert.Equal("Hello Jane Doe!", person.DomainGreeting);
    }
}
```

```sh
dotnet test dotnet/Core/Database/Domain.Tests --filter "FullyQualifiedName~Tests.DomainDerivationTest"
```

Test the changes as well as the first value: change an input, derive again and compare again.
[Getting started](getting-started.md) shows how to run such a test in a clone of the platform's
repository.

## Variations

**Follow a relation from the other end.** Use an association pattern when the value depends on
which objects refer to this one. `PersonOwningRule` keeps `Person.Owning` true while the person
owns an organization:

```csharp
this.Patterns = new Pattern[]
{
    m.Person.AssociationPattern(v => v.OrganisationsWhereOwner)
};
```

```csharp
person.Owning = person.ExistOrganisationsWhereOwner;
```

**Depend on an object further away.** Give the pattern a path when the role that changes is not
on the object that the rule writes; [Patterns](derivations.md#patterns) shows one. The rule
receives the objects at the end of the path.

**Write a rule for an interface.** Name the interface in the pattern and cast the matches to it;
the matches are then objects of every class that implements the interface.

**Report an error.** A rule that finds an object in a state that must not be committed reports
it through `cycle.Validation`, as [Errors](derivations.md#errors) describes.

## When the value is not derived

| Check | Why |
| --- | --- |
| The rule is in the list of rules. | A rule outside the list never runs. |
| Every role and association that `Derive` reads has a pattern. | The rule does not run for a change that no pattern names. |
| The code calls `Derive()` before `Commit()`. | A commit runs no rules, and a later derivation does not see the committed changes. |
| No earlier derivation in the transaction reported an error. | A derivation does not resume the cycles that an [error](derivations.md#errors) cut off; roll back after an error. |
| The generated code is up to date. | A role that was declared after the last generation is not in the typed class. |
| The rule does not count on another rule running first. | The order of the rules in a cycle is not defined; a rule that needs the result of another one has a pattern for it. |

## Where this is implemented

- The rules of the test domain are in `dotnet/Core/Database/Domain/Custom/Rules`, and the rules
  of Core in `dotnet/Core/Database/Domain/Core/Rules`. `Rule` and the patterns are in
  `dotnet/Core/Database/Domain/Core/Derivations/Rules`.
- `Startup` of `dotnet/Core/Database/Server`, `Program` of `dotnet/Core/Database/Commands` and
  `Fixture` of `dotnet/Core/Database/Domain.Tests` each call `Rules.Create` and give the result
  to an `Engine`.
- `DomainDerivationTest`, `RoleDerivationTest` and `ChangedValidationDomainDerivationTest`, in
  `dotnet/Core/Database/Domain.Tests/Domain/Derivations`, check the rules that this guide shows.
