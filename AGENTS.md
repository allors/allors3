# AGENTS.md

## Development Notes (important, read before editing)
- Objects are managed by Allors, always use the API to create or delete objects.
- Relations are managed by Allors, always use the API get or set Relations.
- Relations are bidirectional by design.
- Roles and Associations are RelationEndPoints
- Roles are forward and writable
- Associations are inverse and readonly
- Objects delegate to their Strategy to handle operations.
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

## Build Commands

Build uses Nuke. 
