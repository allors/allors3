---
title: "Terminology"
description: "Agreed names for the platform, domains, the model, relations, objects, derivations, security, the workspace, adapters, populations and configuration."
audience: "Users and maintainers"
kind: "Reference"
status: "Current"
---

> **Status: Current.** Reference for users and maintainers.

Use these names consistently when discussing Allors3. This page records the shared vocabulary and
links to the pages that own the detailed definitions. Exact API identifiers keep their declared
spelling and capitalization. A name that the model declares, such as a class, a unit or a role,
is written in code formatting; the
[writing guide](internals/writing-documentation.md#use-oxford-english) gives the rule.

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

## The model

| Term | Definition and usage |
| --- | --- |
| Model | The classes, interfaces, relations and methods that the repository projects of an application declare; see [The model](model.md). |
| Class, interface | The two kinds of composite, a type that has relations. An object is an instance of a class; an interface declares relations and methods for the classes that implement it. |
| Unit | One of the eight types of a value that the platform defines, such as `String` and `Integer`; see [Types](model.md#types). |
| Multiplicity | How many objects stand at each end of a relation: `OneToOne`, `OneToMany`, `ManyToOne` or `ManyToMany`, with the association named first; see [Multiplicity](model.md#multiplicity). |
| Workspace | The engine in which a client works with objects, in sessions; see [The workspace](workspace.md). A workspace has a name, and the model says which classes, relations and methods the workspaces of each name can use: `[Workspace]` on a declaration names them, and `Default` is the name that it uses when it is given none. Qualify the [workspace adapter](#adapters), which is an implementation of the engine. |

## Relations

An Allors relation is bidirectional. A **role** is the forward, writable end; an **association**
is the inverse, read-only end. Read and change relations through the Allors API so that the
platform can maintain both ends.

In prose, use role and association for these specific ends. Preserve identifiers such as
`IRoleType` and `IAssociationType` when referring to code. A generic UML association describes a
relationship; it does not by itself identify the Allors inverse end. Label that distinction when
a class diagram uses both meanings. Core's authorization has a class named `Role`, which is a set
of permissions: in code formatting `Role` is always that class, and a role in plain text is the
end of a relation. [Security](#security) lists the names of authorization.

A **unit role** holds a value, and a **composite role** holds an object or several objects. Use
role type, association type and relation type for the metadata of a relation, such as
`m.Person.LastName`, and role and association for what an object holds.

## Objects in the database

| Term | Definition and usage |
| --- | --- |
| Object | An instance of a class of the model, which the platform creates, stores and deletes; see [Objects and relations](objects-and-relations.md). |
| Transaction | The unit of work of the database, an `ITransaction`. Session is the term of the workspace; do not use it for a transaction of the database. |
| Builder | The generated class that creates an object of one class and calls its methods `OnBuild` and `OnPostBuild`, such as `PersonBuilder`. |
| Objects class | The generated class that carries the plural name of a class or an interface, such as `People` for `Person` and `Users` for the interface `User`. It inherits from `ObjectsBase` and gives the extent of its type. |
| Extent | The set of the objects of a class or an interface, narrowed by a filter; see [Extents](extents.md). Keep the name: an extent is more specific than a query or a collection. |
| Change set | What changed in a transaction since the previous checkpoint; see [Change sets](objects-and-relations.md#change-sets). |

## Derivations

| Term | Definition and usage |
| --- | --- |
| Derivation | One run that brings the objects of a transaction up to date, started with `Derive()`; see [Derivations](derivations.md). |
| Rule | What an application writes to maintain a derived role or to report an error: patterns and a method `Derive`. |
| Pattern | The part of a rule that says which change makes which objects match. |
| Cycle | One round of a derivation, on one change set. |
| Validation | The errors that a derivation reports, an `IValidation`. |
| Derived role | A role that is declared with `[Derived]`: clients cannot write it, and code in the database maintains it, usually a rule. |

## Security

The names of Core's authorization are the names of its types; [Security](security.md) explains
how they decide access. Write them in code formatting when the text means the type or its
objects.

| Term | Definition and usage |
| --- | --- |
| `User` | Core's interface for someone who signs in. An application declares a class that implements it. A user in plain text is the person. |
| `UserGroup` | A named group of users. |
| `Role` | A named set of permissions. Write it in code formatting: a role in plain text is the end of a relation. |
| `Permission` | The permission for one operation on the objects of one class: to create them, to read or write one of their roles, or to execute one of their methods. |
| `Grant` | Gives the permissions of one `Role` to users and user groups. |
| `SecurityToken` | A set of grants for the objects that refer to it. |
| `Revocation` | A set of permissions that it denies to every user, for the objects that refer to it. |
| Access control list | What one user may do with one object, an `IAccessControlList`. |

## The workspace

[The workspace](workspace.md) explains how these fit together.

| Term | Definition and usage |
| --- | --- |
| Connection | The `DatabaseConnection` of a workspace adapter, which acts for one user and holds the records of the objects that its sessions pulled. |
| Session | The unit of work of a workspace, an `ISession`, which holds objects and their changes. Transaction is the term of the database; do not use it for a session. |
| Record | What a connection holds of an object of the database: its version, its roles and what the user may do with it. |
| Pull | A request that finds objects in the database and brings them into a session; see [Pulls](pulls.md). |
| Push | Sends the new objects and the changes to roles with `Origin.Database` from a session to the database, which derives and commits them. |
| Invoke | Runs a method of an object in the database, for the user of the workspace. |
| Select, include | The path along roles and associations whose objects a result of a pull holds, and the tree of roles and associations along which a pull brings more objects into the session. |
| Procedure | An `IProcedure` of the database that a pull runs by its name; see [Procedures](pulls.md#procedures). |
| Merge error | What a pull reports when the session changed a role that the database changed as well; see [Versions and merges](workspace.md#versions-and-merges). |

## Adapters

**Adapter** is the established project term. Qualify it as a database adapter or a workspace
adapter when the context could be ambiguous.

| Term | Definition and usage |
| --- | --- |
| Database adapter | An implementation of the database: the Memory, Snapshot, SqlClient and Npgsql adapters. The setting `adapter` selects the Snapshot, SqlClient or Npgsql adapter; see [Configuration](configuration.md#settings). |
| Workspace adapter | An implementation of the workspace: the Local adapter, and the Remote adapter with its System.Text.Json and Newtonsoft.Json variants; see [The adapters](workspace.md#the-adapters). |

Do not substitute **codec** for an existing adapter. If a separate encoding and decoding
responsibility needs a name, discuss that distinction and its scope before adopting one. A choice
of serializer does not by itself establish a new name for the surrounding adapter.

## Populations and configuration

| Term | Definition and usage |
| --- | --- |
| Population | The objects and relations that a database holds. `save` writes a population to a file and `load` reads one; see [Populations](populations.md). |
| Setup | The run that builds a new population, `Setup.Apply()`; see [A new population](populations.md#a-new-population). |
| Upgrade | Loading a population that an earlier version saved and bringing it up to date with the new version; see [Upgrade a population](upgrade-a-population.md). |
| Command-line tool | The program of an application that creates, fills, saves, loads and upgrades its database; see [The command-line tool](command-line-tool.md). |
| Configuration root | The folder that `ALLORS_CONFIG_ROOT` names; see [Configuration](configuration.md#the-configuration-root). |
| Template | A configuration root that the repository provides under `config/`, for development and testing. |
| Admin connection | A connection in `ALLORS_SQLCLIENT` or `ALLORS_NPGSQL` that serves database management only; see [Configuration](configuration.md#admin-connections). |
