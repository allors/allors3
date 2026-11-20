---
title: "Getting started"
description: "Clone the platform, run one of its domain tests and write tests of your own that create objects, relate them, derive a value and query them."
audience: "Users"
kind: "Tutorial"
status: "Current"
---

> **Status: Current.** Tutorial for users. It works in a clone of the platform's repository, on
> its test domain. It does not set up an application of your own.

In this tutorial you run the platform on your own machine and write four small tests. They create
objects, relate them, let a rule derive a value and query the result: the things that the code of
an application does in the database. You need no database server.

The tutorial uses the [test domain](domains.md#custom-in-the-platforms-repository) that the
platform's own tests use, with its classes `Person` and `Organisation`. The projects of an
application of your own are a different subject, which
[Domains](domains.md#how-an-application-includes-core) introduces. The
[sample application](../../../../sample/) of the repository shows those projects for the database:
a domain that extends Core, with two classes, a rule and its tests.

## Before you start

You need Git and the .NET SDK, version 10.

If your shell sets `ALLORS_CONFIG_ROOT` or `adapter`, the tests read that
[configuration](configuration.md#the-configuration-root) and can then run on a database server.
Clear both variables for this tutorial.

## 1. Clone and generate

```sh
git clone https://github.com/allors/allors3.git
cd allors3
./build.sh Generate
```

On Windows the last command is `build.cmd Generate`. Make a full clone, not a shallow one: the
build computes its version from the history.

`Generate` writes the typed classes of the model, which are not in the repository. The clone does
not compile until it has run. It ends with `Build succeeded`.

## 2. Run a test of the platform

```sh
dotnet test dotnet/Core/Database/Domain.Tests --filter "FullyQualifiedName~Tests.DomainDerivationTest"
```

The first run builds the projects. The run ends with a test summary: one test, and none failed.

The test is one of the domain tests of Core and of its test domain. They run on the Memory
adapter, a database in memory, so nothing has to be installed or started.

## 3. Look at the test domain

Open `dotnet/Core/Repository/Domain/Custom/Database/Special/Organisation.cs`. It declares the
class `Organisation` with its relations, among them:

| Declaration | Meaning |
| --- | --- |
| `string Name` | An organization has a name. |
| `Person[] Employees`, `OneToMany` | An organization has many employees, and a person is an employee of one organization. |
| `Person Owner`, `ManyToOne` | An organization has one owner, and a person can own many organizations. |

`Person.cs`, next to it, declares `FirstName`, `LastName` and `DomainFullName`. The last one is
marked `[Derived]`: the tests of this tutorial never assign it, because a rule maintains it.

`Generate` wrote a class for each of them in `dotnet/Core/Database/Domain/Generated`. Those
classes are what the tests use. [The model](model.md) describes the declarations.

## 4. Create two objects and relate them

Create the file `dotnet/Core/Database/Domain.Tests/Domain/GettingStartedTests.cs`:

```csharp
namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class GettingStartedTests : DomainTest, IClassFixture<Fixture>
    {
        public GettingStartedTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AnOrganisationHasEmployees()
        {
            var acme = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
            var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Roe").Build();

            acme.AddEmployee(jane);

            Assert.Contains(jane, acme.Employees);
            Assert.Equal(acme, jane.OrganisationWhereEmployee);
        }
    }
}
```

Run it:

```sh
dotnet test dotnet/Core/Database/Domain.Tests --filter GettingStartedTests
```

The test summary reports one test, and none failed.

- `DomainTest` gives every test a new database with the population of the test domain, and a
  transaction, `this.Transaction`. It rolls the transaction back when the test ends.
- A builder creates an object in the transaction. Nothing in this test calls `new Organisation`.
- `AddEmployee` writes the role `Employees`. The test never assigns
  `jane.OrganisationWhereEmployee`: that is the association, the same relation read from the
  other end, and the platform keeps it in step.

## 5. Let the platform keep a relation true

Add a second test to the class:

```csharp
[Fact]
public void APersonHasOneEmployer()
{
    var acme = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
    var globex = new OrganisationBuilder(this.Transaction).WithName("Globex").Build();
    var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Roe").Build();

    acme.AddEmployee(jane);
    globex.AddEmployee(jane);

    Assert.Equal(globex, jane.OrganisationWhereEmployee);
    Assert.DoesNotContain(jane, acme.Employees);
}
```

Run the tests again; two pass. `Employees` is `OneToMany`, so a person is an employee of one
organization. When Globex takes Jane on, the platform removes her from the employees of Acme. No
code of the test did that. [Objects and relations](objects-and-relations.md) describes what each
multiplicity maintains.

## 6. Derive a value

Add a third test:

```csharp
[Fact]
public void TheFullNameIsDerived()
{
    var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Roe").Build();

    Assert.False(jane.ExistDomainFullName);

    this.Transaction.Derive();

    Assert.Equal("Jane Roe", jane.DomainFullName);

    jane.LastName = "Doe";
    this.Transaction.Derive();

    Assert.Equal("Jane Doe", jane.DomainFullName);
}
```

Run the tests again; three pass. The test never assigns `DomainFullName`. `Derive()` looks at
what changed in the transaction and runs the rules that depend on it. One of them is
`PersonFullNameRule`, in `dotnet/Core/Database/Domain/Custom/Rules`: it runs when the first name
or the last name of a person changes, and writes the full name. Open the file; it is short.
[Derivations](derivations.md) explains how a derivation works, and
[Write a rule](write-a-rule.md) how to add a rule.

## 7. Query the objects

Add a fourth test:

```csharp
[Fact]
public void AnExtentFindsThePeopleWithoutAnEmployer()
{
    var acme = new OrganisationBuilder(this.Transaction).WithName("Acme").Build();
    var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Roe").Build();
    var john = new PersonBuilder(this.Transaction).WithFirstName("John").WithLastName("Roe").Build();
    acme.AddEmployee(jane);

    var people = this.Transaction.Extent<Person>();
    people.Filter.AddEquals(this.M.Person.LastName, "Roe");
    people.Filter.AddNot().AddExists(this.M.Person.OrganisationWhereEmployee);

    Assert.Equal(new[] { john }, people.ToArray());
}
```

Run the tests again; four pass. An extent is the set of the objects of a type, narrowed by a
filter: here the people with the last name Roe who are not an employee. `this.M` is the metadata
of the model; `this.M.Person.LastName` names a role without a string. The population of the test
domain holds other people as well, which is why the filter names a last name that only this test
uses. [Extents](extents.md) lists the predicates.

## What you have done

You ran the platform without a database server, and you used its four basic operations in the
database: creating objects, writing a role and reading the association, deriving, and querying
with an extent.

The file that you wrote is yours to experiment with. Delete it when you are done: it is not part
of the platform.

Read on:

- [The model](model.md) for what a declaration can say.
- [Objects and relations](objects-and-relations.md) for creating, relating and deleting objects,
  and for transactions.
- [Extents](extents.md) for queries.
- [Derivations](derivations.md) and [Write a rule](write-a-rule.md) for rules.
- [Domains](domains.md) for the way an application builds on Core.
