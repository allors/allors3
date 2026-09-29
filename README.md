# Allors3

[![CI](https://github.com/allors/allors3/actions/workflows/ci.yml/badge.svg)](https://github.com/allors/allors3/actions/workflows/ci.yml)

Allors3 is an actively developed platform for applications built with **domain inheritance**.
An application declares its own domain that extends Core and inherits its model and behavior;
further domains can extend that domain in turn.

The [documentation](docs/README.md) is written for users and for maintainers. It starts with
the [kinds of domains](docs/domains.md).

## Direction for v3.2

The agreed scope for v3.2 is:

- Keep **System and Core**: the database and workspace engines, metadata, adapters, protocols,
  generators, foundational domain behavior, authorization, security defaults, and hosting.
- Add an **Identity** domain for authentication with ASP.NET Core Identity. Authorization stays
  in Core. Inheriting domains must use Core, and may use Identity or supply their own identity
  domain.
- Remove **Base and Apps**, including their business domains, applications, and Angular/Material
  and Blazor component libraries, without a separate continuation. Their code remains available
  on the `v3.1` branch. For applications built on Base or Apps, partners can be reached through
  [allors.com](https://allors.com).
- Make **signals the default API of every workspace**, both .NET and TypeScript, with breaking
  API changes allowed for this transition.
- Provide **thin UI integrations**. Downstream applications own their screens, forms, tables,
  navigation, and component libraries.
- Retain platform test domains, test servers, and small applications that exercise the integrations.

**Implementation status:** Base and Apps are still present; the Identity domain and the signals
API are planned. This documents the target; the code removal, the Identity domain, and the
reactive workspace changes have not landed yet.
See [ARCHITECTURE.md](ARCHITECTURE.md) for the platform boundary and inheritance rules.

## Development and releases

Pull requests are the default contribution workflow. The retained version branches are `main`,
`v3.0`, and `v3.1`. Work on `main` is intended for v3.2 once the transition settles.

[version.json](version.json) already specifies `3.2.0-alpha.{height}`. This is the development
version, not a completed v3.2 release. Changes are recorded in [CHANGELOG.md](CHANGELOG.md), and
contribution rules are in [AGENTS.md](AGENTS.md).

## Configuration

Runtime configuration lives **outside** the source tree. Each server, command-line tool and integration
test reads its settings from the directory named by the **`ALLORS_CONFIG_ROOT`** environment variable:

```
$ALLORS_CONFIG_ROOT/<domain>/appsettings.json            # server
$ALLORS_CONFIG_ROOT/<domain>/commands/appsettings.json   # command-line tools
```

For the retained platform, `<domain>` is `core`. The current tree also has `base` and `apps`
configuration, which will be removed with those domains. `ALLORS_CONFIG_ROOT` is **required**:
if it is not set, or the expected `appsettings.json` is missing, the app fails to start with a
message telling you what to set.
Environment variables override the JSON, so secrets can be supplied without editing files
(e.g. `ConnectionStrings__DefaultConnection=…`, `adapter=npgsql`).

### Choosing a database

The provider (SQL Server / PostgreSQL) is chosen by **which template you install**, not by your OS. The
repository ships templates under `config/<provider>/<domain>/`:

- `config/sqlclient/` — Microsoft SQL Server (defaults to SQL LocalDB)
- `config/npgsql/` — PostgreSQL (`localhost`, user `allors`)

### Running locally

Point `ALLORS_CONFIG_ROOT` straight at a provider template in the repo — no copy, no root access needed:

```bash
# PostgreSQL (macOS / Linux)
export ALLORS_CONFIG_ROOT="$(pwd)/config/npgsql"
```

```bat
:: SQL Server LocalDB (Windows)
set ALLORS_CONFIG_ROOT=%CD%\config\sqlclient
```

In an IDE, the servers ship `launchSettings.json` profiles (e.g. *Core (Postgres)* / *Core (SqlClient)*)
that set this for you.

### Installing config for deployment

Copy a provider's templates to a stable, FHS-friendly location (`/opt/allors` by default) and point
the server or command-line tool at it:

```bash
./build.sh InstallConfig --provider npgsql --config-root /opt/allors
export ALLORS_CONFIG_ROOT=/opt/allors
```

Then edit the connection strings/secrets under `/opt/allors` (or supply them via environment variables)
for your environment. In containers, copy the templates during the image build and set
`ENV ALLORS_CONFIG_ROOT=/opt/allors`.
