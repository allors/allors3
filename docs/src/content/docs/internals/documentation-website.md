---
title: "Documentation website"
description: "Agreed documentation website design, implementation subtasks and validation."
audience: "Maintainers"
kind: "Decision record"
status: "Planned"
---

> **Status: Planned.** The website, its Markdown exports and the guided example are built, and
> the checks that the documentation workflow runs pass for both deployment paths. Publication
> to GitHub Pages has not been run and the repository's Pages settings are unchanged.

This record describes the documentation website and its first interactive explanation. The
[writing guide](writing-documentation.md) owns the editorial rules and
[Terminology](../terminology.md) owns the shared vocabulary. The pages live in
`docs/src/content/docs/`; [website instructions](../../../../README.md) cover local development,
validation and publication.

## Agreed design

| Area | Decision |
| --- | --- |
| Readers | Human documentation serves application developers and platform maintainers. Agent instructions and discovery have separate entry points and reuse the human technical content. |
| Site | Astro with Starlight. `docs/` is the website project, with the conventional content location `docs/src/content/docs/`. User and maintainer content have separate navigation groups. |
| Authoring | Markdown by default; MDX where a page needs a component. Keep component implementation separate from page prose. |
| Hosting | GitHub Pages, built with GitHub Actions. |
| Visuals | Any diagram or other visual that helps explain the subject. Mermaid where suitable; editable drawing sources and SVG for other diagrams. |
| Rendering | Render Mermaid diagrams in the browser. Verify diagrams in the browser as well as checking the site build. |
| Interaction | Astro with ordinary JavaScript or TypeScript first. Add a UI framework only when a concrete interaction justifies it; no UI framework is selected for the initial implementation. |
| Pilot | A guided walkthrough of workspace session state: prepared scenarios and steps, with values and diagrams that respond to the selected step. Free-form experimentation is outside the first pilot. |
| Language and structure | Oxford English, precise practitioner terminology and Diátaxis, as defined in the writing guide. |
| Editorial enforcement | `AGENTS.md` directs agents to the human guide and vocabulary and requires editorial review. No prose linter initially. Keep automated checks for concrete failures. |

## Agent discovery and exports

Publish `llms.txt` at the documentation website root, relative to its deployment path. Generate
the concise index from the same content metadata used by the site, linking to clean Markdown
where available. Prefer an existing integration after verifying that it preserves page titles,
audience, kind, status, version context and the meaning of custom content. If none satisfies
those requirements, implement only the missing export behaviour.

Treat [llms.txt](https://llmstxt.org/) as a discovery proposal, not a guarantee that an agent
loads it. Repository instructions remain in `AGENTS.md`. Agents working on a checkout should
use its documentation; the published site must make the documented version clear, including
that work on `main` is intended for an unreleased v3.2.

Exports must retain the explanation conveyed by a diagram or interaction. They must not reduce
a component to an empty placeholder or publish a separate, manually maintained account of
platform behaviour. Inspect representative exported pages before accepting an integration.

## Implementation and validation

The work was divided into three subtasks on one branch. Each subtask ended with a review of
its changes and validation, followed by explicit approval before starting the next subtask.

| Subtask | Scope | Validation | State |
| --- | --- | --- | --- |
| 1. Editorial foundation | Writing guide, terminology reference, human and agent entry points, this decision record and changelog. | Check local links and anchors, page inventory and status, terminology and the diff; obtain an independent editorial review. | Complete. |
| 2. Website and exports | Move the human pages into the Starlight project; update links and instructions; add navigation, search, browser diagram rendering, Markdown exports and `llms.txt`; prepare GitHub Pages build and deployment configuration. | Build the site; check links and search; inspect rendering and accessibility at desktop and narrow widths; verify exports and deployment base paths. | Complete. |
| 3. Guided visual example | Extend the workspace session state explanation with appropriate diagrams and prepared interactive scenarios, using ordinary TypeScript. | Test the scenario model against the documented platform cases; verify controls, state changes, keyboard use and the exported explanation. | Complete. |

The website work moves the existing pages and preserves their meaning before broad editorial
rewrites. Update incoming links or provide redirects where paths change. Do not add empty
Diátaxis categories. Keep documentation dependencies separate from the platform's TypeScript
package. Publishing the site and creating a pull request remain explicit review steps.

The pilot follows the current implementation and tests cited by the
[workspace session state page](workspace-session-state.mdx). In particular, an accepted editing
baseline is per object, Reset does not fetch
a new record, a successful Push does not itself accept a new baseline, and a retained data
record is not an authorization snapshot. Preserve the current differences between .NET and
TypeScript. Signals remain planned; the documentation must not suggest that they exist today.

## Website implementation

The website uses Astro 7 and Starlight 0.42, with exact dependencies recorded in the separate
`docs/package-lock.json`. Page metadata supplies the title, description, audience, kind and
status. The website and exports identify the documented version as `main`, intended for an
unreleased v3.2. Source links remain relative Markdown links; the build converts documentation
links to site routes and repository links to the documented GitHub branch, and checks the
heading anchors of repository links against the checkout.

Starlight provides navigation and Pagefind search. `astro-mermaid` renders fenced Mermaid
source in the browser. The domain-arrangement diagram demonstrates that rendering.

The export review found two gaps in existing integrations:

- [`starlight-llms-txt`](https://github.com/delucis/starlight-llms-txt/tree/main/packages/starlight-llms-txt)
  generates combined documentation files; its index and exports do not retain the required
  audience, kind, status and version fields or individual page exports.
- [`astro-llms-md`](https://github.com/tfmurad/astro-llms-md/blob/main/src/index.ts) exports
  individual pages, but does not retain those metadata fields and removes SVG and `noscript`
  elements without preserving their explanation.

The local export integration supplies the missing metadata and preservation behaviour, using
Turndown, its GFM plug-in and LinkeDOM for HTML-to-Markdown conversion. It reads the same
rendered page content and metadata as the website. It preserves code, tables, heading anchors,
Mermaid source and static component explanations, and rejects marked interactions or SVG
drawings with no useful textual equivalent. The website instructions describe the authoring
contract. Representative exports must still receive editorial review.

The documentation workflow validates both `/allors3/` and `/`. Normal push and pull request
runs do not publish. Publication is prepared as a manual workflow on `main` with `publish`
selected, after both validation jobs pass. Repository Pages settings are not changed by this
work.

## Guided example implementation

The workspace session state page uses MDX to include an Astro component. Its website route
and Markdown export URL are unchanged. The finite TypeScript model in
`docs/src/examples/workspace-session-state.ts` supplies seven prepared scenarios and 45 steps:
another session refreshes, disjoint edits, string/to-one/to-many conflicts, Reset after Push,
and a qualified illustration of live permission maps. It makes no platform calls and does
not replace the authoritative workspace tests. Unit equality remains in the page's prose.

Each step shows database, connection and retained-record versions, plus local values, session
reads and diff originals. Versions are illustrative labels. Astro renders all steps into a
static transcript; ordinary TypeScript controls copy the selected step into the live view.
The transcript remains usable without JavaScript and supplies the complete Markdown export.
The website and exports therefore share scenario explanations and calculated snapshots.

The first five scenarios follow the existing .NET and TypeScript characterization tests.
Reset after Push uses their null-to-`"X"` string case. The permission scenario is explicitly an
implementation illustration: a retained grant ID resolves through a live connection map.
It does not claim that the security tests characterize an older editing record, or that Pull
without Sync and changes to grant/revocation IDs provide general permission freshness.

## Validation

The documentation workflow runs the same checks for the `/allors3/` and `/` deployment paths:
the tooling and scenario-model tests, the Astro and TypeScript check, the production build with
its exports, the link check over the published HTML, Markdown and `llms.txt`, and the browser
tests. The browser tests cover keyboard navigation and search, Mermaid rendering in both
themes, the guided example's controls, static transcript and export, and axe WCAG 2 A/AA checks
at desktop and narrow widths. The [website instructions](../../../../README.md) say how to run
the same checks locally.

Automated checks do not establish that an explanation is accurate or clear. The writing guide
requires editorial review of changed pages and of representative exports, and the guided
example's model tests check the cited platform cases without replacing the platform tests.
Deployment to GitHub Pages is exercised only by a publication run.
