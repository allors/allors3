---
title: "Documentation"
description: "Documentation for developers who build applications on Allors3, and for the maintainers of the platform."
audience: "Users and maintainers"
kind: "Index"
status: "Current"
---

> **Status: Current.**

Allors3 is a platform for applications built with domain inheritance. An application declares its
own domain, which extends Core and inherits its model and behaviour. The platform is System, which
holds the database and workspace engines, the adapters, the protocols, the metadata and the
generators, and Core, the foundational domain with authorization, authentication, the JSON API
and the building blocks of a server. An application owns its business domains and its user
interface.

These pages document `main`, the 3.2 line, which is not released.
[Versions and branches](versions.md) says where the line starts, where Base and Apps continue and
where the previous line of `main` is kept.

Users build applications on Allors3: they declare a domain that extends Core, configure the
programs and host the result. Maintainers change the platform.
[README.md](../../../../README.md) introduces the repository. The
[writing guide](internals/writing-documentation.md) defines the rules for these pages;
[Terminology](terminology.md) records the agreed names. These pages are for humans. Agents use the
same technical content, following the instructions in `AGENTS.md`.

The pages cover the boundary of the platform, domain inheritance, creating an application, the
model, objects and relations in the database, extents, derivations, populations and their
upgrade, security, the command-line tool, hosting a server, authentication, the JSON API,
configuration, the versions, and building and testing the repository. They do not yet describe
the workspace or the behaviour of each database adapter: for those, the source and its tests are
the reference.
[Getting started](getting-started.md) is the place to begin.

Allors3 is licensed under the GNU Lesser General Public License, version 3; the `LICENSE` file of
the repository holds the text. Ask questions and report problems through the
[repository's issues](https://github.com/allors/allors3/issues).

## For users

The following pages are written for users. Maintainers read them too.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Getting started](getting-started.md) | Tutorial | Current | Clone the platform, run one of its domain tests and write tests of your own on the test domain: objects, a relation, a derived value and an extent, without a database server. |
| [Domains](domains.md) | Explanation | Current | How the domain of an application extends Core, what it inherits, the order in which the parts of a method run, and how an application includes Core's code. |
| [Create an application](create-an-application.md) | How-to guide | Current | Start an application from the sample, generate and test its projects, and sign in to its server. |
| [The model](model.md) | Reference | Current | What a repository project declares: classes, interfaces, units, relations and their multiplicity, methods, ids, and the attributes with their defaults. |
| [Objects and relations](objects-and-relations.md) | Explanation | Current | Creating, relating and deleting objects in the database, what the platform maintains at both ends of a relation, transactions and change sets. |
| [Extents](extents.md) | How-to guide | Current | Query the objects of a class or an interface: filters, relations in a filter, sorting, combining extents and prefetching. |
| [Derivations](derivations.md) | Explanation | Current | How a derivation runs rules in cycles until nothing changes, what patterns match, errors and required roles, and why to derive before a commit. |
| [Write a rule](write-a-rule.md) | How-to guide | Current | Declare a derived role, write the rule that maintains it, register the rule and test it. |
| [Populations](populations.md) | Explanation | Current | How a database gets its population: initialization, the setup and its hooks, saving and loading, and what a load does with a file of an earlier model. |
| [Security](security.md) | Explanation | Current | How Core decides what a user may do with an object: users, user groups, permissions, grants, security tokens and revocations, what Core's setup creates, and where access is checked. |
| [Grant access](grant-access.md) | How-to guide | Current | Create the permissions, a `Role`, a user group and a grant in the setup, give the `Role` its permissions and test what a user may do. |
| [Upgrade a population](upgrade-a-population.md) | How-to guide | Current | Bring the population of an earlier version up to date: write the changes, declare what the new version removed, save, upgrade and test. |
| [The command-line tool](command-line-tool.md) | Reference | Current | The commands of an application's command-line tool, with the sample's as the example: settings, commands, options, output, exit codes and adapters. |
| [Host a server](host-a-server.md) | How-to guide | Current | Run the server of an application on PostgreSQL or SQL Server: write its settings, create the database and a user, start the server, sign in with curl and prepare a deployment. |
| [Authentication](authentication.md) | Explanation | Current | How a user signs in to the server, what the bearer token holds and how long it lasts, how the server checks it on every request, and what a token outlives. |
| [The JSON API](json-api.md) | Reference | Current | The endpoints of the server: sign-in, pull, sync, push, invoke, access and permission, with their requests, responses, errors and checks. |
| [Configuration](configuration.md) | Reference | Current | The configuration root, the order of its files, environment variables, the settings, the templates and the admin connections. |
| [Versions and branches](versions.md) | Reference | Current | The 3.2 line on `main`, the version branches, the archive branch, and where Base and Apps continue. |
| [Terminology](terminology.md) | Reference | Current | Agreed names for the platform, domains, the model, relations, objects, derivations, security, adapters, populations and configuration. |

## For maintainers

The pages under `internals/` describe how the platform and its repository are built. An
application must not rely on them: they can change with any version.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Architecture](internals/architecture.md) | Explanation | Current | The scope of the platform, the layout of the repository, the folders that carry inheritance, the separation of test scaffolding, the database adapters, and configuration and hosting. |
| [Build and test](internals/build-and-test.md) | How-to guide | Current | Prerequisites, the Nuke targets, the suites and what each needs, the TypeScript workspace and the CI workflows. |
| [Writing documentation](internals/writing-documentation.md) | How-to guide | Current | Audience, Diátaxis, status, Oxford English, stable terminology, visual explanations and review. |
| [Documentation website](internals/documentation-website.md) | Decision record | Current | How the website is built, what was reused, which facts moved here from the root of the repository, validation and publication. |

The writing guide defines [page kinds](internals/writing-documentation.md#choose-the-audience-and-purpose)
and [statuses](internals/writing-documentation.md#state-what-is-current). Audience, kind and
status describe different things: who a page serves, what it helps them do and whether it
describes current behaviour.
