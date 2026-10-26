# AGENTS.md

## Change policy

Visibility: public. Change policy: conservative.

- Make only changes required by the explicit request. Preserve existing architecture,
  organization and conventions unless directly instructed otherwise.
- Follow existing test placement, fixtures and patterns; do not introduce a separate
  testing approach. Preserve established test expectations unless the requested change
  explicitly changes that behaviour.
- Before adding test coverage, inspect the closest existing test methods. When a method
  already checks the same behaviour across types or cases, extend it in place. Add a
  separate method only for a distinct scenario that does not fit the existing test's purpose.
- Keep unrelated test setup, timing and expectations unchanged. Investigate validation
  failures and fix regressions caused by the requested change. Report independent failures
  and resulting validation limits separately; passing validation does not authorize
  unrelated fixes.
- Before committing, review the test diff against these rules. Check every new test method
  and every change to existing setup or expectations for necessity within the requested scope.
- Edit documentation, including README files, changelogs, diagrams and guides, only when
  explicitly requested. Report related improvements or documentation implications
  without implementing them.
- An exception applies only to the change directly authorized; it does not waive the
  remaining rules. Change this policy only when explicitly instructed.

## Working agreement

- Stay within the approved objective and preserve unrelated work. Keep explanations and
  authorized documentation concise.
- Before asking for a decision, explain it fully in chat: the mechanism and relevant code,
  consequences, options, actual costs and available measurements, and a recommendation
  including the argument against it. Ask the question in plain text at the end.
- For long work explicitly divided into subtasks, describe the current subtask's changes,
  files or systems and validation before starting. Approval of a multi-subtask plan
  authorizes only its first subtask.
- At each boundary, summarize changes and verification, describe the next subtask and its
  validation, then wait for explicit approval. Proceed across boundaries only when the
  user explicitly authorizes it for the session. Re-check materially expanded scope.
- Do not invent subtask boundaries for simple tasks or routine implementation steps.
- At agreed boundaries, assess whether a fresh session would help. Before recommending
  one, record the branch, commit SHAs, validation, uncommitted work, pending decisions and
  exact next step. Use authorized project notes, otherwise provide the record in chat.
  Include a 2–4-line restart prompt; do not require a restart after every small task.

## Direction for v3.2

This direction guides explicitly requested work; it does not authorize starting the
transition or expanding another task.

- Allors3 remains actively maintained and developed around **domain inheritance**.
- The agreed platform scope is **System, Core, and the authentication plug-ins Identity and
  Entra**, a reactive workspace, thin UI integrations, and the test infrastructure needed to
  verify them. See [ARCHITECTURE.md](ARCHITECTURE.md).
- Identity and Entra are plug-ins for authentication, with ASP.NET Core Identity and with
  Microsoft Entra ID, hosted by Core; authorization stays in Core. See
  [Domains](docs/src/content/docs/domains.md). The plug-ins hold authentication; Core's `User`
  keeps no authentication field.
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
- A domain extends one or more domains, and the order of the domains, which decides the order
  of the hooks, follows from the inheritance graph and the domain ids; see
  [Domains](docs/src/content/docs/domains.md). Generate the id of a new domain; never choose
  it to steer the order.
- Never edit generated files (`*.g.ts`, `*.g.cs`); regenerate with `./build.sh Generate` when needed.
- The approved signals transition follows the order under Direction for v3.2; it does not
  authorize unrelated API changes.

## Git

- Pull requests are the default. Creating or merging a pull request requires explicit
  approval. Keep one branch and one PR for an agreed body of work, with focused commits;
  open the PR when that work is complete.
- Use purpose prefixes such as `feature/`, `fix/`, `docs/` or `chore/`, not agent/tool names.
  Use conventional commits (`type(scope): description`) with no AI attribution.
- Keep the PR title and body accurate for the final diff. Update them when the branch's
  scope changes, including after review commits; describe the resulting behaviour and
  validation, not the development history.
- Default to squash merge. Use the PR title as the commit subject and its full description
  as the body, with wording that remains true after the merge.

- The retained version branches are `main`, `v3.0`, and `v3.1`. Work on `main` is intended for
  v3.2 once the transition settles; do not create the v3.2 branch or release early.

## Code Style

- Error messages should be actionable

## Workflow

- Follow Test Driven Development
- Verify builds succeed and tests are green before considering a task complete
- For bug fixes, first add a failing regression in a suitable existing test class or file.
  Amend existing tests only as required by the requested change; preserve established
  expectations unless that change explicitly changes the behaviour.
- When a changelog update is explicitly requested, use `CHANGELOG.md` under `[Unreleased]`
  (Keep a Changelog format).
- When porting an authorized changelog entry between `main` and a version branch, keep the
  target branch's `CHANGELOG.md` and add the entry by hand. The union merge would otherwise
  bring back old entries without reporting a conflict.

## Documentation

- `docs/src/content/docs/` is human documentation for application developers and platform
  maintainers. The [documentation index](docs/src/content/docs/index.md) lists the pages.
  This file holds agent instructions; use the human pages for technical facts rather than
  maintaining a separate account for agents.
- Before changing documentation, comments or public-facing explanations, read and apply the
  [writing guide](docs/src/content/docs/internals/writing-documentation.md) and
  [terminology reference](docs/src/content/docs/terminology.md). The guide owns the audience,
  Diátaxis, status, Oxford English and visual explanation rules. Preserve exact identifiers
  and external names. Its maintenance rules do not expand the authorized documentation scope.
- Do not rename an established concept or substitute a competing term without prior agreement.
  Explain the proposed meaning, reason and affected scope before asking. Existing agreement
  in the current work is sufficient; do not ask again. In particular, retain adapter rather
  than silently substituting codec.
- Apply an approved terminology change consistently across its agreed scope, including code,
  prose, diagrams, examples and pull request text. Search for remaining uses and explain any
  intentional exceptions. Approval of a term does not itself authorize a breaking API change.
- For explicitly requested documentation changes, maintain affected pages, the index, statuses
  and links within that scope. Otherwise, report pages made inaccurate by code changes.
  Use documentation from the checkout's branch/version when a published page differs.
- Before completing work, review the changed material for factual accuracy, agreed terminology,
  Oxford spelling and clear practitioner language. Include diagram labels, example output,
  interface text, commit messages and pull request descriptions where affected. Verify links
  and relevant examples or interactive behaviour. Editorial review is required; no prose
  linter is part of the initial tooling.
- The website in `docs/` has its own checks. Run them from `docs/` before completing
  documentation work, as [docs/README.md](docs/README.md) describes: `npm test`, `npm run check`,
  `npm run build`, `npm run check:links` and `npm run test:browser`.

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
- The platform test domains are `Level1`, `Level2` and the test plug-in `Plugin1` in the Diamond
  tree, `dotnet/Diamond`. They exist to prove the platform across domain levels and carry no
  production behaviour.
- The Base and Apps tests retired with their domains. Platform coverage that only they provided,
  including behaviour across inherited domain levels, is still to be restored in the platform
  test domains and, once they exist, in test applications, where it remains relevant.

## Build Commands

Build uses Nuke. The documentation website in `docs/` builds with npm;
[docs/README.md](docs/README.md) lists its commands.
