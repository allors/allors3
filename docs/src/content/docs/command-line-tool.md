---
title: "The command-line tool"
description: "The commands of an application's command-line tool, with the sample application's tool as the example: settings, commands, options, output, exit codes and adapters."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code.

An application has a command-line tool that creates, fills, saves, loads and upgrades its
database. This page describes the tool of the [sample application](../../../../sample/),
`sample/Database/Commands`, which takes three of its commands from Core. Run it from the root of
the platform's repository, after `./build.sh SampleGenerate`:

```sh
dotnet run --project sample/Database/Commands -- <command> [options]
```

## Usage

```text
Usage: Commands [command] [options]
```

| Option | Meaning |
| --- | --- |
| `-t` | The command timeout of the database, in seconds. It precedes the command: `-t 300 upgrade`. |
| `-?`, `-h`, `--help` | Show the help of the tool, or of the command that precedes it. |

Without a command the tool shows its help and exits with code 1.

## Settings

The tool reads the folders `custom` and then `custom/commands` of the
[configuration root](configuration.md#the-configuration-root), and environment variables, which
override every file.

| Setting | Meaning |
| --- | --- |
| `adapter` | Required: `snapshot`, `sqlclient` or `npgsql`. |
| `ConnectionStrings:DefaultConnection` | Required with `sqlclient` and `npgsql`. |
| `populationFile` | The file of `save`, `load` and `upgrade` when the option `-f` is absent. Without the setting the file is `population.xml`. |

A command stops with exit code 1 when a required setting has no value. Without a configuration
root the message is:

```text
The configuration has no value for 'adapter'. ALLORS_CONFIG_ROOT is not set, so only environment variables are read: set 'adapter' in the environment, or point ALLORS_CONFIG_ROOT at a folder that holds 'custom/appsettings.json'.
```

`init` also uses the admin connection of the server;
[Admin connections](configuration.md#admin-connections) describes it.

## Commands

| Command | From | What it does | Option |
| --- | --- | --- | --- |
| `init` | The application | Drops and creates the database of the connection string, through the admin connection; [Admin connections](configuration.md#admin-connections) says what it does with the login of the connection string. With the Snapshot adapter it logs that there is nothing to drop or create. | |
| `populate` | The application | Empties the database with `Init()` and runs the [setup](populations.md#a-new-population), which builds a new population. | |
| `save` | Core | Writes the population to a file. | `-f` the file |
| `load` | Core | Replaces the population with the one in a file. | `-f` the file |
| `upgrade` | The application | Loads a population that an earlier version saved and brings it up to date; [Upgrade a population](upgrade-a-population.md) describes its steps. | `-f` the file |
| `index` | Core | On SQL Server and PostgreSQL, creates the indexes that the adapter derives from the model, and drops the adapter's indexes that the model no longer needs. The SQL adapters also do this at the end of `Init()` and after a load. A transaction creates no index, and none opens while the database lacks one: when these indexes change and the database stays, run `index` at deployment. Other adapters keep no indexes, and the command logs so. | |

A relative file name is relative to the working directory, and the tool logs the full path of the
file that it saves or loads.

## Output and exit codes

The tool logs to the console, from the level `Info` up, one line per message:

```text
13:19:57 INFO Begin
13:19:57 INFO Saving /home/user/allors3/population.xml
13:19:57 INFO End
```

| Exit code | Meaning |
| --- | --- |
| 0 | The command succeeded. |
| 1 | The command failed, its arguments were not recognized, or no command was given. The tool logs the message at the level `Error`, except when no command was given: then it shows its help. |

## Adapters

On the Snapshot adapter the population ends with the process, so each run starts with an empty
database: `save` then writes a population without objects, and `load` or `upgrade` holds the
loaded population only until the run ends. A sequence of commands, such as `populate` followed by
`save`, needs SQL Server or PostgreSQL. The tests of the sample call the commands in one process
instead, on the Snapshot adapter.

## What an application writes

Core provides `save`, `load` and `index`, in `dotnet/Core/Database/Commands/Core`; the project of
the tool includes them with a glob, as the [projects of an application](create-an-application.md)
include Core. The application writes the rest:

- **`Program`**, which lists the commands with `[Subcommand]`, reads the configuration of the
  application's domain, `AddAllorsConfiguration("custom", "commands")` in the sample, and builds
  the database with the application's rules. Core's commands ask it for `Configuration` and
  `Database`.
- **`ExitCode`**, with `Success` and `Error`, which Core's commands return.
- **`populate` and `upgrade`**, which depend on the application's setup and model, and
  **`init`**, which Core keeps in its folder `Custom`; the sample's is a copy of it.
- **`nlog.config`**, which the build copies next to the tool.

## Where this is implemented

- The sample's tool is in `sample/Database/Commands`: `Program.cs`, `ExitCode.cs`, `nlog.config`
  and the commands in `Custom`. Its tests are in `sample/Database/Commands.Tests`.
- Core's commands are in `dotnet/Core/Database/Commands/Core`. `DatabaseBuilder` and
  `DatabaseProvisioning`, which build the database and serve `init`, are in
  `dotnet/System/Database/Adapters/Allors.Database.Builder`.
