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

## Derivation Rules
- A rule is a class extending `Rule` that declares a `Patterns` array and implements `Derive(ICycle, IEnumerable<IObject> matches)`. The engine re-runs `Derive` **only** for objects matched by a `Pattern` when a watched role/association changes. Pattern types (`RolePattern`, `AssociationPattern`, …) live in `dotnet/Core/Database/Domain/Core/Derivations/Rules`.
- **A live derivation must declare a `Pattern` for every role/association its `Derive` reads** — otherwise the derived value goes stale when that input changes (the single most common bug found in review). Idiom: `m.Type.RolePattern(v => v.Role)` / `m.Type.AssociationPattern(v => v.Assoc)`, with a tree/path 2nd arg to watch across associations (e.g. `m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity, v => v.Assignment)`). Model: `Apps/.../Rules/Relations/PersonDisplayNameRule.cs` watches FirstName/MiddleName/LastName/UserName (everything it reads) and writes `DisplayName`:
  ```csharp
  public PersonDisplayNameRule(MetaPopulation m) : base(m, new Guid("…")) =>
      this.Patterns = new Pattern[]
      {
          m.Person.RolePattern(v => v.FirstName),
          m.Person.RolePattern(v => v.LastName),   // …one Pattern for every role Derive reads
      };
  ```
- **Exception — a rule used in place of `OnInit` deliberately does NOT watch what it reads.** Before adding a "missing" Pattern, check intent: a creation-time initializer must *not* re-fire on later edits, or it would clobber the value (often the user's own change). Two real forms:
  - *Created-snapshot* (`*Created*Rule` family): gated by `.Where(v => v.SomeState.IsCreated)` and intentionally omits the state Pattern so the value freezes after creation — e.g. `Apps/.../Rules/Order/PurchaseOrderCreatedBillToContactMechanismRule.cs`.
  - *Idempotent default-setter*: guards with `if (!@this.Exist<Role>) { … }` so a later input change never overwrites a set value — e.g. `Apps/.../Rules/Order/EngagementBillToPartyRule.cs`, `Apps/.../Rules/Invoice/SalesInvoiceStoreRule.cs`.
  - (The true once-at-creation hooks are `OnInit`/`OnBuild`/`OnPostBuild`; `OnInit` runs once per new object in the first derivation cycle — see `Core/.../Derivations/Default/Derivation.cs`. A `*Created*Rule` is the rule-shaped equivalent.)
- **Rollup/accumulation rules**: reset the total **before** the loop and accumulate with `+=`, never `=` — otherwise totals undercount or keep only the last item.
- **One rule per derived role**: never write the same role from two rules — execution order is undefined, so it is a last-writer-wins race. Consolidate into one rule that watches all inputs and encodes the precedence (e.g. `DisplayName = Name ?? UserName ?? "N/A"`).
- **`DisplayName` vs `Name`**: many types have a writable `Name` plus a derived `DisplayName`; in tests, re-derive and assert `DisplayName`, not raw `Name`.
- **TypeScript derivation rules**: the analog of `Patterns` is **`this.dependencies`** — a TS rule that reads roles without assigning `this.dependencies` won't re-derive. (These TS derivation projects are CI-ungated — see Build & Test.)

## Common Pitfalls
Recurring bug classes found in review (outside the rule engine):
- **Builders must end in `.Build()`** (including every nested builder). A `.WithX()…` chain without the terminal `.Build()` never creates the object — `ObjectBuilder<T>`'s finalizer throws `"… was not built."`, and a test that drops it silently stops exercising the relation. See `dotnet/Core/Database/Domain/Core/ObjectBuilder.cs`.
- **Copy methods assign to the copy, not the source**: in `AppsCopy`/clone code, write `copy.SomeRole.Add(…)`, never `this.SomeRole.Add(…)`.
- **Null-guard optional roles** with `?.`, `?? <default>`, or `Exist*` before dereferencing a nullable role or `FirstOrDefault()`.
- **Multi-line addresses**: populate `Address1`/`Address2`/`Address3` separately; never overwrite `Address1` with a newline-joined blob (it drops lines and prepends a stray `\n`).
- **Forms & lists must bind to the *correct* role** (TypeScript): copy-paste handler/role swaps are the most common form bug — e.g. a ship-**from** handler that clears ship-**to** contacts, or a list sorter keyed to a sibling type (`m.Brand` instead of `m.SerialisedItem`). Trace each `(event)="handler"` / `[field]="role"` back to the role it should touch.
- **Resource lifetime**: dispose transactions on every path (C# `try/finally`); unsubscribe Angular subscriptions in `ngOnDestroy`.

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
- **CI flakes that are not your code** (re-run, don't chase): nbgv sometimes writes `Invalid format
  '<version>'` to `$GITHUB_ENV` after an otherwise-green build (`gh run rerun --job <id>`);
  `CiTypescriptWorkspaceAdaptersJsonTest` occasionally fails with a `FetchError`/"Premature close".
  The relevant gate (e.g. `CiDotnetAppsDatabaseTest`) staying green is the signal.
- GitHub sometimes **drops the `pull_request` CI trigger** under a burst of PR creates/merges —
  validate with a `gh workflow run ci.yml --ref <branch>` dispatch before merging.
- The `CHANGELOG.md` `[Unreleased]` section is a **merge-conflict cascade** when several PRs land in
  sequence (each prepends the same anchor): rebase each branch onto the latest `main` before merge, or
  resolve with `git checkout --theirs CHANGELOG.md` + re-prepend when the code is already CI-green.

## Code Style
- Error messages should be actionable

## Workflow
- Follow Test Driven Development
- Never change an existing test unless explicitly instructed to
- Verify builds succeed and tests are green before considering a task complete
- When fixing bugs, always write a failing test first or at least amend an existing test
- When creating a new test, find a suitable existing class to add it to.
- Record notable changes in `CHANGELOG.md` under the `[Unreleased]` section (Keep a Changelog format).
- Run only the **targeted** test locally (`dotnet test --filter …`, prefer the no-DB Memory variant); CI re-runs the full Memory/SqlClient/Npgsql/Core/TypeScript suites on every PR, so gate "done"/merge on **CI green** rather than running full suites locally.
- Make assertions able to fail: after a mutation, **re-derive** (`transaction.Derive()`) and assert the *derived* value (e.g. `DisplayName`), not the raw input (`Name`) or a pre-derive value; no tautologies (`Assert.Equal(x, x)`); never bake a buggy output into the expected value; and ensure every test actually asserts (a stray `expect()` with no matcher passes vacuously).
- Watch for **duplicate/copy-paste test bodies** that never exercise their stated case.

## Testing — where test code may live (important)
- `typescript/modules/apps/**` (the apps: `apps-intranet`, `base`) MAY contain test-only code, including **test-only pages/routes**. App code is NOT inherited by other domains, so test scaffolding here is safe and isolated.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: to exercise a reusable lib component (e.g. a shared panel) whose bug is not reachable through existing shipped screens, add a **test-only page in the relevant app** that wires the component to the triggering data, then drive it from `typescript/e2e/**`. Never add test hooks, test routes, or test-only config to the lib itself.

## Build & Test
- Build/test runner is **Nuke**: `.\build.ps1 <Target>` (Windows), `./build.sh <Target>` (Unix), or `nuke <Target>`. The SDK is pinned in `global.json` (**.NET 10.0.100**, `rollForward: latestFeature`).
- **Codegen first**: test projects don't compile until meta is generated — run the matching `Dotnet{System,Core,Identity,Base,Apps}Generate` before building/testing that level (`DotnetAppsGenerate` also runs `DotnetAppsMerge`). Generated output lives under `…/Meta/Generated/` and is gitignored (`[Gg]enerated/`), so it never appears as a diff.
- **CI gates** (run on every PR; the DB matrix reports each as `<Name> (sqlclient)` / `(npgsql)`, so required-check names must match those suffixed forms exactly):
  - *Memory job, no DB:* `CiDotnetSystemAdaptersTestMemory`, `CiDotnetSystemWorkspaceSignalsTest`.
  - *Npgsql job (Postgres):* `CiDotnetSystemAdaptersTestNpgsql`.
  - *SqlClient job (SQL Server):* `CiDotnetSystemAdaptersTestSqlClient`, `CiDotnetCoreDatabaseTest`, `CiDotnetCoreWorkspaceLocalTest`, `CiDotnetCoreWorkspaceRemoteJsonSystemTextTest`, `CiDotnetCoreWorkspaceRemoteJsonNewtonsoftTest`, `CiDotnetIdentityDatabaseTest`, `CiDotnetBaseDatabaseTest`, `CiDotnetAppsDatabaseTest`, `CiTypescriptWorkspaceTest`, `CiTypescriptWorkspaceAdaptersJsonTest`, `CiTypescriptE2EAngularBaseTest`, `CiTypescriptWorkspacesE2EAngularAppsIntranetTest`.
- **Fast local loop**: `dotnet test <proj>.csproj --filter "FullyQualifiedName~<Class>.<Method>"`. Apps domain tests = `dotnet/Apps/Database/Domain.Tests/Domain.Tests.csproj`; the adapters have a no-DB **Memory** variant — `dotnet/System/Database/Adapters/Tests.Static/Tests.Static.csproj` with `--filter "FullyQualifiedName~Allors.Database.Adapters.Memory"`.
- **Databases**: SqlClient (SQL Server / LocalDB, connection string in `ALLORS_SQLCLIENT`) and Npgsql (Postgres, `ALLORS_NPGSQL`). DB names `Core` / `Identity` / `Base` / `Apps`; reset via `Dotnet{Core,Identity,Base}ResetDatabase` and `DotnetAppsResetDataapps`.
- **TypeScript** (`typescript/modules`): **Nx 14.8.9 + Jest**, **Node 24**. Install with **`npm install`, not `npm ci`** — `**/package-lock.json` is gitignored, so `npm ci` errors (CI's Nuke `NpmInstall` runs `npm install`). Run one project with `npx nx test <project> --skipNxCache` (or the `…:test` npm scripts).
- **Suites CI does NOT gate** (green CI does not cover these — validate manually): the TypeScript *derivations* Nx projects (`apps-intranet`/`base`/`core` `…-workspace-derivations`) have no Nuke target or npm script; **apps-extranet e2e** has no working Nuke target and no `ci.yml` job (only Base + AppsIntranet e2e exist).
