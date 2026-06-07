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
  reading/writing/deleting file-backed content. Its base directory comes from the dedicated
  `Media:Directory` configuration key (a `media` subfolder of the data path; falls back to a local
  `media` directory) and is resolved in one place — the service registration — so the server and
  command-line tools always resolve the same directory.
- `PruneMediaFiles` command (in `Base` Commands, shared into `Apps`) reclaims orphaned file-backed media:
  files whose id is below the highest live `ExternalMediaContent` id ("ceiling") and that no live content owns.
  Backed by `ExternalMediaContents.RemoveOrphanedFiles` and a new `IMediaContentStorage.Enumerate`.
- `ExternalizeMedia` command (in `Base` Commands, shared into `Apps`) converts every `Media`'s
  `EmbeddedMediaContent` to an `ExternalMediaContent` (moving bytes from the database to external storage) and sets
  `Singleton.StoreMediaContentExternal` so new media is stored externally too. Idempotent; backed by
  `Medias.ConvertEmbeddedMediaContentToExternal`.

### Changed

- Media content is now **write-once**: changing a `Media`'s data builds a fresh `MediaContent`
  and cascade-deletes the previous one (removing the old file for `ExternalMediaContent`) instead
  of mutating it in place. `ExternalMediaContent.Data` enforces this — reassigning it on an
  already-persisted content throws rather than overwriting the file (an overwrite is not rollback-safe).

### Migration

- `EmbeddedMediaContent` reuses the former `MediaContent` class id, so existing rows are re-typed
  to `EmbeddedMediaContent` automatically by the standard save/load upgrade — no data migration
  code is required. Because the table is renamed (`mediacontent` → `embeddedmediacontent`), run the
  Upgrade (save then load) rather than opening an old database in place.

### Fixed

- File-backed media (`ExternalMediaContent`) no longer unlinks its file during derivation, which was not
  rollback-safe: a rolled-back delete (or content replacement) permanently lost the file even though
  the database object was restored. Deletion is now deferred; orphaned files are reclaimed by the
  `PruneMediaFiles` command.
- Command-line media tools (`ExternalizeMedia`/`PruneMediaFiles`) resolved their storage directory
  differently from the server and could operate on a stray `media/` folder the server never read.
  Both now resolve the same `Media:Directory` through the shared service registration.
- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
