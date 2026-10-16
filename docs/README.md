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
| [Domains](domains.md) | Explanation | Current | The kinds of domains: functional domains, plug-ins and their hosts, and the concrete domain that selects plug-ins; extending several domains, and the order of the domains, which decides the order of the hooks. |
| [Authentication](authentication.md) | Explanation | Current | How Core, an authentication plug-in and the application's domain share a sign-in: the session and the scheme per request, the user resolver, and the user factory. |
| [Sign in with Microsoft Entra ID](entra.md) | How-to guide | Current | The steps for an application that selects the Entra plug-in: the app registration, the configuration, `Startup`, the user factory, browsers, clients and programs, guests, and a check against a tenant. |
| [Logging](logging.md) | Reference | Current | How Allors logs through `Microsoft.Extensions.Logging`, what the host does to receive the logs, and which messages Allors writes. |
| [Complete workspace reads](workspace-reads.md) | Reference | Current | SQL workspace backends, ready connection factories and cleanup, complete reads, Session-owned baselines, protected Push/Invoke, shared results and explicit query completion. |
| [Maintain a SQL population](database-maintenance.md) | How-to guide | Current | Offline enrollment, initialization/import, restoration and recovery, incarnation/readiness metadata, and the duties before reopening service. |

## For maintainers

The pages under `internals/` describe how the platform is built and how work on it is planned.
An application must not rely on them: they can change with any version.

| Page | Status | Content |
| --- | --- | --- |
| [Domain inheritance](internals/domain-inheritance.md) | Current | Which part owns what, from `[Extends]` to the order of the hooks: the attribute, the parser, the template, the meta sort, the method compiler and the hand-written dispatch; why the ids decide the order; and the platform test domains of the Diamond tree. |
| [Workspace session state](internals/workspace-session-state.md) | Current | Ownership of editing baselines, merge/diff/reset characterization across .NET and TypeScript, unit merge equality, normal lifecycle/service ownership, protected Push/Invoke admission, result application and live authority. |
| [Workspace read foundations](internals/workspace-read-foundations.md) | Current | SQL population lifecycle and protected read/write ownership, trusted scope inputs, transaction-local security, committed ACL/version provenance, complete result construction, and strict transport entrypoints. |
| [Workspace read protocol](internals/workspace-read-protocol.md) | Current | .NET and TypeScript serialization and validation ownership, structured scope, separate PushRead/InvokeRead outcomes, shared wire fixtures, optional/null semantics, and server/transport/read-assembly use. |
| [Workspace read cache](internals/workspace-read-store.md) | Current | Internal connection-owned cache, immutable references, exact historical identities, current authority, atomic publication and lookup cleanup after drain. |
| [Workspace read assembly](internals/workspace-read-coordinator.md) | Current | Internal discover/complete assembly, exact cache inventories and publication under the connection's single operation owner. |
| [Workspace connection and read protocol](internals/workspace-connection-session-plan.md) | Historical | Completed four-step read and scoped PushRead plan, including cross-runtime and SQL-provider integration. Links to current behavior and the later caller cutover; records the separate endpoint-removal and signals boundary. |
| [Workspace caller migration inventory and design](internals/workspace-caller-migration-inventory.md) | Planned | Completed inventory and scoped test owners; prepared-declaration, procedure, standalone result-value, named-route and Memory workspace removal, protected ordinary Push and atomic Invoke/shared results, normal lifecycle/caller cutover and remaining implementation boundaries. |
| [Workspace experiment archive](internals/workspace-experiment-archive-2026-10-16/README.md) | Historical | Snapshot of the workspace experiment, its validation limits, exact test maps and the decision to restart Local-adapter removal from main. |

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
