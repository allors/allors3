---
title: "Create an application"
description: "Start an application in a repository of its own from the sample application: add the platform as a submodule, copy and point the projects, generate, test, and make the domain your own."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the build targets.

Use this guide to start an application in a repository of its own. It assumes
[Domains](domains.md), which explains how an application extends Core and includes its source.

The starting point is the [sample application](../../../../sample/) in the platform's
repository. Its projects are those of an application: the model, the database layers, a
command-line tool and their tests. The platform's CI generates, builds and tests the sample for
every pull request that changes more than the documentation, so the sample follows the platform.

You need Git, the .NET SDK, version 10, and a POSIX shell with Perl, as on Linux and macOS: the
commands below are written for one.

## 1. Create the repository and add the platform

An application compiles the source of the platform, so it keeps a copy of the platform's
repository next to its own projects. A Git submodule holds that copy:

```sh
mkdir my-app
cd my-app
git init
git submodule add https://github.com/allors/allors3.git allors
```

The build computes the version of the platform's projects from the history of that copy, and
stops in a shallow one. Give CI the full history as well: with GitHub's checkout action, set
`fetch-depth: 0` next to `submodules: true`. Issue
[#409](https://github.com/allors/allors3/issues/409) records the details.

## 2. Copy the sample's projects

```sh
cp -R allors/sample/Repository allors/sample/Database .
```

| Project | What it holds |
| --- | --- |
| `Repository/Domain` | The [model](model.md): Core's, which the project includes, and the application's own, in `Custom`. |
| `Database/Meta` | The metadata, which a generator writes from the model. |
| `Database/Resources` | The messages of the domain, merged from Core's. |
| `Database/Domain` | The domain: Core's code, the calls to the hooks in `Virtual`, the application's code in `Custom`, and the list of rules in `Rules.cs`. |
| `Database/Configuration` | The services of the database. |
| `Database/Generate` | The generator of the classes of the domain. |
| `Database/Commands` | The [command-line tool](command-line-tool.md). |
| `Database/Domain.Tests` and `Database/Commands.Tests` | The tests. |

## 3. Point the projects at the submodule

In the platform's repository the projects of the sample reach the platform's source with paths
that start with `..\..\..\dotnet\`. Next to the submodule those paths start with
`..\..\allors\dotnet\`. The generator of the domain also names its template with a path that
starts with `../dotnet/`, relative to the folder from which it runs. These commands change both:

```sh
grep -rl --include='*.csproj' '\.\.\\\.\.\\\.\.\\dotnet\\' Repository Database | xargs perl -pi -e 's/\.\.\\\.\.\\\.\.\\dotnet\\/..\\..\\allors\\dotnet\\/g'
perl -pi -e 's#"\.\./dotnet/#"allors/dotnet/#' Database/Generate/Program.cs
```

The projects then include Core with lines such as this one, from `Database/Domain/Domain.csproj`:

```xml
<Compile Include="..\..\allors\dotnet\Core\Database\Domain\Core*\**\*.cs" />
```

## 4. Leave the build's output out of Git

The build and the generators write to folders named `bin`, `obj` and `Generated`, which the
platform's repository ignores. Ignore them in the application's repository too:

```sh
printf '[Bb]in/\n[Oo]bj/\n[Gg]enerated/\n' > .gitignore
```

## 5. Generate

[Generated code](domains.md#generated-code) is not committed. Three tools write it; run them from
the root of the application's repository:

```sh
mkdir -p Database/Resources/Generated
dotnet run --no-launch-profile --project allors/dotnet/Core/Database/Merge -- allors/dotnet/Core/Database/Resources/Core Database/Resources/Generated
dotnet run --no-launch-profile --project allors/dotnet/System/Repository/Generate -- Repository/Domain/Repository.csproj allors/dotnet/System/Repository/Templates/meta.cs.stg Database/Meta/Generated
dotnet run --project Database/Generate
```

`--no-launch-profile` keeps `dotnet run` from reading the launch settings that the two projects of
the platform hold for their own development.

1. **Merge** writes Core's messages to `Database/Resources/Generated`. The folder has to exist:
   without it, Merge exits with code 1 and prints nothing. An application with messages of its
   own passes their folder as well: every argument but the last is a folder to read.
2. **The repository generator** reads the model and writes the metadata to
   `Database/Meta/Generated`.
3. **The application's generator** writes the classes of the domain to
   `Database/Domain/Generated`.

Generate again after every change to the model. In the platform's repository the targets
`SampleMerge` and `SampleGenerate` of the build run the same tools on the sample.

## 6. Build and test

```sh
dotnet test Database/Domain.Tests
dotnet test Database/Commands.Tests
```

The tests of the domain run on the Memory adapter, and those of the command-line tool on the
Snapshot adapter, which they set themselves, so neither needs a database server. All 8 tests of
the domain and all 7 of the command-line tool pass.

## 7. Make the domain your own

Change the sample into the application one part at a time, and generate, build and test after
each change:

- **The domain.** Give the domain an id of its own: the `[Domain]` attribute of the struct
  `Custom`, in `Repository/Domain/Custom/Custom.cs`, holds the sample's.
- **The model.** Replace `WorkItem`, and the roles that the sample adds to `Person`, with the
  classes of the application, each with ids of its own; [The model](model.md) describes the
  declarations. The users of an application are objects of a class that implements Core's
  interface `User`, as `Person` does.
- **The rules.** `Database/Domain/Rules.cs` lists Core's six rules and the application's own;
  [Write a rule](write-a-rule.md) shows how to add one.
- **The setup and the security.** The folder `Database/Domain/Custom` holds the sample's setup,
  security and upgrade. [Grant access](grant-access.md) and
  [Upgrade a population](upgrade-a-population.md) explain them.
- **The tests.** Replace the tests of the sample with the application's.

## Files that follow the platform

Core's code needs classes and members that the platform keeps outside its folders `Core`, most of
them in the folders of its test domain, so an application does not include them. The sample has
its own versions of them, and an application keeps these:

| Files | What the application does with them |
| --- | --- |
| `Database/DatabaseServices.cs`, `Database/DefaultDatabaseServices.cs`, `Database/PreparedFetches/PreparedSelects.cs` and `Transaction/TransactionServices.cs`, below `Database/Configuration/Custom` | Keeps them as the platform has them. |
| The three files in `Database/Domain/Virtual`, and `Database/Domain/Custom/ObjectsBase.cs` | Keeps them as the platform has them while its domain extends Core directly. With domains in between, the files in `Virtual` call the part of each domain, as [Domains](domains.md#setup-and-security) explains. |
| `Database/PreparedExtents/PreparedExtents.cs` and `Database/WorkspaceMask/WorkspaceMask.cs`, below `Database/Configuration/Custom` | Adds entries of its own; the sample's are empty. |
| `Database/Meta/Custom/MetaBuilder.cs` | Keeps the method `BuildCustom`, which the generated metadata calls, and changes the metadata of its domain there when it needs to. |
| `Database/Domain/Rules.cs` | Lists Core's six rules next to its own. |
| `Database/Generate/Program.cs` | Holds the paths of the template and of the output, relative to the folder from which the generator runs; step 3 points the template at the submodule. |

Issue [#408](https://github.com/allors/allors3/issues/408) records why an application needs these
files.

## Update the platform

Move the submodule to the newest commit of the platform, then generate, build and test again:

```sh
git submodule update --remote allors
```

The sample changes together with the platform, so compare the files that the application keeps as
the platform has them with the sample's, for example:

```sh
git diff --no-index allors/sample/Database/Domain/Virtual Database/Domain/Virtual
```

Commit the new commit of the submodule together with the changes that it asks for.

## Where this is implemented

- The sample application is in `sample/`. The targets `SampleMerge` and `SampleGenerate` of the
  Nuke build, in `build/Sample/Build.cs`, run the platform's tools on it, and `SampleTest` runs its
  tests.
- Merge is in `dotnet/Core/Database/Merge`, the repository generator in
  `dotnet/System/Repository/Generate`, with its template
  `dotnet/System/Repository/Templates/meta.cs.stg`, and the generator of Core's domain, which the
  sample's generator includes, in `dotnet/Core/Database/Generate`, with the template
  `dotnet/Core/Database/Templates/domain.cs.stg`.
