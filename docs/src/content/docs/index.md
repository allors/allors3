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

The pages cover the boundary of the platform, domain inheritance, configuration, the versions, and
building and testing the repository. They do not yet describe the database and workspace APIs,
derivations, security or the JSON API: for those, the source and its tests are the reference.

Allors3 is licensed under the GNU Lesser General Public License, version 3; the `LICENSE` file of
the repository holds the text. Ask questions and report problems through the
[repository's issues](https://github.com/allors/allors3/issues).

## For users

The following pages are written for users. Maintainers read them too.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Domains](domains.md) | Explanation | Current | How the domain of an application extends Core, what it inherits, the order in which the parts of a method run, and how an application includes Core's code. |
| [Configuration](configuration.md) | Reference | Current | The configuration root, the order of its files, environment variables, the settings, the templates and the admin connections. |
| [Versions and branches](versions.md) | Reference | Current | The 3.2 line on `main`, the version branches, the archive branch, and where Base and Apps continue. |
| [Terminology](terminology.md) | Reference | Current | Agreed names for the platform, domains, relations, adapters, populations and configuration. |

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
