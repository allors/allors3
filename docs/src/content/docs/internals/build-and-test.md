---
title: "Build and test"
description: "Prerequisites, the Nuke targets, the suites and what each needs, the TypeScript workspace and the CI workflows."
audience: "Maintainers"
kind: "How-to guide"
status: "Current"
sidebar:
  order: 2
---

> **Status: Current.** How-to guide for maintainers.

The build is [Nuke](https://nuke.build). Run a target with `./build.sh <target>` on macOS and
Linux, and with `build.cmd <target>` on Windows. The targets are defined in `build/`.

## Prerequisites

- The .NET SDK that `global.json` names, version 10. Without an installed `dotnet`, `build.sh`
  downloads that SDK into `.nuke/temp`.
- Node.js 24 and npm, for the TypeScript workspace.
- A clone with its full history. Nerdbank.GitVersioning computes the version of a build from the
  history, which a shallow clone does not have.
- No database server. The suites run on the Snapshot adapter unless you choose another one; SQL
  Server and PostgreSQL are needed only for [their suites](#run-a-suite).

## Generate first

A fresh clone does not compile until the generators have run:

```sh
./build.sh Generate
```

`Generate` produces the metadata and the typed classes of the test model of the adapters and of
Core with its test domain, for .NET and for TypeScript. Generated files are not committed and are
never edited by hand. Run `Generate` again after a change to a repository project or to a
template.

`./build.sh` without a target runs `Install`, which installs the dependencies of the TypeScript
workspace, and then `Generate`.

## Choose the adapter

The tests, the command-line tool and the server that the build starts read a
[configuration root](../configuration.md#the-configuration-root). The build chooses it in this
order:

1. `--provider` with `snapshot`, `sqlclient`, `npgsql` or `localdb`: the template of that name
   under `config/`.
2. The environment variable `ALLORS_CONFIG_ROOT`, when `--provider` is absent.
3. The template `config/snapshot`, when neither is given.

```sh
./build.sh DotnetCoreDatabaseTest                        # Snapshot adapter, no database server
export ALLORS_SQLCLIENT='Server=localhost;User Id=sa;Password=…;TrustServerCertificate=true'
./build.sh DotnetCoreDatabaseTest --provider sqlclient   # SQL Server through config/sqlclient
```

With a SQL template the build needs the [admin connection](../configuration.md#admin-connections)
of that adapter as well: it creates the database and its login before the tests run.

## Run a suite

The suites that CI runs are the `Ci*` targets in `build/Build.ci.cs`. Each of them runs `Reset`,
which on Windows stops the Node.js and browser processes that an earlier run left behind, and
then the target without the prefix, which you can run directly as well. The two TypeScript suites
also run `Install` first; run `./build.sh Install` once before you run
`TypescriptWorkspaceTest` or `TypescriptWorkspaceAdaptersJsonTest` directly.

| Suite | Tests | Needs |
| --- | --- | --- |
| `CiDotnetSystemAdaptersTestMemory` | The shared adapter tests on the Memory adapter | Nothing |
| `CiDotnetSystemAdaptersTestSnapshot` | The shared adapter tests on the Snapshot adapter | Nothing |
| `CiDotnetSystemAdaptersTestSqlClient` | The shared adapter tests on SQL Server | The admin connection in `ALLORS_SQLCLIENT`, or LocalDB on Windows |
| `CiDotnetSystemAdaptersTestNpgsql` | The shared adapter tests on PostgreSQL | The admin connection in `ALLORS_NPGSQL` |
| `CiDotnetCoreDatabaseTest` | The merge, commands, generate, meta and domain tests of Core, and the API tests against a database and against the Core test server | A configuration root |
| `CiDotnetCoreWorkspaceLocalTest` | The .NET workspace on the Local adapter | A configuration root |
| `CiDotnetCoreWorkspaceRemoteJsonSystemTextTest`, `CiDotnetCoreWorkspaceRemoteJsonNewtonsoftTest` | The .NET workspace on the Remote adapter against the Core test server, with System.Text.Json and with Newtonsoft.Json | A configuration root |
| `CiTypescriptWorkspaceTest` | The TypeScript workspace libraries that need no server | Nothing |
| `CiTypescriptWorkspaceAdaptersJsonTest` | The TypeScript workspace against the Core test server | A configuration root |
| `CiSampleTest` | The sample domain tests on Memory, and the command-line and HTTP server tests on Snapshot | Nothing |

A suite that needs a configuration root takes it as [Choose the adapter](#choose-the-adapter)
describes: `config/snapshot` unless you name another template. The adapter suites read no
configuration root; they create and open their own test databases with the admin connection.

The targets write their results as TRX files to `artifacts/Tests`.

### The sample application

`sample/` holds an application as a user of the platform builds one: its projects include the
folders named `Core` by path and never the folders named `Custom`, which hold the test domain.
`Generate` does not touch it, and the platform builds without it. `SampleTest` first runs
`SampleGenerate`, which merges the messages of Core into the sample, runs the repository generator
on the sample's model and runs the sample's own generator; then it runs the sample's tests.

The suite is the repository's check on what a change to Core asks of an application. A hook
that an application has to define, or a class that it has to provide, fails the build of the
sample. A rule of Core that an application has to add to its list of rules does not: the suite
notices only when a test of the sample needs that rule. The suite counts for `CI result`, so a
change to Core that breaks the sample merges together with the change that the sample needs.
[Create an application](../create-an-application.md) tells users how to start from the sample, so
a change to the sample's projects, or to the files that it copies from Core's folders `Custom`
and `Virtual`, can change that guide as well.

## The TypeScript workspace

`typescript/modules` is an nx workspace. After `npm install` in that folder, these scripts run
without a server:

```sh
npm run system-workspace-meta:test
npm run system-workspace-meta-json:test
npm run system-workspace-adapters:test
npm run system-workspace-domain:test
```

`npm run system-workspace-adapters-json:test` needs the Core test server. The target
`TypescriptWorkspaceAdaptersJsonTest` publishes and starts it before it runs the script.

## Continuous integration

Three workflows run the suites. Every suite is a job of its own. A leg, which is the suites of
one adapter on one operating system, is one call of `.github/workflows/tests.yml`; the sample
application is a leg of its own.

| Event | Workflow | Runs |
| --- | --- | --- |
| A pull request, whatever its base branch | `ci.yml` | On Linux: the four adapter suites, with SQL Server and PostgreSQL as service containers, the Core and TypeScript suites with `config/snapshot`, and the suite of the sample application. The domain tests of Core then run on the Memory adapter. |
| A push to `main` | `ci.yml` | The same, and the Core and TypeScript suites with `config/sqlclient` and `config/npgsql` as well, the domain tests included. |
| Every night on `main`, and on demand | `nightly.yml` | On Windows: the adapter suites, and the Core and TypeScript suites on the Snapshot adapter, on SQL Server LocalDB and on PostgreSQL. The suite of the sample application is not among them. |

A branch runs through its pull request. Start a run on demand for a branch that has none, or for
the Windows legs:

```sh
gh workflow run ci.yml --ref <branch>
gh workflow run nightly.yml -f os=both -f provider=npgsql
```

`-f os=` takes `windows-latest`, `ubuntu-latest` or `both`, and `-f provider=` takes `all`,
`snapshot`, `sqlclient` or `npgsql`.

The check `CI result` stands for the suites of a pull request and of a push, and it is the one
check that the ruleset of `main` requires. A change that touches only documentation skips the
suites, and `CI result` passes without them: the files under `docs/`, the documentation workflow,
and `AGENTS.md`, `CHANGELOG.md` and `README.md` at the root of the repository.

The documentation has its own workflow and its own checks; the
[website instructions](../../../../README.md#check-a-change) describe them.
