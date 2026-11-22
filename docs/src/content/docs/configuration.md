---
title: "Configuration"
description: "The configuration root, the order of its files, environment variables, the settings, the templates and the admin connections."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code
> and the tests.

A server, the command-line tool and the tests that need a database take their settings from two
sources: the configuration root, and environment variables. No `appsettings` file is read from
the folder of a project.

## The configuration root

The configuration root is the folder that the environment variable `ALLORS_CONFIG_ROOT` names. No
program defaults it. Without the variable no file is read, and environment variables are the only
source of settings.

The Nuke build, the local API tests and the local workspace tests are the exception: when
`ALLORS_CONFIG_ROOT` is not set, they use the template `config/snapshot` of the repository, so a
test run needs no database server.
[Build and test](internals/build-and-test.md#choose-the-adapter) says how to choose another
template.

Below the root every domain has a folder. A program names the folder of its domain, and
optionally one folder below it, when it calls `AddAllorsConfiguration`. Core's folder is `core`,
and the programs of the platform's repository read it:

| Program | Folders, in the order they are read |
| --- | --- |
| The Core test server | `core`, then `core/<application>`, where `<application>` is the application name of the host |
| The command-line tool of Core | `core`, then `core/commands` |
| The local API tests and the local workspace tests | `core` |
| The domain tests | `core`. They run on the Memory adapter unless the setting `adapter` names SQL Server or PostgreSQL. |

An application passes the folder of its own domain in the same call. The command-line tool of the
sample application reads `custom`, then `custom/commands`; see
[The command-line tool](command-line-tool.md#settings).

The Core test server reads no application setting from its command line; it keeps only the
hosting URLs of `--urls`.

## The order of the files

In each folder these files are read in this order. Every file is optional, and a value in a later
file replaces the value of an earlier one:

```text
appsettings.json
appsettings.{environment}.json              # Development, Staging, Production; servers only
appsettings.{platform}.json                 # windows, linux, osx
appsettings.{environment}.{platform}.json
```

Each of these names is also read with the spelling `appSettings`. A folder or a file that does
not exist is skipped.

## Environment variables

Environment variables override every file. In the name of a variable two underscores separate
the levels of a setting:

```sh
adapter=npgsql
ConnectionStrings__DefaultConnection='Host=localhost;Database=core;Username=allors;Password=…'
JwtToken__Key=…
```

## Settings

| Setting | Read by | Meaning |
| --- | --- | --- |
| `adapter` | Every program | The database adapter: `snapshot`, `sqlclient` or `npgsql`. Upper and lower case are equal. |
| `ConnectionStrings:DefaultConnection` | Every program | The connection string of the database. Required with `sqlclient` and `npgsql`; the Snapshot adapter keeps its population in memory and needs none. |
| `JwtToken:Key` | A server | The key that signs and validates the bearer tokens of the sign-in endpoint. Required. |
| `JwtToken:Issuer` | A server | The issuer of a bearer token. |
| `JwtToken:Audience` | A server | The audience of a bearer token. Without the setting it is the issuer. |
| `JwtToken:Expiration` | A server | The lifetime of a bearer token, as a .NET `TimeSpan` such as `7.00:00:00` for seven days. Without a value that is a `TimeSpan`, the lifetime is 30 days. |
| `populationFile` | The command-line tool | The file that `load` reads and `save` writes, and that the `upgrade` of the sample application reads, when the `-f` option is absent. Without the setting the file is `population.xml`. |
| `datapath` | The command-line tool of Core | The name of a folder. The tool looks for a folder of that name next to its working directory, and then next to each folder above it, and passes it to the setup of `populate` and to `upgrade`. Core's own setup does not read it. |

A program stops when a setting that it needs has no value. The message names the setting and its
environment variable, and says where the value was looked for. Without a configuration root:

```text
The configuration has no value for 'adapter'. ALLORS_CONFIG_ROOT is not set, so only environment variables are read: set 'adapter' in the environment, or point ALLORS_CONFIG_ROOT at a folder that holds 'core/appsettings.json'.
```

With a configuration root the message names the root and the folders that were read.

## Templates

The repository holds four templates for a configuration root. Point `ALLORS_CONFIG_ROOT` at one
of them, or at a copy that holds your own values.

| Template | Adapter | Database |
| --- | --- | --- |
| `config/snapshot` | `snapshot` | None. The Snapshot adapter keeps the population in memory and needs no connection string. |
| `config/sqlclient` | `sqlclient` | SQL Server on `localhost`, with the login `allors`. |
| `config/npgsql` | `npgsql` | PostgreSQL on `localhost`, with the login `allors`. |
| `config/localdb` | `sqlclient` | SQL Server LocalDB on Windows, with Windows authentication. |

The templates are for development and testing. Their signing key and their passwords are
published in the repository: give a deployment its own values.

With the Snapshot adapter a population lasts as long as its process. A population that the
command-line tool builds ends with the tool, and the Core test server builds its own through its
endpoint `Test/Setup`.

## Admin connections

`ALLORS_SQLCLIENT` and `ALLORS_NPGSQL` hold an admin connection to the server of the configured
database: SQL Server for `sqlclient`, PostgreSQL for `npgsql`. They serve database management
only; no program runs on them.

- The `init` command of Core's command-line tool drops and creates the database of the
  configured connection and makes the login of that connection its owner. It creates the login
  when the server does not know it. With the Snapshot adapter `init` has nothing to drop or
  create. The command calls `DatabaseProvisioning.DropCreate`, which the command-line tool of an
  application can call as well.
- The adapter test suites create and open their test databases with the admin connection.
- On Windows the admin connection for SQL Server defaults to LocalDB.
- `ALLORS_TEST_SQLCLIENT_CONNECTION` and `ALLORS_TEST_NPGSQL_CONNECTION` are accepted as other
  names for the two variables.

```sh
export ALLORS_CONFIG_ROOT="$PWD/config/sqlclient"
export ALLORS_SQLCLIENT='Server=localhost;User Id=sa;Password=…;TrustServerCertificate=true'
```

## Where this is implemented

- `ConfigurationBuilderExtensions`, in `dotnet/Core/Database/Configuration/Core`, reads the
  folders and the files in `AddAllorsConfiguration` and `AddCrossPlatform`, and builds the message
  for a missing setting in `Require`. `ConfigurationBuilderExtensionsTests`, in
  `dotnet/Core/Database/Server.Local.Tests`, checks the order of the folders, the override by
  environment variables and `Require`.
- `DatabaseBuilder` and `DatabaseProvisioning`, in
  `dotnet/System/Database/Adapters/Allors.Database.Builder`, select the adapter and manage the
  database through the admin connection.
- `Program` of `dotnet/Core/Database/Server` and `Program` of `dotnet/Core/Database/Commands` name
  the folders that the Core test server and its command-line tool read.
