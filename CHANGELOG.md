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

- `MediaContent` is now an interface with two strategy implementations: `InlineMediaContent`
  (bytes in the database, the previous behaviour) and `FileMediaContent` (bytes on the
  filesystem, named by the object id).
- Global setting `Singleton.StoreMediaContentOnFile` selects which implementation new media use
  (defaults to inline).
- `IMediaContentStorage` service (filesystem implementation `FileMediaContentStorage`) for
  reading/writing/deleting file-backed content. Its base directory comes from the `datapath`
  configuration key (falls back to a local `media` directory).

### Changed

- Media content is now **write-once**: changing a `Media`'s data builds a fresh `MediaContent`
  and cascade-deletes the previous one (removing the old file for `FileMediaContent`) instead
  of mutating it in place.

### Migration

- `InlineMediaContent` reuses the former `MediaContent` class id, so existing rows are re-typed
  to `InlineMediaContent` automatically by the standard save/load upgrade — no data migration
  code is required. Because the table is renamed (`mediacontent` → `inlinemediacontent`), run the
  Upgrade (save then load) rather than opening an old database in place.

### Fixed

- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
