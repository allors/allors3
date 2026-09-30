# Changelog

This changelog starts with the v3.2 platform direction. For the changelog up to v3.1, see the
[`v3.1` branch](https://github.com/allors/allors3/blob/v3.1/CHANGELOG.md).

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
[version.json](version.json) specifies `3.2.0-alpha.{height}` for development builds.
Changes accumulate under **[Unreleased]** until a version is released.

## [Unreleased]

### Added

- Documentation for users and maintainers under `docs/`, starting with the domain model:
  functional domains, plug-ins and their hosts, and the concrete domain that selects plug-ins.
  `AGENTS.md` holds the rules for these pages.
- The build target `DotnetSystemSharedTest` and the CI job `CiDotnetSystemSharedTest` for the
  `Ranges` tests in `dotnet/System/Shared.Tests`, which no target or job ran before.

### Changed

- Document the v3.2 platform scope: System, Core, and a planned Identity domain for
  authentication, continued domain inheritance, and a planned signals-based API for the .NET and
  TypeScript workspaces with thin UI integrations. Base and Apps are removed without a separate
  continuation; the Identity domain and the reactive workspace changes have not landed yet.
- Update development guidance for the platform scope, a default pull-request workflow in which
  creating a pull request requires approval, one pull request per agreed body of work with a
  correct title and description, Purpose Prefixes for branch names, and changelog handling for
  changes ported between branches.
- Start a new changelog from this point; the changelog up to v3.1 remains on the `v3.1` branch.

### Removed

- Apps: the domain `dotnet/Apps`, the Angular application and libraries `apps-intranet`, the
  end-to-end tests under `typescript/e2e/AppsIntranet` and `typescript/e2e/old`, their
  configuration templates, build targets and CI jobs. Base and Apps continue on the `v3.1`
  branch.
- Base: the domain `dotnet/Base` with its Blazor component libraries, the Angular applications
  and libraries under `typescript/modules/apps/base` and `typescript/modules/libs/base`, the
  end-to-end tests under `typescript/e2e/Base` with the scaffold generator
  `typescript/e2e/Scaffold`, their configuration templates, build targets and CI jobs, and the
  `libfontconfig1` CI step that only the Base barcode tests needed. The inheritance guard tests
  in Core now cover Core and its test domain only.
- Packages that only Base used, from Core's `Configuration` and `Server` projects:
  `Allors.Documents`, `DataUtils.DataUrl`, `MailKit`, `SkiaSharp`, `SkiaSharp.NativeAssets.Linux`
  and `ZXing.Net.Bindings.SkiaSharp`.
- The Angular toolchain and the packages that only the Base applications used, from the
  TypeScript workspace `typescript/modules`: every `@angular` and `@angular-eslint` package,
  `@nrwl/angular`, `jest-preset-angular`, `jest-chain`, `bootstrap`, `common-tags`, `date-fns`,
  `easymde`, `jsnlog`, `luxon`, `rxjs` and `zone.js`, with the `ngcc` post-install step. The
  workspace keeps nx, jest and eslint; its libraries depend on `cross-fetch` and `tslib` only.
- The obsolete bugfix integration review checklist.
- The `CLAUDE.md` symlink to `AGENTS.md`. Claude Code reads `AGENTS.md` directly from version
  2.1.277.
- Dependabot version updates for the GitHub Actions used in the workflows. Action versions are
  updated by hand.

### Fixed

- The .NET workspace sorts the collection that a composites role is set from. It kept the order
  of the collection, such as objects in the order they were created, and lookups in the role
  then missed objects: the association of such an object was empty and removing it from the role
  had no effect.
- `ITransaction.Instantiate(IObject)` returns null for null in the SQL adapters, as it does in
  the memory adapter. The SQL adapters threw a `NullReferenceException`.
