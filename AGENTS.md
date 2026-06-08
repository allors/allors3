# AGENTS.md

## Development Notes (important, read before editing)
- Objects are managed by Allors, always use the API to create or delete objects.
- Relations are managed by Allors, always use the API get or set Relations.
- Relations are bidirectional by design.
- Roles and Associations are RelationEndPoints
- Roles are forward and writable
- Associations are inverse and readonly
- Objects delegate to their Strategy to handle operations.
- Object ids: `0` denotes null (no object). Database object ids are positive and start at 1;
  workspace/session ids are negative and start at -1 (`Session.IsNewId(id) => id < 0`).
  Object-id ranges (`IRanges<long>`) therefore never contain `0`.
- Follow existing patterns; prefer minimal changes in public APIs.
- Never edit generated files (`*.g.ts`, `*.g.cs`); regenerate with `./build.sh Generate` when needed.
- Follow existing naming and structure; avoid new conventions without reason.
- Allors is in a refactoring/reeingineering cycle. Breaking changes are allowed but ask first.

## Allors Meta-Modeling
- `[Workspace(Default)]` is valid on classes, roles, methods, and properties but **not on an interface declaration** — put it on the interface's roles, not the type.
- Unit `bool` roles are generated as nullable (`bool?`, null = unset) — compare with `== true`, don't rely on plain `&&`.
- To expose behavior polymorphically across an interface's implementations, declare a C#-only member on a hand-written `partial interface` in `*/Database/Domain` (not a meta-role) and implement it per class (precedent: `Invoice.InvoiceItems`).
- Roles/relations are persisted by their own `[Id]`. To refactor a concrete type into an interface + implementations without losing data, give one implementation the original type's `[Id]` (existing rows re-type to it for free on the save/load Upgrade) and give the new interface a fresh `[Id]`.

## Services & Layers
- Domain code resolves app services via `this.Strategy.Transaction.Database.Services.Get<T>()`.
- Each layer (Core/Base/Apps) has its own `DatabaseServices` registry (plus `Default`/`Test` variants) — register a new service in every layer that uses it.
- Services can be **optional** — a registry's `Get<T>()` may return `null` when the service isn't configured; callers null-check and fail hard only when the feature is actually used. Example: `IMediaContentStorage` is `null` unless the `Media:Directory` setting is present (via `FileMediaContentStorage.CreateOrNull`), so embedded-only installs don't configure it and storage-dependent paths (e.g. `ExternalMediaContents.ReconcileFiles`) skip it; using external media while unconfigured throws.
- **External media content must have its backing file (fail fast on read).** With file-backed storage configured (`Media:Directory` → `FileMediaContentStorage`), every live `ExternalMediaContent` must have its file on disk. A *missing* file is a data-integrity bug, not a tolerated state: the storage read path (`FileMediaContentStorage.Read` → `ExternalMediaContent.Data`) throws an actionable error rather than returning silent `null` bytes, so the loss surfaces instead of festering. (Derive-time validation already rejects no-data content via `CoreOnPostDerive`/`HasData`; the `Length`/`HasData`/`Exists` probes stay tolerant and must not throw.) This applies at the edge too: the HTTP media/image endpoints (`BaseMediaController.Get`, `BaseImageController.Get`) return `500` (not `204`) when a live `MediaContent` has no bytes — only genuinely-absent content (`MediaContent == null`) is `204`. The integrity check sits before the ETag/304 short-circuit and is marked `no-store`, so the error is never cached. Internal byte consumers (printing, exports, template cache) are strict too.

## Git
- No AI attribution in commits (no "Generated with", "Co-authored-by", or similar trailers)
- Keep commits focused and well-described
- Use conventional commit format: type(scope): description

## Code Style
- Error messages should be actionable

## Workflow
- Follow Test Driven Development
- Never change an existing test unless explicitly instructed to
- Verify builds succeed and tests are green before considering a task complete
- When fixing bugs, always write a failing test first or at least amend an existing test
- When creating a new test, find a suitable existing class to add it to.
- Record notable changes in `CHANGELOG.md` under the `[Unreleased]` section (Keep a Changelog format).

## Testing
- Domain tests use the in-memory adapter (`Adapters.Memory`) — no database server required.
- Iterate with `dotnet build <Domain.Tests>.csproj` then `dotnet test --no-build --filter "FullyQualifiedName~XTest"`.

## Testing — where test code may live (important)
- `typescript/modules/apps/**` (the apps: `apps-intranet`, `base`) MAY contain test-only code, including **test-only pages/routes**. App code is NOT inherited by other domains, so test scaffolding here is safe and isolated.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: to exercise a reusable lib component (e.g. a shared panel) whose bug is not reachable through existing shipped screens, add a **test-only page in the relevant app** that wires the component to the triggering data, then drive it from `typescript/e2e/**`. Never add test hooks, test routes, or test-only config to the lib itself.

## Build Commands

Build uses Nuke.

- After editing `Repository/Domain` POCOs, run `./build.sh Generate` before building.
- Generated code (`**/Generated/*.g.cs`, `**/Meta/Generated/`) is gitignored and reproduced by Generate — never commit it.
- IDE/LSP diagnostics are stale right after regeneration (the whole `MetaPopulation` may appear to be missing members, or `MetaBuilder().Build()` mis-resolves) — trust `dotnet build`, not the live errors.

## Known Issues
- The full `Base/Database/Domain.Tests` run can crash the test host via a SkiaSharp GC finalizer when the native `libSkiaSharp.so` is missing. Install the native library, or filter to non-image tests (`--filter`) while iterating.
