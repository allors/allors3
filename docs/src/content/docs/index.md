---
title: "Documentation"
description: "Documentation for Allors3 application developers and platform maintainers."
audience: "Users and maintainers"
kind: "Index"
status: "Current"
---

> **Status: Current.**

Documentation for Allors3, written for two audiences. Users build applications on Allors3: they
declare a domain, inherit Core, select plug-ins, and host the result. Maintainers change the
platform itself.

[README.md](../../../../README.md) introduces the platform and [ARCHITECTURE.md](../../../../ARCHITECTURE.md) draws
its boundary. The [writing guide](internals/writing-documentation.md) defines the rules for
these pages; [Terminology](terminology.md) records the agreed names. These pages are for
humans. Agents use the same technical content, following the instructions in `AGENTS.md`.

## For users

The following pages are written for users. Maintainers read them too.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Domains](domains.md) | Explanation | Current | The kinds of domains: functional domains, plug-ins and their hosts, and the concrete domain that selects plug-ins; extending several domains, and the order of the domains, which decides the order of the hooks. |
| [Authentication](authentication.md) | Explanation | Current | How Core, an authentication plug-in and the application's domain share a sign-in: the session and the scheme per request, the user resolver, and the user factory. |
| [Sign in with Microsoft Entra ID](entra.md) | How-to guide | Current | The steps for an application that selects the Entra plug-in: the app registration, the configuration, `Startup`, the user factory, browsers, clients and programs, guests, and a check against a tenant. |
| [Logging](logging.md) | Reference | Current | How Allors logs through `Microsoft.Extensions.Logging`, what the host does to receive the logs, and which messages Allors writes. |
| [Terminology](terminology.md) | Reference | Current | Agreed names for domain concepts, relations and adapters, with links to their definitions. |

## For maintainers

The pages under `internals/` describe how the platform is built and how work on it is planned.
An application must not rely on them: they can change with any version.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Domain inheritance](internals/domain-inheritance.md) | Explanation | Current | Which part owns what, from `[Extends]` to the order of the hooks: the attribute, the parser, the template, the meta sort, the method compiler and the hand-written dispatch; why the ids decide the order; and the platform test domains of the Diamond tree. |
| [Workspace session state](internals/workspace-session-state.mdx) | Explanation | Current | A guided example of pinned data/security snapshots, session isolation, diff and Reset without another request. |
| [Hybrid Sync and session snapshots](internals/workspace-hybrid-sync.md) | Explanation | Current | Bundled Sync security, exact known-version omission and immutable cache bindings for session snapshots. |
| [Writing documentation](internals/writing-documentation.md) | How-to guide | Current | Audience, Diátaxis, status, Oxford English, stable terminology, visual explanations and review. |
| [Documentation website](internals/documentation-website.md) | Decision record | Current | Agreed website design, implementation subtasks, validation and publication. |

The writing guide defines [page kinds](internals/writing-documentation.md#choose-the-audience-and-purpose)
and [statuses](internals/writing-documentation.md#state-what-is-current). Audience, kind and
status describe different things: who a page serves, what it helps them do and whether it
describes current behaviour.
