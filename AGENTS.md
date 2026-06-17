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

## Domain Architecture
- `Core ← Base ← Apps` is the functional track. These are **abstract** domains: they must have no dependency on ASP.NET Identity (or any other security stack).
- `Identity` (`dotnet/Identity`) is an **orthogonal track** that extends `Core` and encapsulates all Microsoft ASP.NET Identity integration. New identity-coupled code goes in the Identity track, never in Core/Base/Apps domain folders.
- Each level's `Custom` domain is the **concrete composition point**: it extends both its functional domain and `Identity` (multiple `[Extends]`), and is where concrete test classes and level-specific glue live.
- A future security stack should be another orthogonal domain (a sibling of Identity) that Custom domains compose instead.

## Domain & Level Mechanics
- Domain inheritance = `[Extends]` on the domain marker struct (multiple allowed) + `<Compile Include="..\..\..\<Track>\<Part>\<Domain>*\**\*.cs">` source globs in the consuming level's csprojs (Repository, Meta, Domain, Configuration, Server — and the Blazor site server).
- Domain membership of repository types/properties = the directory containing the `[Domain]` struct. Partial interfaces/classes across domain folders merge; an inheriting domain may add members and supertypes to an inherited type.
- Runtime behavior dispatch binds extension methods by name convention `{DomainName}{MethodType}` (e.g. `CoreOnPostBuild`, `IdentityDelete`) per interface per domain (`MethodCompiler`).
- Setup/Security/ObjectsBase per-domain dispatch is **handwritten** per level in `Database/Domain/Virtual/{Setup,Security,ObjectsBase}.v.cs` — adding a domain means updating these at every composing level, plus providing the domain's hook partials.
- The generated `MetaBuilder.Build()` calls `Build{DomainName}` for every domain in the repository — a handwritten partial `MetaBuilder` per domain folder is mandatory.
- Level-local (never inherited via globs): `Custom/` folders, `Virtual/`, `Rules.cs`, server `Custom/`, Commands, and the Configuration `Custom/` services (`DatabaseServices`, `DefaultDatabaseServices`, `TransactionServices`, …). Each level provides its own copies.

## Git
- No AI attribution in commits (no "Generated with", "Co-authored-by", or similar trailers)
- Keep commits focused and well-described
- Use conventional commit format: type(scope): description

## Branching
- Branch names do **not** affect CI. CI runs on every pull request (including drafts)
  targeting `main`/`v*`, and on pushes to `main`/`v*` themselves.
- Use conventional-commit-style prefixes for readability (`feat/`, `fix/`, `chore/`,
  `test/`, `refactor/`, `docs/`…), matching the commit type. `feat/` vs `feature/` — both fine.
- Pre-PR CI (no PR needed): push the branch, then dispatch manually — Actions → CI → **Run
  workflow** → pick the branch, or `gh workflow run ci.yml --ref <branch>`. Tests the branch tip.
- A **draft PR** also runs the full suite (and tests the merge-with-`main` result); it just
  can't be merged and doesn't request review until you mark it ready.

## Code Style
- Error messages should be actionable

## Workflow
- Follow Test Driven Development
- Never change an existing test unless explicitly instructed to
- Verify builds succeed and tests are green before considering a task complete
- When fixing bugs, always write a failing test first or at least amend an existing test
- When creating a new test, find a suitable existing class to add it to.
- Record notable changes in `CHANGELOG.md` under the `[Unreleased]` section (Keep a Changelog format).

## Testing — where test code may live (important)
- `typescript/modules/apps/**` (the apps: `apps-intranet`, `base`) MAY contain test-only code, including **test-only pages/routes**. App code is NOT inherited by other domains, so test scaffolding here is safe and isolated.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: to exercise a reusable lib component (e.g. a shared panel) whose bug is not reachable through existing shipped screens, add a **test-only page in the relevant app** that wires the component to the triggering data, then drive it from `typescript/e2e/**`. Never add test hooks, test routes, or test-only config to the lib itself.

## Build Commands

Build uses Nuke. 
