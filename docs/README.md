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
| [Workspace connection](connection.md) | Reference | Current | The lowest layer of the .NET and TypeScript workspaces: the libraries, the bootstrap, the contract in ids, the records and permissions, what a pull does afterwards, the transports, the envelope, and which test pins what. |

## For maintainers

The pages under `internals/` describe how the platform is built and how work on it is planned.
An application must not rely on them: they can change with any version.

| Page | Status | Content |
| --- | --- | --- |
| [Domain inheritance](internals/domain-inheritance.md) | Current | Which part owns what, from `[Extends]` to the order of the hooks: the attribute, the parser, the template, the meta sort, the method compiler and the hand-written dispatch; why the ids decide the order; and the platform test domains of the Diamond tree. |
| [Workspace layers](internals/workspace-layers.md) | Planned | Which part owns what, from the protocol to the session API: the server's `Api`, the transports, the connection, the session and the domain library; why the cut is where it is; which parts exist; and the waves that follow: the wire schema, gRPC and signals. |

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
