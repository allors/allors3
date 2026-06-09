# Allors3

[![CI](https://github.com/allors/allors3/actions/workflows/ci.yml/badge.svg)](https://github.com/allors/allors3/actions/workflows/ci.yml)

## Configuration

Runtime configuration lives **outside** the source tree. Each server, command-line tool and integration
test reads its settings from the directory named by the **`ALLORS_CONFIG_ROOT`** environment variable:

```
$ALLORS_CONFIG_ROOT/<domain>/appsettings.json            # server
$ALLORS_CONFIG_ROOT/<domain>/commands/appsettings.json   # command-line tools
```

`<domain>` is `core`, `base` or `apps`. `ALLORS_CONFIG_ROOT` is **required**: if it is not set, or the
expected `appsettings.json` is missing, the app fails to start with a message telling you what to set.
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

Copy a provider's templates to a stable, FHS-friendly location (`/opt/allors` by default) and point the
apps at it:

```bash
./build.sh InstallConfig --provider npgsql --config-root /opt/allors
export ALLORS_CONFIG_ROOT=/opt/allors
```

Then edit the connection strings/secrets under `/opt/allors` (or supply them via environment variables)
for your environment. In containers, copy the templates during the image build and set
`ENV ALLORS_CONFIG_ROOT=/opt/allors`.
