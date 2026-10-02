# Documentation

Documentation for Allors3, written for two audiences. Users build applications on Allors3: they
declare a domain, inherit Core, select plug-ins, and host the result. Maintainers change the
platform itself.

[README.md](../README.md) introduces the platform, [ARCHITECTURE.md](../ARCHITECTURE.md) draws
its boundary, and [AGENTS.md](../AGENTS.md) holds the rules for working in this repository,
including the rules for these pages.

## For users

The pages in this folder are written for users. Maintainers read them too.

| Page | Kind | Status | Content |
| --- | --- | --- | --- |
| [Domains](domains.md) | Explanation | Planned | The kinds of domains: functional domains, plug-ins and their hosts, and the concrete domain that selects plug-ins. |
| [Logging](logging.md) | Reference | Current | How Allors logs through `Microsoft.Extensions.Logging`, what the host does to receive the logs, and which messages Allors writes. |

## For maintainers

The pages under `internals/` describe how the platform is built and how work on it is planned.
An application must not rely on them: they can change with any version.

No internals page exists yet.

## Kind

Every user page is of one kind.

| Kind | What the reader gets |
| --- | --- |
| Tutorial | A first result, step by step. |
| How-to guide | The steps for one task. |
| Reference | Facts to look up. |
| Explanation | The model behind the facts. |

## Status

Every page opens with a status line.

| Status | Meaning |
| --- | --- |
| Current | Describes what the repository does today. |
| Planned | Agreed, and not built yet or only partly. The page says which parts exist. |
| Historical | Kept for its reasoning. It points to the page that holds the current truth. |
