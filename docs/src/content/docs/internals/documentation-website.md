---
title: "Documentation website"
description: "How the website is built, what was reused, which facts moved here from the root of the repository, validation and publication."
audience: "Maintainers"
kind: "Decision record"
status: "Current"
sidebar:
  order: 4
---

> **Status: Current.** The website, its Markdown exports and the checks that the documentation
> workflow runs are built for both deployment paths. The website is published at
> `https://docs.allors.com/allors3/`, beneath the Allors documentation home, by a manual run of
> the documentation workflow on `main`.

This record describes the documentation website of the 3.2 line: what was decided, what was
reused, which facts moved into it and what remains open. The
[writing guide](writing-documentation.md) owns the editorial rules and
[Terminology](../terminology.md) owns the shared vocabulary. The pages live in
`docs/src/content/docs/`; the [website instructions](../../../../README.md) cover local
development, validation and publication.

## Decision

The 3.2 line started without a documentation website. The first Allors3 website, with the pages
of the previous line of `main`, stayed on the
[archive branch](../versions.md#the-archive-branch). This website replaces it: it documents what
`main` does, and it is the home of the technical facts that the root of the repository held.

| Area | Decision |
| --- | --- |
| Readers | Human documentation serves users, who build applications on Allors3, and maintainers, who change the platform. Agent instructions in `AGENTS.md` point to the same content. |
| Site | Astro with Starlight. `docs/` is the website project, with the content in `docs/src/content/docs/`. User and maintainer content have separate navigation groups. |
| Authoring | Markdown. MDX and components are available when a page needs them; none is used yet. |
| Hosting | GitHub Pages, built with GitHub Actions, at the path `/allors3/` beneath the shared origin `https://docs.allors.com`, whose root is the Allors documentation home; `/` is also validated. The sidebar's first entry links to the home, and the link checker leaves that link to the home's own build. The origin's own site and its custom domain belong to the documentation home, not to this repository. |
| Version context | Pages and exports identify the documented version as `main · v3.2 (unreleased)`: the major and minor number of `version.json`, and the fact that nothing built from `main` is a release. |
| Visuals | Mermaid where suitable; the diagrams in [Domains](../domains.md) and [Architecture](architecture.md) are the first. No interactive component yet. |
| Language and structure | Oxford English, precise practitioner terminology and Diátaxis, as defined in the writing guide. |
| Checks | The tooling tests, the Astro check, the production build with exports, the link check and browser tests with accessibility checks, for both deployment paths. |

## One home for the facts

Until this website, `README.md` and `ARCHITECTURE.md` at the root of the repository were the
human documentation of the 3.2 line. A fact has one home, so their content moved here:

| Was in | Is now in |
| --- | --- |
| `README.md`: the 3.2 line, development and releases | [Versions and branches](../versions.md) |
| `README.md`: configuration | [Configuration](../configuration.md) |
| `README.md`: building and testing | [Build and test](build-and-test.md) |
| `ARCHITECTURE.md`: domain inheritance | [Domains](../domains.md) for users, and [Folders carry inheritance](architecture.md#folders-carry-inheritance) for the rules that maintainers keep |
| `ARCHITECTURE.md`: scope, test scaffolding, the adapters, configuration and hosting | [Architecture](architecture.md) |

`README.md` remains the introduction of the repository and links to these pages. `ARCHITECTURE.md`
no longer exists. `AGENTS.md` points agents to these pages.

## What was reused

The tooling was copied from the Allors Excel repository at commit `1431234a`, inspected on
2026-11-19. Of the copies of the Allors documentation tooling, that one carries the fixes made
since the first Allors3 website: the link checker names files with forward slashes on every
platform, the `focusable-code-blocks.mjs` plugin gives every code block its `tabindex` at build
time, and the title and the description of the website are read from `site.mjs`. The changes for
Allors3:

| File | Change for Allors3 |
| --- | --- |
| `site.mjs` | The title, the description, the repository, the default base path `/allors3/`, and a version label that takes the major and minor number of `version.json`. |
| `astro.config.mjs` | The sidebar entries. |
| `integrations/source-links.mjs` | A fix: a link to a page in a subfolder gets forward slashes on Windows, where it kept a backslash. Port it to the other copies. |
| `integrations/exports.mjs`, `integrations/scrollable-content.mjs`, `integrations/focusable-code-blocks.mjs` | Unchanged. |
| `scripts/check-links.mjs` | The default base path. |
| `src/content.config.ts`, `src/components/PageTitle.astro`, `src/styles/documentation.css`, `public/favicon.ico`, `tsconfig.json`, `playwright.config.ts`, `.gitignore` | Unchanged. |
| `package.json`, `package-lock.json` | The package name; the pinned dependency versions are identical. |
| `tests/exports.test.mjs`, `tests/links.test.mjs`, `tests/source-links.test.mjs`, `tests/fixtures/anchors.md` | Base paths, repository paths, the version label and the pages that the tests name. `source-links.test.mjs` also checks a link to a page in a subfolder. |
| `tests/browser/documentation.spec.ts` | The page names, the diagram and the table that it exercises. |
| `.github/workflows/documentation.yml` | The base path matrix. A pull request runs whatever its base branch, as in `ci.yml`, and a push runs for `main` only. `version.json` is a trigger path, because the version label depends on it. |
| `.github/workflows/ci.yml` | The detection of documentation-only changes covers `docs/` and the documentation workflow. |

Since [#413](https://github.com/allors/allors3/pull/413) the values that differed between the
copies of the tooling are in `site.mjs`: the audiences, which the content schema and `exports.mjs`
read, the root of the repository, which `source-links.mjs` reads, and the base path, from which
`check-links.mjs` takes its default. The link checker leaves two kinds of link to the build that
publishes their target: the link to the documentation home, and links into the documentation
areas, which the copy of the home needs. With the same change in the other four copies, ten files
are the same, byte for byte, in all five: the four integrations, the link checker, the content
schema, the page title component, the stylesheet and the Playwright and TypeScript configuration.
A copy differs in `site.mjs`, `astro.config.mjs`, its tests, its workflow and its content.

Since [#415](https://github.com/allors/allors3/pull/415) the website takes `mermaid` 12 and shows
every diagram at its natural size: `astro.config.mjs` sets `flowchart.useMaxWidth` to `false`, and
the stylesheet gives a diagram neither a minimum nor a maximum width. A label is 16px on every
screen, and a diagram that is wider than its column scrolls. Before, the stylesheet gave every
diagram a minimum width of 34rem, which enlarged the narrower diagrams that `mermaid` 12 draws.
The rule is the same in the five copies of the tooling. It covers flowcharts, the one kind of
diagram in use; another kind needs its own `useMaxWidth`.

Not carried over from the first Allors3 website: its pages on the kinds of domains,
authentication, Microsoft Entra ID, logging, the internals of domain inheritance and workspace
session state, which describe the previous line, and the guided example of workspace session
state with its components and tests. Of those pages only `/domains/` has a successor at the same
address; the other addresses no longer exist. The writing guide is carried over with its examples
adapted, and keeps its address.

Extracting the reused set into a shared implementation is a decision for the Allors documentation
home. Until then, keep the files identical to the other copies where the table says so, and port
fixes between the repositories.

## Validation

The documentation workflow runs the same checks for the `/allors3/` and `/` deployment paths: the
tooling tests, the Astro and TypeScript check, the production build with its exports, the link
check over the published HTML, Markdown and `llms.txt`, and the browser tests. The browser tests
cover keyboard navigation and search, Mermaid rendering in both themes, focusable code blocks and
tables, and axe WCAG 2 A/AA checks at desktop and narrow widths. The
[website instructions](../../../../README.md#check-a-change) say how to run the same checks
locally.

The workflow runs on Linux. The fix in `source-links.mjs` shows only where the path separator is a
backslash, so its check in `source-links.test.mjs` can fail only when the tooling tests run on
Windows.

Automated checks do not establish that an explanation is accurate or clear. The writing guide
requires editorial review of changed pages and of representative exports.

## Publication

Pull requests and pushes validate the website without publishing it. Publication is a manual run
of the documentation workflow on `main` with **publish** selected, after both validation jobs
pass. It deploys the `/allors3/` build to the repository's GitHub Pages site, which the custom
domain of the organization site `allors/allors.github.io` serves at
`https://docs.allors.com/allors3/`. A merged change is live only after such a run, so the
published pages can lag behind `main`.

## Remaining work

- Describe security, the workspace, the JSON API, populations and the command-line tool, and the
  database adapters, which the [documentation index](../index.md) names as not yet covered.
- [Domains](../domains.md#the-behaviour) tells readers not to rely on the order among the parts
  of interfaces that do not inherit from each other, because the comparison in `MethodCompiler`
  does not define one. Replace that caution with the rule once the order is defined and a test
  pins it.
- Decide with the other Allors repositories whether to extract the reused tooling.
