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

## Research records and agent memory

- Store agent research notes, external comparisons, code-evidence inventories and
  investigation checkpoints in the agent's own built-in memory outside Git. Do not create
  committed memory files or a separate shared memory store for these records.
- Do not append sections such as "Research basis and code evidence" merely to record how
  the agent investigated a task. Keep resulting guidance, designs, decisions and essential
  rationale in repository documentation when that documentation is authorized.
- Retain supporting references where a substantive documented claim depends on them.
  Include an inspected revision or date only when needed to interpret or verify that claim;
  routine investigation stamps belong in memory.
- Keep repository documentation understandable without access to any agent's memory.
  Memory is working context, not authoritative product documentation; verify remembered
  implementation details against the source relevant to the task.
- Keep agent-only handoff notes in built-in memory and provide the necessary continuation
  context in chat. If memory is unavailable, use chat and state the limitation; do not claim
  to have saved memory or substitute committed files. Explicitly requested research
  deliverables and project plans remain governed by their authorized scope.

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
  exact next step. Use explicitly authorized project notes for project deliverables; keep
  agent-only notes in built-in memory and provide the necessary continuation context in chat.
  Include a 2–4-line restart prompt; do not require a restart after every small task.

## Direction for the 3.2 line

This direction guides explicitly requested work; it does not authorize expanding another task.

- Allors3 remains actively maintained and developed around **domain inheritance**.
- `main` restarted from `0e907fea68`, the platform state of 8 May 2026 that a downstream
  application inlined and runs in production, plus that application's changes to System and
  Core. The platform scope is **System and Core**, the TypeScript system and core libraries, the
  generator templates, and the test infrastructure needed to verify them. See
  [ARCHITECTURE.md](ARCHITECTURE.md).
- Base and Apps are not part of the platform; they continue on the `v3.1` branch. Downstream
  applications own business domains and production UI, including forms, tables, navigation and
  component libraries. Keep platform integration examples small and test-focused.
- The previous line of `main`, up to 27 October 2026, is kept on the archive branch
  `archive/main-2026-10`: the fixes of June 2026 shared with `v3.1`, the security hardening of
  the server, the rename of Core's test domain to Test, the Identity and Entra authentication
  plug-ins, a domain that extends several domains, the workspace API changes and the
  documentation website. Nothing from it returns wholesale. Fixes come back first, from `v3.1`,
  each in a pull request of its own that the downstream application validates through a
  submodule update; features return when an application needs them.
- Configuration follows the downstream application: `appsettings` files next to each server,
  command-line tool and test project, with environment, platform and environment-platform
  variants, and environment variables override every file. See the
  [README](README.md#configuration).
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
- A domain extends one domain and inherits its model and behaviour; Core is the root of the
  platform's domains, and Core's `Custom` folders hold its test domain. Generate the id of a new
  domain.
- Never edit generated files (`*.g.ts`, `*.g.cs`); regenerate with `./build.sh Generate` when needed.

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

- The retained version branches are `main`, `v3.0`, and `v3.1`. Work on `main` is the 3.2 line;
  do not create a v3.2 branch or release early.

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

- The human documentation is [README.md](README.md) and [ARCHITECTURE.md](ARCHITECTURE.md),
  written for application developers and platform maintainers. This file holds agent
  instructions; keep technical facts in the human pages rather than maintaining a separate
  account for agents.
- Write documentation, comments and public-facing explanations in Oxford English, in clear
  practitioner language, and preserve exact identifiers and external names.
- Do not rename an established concept or substitute a competing term without prior agreement.
  Explain the proposed meaning, reason and affected scope before asking. Existing agreement
  in the current work is sufficient; do not ask again. In particular, retain adapter rather
  than silently substituting codec.
- For explicitly requested documentation changes, maintain the affected pages and links within
  that scope. Otherwise, report pages made inaccurate by code changes.
- Before completing work, review the changed material for factual accuracy, agreed terminology,
  Oxford spelling and clear practitioner language, including example output, interface text,
  commit messages and pull request descriptions where affected.

## Testing — where test code may live (important)

- Core's `Custom` folders hold the platform's test domain: the concrete domain that extends Core,
  its population, and the endpoints and helpers that the Core tests and the Core test server
  need. The `Core*` compile globs that a downstream project uses exclude them, so an application
  never compiles the scaffolding. `Custom` is also the conventional name of an application's own
  extension domain; the two do not meet in one build.
- `typescript/modules/libs/**` is **inheritable** by downstream workspaces and MUST NOT contain
  test code or test-only scaffolding; keep it production-only. The dedicated `*-tests` libraries
  are the exception.
- This line has no test applications; `typescript/modules/apps` left with Base and Apps. Browser
  tests for UI integrations belong in isolated test applications when those return. Never add test
  hooks, test routes or test-only config to a production library.
- The Base and Apps tests retired with their domains. Platform coverage that only they provided
  is to be restored in platform test domains and test applications, where it remains relevant.

## Build Commands

Build uses Nuke: `./build.sh <target>` on macOS and Linux, `build.cmd <target>` on Windows.
`Generate` produces the code a fresh clone needs; the `Ci*` targets in `build/Build.ci.cs` are
the suites that CI runs. The [README](README.md#building-and-testing) says what each suite needs.
