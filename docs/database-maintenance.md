# Maintain a SQL population

> **Status: Current.** How-to guide.

Use this procedure to initialize, import, enroll, restore, or recover a population served by the
Npgsql or SQL Server adapter. Each configured schema has its own generated population incarnation
and a state, `Maintenance` or `Ready`. Replacing a population can reuse object IDs and versions;
the incarnation distinguishes the replacement from the old population.

Normal workspaces use [complete reads and protected mutations](workspace-reads.md), which enforce
this state. The retained legacy server endpoints do not enforce it. Your maintenance procedure must stop
access explicitly. `RequireReady()` is an administrative metadata check, not a coherent read
snapshot or permission to keep serving during replacement.

## Take the application offline

1. Stop admission of new requests, drain active reads, and stop every serving host and writer,
   including background jobs and other processes connected to the population. Serialize
   administrative operations too. The adapter does not provide distributed maintenance exclusion.
2. Complete or validate the application's setup, recovery, and committed derivations before
   publishing `Ready`. Schema validation cannot establish that business data is correct or that
   derivations have committed.
3. Before reopening, recreate every serving database instance and its services/caches. Retire
   credentials and Local identity bindings that contain only a possibly reused population-local
   user ID, or revalidate them against a stable identity. Changing population metadata does not
   sign a user out. The authentication plug-in and application own that work; see
   [Authentication](authentication.md).

Keep the application offline throughout the applicable steps below. The `--offline` option is
your acknowledgment that these prerequisites are met; it does not detect running hosts or
validate the application's data for you.

## Inspect or enroll an existing population

The Core, Identity, and Entra command applications expose the same `population` command. Use
the command application and configuration of the concrete domain you are maintaining. For the
Core command application, run from the repository root:

```sh
export ALLORS_CONFIG_ROOT=/path/to/configuration
dotnet run --project dotnet/Core/Database/Commands -- population --action status
dotnet run --project dotnet/Core/Database/Commands -- population --action enroll --offline
```

Configuration appends the domain directory to `ALLORS_CONFIG_ROOT`: Core reads
`$ALLORS_CONFIG_ROOT/core/appsettings.json` and optional `core/commands/appsettings.json`;
environment variables override their settings. Check the configured adapter and database before
a mutating command. Replace `Core` with `Identity` or `Entra` to use those command applications,
which read the corresponding `identity` or `entra` configuration subdirectory.

| Action | Behavior |
| --- | --- |
| `status` | Reads and logs the current incarnation/state, or reports missing metadata. It does not enroll, initialize, or repair a population. This is the default action. |
| `enroll --offline` | Enrolls an existing population without changing its domain objects, IDs, or versions, then validates the schema before publishing `Ready`. Repeating it for an already ready population keeps the incarnation. It refuses an existing `Maintenance` state. |
| `renew --offline` | Assigns a fresh incarnation and enters `Maintenance`, validates the schema, then publishes `Ready`. Use only after validating a restored or recovered population. Every renewal creates another incarnation. |

Missing or malformed metadata is not evidence of readiness. Do not use `init` to enroll an
existing population: the command drops and recreates the configured database. If enrollment or
renewal fails, resolve the reported problem while the application remains offline.

The command behavior is checked by `RepeatedSetupTests.PopulationCommandRequiresOfflineBeforeOpeningTheDatabase`,
`PopulationCommandStatusDoesNotEnrollAnUninitializedSchema`,
`PopulationCommandEnrollDoesNotCompleteInterruptedMaintenance`, and
`PopulationCommandRenewRetainsDataAndPublishesANewIncarnation`.

## Initialize or import

The command applications distinguish physical database creation from population setup:

| Command | Completion boundary |
| --- | --- |
| `init` | Drops and recreates the configured physical database using its administrative connection. The new database has no ready population; follow with population setup or import. |
| `populate` | Enters maintenance before schema initialization and publishes `Ready` only after the concrete domain's setup and subsequent command work have committed. |
| `load -f population.xml` | Enters maintenance before importing, then publishes `Ready` only after the complete import and schema validation succeed. |
| Core's `upgrade -f population.xml` | Keeps maintenance open across import, compatibility checks, permission synchronization, and the command's upgrade/security commits. An early return or failure does not publish `Ready`. |

XML carries domain object IDs and versions, but never adopts the source population incarnation.
An import receives a new incarnation even when every imported ID and version is unchanged.
Import remains a multi-phase operation: failure does not mean its earlier work was rolled back.
`RepeatedSetupTests.PopulationLoadCommandPublishesReadyAfterSuccessfulImport` and
`PopulationLoadCommandFailureLeavesMaintenance` check the command's completion boundary.

Applications that call the SQL adapter directly must own the complete administrative workflow:

```csharp
var sql = (Allors.Database.Adapters.Sql.Database)database;
using var maintenance = sql.PopulationLifecycle.BeginMaintenance();
database.Init();
new Setup(database, config).Apply(); // Application setup commits before completion.
maintenance.Complete();
```

For import, call `database.Load(reader)` within the same kind of enclosing maintenance scope,
perform any required application validation or post-import work, then call `Complete()`.
`Init`, `Database.Load`, and a provider loader's public `Execute` never publish `Ready` by
themselves. Nested initialization and failed-import cleanup keep the enclosing population in
maintenance. Disposing an owner without completing it leaves `Maintenance` in the database.

## Restore, clone, or recover

A native backup includes its population metadata. A restored `Ready` row does not distinguish
the restored population from the one clients previously used. After a native restore or clone,
while all access remains stopped:

1. Validate the restored population and complete any application recovery, setup, and derivation
   work. Do not infer completion from an empty or structurally valid schema.
2. Run `population --action renew --offline`. This renews the restored incarnation before the
   population is marked ready again. The adapter cannot automatically detect an out-of-band
   restore; omitting renewal violates this maintenance procedure.
3. Inspect `population --action status`, recreate all serving database instances/services/caches,
   and perform the credential/identity checks described above before reopening admission.

After an interrupted or failed initialization/import, keep the application offline and finish
or validate recovery. Use a new complete initialization/import workflow, or explicitly renew the
validated recovered population. A restart does not automatically change `Maintenance` to `Ready`.
Ordinary committed application writes do not change the incarnation.
