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
- `typescript/modules/apps/**` (the apps: `apps-intranet`, `base`) MAY contain test-only code, including **test-only pages/routes**. App code is NOT inherited by other domains, so test scaffolding here is safe and isolated.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: to exercise a reusable lib component (e.g. a shared panel) whose bug is not reachable through existing shipped screens, add a **test-only page in the relevant app** that wires the component to the triggering data, then drive it from `typescript/e2e/**`. Never add test hooks, test routes, or test-only config to the lib itself.

## Build Commands

Build uses Nuke. 
