# Changelog

All notable changes to Allors3 are documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses
[Semantic Versioning](https://semver.org/). The changelog of the previous line of `main` is on
the archive branch `archive/main-2026-10`; the changelog up to v3.1 remains on the `v3.1` branch.

## [Unreleased]

### Added

- The Snapshot adapter, `Allors.Database.Adapters.Snapshot`: an in-memory database that many
  transactions share, also on different threads, for test runs without a database server (#307).
  A transaction reads the immutable snapshot that was current when it started or when it last
  committed or rolled back, writes to a private overlay and publishes a new snapshot when it
  commits; commits are serialized by one lock. A commit fails with `ConcurrencyException`, a
  `DbException`, when another transaction changed or deleted an object that this transaction
  changed or deleted after it read it, as a database server does under snapshot isolation; a
  change to the other end of a relation is reconciled instead (#311). The adapter saves and loads
  the population format of the other adapters, and `TakeRestorePoint` and `Restore` put a
  population back without reloading it, so a test suite resets its database in memory.
  `DatabaseBuilder` accepts the adapter name `snapshot` without a connection string, and
  `config/snapshot` is a configuration root template for it. The shared adapter tests gain
  `ConcurrencyTest`, which the Snapshot, SqlClient and Npgsql adapters pass alike.
- The Nuke build takes `--provider` with `snapshot`, `sqlclient`, `npgsql` or `localdb`: the
  template under `config/` that is the configuration root of the tests, the command-line tool and
  the server that its targets start. It wins over `ALLORS_CONFIG_ROOT` (#314).
- `Database.EnsureIndexes()` of `Allors.Database.Adapters.Sql`, which brings the indexes of the
  schema to what the adapter wants, and the `index` command of Core's command-line tool, which
  calls it. For an adapter that keeps no indexes the command does nothing (#388).
- The documentation website of the 3.2 line. `docs/` holds an Astro and Starlight website that
  documents `main`, with a Markdown export of every page and an agent index, published at
  `https://docs.allors.com/allors3/` by a manual run of the Documentation workflow. Its pages are
  the home of the facts that the root of the repository held: `README.md` is an introduction that
  links to them, and `ARCHITECTURE.md` is gone, replaced by the pages Domains and Architecture. The
  first website, with the pages of the previous line, stays on `archive/main-2026-10` (#387).

### Changed

- `main` restarts from `0e907fea68`, the platform state of 8 May 2026 that a downstream
  application inlined and runs in production, and takes over that application's changes to
  System and Core: configuration variants per environment and platform, with environment
  variables winning over every file; a password policy of 12 characters with 4 distinct ones; a
  self-service password change that requires the current password and rotates the security
  stamp; SQL Server test provisioning from the admin connection in `ALLORS_SQLCLIENT`; the
  adapters-json fix for the permissions a concurrent pull left pending; and dependency updates.
  The previous line of `main`, up to 27 October 2026, is kept on `archive/main-2026-10`. The
  fixes released on `v3.1` after 8 May 2026 and the archived work are to be ported in later pull
  requests.
- The Core and TypeScript suites default to the Snapshot adapter and need no database server: the
  test projects and the Nuke build use the `config/snapshot` template when `ALLORS_CONFIG_ROOT` is
  not set (#314). The local workspace tests build their database from the configuration root instead
  of constructing a Memory database (#317), the remote API tests create their users through the
  test-only `Test/CreateUser` of the Core test server instead of a database connection of their own
  (#316), and the shared pull tests compare the objects of a pull without sorting regardless of
  their order (#318). Every Core and TypeScript suite now runs on the Snapshot adapter, on SQL
  Server and on PostgreSQL. The Core domain tests run on the Memory adapter, the adapter for a
  single transaction over a population, unless the configuration root names SQL Server or PostgreSQL
  (#320).
- The SqlClient adapter suite skips its provisioning theory, which creates a SQL Server login, when
  the admin connection names LocalDB, which accepts Windows authentication only (#320).
- CI runs every suite as a job of its own, in tiers (#320). A pull request, whatever its base branch
  (#321), and a push to `main` run on Linux the adapter suites, those of SQL Server and PostgreSQL
  against service containers, and the Core database, Core workspace and TypeScript suites on the
  Snapshot adapter. A push to `main` runs those Core and TypeScript suites on SQL Server and
  PostgreSQL as well. A nightly workflow, which also starts on demand, runs every suite on Windows:
  the adapter suites, and the Core and TypeScript suites on the Snapshot adapter, on SQL Server
  LocalDB and on PostgreSQL. The aggregate check `CI result` stands for the suites of a pull request
  and of a push; a change that touches only documentation skips them: the files under `docs/`, the
  documentation workflow, and `AGENTS.md`, `CHANGELOG.md` and `README.md` at the root. A push to a
  `feature/` or `issue/` branch no longer starts a run of its own: a branch runs through its pull
  request, or on demand.
- Nerdbank.GitVersioning versions the line as `3.2.0-alpha.{height}`, with public-release
  versions on `main` and the `v*` branches.
- The TypeScript workspace runs on nx 23, jest 30 and TypeScript 6 and depends on cross-fetch and
  tslib only.
- Dependabot keeps the GitHub Actions of the workflows current, in one grouped pull request a week.
- The PostgreSQL adapter tests take their admin connection from `ALLORS_NPGSQL`, as the SQL Server
  tests take theirs from `ALLORS_SQLCLIENT`, and create their databases through `Provisioning` in
  `Allors.Database.Adapters.Sql.Npgsql` (#80, released on `v3.1` in June 2026);
  `ALLORS_TEST_NPGSQL_CONNECTION` is accepted as an alias. The account `postgres` with the password
  `root` on `localhost` is no longer built in, and CI's PostgreSQL service runs as the user `allors`.
- The SqlClient adapter sends its statements itself. The 27 commands that called a stored procedure
  send a statement with the same parameters: the statement that their procedure held, except for the
  creation of several objects in one call, `ITransaction.Create(class, count)`, which sends one
  `INSERT` with two `OUTPUT` clauses where its procedure filled a table variable in a loop. SQL
  Server accepts that statement only when the table of the class has no enabled trigger, foreign key
  or check constraint and the objects table no enabled insert trigger; the adapter creates none of
  these. The members `ProcedureNameFor…` of `Mapping` in `Allors.Database.Adapters.Sql` are named
  `CommandTextFor…`, and those of the SqlClient adapter hold statements (#366).
- The Npgsql adapter prepares its commands. A transaction has 27 commands that are not extents: they
  create, instantiate and delete objects, read and write roles, read associations, read and update
  versions, and prefetch. Each of them is prepared when it is first executed, so that PostgreSQL
  parses and plans it once for a connection instead of at every execution. Npgsql keeps a prepared
  statement for as long as its physical connection lives: a later transaction that gets the
  connection from the pool finds the statement there, and the server holds it for that long as
  well. That includes the `UPDATE` for the unit roles that changed together on one object, which is
  a statement of its own for every combination of those roles. The statements of an extent, of a
  load and of a save are not prepared. `Init` closes the pooled connections of its connection
  string after it has recreated the schema, because a statement that was prepared for the earlier
  schema fails once the type of its result has changed. `ICommand` of
  `Allors.Database.Adapters.Sql` has the new member `Prepare()`, which `Commands` and `Prefetcher`
  call for every command that they create, and which another implementation of `ICommand` has to
  add. The SqlClient adapter does nothing in it (#370).
- The Npgsql adapter sends its statements itself and calls no function of the schema. The 25
  commands of a transaction that called a function send the statement that their function held,
  with the same parameters and with an array behind a sub-select, `UNNEST((SELECT :p_arr_o))`, so
  that PostgreSQL plans the statement without the values of the array. The lookup of a to-one role
  or association sends the `SELECT` of its function. The creation of an object, the creation of
  several objects in one call, `ITransaction.Create(class, count)`, and the deletion of an object,
  whose functions held more than one statement, each send one statement with a data-modifying
  `WITH`. A load sends the statements for its relations with their arrays as they are: each of
  them runs once, with all the relations that it sets. The `CommandTextFor…` members of the
  adapter's `Mapping` hold statements, as in the SqlClient adapter (#378).
- The SqlClient and Npgsql adapters ensure the indexes of their schema, where they created them
  only when `Init` built the tables. A database object reads the indexes of the schema once, before
  its first transaction: it creates the ones that are missing and drops the ones of the adapter
  that it no longer wants. `EnsureIndexes()` does the same on request, and `Load` calls it at its
  end, so that the indexes are built on the loaded rows. `Init` creates no index any more. So an
  index that the schema lacks, because its relation type was marked as indexed after the tables
  were created or because somebody dropped it, appears at the next start; before, the schema
  counted as valid without it and `Init` did not bring it back. An index counts as present by its
  table and the column that it leads with, so an index that somebody else created is accepted. An
  index is dropped only when it has the name that the adapter gives to an index for a unit role
  and that role is not marked as indexed; an index on a composite relation is never dropped. Each
  index is built on a connection of its own, in a transaction of its own under a lock for the
  schema, `pg_advisory_xact_lock` or `sp_getapplock`, and without a command timeout. While an index
  is built, writes to its table wait, and on SQL Server a snapshot transaction that was open fails
  when it next uses that table: give a database that is in use its indexes at deployment, with the
  `index` command or a call of `EnsureIndexes()`. When an index can not be created, for example
  because the login does not own the table, no transaction opens and the message names the
  statements to run. A database that lacked an index started before this change, and now refuses
  to until it has the index (#388).
- `Init` of the SqlClient and Npgsql adapters empties a valid schema in one transaction and
  truncates only the tables that hold data, where it truncated every table, each in a transaction
  of its own. An `Init` that fails has changed nothing; before, it left the tables that it had
  reached empty, the objects table first. The objects table is truncated whatever it holds, so the
  numbering of the objects starts again as before. On PostgreSQL `Init` locks every table of the
  adapter, `LOCK TABLE … IN ACCESS EXCLUSIVE MODE`, and then truncates in one statement the tables
  whose file holds a page, and whatever its size every table that has partitions or children or
  that uses another access method than `heap`. PostgreSQL gives a truncated table and each of its
  indexes a new file, so an `Init` of a database that holds little data no longer creates a file
  for every table and index of the schema. On SQL Server one batch truncates each table that holds
  a row, and looks for that row under an exclusive lock on the table, `WITH (TABLOCKX)`, so that it
  waits for a row that another transaction has not committed, also with read committed snapshot
  on. `Init` keeps its lock on each table until it ends, where it let go of a table after the
  truncate. A transaction that is in flight, holds one of the tables and then asks for one that
  `Init` holds ends in a deadlock with it, and the server ends one of the two. The command timeout
  of the connection string bounds the wait for all tables together, where each table had its own.
  On PostgreSQL the transaction takes room in the lock table of the server, which all sessions
  share: a lock for every table of the adapter and for the sequence of the objects table, and for
  a table that is truncated also for each of its indexes, for its TOAST table and for the index of
  that table. PostgreSQL has room for `max_locks_per_transaction` × (`max_connections` +
  `max_prepared_transactions`) locked objects, 6,400 with its defaults; before, `Init` held the
  locks of one table at a time. When the room runs out PostgreSQL refuses a lock with
  `out of shared memory`, and `Init` fails: raise `max_locks_per_transaction`, which takes a
  restart of the server. On SQL Server `Init` asks for the state of snapshot isolation and sends
  the batch that switches it on only when it is off (#392).
- The Npgsql adapter reads its schema from the catalogue of PostgreSQL, `pg_namespace`, `pg_class`,
  `pg_attribute` and `pg_type`, where it read the views `schemata`, `tables` and `columns` of
  `information_schema`. It is the read of the validation, which `Init` starts with and which a
  database object does before its first transaction. The read keeps the conditions of the views, so
  the validation sees what it saw: a schema that the login owns or may use, the base tables and the
  columns that the login owns or may touch, and for a column of a domain the type that the domain
  is made of. The query on the views joined `tables` and `columns` on the name of the table, which
  can take PostgreSQL through the columns of the whole schema once for every table (#394).
- The SqlClient and Npgsql adapters give every composite relation an index, where they gave one
  only to a composite relation that the domain marks as indexed. The index is on the column of the
  relation in the table of a class, or on the role column of the table of the relation. The
  adapters look rows up by these columns themselves: to read an association or a one-to-many role,
  to delete an object, and to find the object that has a role before they set a one-to-one role or
  add to a one-to-many role. `[Indexed]` stays the hint of the modeller for a unit role, which gets
  an index only when it is marked; on a composite relation the mark no longer changes anything in
  these adapters. A database that was created before this change gets the indexes that it lacks
  before the first transaction of the next database object that starts on it (#388): give a
  database that is in use its indexes at deployment, with the `index` command or a call of
  `EnsureIndexes()`, and with a login that may create an index on the tables, because no
  transaction opens until the indexes exist. A database whose domain marks every composite relation
  as indexed has these indexes already. Creating an object and writing a relation now maintain
  these indexes as well, and the indexes take room in the database. On PostgreSQL two index names
  that are equal in their first 63 bytes refuse every transaction (#407), which held for the
  relation types that are marked and now holds for every composite relation (#417).

### Fixed

- The Local workspace adapter releases its database transaction after Pull, Push and Invoke, and
  Sync selects the roles of the connection's workspace only (#44, #45, #46, #259).
- Loading a population in the Memory, SqlClient and Npgsql adapters keeps a unit value that
  precedes a self-closing relation element; the loaders replaced it with an empty value (#306).
- Loading a population reports every relation of a relation type that the meta population no
  longer knows; the reporters stepped into the next relation, so the Memory adapter failed such a
  load and the SQL adapters reported every second relation only (#308).
- After a load, the Memory and Snapshot adapters initialize the database services again, so that
  the services read the loaded objects. The services kept what they had read while the population
  was still empty: with Core's services the loaded permissions were present without their metadata,
  and constructing security failed with a `KeyNotFoundException` (#323).
- Loading a population reads an object type element without objects, self-closing or with an end
  tag, as an element that holds no objects. The form with an end tag failed the load on every
  adapter, and on the SqlClient and Npgsql adapters a self-closing element made the loader skip the
  object type element that followed it directly, so its objects were not loaded (#325).
- On PostgreSQL, `Init` starts the numbering of the objects again and `Load` continues it after the
  highest loaded id, as the Memory, Snapshot and SqlClient adapters do. The Npgsql adapter left the
  sequence of the object ids where it was, so an object created after a load could get the id of a
  loaded object and fail its transaction with a duplicate key: after a load into a new database,
  and after `Init` had created the tables again for a changed schema (#326).
- On the SqlClient and Npgsql adapters a failure while a load reads or writes relations stops the
  load. The loaders rolled back the relation type that failed and went on, so the load returned
  normally without any relation of that relation type. That happened for a composite relation that
  the schema does not allow when no `RelationNotLoaded` handler is attached, for an exception
  thrown by such a handler, and for an error of the database (#329).
- Loading a population reports a value that its role type cannot hold as it is, as it reports a
  value that the unit type cannot read: a string that is longer than the size, and a decimal that
  the precision and the scale cannot hold exactly. The adapters cut, rounded or kept such a value,
  or the database failed the load (#330).
- Setting a decimal on a role gives the same result on every adapter: a decimal that the precision
  and the scale of its role type cannot hold exactly is refused with an `ArgumentException`, and
  none is rounded. The Memory and Snapshot adapters kept the decimal as it was set; the SqlClient
  and Npgsql adapters rounded it to the scale at the commit, and failed the commit for a decimal
  that was too large (#331, #349).
- Loading a population on the SqlClient and Npgsql adapters stops at an element that the loader
  does not know, as it does on the Memory and Snapshot adapters, with the same error:
  `Unknown child element <…> in parent element <…>`. The SQL loaders skipped such an element with
  everything in it and returned normally, so a file with `objects`, `relations`, a `database`
  element, an object type, a relation type or a relation under another name or in another place
  loaded as an empty database, or without those objects or relations. A self-closing element under
  an unknown name among the relation types, which every adapter skipped, stops the load too (#335).
- Loading a file without a `population` element fails with an `ArgumentException`. Every adapter
  returned normally and left an empty database, for `<allors/>` as for any other XML document. A
  population without content still loads as a population without objects (#336).
- Loading a population refuses an object with an id of 0 or below, with an `ArgumentException`: the
  id of a database object is 1 or higher. Every adapter loaded such an object, under an id that the
  platform reads as no object or as a workspace object that is not saved yet (#337).
- Loading a population stops at a relation of an object that the file does not hold, as its
  association or as a role, with an `ArgumentException` that names the relation type and the
  objects. With a `RelationNotLoaded` handler attached every adapter reported such a relation and
  went on, as if a schema change explained it, and a file with its relations before its objects
  loaded without any relation. A relation that a schema change explains is still reported, but only
  when the file holds all its objects; a role that is not a number in such a relation stops the
  load, where it was reported with its text. A handler can no longer let a relation of a missing
  object pass: such a file has to be repaired before it loads (#338).
- `Save` on SQL Server and PostgreSQL wrote a relation to an object that another transaction had
  deleted, and a load refuses such a file. `Save` now leaves out a composite relation whose
  association or role is not among the objects that it writes (#343).
- The `save` command of Core finds its file as `load` does: the `-f` option, then the
  `populationFile` setting, then `population.xml`. The option had the default `population.xml`, so
  that with the setting a `save` without `-f` wrote `population.xml` in the working directory,
  another file than the one that a `load` without `-f` reads. The new `Commands.Tests` project in
  the Core database test suite covers it (#345).
- A unit value that the four adapters do not hold alike is refused on every adapter: a float that is
  NaN or an infinity, a string with the character U+0000 or with a lone surrogate, and a binary that
  is longer than the size of its role type. Setting a role to one throws an `ArgumentException`, and
  loading a population reports such a float, a string with U+0000 and such a binary as it reports a
  value that its role type cannot hold. Every adapter accepted these values when a role was set. The
  Memory and Snapshot adapters held them, and saved U+FFFD in the place of a lone surrogate. On SQL
  Server the commit failed for the float, stored U+FFFD in the place of a lone surrogate and kept
  the bytes of the binary up to the size without an error; a load stopped at the float and loaded
  the binary cut to the size without a report. On PostgreSQL the commit failed for both strings,
  and a load stopped at the string with U+0000 (#347).
- A date and a decimal that a role returns have one form on every adapter, whenever they are read.
  Every date has the kind `Utc`: `DateTime.MinValue` and `DateTime.MaxValue` kept the kind in which
  they were set, and the SqlClient and Npgsql adapters returned them with the kind `Unspecified`
  once they came from the database. A decimal has the decimals of the scale of its role type, so
  that 1.5 is 1.50 at the scale 2: the Memory and Snapshot adapters kept the decimals with which it
  was set or loaded, in a saved file as well, and the SqlClient and Npgsql adapters did so until the
  commit. A zero has no sign: a negative zero, such as -0.004 rounded to two decimals, stayed a
  negative zero on the Memory and Snapshot adapters, and on the SqlClient and Npgsql adapters until
  the commit (#350).
- The validation of a meta population refuses a decimal role type outside a precision from 1 to 28
  and a scale from 0 to its precision, with an error that names the role type and states the rule,
  so that the generators fail on such a model and no database starts with it. Nothing checked a
  precision or a scale: SQL Server cannot create the column for a scale below zero, for a scale
  above the precision or for a precision above 38, and neither server for a precision of 0; at a
  scale above 28 the Npgsql adapter could not read the decimal that it had stored; and at a
  precision above 28 the SqlClient and Npgsql adapters stored a decimal for whose decimals `decimal`
  has no room, and then failed to read it and to save the population. The exception of
  `ObjectFactory` for a meta population that is not valid has the errors of the validation as its
  message, which was the name of the class `ValidationLog` (#351).
- Loading a population reads a value only from the text that a save writes for it, and reports any
  other text as it reports a value that the unit type cannot read. The adapters read what
  `XmlConvert`, `Guid.Parse` and `Convert.FromBase64String` take as well: white space around a value
  and in base64; a plus sign and zeros before the digits of a number; `1` and `0` as a boolean; a
  date without a zone or with an offset, and a day, a time, a month or a year as a date, so that the
  integer `2000` loaded as 1 January 2000 and a time as that time on the day of the load; a GUID in
  upper case, without hyphens or in braces, and one with a sign in place of a digit as another GUID;
  a decimal with more than 28 decimals, rounded; and in a string bytes that are not UTF-8, as
  U+FFFD. A float is read by the form of its text, so that a file with the digits that the .NET
  Framework wrote for a float still loads. A date with a fraction of a second beyond milliseconds is
  reported as well, unless it is the largest date: every adapter returned it cut to milliseconds,
  and a later save wrote it cut to milliseconds on the Memory and Snapshot adapters, cut to
  microseconds on the Npgsql adapter and as it was on the SqlClient adapter.
  `Serialization.ReadString` refuses a text that no save writes with a `FormatException` (#352).
- `Save` writes the same file for one population on every adapter: the file of the population as its
  roles return it. The Memory and Snapshot adapters wrote the id of an object type and of a relation
  type without hyphens, where the SqlClient and Npgsql adapters write it with hyphens, and the
  Memory adapter wrote the roles of a to-many relation in the order of its collection, which
  followed the order in which they were added and removed and, after a load, the order of the file.
  Both now write the ids with hyphens, and the Memory adapter writes the roles ascending by id. The
  SqlClient and Npgsql adapters wrote a date as the database holds it, also a date with a fraction
  of a second beyond milliseconds, which a role does not write. They now write a date as a role
  returns it, cut to milliseconds unless it is the largest date, so that a load of the file does
  not report it. A load reads the earlier forms as before (#353).
- Setting a unique role or a binary role to a unit of another type is refused with a message that
  names the unit: `RoleType is not a Unique.` and `RoleType is not a Binary.` Both said
  `RoleType is not a Boolean.` (#358).
- A load that fails leaves a database as `Init` leaves it, on every adapter: without objects, and
  with the first id for a new object. The Memory adapter kept what it had loaded before the failure,
  uncommitted in its one transaction, so that a later commit committed it and a save wrote it. The
  Memory and Snapshot adapters gave the next new object the id after the highest id that the failed
  load had read (#362).
- When a load fails on the SqlClient and Npgsql adapters, its exception reaches the caller with the
  stack trace of where it was thrown. The adapters rethrew it after they had initialized the
  database again, and the stack trace then started at `Database.Load` (#363).
- When the stream of a save fails on the SqlClient and Npgsql adapters, the caller gets the
  exception of the stream, as on the Memory and Snapshot adapters. The SQL adapters gave
  `InvalidOperationException: The Writer is closed or in error state.` in its place: their relation
  writers wrote an end element to the writer that had just failed (#364).
- A transaction on the SqlClient and Npgsql adapters keeps the command that sets a unit role, as it
  keeps its other commands. It created that command again for every flush that set the role (#370).
- On the SqlClient and Npgsql adapters the prefetch of a to-one association with a nested policy
  returns when its transaction already knows that an object has no such association: from a read,
  from the removal of the role or, for a one-to-many relation type, from an earlier prefetch of the
  association. It threw a `NullReferenceException`, for a one-to-one or one-to-many relation type
  that the adapters keep in a column of a class table (#372, #374).
- On the SqlClient and Npgsql adapters a prefetch uses what its transaction already knows of a
  to-many association, and of a to-one association of a relation type that the adapters keep in a
  relation table. It applies a nested policy to such an association when the transaction knows it,
  and it does not fetch a to-many association again that the transaction knows for every object of
  the prefetch. The policy was applied only to an association that the prefetch fetched, so that
  reading a role that the policy names still went to the database, and a to-many association was
  fetched at every prefetch (#375).
- On the SqlClient and Npgsql adapters the prefetch of a composite role with a rule without a nested
  policy loads the references of the role objects in one command, also for a role that the cache of
  the database already holds. For such a role it loaded none, so that a transaction that did not
  hold the role objects yet sent a command at the read of the role of each object (#376, #377).
- The Memory adapter's extent sort treats two objects without a value for a sort order as equal
  and lets the next sort order decide; the comparer contradicted itself, so short lists kept an
  arbitrary order and longer ones could fail the sort (#309).
- Initializing a database again in the same process forgets the objects of the previous
  population: Core's database services kept their caches of unique ids, classes, permissions,
  security and versioned ids across `Init`, `Load` and `Restore`. On PostgreSQL, where the object
  ids continued after `Init` until #326, the second `Test/Setup` of the Core test server failed;
  the Snapshot adapter and SQL Server reuse the ids, which hid the stale entries (#319).
- The System adapter fixes released on `v3.1` in June 2026 (#29, #32 to #36, #38, #39, #41 to #43,
  #47 to #49): the `Union` of ranges keeps an element equal to `default(T)`; the Memory adapter's
  `Like` follows SQL semantics and escapes metacharacters, and the SQL Server adapter's `Like`
  follows ANSI semantics with `[` as a literal; a one2one change set reports the displaced
  association; `NotAssociationContainedInEnumerable` emits balanced parentheses; `Instantiate`
  tolerates duplicate object ids; prefetching uses an object's modified composites role; a converted
  `Extent.CopyTo` honours the destination index; `Load` honours the configured schema name; the
  shared `Database` lazy caches are thread-safe; `CreateObjects` holds object ids as `bigint`;
  `ContainedIn` by objects round-trips over JSON; the Remote adapter's `GetValue<T>` converts pulled
  JSON values; and `SetCompositeRoleMany2One` detaches the previous role.
- A procedure's decimal output values travel as the protocol's invariant strings, like decimal
  roles, so the Remote adapter's `GetValue<decimal>` keeps their precision on both JSON libraries;
  untyped consumers of a pull response receive strings for decimal values.
- The Core server's default SQL Server connection string disables the connect timeout, as the
  command and test configurations already did.
- The Core fixes released on `v3.1` in June 2026 (#50 to #57, #61 to #64): a nested prefetch policy
  is built on the nested builder; the resource `Merger` preserves the structure of a duplicate key's
  data element and gets its own test project, `Merge.Tests`, in the Core database test suite;
  `commands.sh` quotes the arguments it forwards; the UML template renders a many-valued role as an
  array; a pull dependency with an unknown or mismatched meta tag is logged and ignored instead of
  failing the pull; `Invoke` and `Push` are no longer retried on a database exception, since a
  retry after an ambiguous commit would apply the unit twice, while `Pull` and `Sync` still are; the
  test share holders controller resolves the current user through `IUserService`; and the Core test
  suites drop a denied-permissions stub, a `SelectRef` stub and a vacuous sign-out test, and run the
  push test of a workspace object without a workspace.
- The TypeScript workspace fixes released on `v3.1` in June 2026 (#84, #85): `nodeLeafs` returns the
  leaf nodes of a tree, covered by the new `system-workspace-domain-tests` project in the TypeScript
  workspace suite, and an object's `toJSON` serializes it through its strategy. The pull retry is
  covered by the regression test of #276, which fails a pull's first SQL command.
- The fixes of the archived line of `main` from August and September 2026 (#270 to #272, and the
  platform part of #261): the .NET workspace sorts the collection a composites role is set from, so
  lookups in the role no longer miss objects given in creation order; `ITransaction.Instantiate(IObject)`
  returns null for null in the SQL adapters, as it does in the memory adapter; the generated lazy meta
  of the .NET workspace compiles for a workspace without inheritance, covered by the new
  `Generate.Tests` project in the Core database test suite, which generates and compiles the
  workspace meta of every workspace of the test domain; `AllorsUserStore.CreateAsync` no longer fails
  for a user created with a password hash, the store logs the exceptions it turns into failed
  results, and the generated builder guard spells its message "One multiplicity".
- `AllorsUserStore.HasPasswordAsync` answers whether the user has a password hash, a fix from the
  security hardening of the archived line of `main` (#255). It returned the inverse, so the account
  pages of ASP.NET Core Identity sent a user with a password to "set password" and a user without
  one to "change password".
- The Npgsql adapter fixes of #80, released on `v3.1` in June 2026. The adapter sets
  `Npgsql.EnableLegacyTimestampBehavior` itself, in a module initializer; only the fixture of its
  tests set it. The adapter connects to the lower-cased database name, the name PostgreSQL gives a
  database created without quotes, so `Database=AllorsCore` reaches `allorscore`; a database created
  under a quoted name with capitals has to be renamed.
- The PostgreSQL adapter tests pool their connections, a fix of the archived line of `main` from
  September 2026 (#273). Their connection string set `Pooling=false`, so every transaction opened
  and closed a connection, and every close left a port in `TIME_WAIT`. Against a PostgreSQL in a
  local container, whose port forwarder holds a second port per connection, the suite in parallel
  used up the ephemeral ports of the host within seconds (`Can't assign requested address`). It now
  passes in parallel.
- `Init` of the SqlClient and Npgsql adapters no longer keeps a refusal. A database object that
  was refused a transaction, because its schema was missing or invalid, stayed refused after
  `Init` had built the schema. It now validates the schema again and opens the transaction
  (#388).
- `Init` of the SqlClient adapter switches snapshot isolation on whatever the name of the
  database. Its statement named the database, without brackets and inside a string literal, so it
  did not compile for a name that needs brackets or that holds a quote or a bracket, such as
  `allors-core`, and `Init` failed on a database with such a name. `Init` now alters the database
  of its connection, `ALTER DATABASE CURRENT` (#393).
- The SqlClient adapter refuses further statements and `Commit()` when the driver reports a
  failure while executing a command or reading its rows or column values. It also refuses them
  when SQL Server has ended the database transaction. Both throw `TransactionFailedException`,
  a new `DbException` in `Allors.Database.Adapters.Sql.SqlClient`, until `Rollback()` is called.
  Its inner exception is the first failure the driver reported for that transaction, if any.
  Previously, writes after an update conflict could commit immediately, and a commit after a
  command timeout could omit the changes that timed out. Roll back and redo the work after a
  reported failure. If the driver suppresses an error when a reader closes and SQL Server
  keeps the transaction open, the transaction can still commit. PostgreSQL already refuses
  further statements after a server-side statement error (#414).
- On the SqlClient and Npgsql adapters two unit roles of one object whose full names differ only in
  case, such as `Work.FlowOwner` and `Workflow.Owner` of a class that implements both interfaces,
  each keep their own value. When both were set before one flush, both columns received the value
  of one of them, without an error: the adapters named the parameter of a unit role after the full
  name of the role, and the statement that sets several unit roles of an object took the parameter
  of the one for the other. The parameter now carries the id of the relation type (#422).

### Security

- A failed sign-in counts towards Identity's lockout, so repeated wrong passwords lock the account
  for the configured period (#58).
- An Identity update is rejected when the user's security stamp changed since the user was loaded,
  so recording a failed sign-in cannot undo an intervening password change.
- A production error response carries a generic message instead of the exception's message; the
  development environment keeps the message and the stack trace (#60).

### Removed

- Base and Apps: the domains `dotnet/Base` and `dotnet/Apps`, the Angular applications and
  libraries under `typescript/modules/apps` and `typescript/modules/libs`, the end-to-end tests
  under `typescript/e2e`, the Angular workspace script, their configuration templates, build
  targets, solution folders and CI jobs, and the Angular toolchain of the TypeScript workspace.
  Base and Apps continue on the `v3.1` branch.
- The isolation level setting of the SQL adapters: `Configuration.IsolationLevel`,
  `Database.IsolationLevel` and `Database.DefaultIsolationLevel` of `Allors.Database.Adapters.Sql`,
  the `isolationLevel` argument of `DatabaseBuilder` and the `-i` option of the Core commands. The
  SQL Server and PostgreSQL adapters begin every transaction at the isolation level Snapshot, which
  was the default (#342).
- The stored procedures of the SqlClient adapter, and its table types `_ti_…`, which nothing used.
  `Init` creates the tables, their indexes and the table types that the statements of the adapter
  use, and a transaction no longer compares the procedures of a schema with those of the mapping.
  With them go `SchemaProcedure`, `Schema.ProcedureByName`, `Schema.GetProcedure`,
  `Validation.MissingProcedureNames`, `Validation.InvalidProcedures`,
  `Mapping.ProcedureDefinitionByName` and `Mapping.GetTableTypeNameForIn` of
  `Allors.Database.Adapters.Sql.SqlClient`. A database that is initialized after this change holds
  no procedures, so an earlier version does not accept it. A database that was initialized before
  this change keeps its procedures and stays valid, and an `Init` that only empties its tables
  works. `Init` no longer drops the procedures, though, and SQL Server does not drop a table type
  that a procedure uses: an `Init` that has to recreate the schema of such a database fails, before
  it has touched a table. Drop and create the database between the save and the load of an upgrade,
  which `DatabaseProvisioning.DropCreate` of `Allors.Database.Builder` does (#368).
- The table type `_t_vo` of the SqlClient adapter and its class `VersionedObjectDataRecord`, which
  nothing used. A database that was initialized before this change keeps the table type and stays
  valid (#369).
- The functions of the Npgsql adapter. `Init` creates the tables and their indexes, and a
  transaction no longer compares the functions of a schema with those of the mapping. With them go
  `SchemaProcedure`, `Schema.ProcedureByName`, `Schema.GetProcedure`,
  `Validation.MissingProcedureNames` and `Validation.InvalidProcedures` of
  `Allors.Database.Adapters.Sql.Npgsql`, and what only the signatures of the functions used: the
  fields `ElementType`, `TypeName` and `Name` of `MappingArrayParameter`, whose constructor now
  takes the mapping and the name. A database that is initialized after this change holds no
  functions, so an earlier version does not accept it. A database that was initialized before this
  change keeps its functions and stays valid. `Init` no longer drops them, and PostgreSQL drops a
  table while a function still names it: an `Init` that has to recreate the schema of such a
  database works, and leaves the functions, which nothing calls. Drop and create the database
  between the save and the load of an upgrade, which `DatabaseProvisioning.DropCreate` of
  `Allors.Database.Builder` does, and it holds none. The adapter no longer sets the process-wide
  switch `Npgsql.EnableStoredProcedureCompatMode`, with which Npgsql calls a function for a command
  of the type `StoredProcedure`. Such a command of an application in the same process now calls a
  procedure, as Npgsql does without the switch, and fails for a function with `42809: … is not a
  procedure` unless the application sets the switch itself (#379).
- `ICommand.CommandType` of `Allors.Database.Adapters.Sql`, and the property `CommandType` of
  `Command` in the SqlClient and Npgsql adapters. The adapters send every command as a statement,
  the type `Text` that a command of both drivers has from its creation, so they no longer set the
  member. A command that an application creates through `IConnection.CreateCommand()` is a
  statement as well and cannot be given another type (#383).
- The classes `SchemaTableType`, `SchemaTableTypeColumn` and `SchemaIndex` of
  `Allors.Database.Adapters.Sql.Npgsql`, which the adapter never instantiated, and the members that
  it never filled: `Validation.MissingTableTypeNames`, `Validation.InvalidTableTypes` and
  `Schema.IndexByIndexNameByTableName`, with `Schema.GetIndex`, which always returned null. The
  validation of a schema reports its missing and invalid tables, as before (#384).
- The index read of the SqlClient adapter, which nothing used: the validation of a schema read its
  indexes from `sys.indexes` into `Schema.IndexByIndexNameByTableName` and never looked at them.
  That query goes, with `SchemaIndex`, `Schema.IndexByIndexNameByTableName` and `Schema.GetIndex`
  of `Allors.Database.Adapters.Sql.SqlClient`. The validation of a schema reports its missing and
  invalid tables and table types, as before (#385).
