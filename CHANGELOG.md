# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(`3.1.0-alpha.{height}`), so the version auto-increments with each commit on `main`.
Changes accumulate under **[Unreleased]** until a version is cut, at which point they move
under a dated version heading.

## [Unreleased]

<!-- Add entries under one of: Added, Changed, Deprecated, Removed, Fixed, Security -->

### Added

- `MediaContent` is now an interface with two strategy implementations: `EmbeddedMediaContent`
  (bytes in the database, the previous behaviour) and `ExternalMediaContent` (bytes in external
  storage; the filesystem backend names them by the object id).
- Global setting `Singleton.StoreMediaContentExternal` selects which implementation new media use
  (defaults to embedded).
- `IMediaContentStorage` service (filesystem implementation `FileMediaContentStorage`) for
  reading/writing/deleting file-backed content and for enumerating and sizing it (`Enumerate`,
  `Length`). Its base directory comes from the `Media:Directory` configuration key (falls back to a
  local `media` directory; an empty value also falls back rather than throwing) and is resolved in
  one place — the service registration — so every host resolves it the same way.
- `MediaContent.HasData`, a cheap presence/non-empty probe (no full read for external storage), used
  by validation and the media controller.
- `ExternalizeMedia` command (in `Base` Commands, shared into `Apps`) converts every `Media`'s
  `EmbeddedMediaContent` to an `ExternalMediaContent` (moving bytes from the database to external
  storage) and sets `Singleton.StoreMediaContentExternal` so new media is stored externally too.
  Idempotent; backed by `Medias.ConvertEmbeddedMediaContentToExternal`.

### Changed

- Media content is now **write-once**: changing a `Media`'s data builds a fresh `MediaContent` and
  cascade-deletes the previous one instead of mutating it in place. `ExternalMediaContent.Data`
  enforces this — reassigning it on an already-persisted content throws (an overwrite is not
  rollback-safe). An empty data update is rejected as a validation error without destroying the
  existing content.
- Orphaned file-backed media (left by a rolled-back write or a deferred delete) is reclaimed
  automatically during a Load/Upgrade via `ExternalMediaContents.ReconcileFiles` (invoked from
  `Upgrade.Execute()`), where the load process is the only connection so a file with no live owner is
  always a true orphan. This replaces the earlier ceiling-based `PruneMediaFiles` command (removed),
  which could not reclaim the highest-id orphans and was unsafe to run against a live database.

### Migration

- `EmbeddedMediaContent` reuses the former `MediaContent` class id, so existing rows are re-typed
  to `EmbeddedMediaContent` automatically by the standard save/load upgrade — no data migration
  code is required. Because the table is renamed (`mediacontent` → `embeddedmediacontent`), run the
  Upgrade (save then load) rather than opening an old database in place.

### Fixed

- File-backed media (`ExternalMediaContent`) no longer unlinks its file during derivation, which was
  not rollback-safe: a rolled-back delete (or content replacement) permanently lost the file even
  though the database object was restored. Deletion is now deferred and reclaimed at the next
  Load/Upgrade.
- All hosts now resolve the same external-media directory. The `Apps` server and command-line tools
  and the Blazor host did not read `Media:Directory` (they fell back to a stray, working-directory
  relative `media/` folder); they now resolve it through the shared service registration like the
  `Base` server. Set an absolute `Media:Directory` in production for hosts that share a database.
- The media controller now returns `204 No Content` (not `304 Not Modified`) for a conditional
  request when an external file is missing, so a client is not told its stale cached copy is valid.
- Print and document-template rendering tolerate a missing external file: the affected logo/photo is
  skipped instead of throwing a `NullReferenceException`.
- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
