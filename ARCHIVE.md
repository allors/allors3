# Archived line: main up to 27 October 2026

> **Status: Historical.** This branch keeps `main` as it stood on 27 October 2026, at
> `91df37f528`. It is not meant to be merged. The current platform is on `main`, which
> restarted from `0e907fea68`.

## What this branch is

The line of `main` from `0e907fea68` of 8 May 2026 to `91df37f528` of 27 October 2026: 166
commits, of which the 74 up to `9f21fbaccf` of 22 June 2026 are also on the `v3.1` branch and
the 92 above it are only here. The two spike archives,
`archive/workspace-connection-session-layers` and `archive/workspace-protected-reads`, fork from
commits of this line, `5b3d93025c` (#278) and `d034e53d70` (#282). Nothing of this line is on
`main` by ancestry; the commits above `0e907fea68` there are new.

## Why it was set aside

`main` restarted from `0e907fea68`, the platform state that a downstream application inlined on
24 August 2026 and runs in production, plus that application's changes to System and Core. The
work on this line had no consumer: its platform fixes never reached the application, and its
plug-ins, the domain that extends several domains and the workspace API changes were built ahead
of any use. Rather than carry a year of change untested in production into the relink, the
platform re-anchored on what runs. Fixes return first, from `v3.1`, one pull request at a time;
features return when an application needs them. The changelog on `main` records the restart.

## What it holds

Also on `v3.1`, up to `9f21fbaccf`:

- The fix batch of June 2026: the adapter and database fixes #29 to #49, the Core fixes #50 to
  #64, the TypeScript workspace fixes #84 and #85, and the externalized configuration with
  `ALLORS_CONFIG_ROOT` and the cross-platform container CI (#80).
- Nerdbank.GitVersioning (`43b976e8d1`).

Only here:

- Workspace fixes of June 2026 (#87 to #94), the bracketed database name when enabling snapshot
  isolation (`ccb1ac67e4`) and the FetchClient keep-alive fix (`3f37891135`); the Base and Apps
  fixes of June and September 2026 (#100 to #138).
- The removal of the extranet application (#253) and of Dependabot (#267).
- The security hardening of the server: Identity cookie authentication, a default-deny API and
  account recovery (#255).
- The rename of Core's test domain from Custom to Test (#256), and the fixes #257, #259, #260,
  #261 and #262.
- The v3.2 direction documents (#265, #268) and the removal of Base and Apps (#269).
- The fixes #270 to #274; #274 is also on `v3.1` as #276.
- The Identity plug-in (#275) and the Entra plug-in (#277): authentication out of Core, Core's
  `User` without authentication fields.
- A domain that extends several domains, with the Diamond tree of platform test domains (#278).
- The workspace API: `Api` into `Allors.Database.Workspace.Json` (#279), the local adapter over
  the JSON API (#280), the fixes #282 and `02a55627ad`, the Local adapter removed (#283), object
  masking removed (#288), procedures removed and unknown JSON fields rejected (#290).
- The documentation website (#284, #285), published at https://docs.allors.com/allors3/ from
  this line, and the conservative change policy in `AGENTS.md` (October 2026).

## Where to read more

`CHANGELOG.md` on this branch lists everything above #269 in detail; `docs/src/content/docs/`
holds the website's pages, including the writing guide and the maintainer notes on domain
inheritance, the workspace session state and the spikes. The spike archives have their own
`ARCHIVE.md`.
