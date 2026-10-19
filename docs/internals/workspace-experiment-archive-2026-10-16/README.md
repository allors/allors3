# Workspace experiment archive — 2026-10-16

> **Status: Historical.** Snapshot of unfinished workspace work, preserved for reference before
> starting a narrower Local-adapter removal from `main`. This branch is an archive, not a release
> or a claim that the full integration matrix passes.

This archive combines the result of 16 commits on `refactor/workspace-pull-refresh` and all 320
uncommitted paths into one commit above `main` at
`d034e53d70f0045e862bd388a283f7029db4b59c`. The original head was
`dd34ff56f261d1060ef4bc87eaa37abc4598a867`, with parent
`a8265eb08059f66ddb9dfa0289d93d29b429c0b4`. [Prior commit subjects](prior-commits.txt) preserve
that development sequence; [source hashes](source-snapshot.json) identify the exact incoming files
and deletions. Existing source bytes are retained. The only archive-specific edit to an existing
file is an added entry in the documentation index; this directory contains the additional notes.

## State preserved

The [migration inventory](../workspace-caller-migration-inventory.md) holds the designs and
implementation history. The snapshot includes protected reads and mutation results, caller and
lifecycle migrations, removed workspace features, security/database changes and the completed
cache ownership simplification. The cache and two-step discovery remain. The later test-consolidation
map is a proposal only; it was not implemented. Local-adapter removal was explored but not applied.

The [handoff before archiving](handoff-before-archive.txt) is preserved verbatim. Its approval and
next-step statements describe that earlier boundary; this archive decision supersedes them.
The approved direction is to retain this snapshot, then separately rebuild PR #283 from `main`
with one commit limited to removing the Local workspace adapter and its necessary test/build/docs
changes. The original PR branch is not changed by creating this archive.

The Local-removal explorations beside this page describe the experimental tree. They must not be
used as the implementation map for `main`: that baseline has a smaller Local adapter, Memory-backed
tests, and none of this experiment's protected-read machinery. Make a fresh baseline-specific
coverage map before implementation. The archived security and protocol changes do not enter the
replacement PR automatically.

## Validation record

The latest completed cache-ownership validation built the .NET solution and passed 710 .NET pure
cases, 631 TypeScript pure cases, and 417 selected integration cases on each SQL provider: 2,175
selected executions. The [completion record](cache-ownership-completion.txt),
[validation totals](cache-ownership-validation.json) and
[tested source hashes](cache-ownership-tested-source-hashes.json) preserve the evidence boundary.
Previously recorded expectation failures remain pending. No full current provider matrix is
claimed green. Archive verification compares content, file modes, deletions and commit ancestry;
it does not rerun or reinterpret earlier tests.

## Retained evidence

The files in this directory preserve the handoff, validation summaries, source preservation,
exact consolidation maps and Local-removal exploration. Temporary paths mentioned in those copied
reports locate the original raw evidence and may not exist on another machine. Raw runner logs,
binaries, installed dependencies and ignored generated outputs are not added to Git. The
[339 generated-output hashes](generated-output-hashes.json) retain their validation identity;
regenerate outputs through the normal build when working from this snapshot.

No runtime or test behavior changes are made solely for archiving. Recover this experiment from
`archive/workspace-protected-reads`; use `main` as the starting point for the separate replacement
PR. Do not merge the archive as the Local-removal change.
