---
title: "Terminology"
description: "Agreed names for the platform, domains, relations, adapters, populations and configuration."
audience: "Users and maintainers"
kind: "Reference"
status: "Current"
---

> **Status: Current.** Reference for users and maintainers.

Use these names consistently when discussing Allors3. This page records the shared vocabulary and
links to the pages that own the detailed definitions. Exact API identifiers keep their declared
spelling and capitalization.

## The platform

| Term | Definition and usage |
| --- | --- |
| Platform | System and Core together, with the TypeScript system and core libraries and the generator templates. [Versions and branches](versions.md#the-32-line) lists the scope. |
| System | The database and workspace engines, the adapters, the protocols, the metadata and the generators. |
| Core | The foundational domain and the code that goes with it: authorization, authentication, the JSON API and the building blocks of a server. Core names both the domain and its part of the repository, `dotnet/Core`. |
| Core test server | The server in `dotnet/Core/Database/Server`, which hosts Core with its test domain for the platform's tests. It is a test harness, not a production deployment; an application has a server of its own. |
| Application | What a user of the platform builds: a domain that extends Core, with its own programs and user interface. Use downstream application where the direction matters, as in a change that an application has to follow. |
| Base and Apps | The business domains of earlier versions. They are not part of the platform; see [Versions and branches](versions.md). |

## Domains

| Term | Definition and usage |
| --- | --- |
| Domain | See [Domains](domains.md). Inheritance connects domains; do not use domain and application interchangeably. |
| Extend | A domain extends one domain and inherits its model and behaviour. Preserve `Extends` where it is the attribute. |
| `Custom` | The conventional name of an application's own domain, and the name of the platform's test domain in Core's `Custom` folders. Say which one is meant; [Domains](domains.md#custom-in-the-platforms-repository) explains why the two do not meet. |
| Repository project | The C# project that declares the model of the domains, such as `dotnet/Core/Repository/Domain`. Qualify the Git repository as the repository of the platform, or of an application, where the context could be ambiguous. |
| Part of a method | What one domain contributes to a method, in a member named after the domain and the method; see [The behaviour](domains.md#the-behaviour). |

## Relations

An Allors relation is bidirectional. A **role** is the forward, writable end; an **association**
is the inverse, read-only end. Read and change relations through the Allors API so that the
platform can maintain both ends.

In prose, use role and association for these specific ends. Preserve identifiers such as
`IRoleType` and `IAssociationType` when referring to code. A generic UML association describes a
relationship; it does not by itself identify the Allors inverse end. Label that distinction when
a class diagram uses both meanings. A role of Core's authorization, the class `Role`, has a
different meaning; qualify it where the context could be ambiguous.

## Adapters

**Adapter** is the established project term. Qualify it as a database adapter or a workspace
adapter when the context could be ambiguous.

| Term | Definition and usage |
| --- | --- |
| Database adapter | An implementation of the database: the Memory, Snapshot, SqlClient and Npgsql adapters. The setting `adapter` selects the Snapshot, SqlClient or Npgsql adapter; see [Configuration](configuration.md#settings). |
| Workspace adapter | An implementation of the workspace: the Local adapter, and the Remote adapter with its System.Text.Json and Newtonsoft.Json variants. |

Do not substitute **codec** for an existing adapter. If a separate encoding and decoding
responsibility needs a name, discuss that distinction and its scope before adopting one. A choice
of serializer does not by itself establish a new name for the surrounding adapter.

## Populations and configuration

| Term | Definition and usage |
| --- | --- |
| Population | The objects and relations that a database holds. `save` writes a population to a file and `load` reads one. |
| Configuration root | The folder that `ALLORS_CONFIG_ROOT` names; see [Configuration](configuration.md#the-configuration-root). |
| Template | A configuration root that the repository provides under `config/`, for development and testing. |
| Admin connection | A connection in `ALLORS_SQLCLIENT` or `ALLORS_NPGSQL` that serves database management only; see [Configuration](configuration.md#admin-connections). |
