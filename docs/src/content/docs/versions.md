---
title: "Versions and branches"
description: "The 3.2 line on main, the version branches, the archive branch, and where Base and Apps continue."
audience: "Users and maintainers"
kind: "Reference"
status: "Current"
---

> **Status: Current.** Reference for users and maintainers.

Allors3 is developed on `main`, which is the 3.2 line. These pages document that line. Use the
documentation of the relevant branch when working on another version: a published page is not
evidence of what another branch does.

## Branches

| Branch | Holds |
| --- | --- |
| `main` | The 3.2 line: the platform, which is System and Core. Not released. |
| `v3.1` | The 3.1 line. Base and Apps continue here. |
| `v3.0` | The 3.0 line. |
| `archive/main-2026-10` | The previous line of `main`, up to 27 October 2026. |
| `archive/workspace-connection-session-layers`, `archive/workspace-protected-reads` | Two spikes that fork from the previous line. |

`main`, `v3.0` and `v3.1` are the retained version branches. There is no `v3.2` branch: work on
`main` is the 3.2 line.

## The 3.2 line

The 3.2 line starts from commit `0e907fea68`, the state of the platform of 8 May 2026 that a
downstream application inlined and runs in production, and takes over that application's changes
to System and Core.

Its scope is the platform:

- **System**: the database and workspace engines, the adapters, the protocols, the metadata and
  the generators;
- **Core**: the foundational domain behaviour, authorization, authentication, the JSON API and
  the building blocks of a server;
- the TypeScript system and core libraries;
- the generator templates.

**Base and Apps are not part of the platform.** Their business domains, applications, component
libraries, configuration and build targets remain available on the `v3.1` branch. An application
owns its business domains, and its screens, forms, tables, navigation and component libraries;
[Domains](domains.md) describes how it builds on Core.

## The version number

[`version.json`](../../../../version.json) specifies `3.2.0-alpha.{height}`, which
Nerdbank.GitVersioning turns into the version of a build; `{height}` is the number of commits
since the file set that version. It is the development version of the line and not a completed
release. The repository has no release of the 3.2 line, and an application takes the platform as
source, as [Domains](domains.md#how-an-application-includes-core) describes.

## The archive branch

`archive/main-2026-10` keeps the line that `main` held before the 3.2 line. It holds work that
the 3.2 line does not have, or has only in part:

- fixes that have not returned yet;
- the security hardening of the server, of which the changelog records the fixes that returned;
- the rename of Core's test domain to Test;
- the Identity and Entra authentication plug-ins;
- a domain that extends several domains;
- the changes to the workspace API;
- the first documentation website, with the pages that describe that work.

`ARCHIVE.md` on that branch lists its content in detail. Nothing from the archive branch returns
wholesale. Fixes return first, from `v3.1`, one pull request at a time; a feature returns when an
application needs it.

## Changes

Changes reach `main` through pull requests. [CHANGELOG.md](../../../../CHANGELOG.md) records the
changes of the 3.2 line, including the ones that break an earlier database or API and what to do
about them. The changelog of the previous line is on the archive branch, and the changelog up to
v3.1 is on the `v3.1` branch.
