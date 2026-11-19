# Allors3

[![CI](https://github.com/allors/allors3/actions/workflows/ci.yml/badge.svg)](https://github.com/allors/allors3/actions/workflows/ci.yml)

Allors3 is an actively developed platform for applications built with **domain inheritance**.
An application declares its own domain that extends Core and inherits its model and behaviour.

The platform is **System**, the database and workspace engines, adapters, protocols, metadata and
generators; **Core**, the foundational domain behaviour, authorization, the JSON API and the
server building blocks; the TypeScript system and core libraries; and the generator templates.
An application owns its business domains, screens, forms, tables, navigation and component
libraries.

`main` is the 3.2 line, which is not released. **Base and Apps** are not part of the platform;
their code remains on the `v3.1` branch.

## Documentation

The [documentation](docs/src/content/docs/index.md) is written for the developers of applications
and for the maintainers of the platform. It is published at
[docs.allors.com/allors3](https://docs.allors.com/allors3/) from `main`; when a published page
differs from the checkout, the page in the checkout describes the checkout.

| Page | Read it to |
| --- | --- |
| [Domains](docs/src/content/docs/domains.md) | understand how the domain of an application extends Core and includes its code |
| [Configuration](docs/src/content/docs/configuration.md) | give a server, the command-line tool and the tests their settings |
| [Versions and branches](docs/src/content/docs/versions.md) | find the 3.2 line, the version branches and the archive branch |
| [Architecture](docs/src/content/docs/internals/architecture.md) | know the boundary of the platform and the rules that keep it inheritable |
| [Build and test](docs/src/content/docs/internals/build-and-test.md) | build the repository, run the suites and read the CI workflows |

Changes are recorded in [CHANGELOG.md](CHANGELOG.md). Agent instructions are in `AGENTS.md`.

## Build

```sh
./build.sh Generate                 # a fresh clone does not compile until this has run
./build.sh DotnetCoreDatabaseTest   # the Core database suite on the Snapshot adapter, without a database server
```

On Windows use `build.cmd`. [Build and test](docs/src/content/docs/internals/build-and-test.md)
describes the targets, the adapters and what each suite needs.
