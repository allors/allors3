# AGENTS.md

## Direction for v3.2

- Allors3 remains actively maintained and developed around **domain inheritance**.
- The agreed platform scope is **System, Core, and Identity**, a reactive workspace, thin UI
  integrations, and the test infrastructure needed to verify them. See
  [ARCHITECTURE.md](ARCHITECTURE.md).
- Identity is to be a new domain for ASP.NET Core Identity. Authentication moves to Identity;
  authorization stays in Core. Inheriting domains must use Core, and may use Identity or supply
  their own identity domain. The Identity domain does not exist yet: authentication is still
  part of Core.
- Base and Apps are to be removed without a separate continuation, including their applications
  and Angular/Material and Blazor component libraries. They are still present until removal lands.
- Signals are to become the default API of every workspace, both .NET and TypeScript. Breaking
  workspace API changes for this transition are approved; a parallel compatibility API is not
  required. Signals are not yet implemented in the current workspace API.
- Change the workspace API only after the platform-only baseline, without Base and Apps, builds
  and passes its retained tests.
- Downstream applications own business domains and production UI, including forms, tables,
  navigation, and component libraries. Keep platform integration examples small and test-focused.
- Public documentation, code comments, commit messages, and GitHub text must describe Allors3
  independently. Do not name or link private, unpublished projects.

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
- Follow existing patterns; keep public API changes focused on the agreed work.
- Never edit generated files (`*.g.ts`, `*.g.cs`); regenerate with `./build.sh Generate` when needed.
- Follow existing naming and structure; avoid new conventions without reason.
- Breaking workspace API changes for the signals transition are approved, in the order given
  under Direction for v3.2. Ask before introducing unrelated breaking changes.

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

## Testing — where test code may live (important)

- `typescript/modules/apps/**` MAY contain isolated platform integration test applications and
  test-only pages/routes. App code is NOT inherited by other domains, so test scaffolding here
  is safe and isolated.
- The platform test applications do not exist yet: the only applications are `apps-intranet` and
  `base`, which leave with Base and Apps. Until the platform test applications exist, test-only
  pages and routes go in those two applications.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: exercise reusable workspace and UI integrations through **test-only pages
  in isolated test applications**, driven from `typescript/e2e/**`. Never add test hooks, test
  routes, or test-only config to the production library itself.
- Preserve relevant platform coverage when retiring Base/Apps tests, including behavior across
  inherited domain levels. Business-specific tests retire with their domains.

## Build Commands

Build uses Nuke. 
