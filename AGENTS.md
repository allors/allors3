# AGENTS.md

## Direction for v3.2

- Allors3 remains actively maintained and developed around **domain inheritance**.
- The agreed platform scope is **System, Core, and the authentication plug-ins Identity and
  Entra**, a reactive workspace, thin UI integrations, and the test infrastructure needed to
  verify them. See [ARCHITECTURE.md](ARCHITECTURE.md).
- Identity and Entra are plug-ins for authentication, with ASP.NET Core Identity and with
  Microsoft Entra ID, hosted by Core; authorization stays in Core. See
  [docs/domains.md](docs/domains.md). The plug-ins hold authentication; Core's `User` keeps no
  authentication field.
- Base and Apps were removed without a separate continuation. They continue on the `v3.1` branch.
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

- Pull requests are the default. Creating a pull request requires approval.
- Use one branch and one pull request for an agreed body of work. Its parts land as focused
  commits on that branch. Create the pull request only when that work is complete; if one already
  exists, update it. Do not create a separate pull request for each part.
- Keep the title and description of a pull request correct for everything on its branch, and
  update them when the branch changes. Before merging, review and update both to describe the
  final changes and their validation.
- For a squash merge, use the pull request title as the commit subject and the full pull
  request description as the commit body. The title follows the conventional commit format,
  and the description says what changed and why, with no AI attribution and nothing that stops
  being true after the merge.
- Use **Purpose Prefixes** for descriptive branch names: a prefix that states what the branch is
  for, such as `feature/`, `fix/`, `docs/`, or `chore/`. Do not use **AI Agent Source Prefixes**:
  a prefix that names the agent or tool that created the branch, such as `claude/`, `codex/`,
  or `copilot/`.
- The retained version branches are `main`, `v3.0`, and `v3.1`. Work on `main` is intended for
  v3.2 once the transition settles; do not create the v3.2 branch or release early.
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
- When porting a change between `main` and a version branch, keep the target branch's
  `CHANGELOG.md` and add the entry by hand. The union merge would otherwise bring back old
  entries without reporting a conflict.

## Documentation

- Documentation lives in `docs/`, for two audiences. Pages for users, who build applications on
  Allors3, sit at the top of `docs/`. Pages for maintainers, who change the platform, sit under
  `docs/internals/`. [docs/README.md](docs/README.md) lists every page and defines the kinds and
  the statuses.
- Every page opens with a status line: Current, Planned, or Historical. Every user page is of
  one kind: tutorial, how-to guide, reference, or explanation.
- A user page never requires reading an internals page, and it says what an application can
  rely on today.
- An internals page assumes the user pages and links to them instead of repeating them. It is a
  map: it says which part owns what, and why. Depth stays in code comments and tests.
  Applications must not rely on internals pages.
- Create a folder together with its first page, not before.
- A fact has one home. Link to it instead of repeating it.
- When a test can check a rule, write the test and name it in the page. The test is authoritative.
- Update the pages that a change makes untrue in the same pull request.
- A plan that has landed becomes Historical. It keeps its path and points to the page that holds
  the current truth.
- Use the vocabulary of [docs/domains.md](docs/domains.md): functional domain, plug-in, host, and
  concrete domain.

## Testing — where test code may live (important)

- `typescript/modules/apps/**` MAY contain isolated platform integration test applications and
  test-only pages/routes. App code is NOT inherited by other domains, so test scaffolding here
  is safe and isolated.
- The platform test applications do not exist yet: `typescript/modules/apps` is empty until they
  do.
- `typescript/modules/libs/**` is **inheritable** by other domains and MUST NOT contain test code or test-only scaffolding — keep it production-only. (The only exception is a dedicated test project, e.g. a `*-tests` lib.)
- Consequence for e2e: exercise reusable workspace and UI integrations through **test-only pages
  in isolated test applications**, driven from `typescript/e2e/**`. Never add test hooks, test
  routes, or test-only config to the production library itself.
- The Base and Apps tests retired with their domains. Platform coverage that only they provided,
  including behavior across inherited domain levels, is to be restored in platform test domains
  and test applications where it remains relevant.

## Build Commands

Build uses Nuke. 
