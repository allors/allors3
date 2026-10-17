import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { rewriteLink } from '../integrations/source-links.mjs';

const source = fileURLToPath(new URL('../src/content/docs/internals/workspace-session-state.mdx', import.meta.url));

test('source links become site links at a project or domain root', () => {
  assert.equal(rewriteLink('../domains.md#kinds-of-domains', source, '/allors3/'), '/allors3/domains/#kinds-of-domains');
  assert.equal(rewriteLink('../index.md', source, '/'), '/');
  assert.equal(rewriteLink('#ownership', source, '/allors3/'), '#ownership');
});

test('repository references resolve to the documented branch', () => {
  assert.equal(rewriteLink('../../../../../ARCHITECTURE.md#domain-inheritance', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/ARCHITECTURE.md#domain-inheritance');
  assert.equal(rewriteLink('https://example.com/guide.md', source, '/allors3/'), 'https://example.com/guide.md');
});

test('broken source links fail the build with a useful target', () => {
  assert.throws(() => rewriteLink('../missing.md', source, '/allors3/'), /missing\.md/);
});

test('repository fragments are checked against the local Markdown headings', () => {
  assert.equal(rewriteLink('../../../../../ARCHITECTURE.md#test-scaffolding-is-separate-from-inherited-code', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/ARCHITECTURE.md#test-scaffolding-is-separate-from-inherited-code');
  assert.throws(() => rewriteLink('../../../../../ARCHITECTURE.md#no-such-heading', source, '/allors3/'),
    /no-such-heading.*ARCHITECTURE\.md/);
  const fixture = 'https://github.com/allors/allors3/blob/main/docs/tests/fixtures/anchors.md';
  for (const id of ['added', 'added-1', 'use-rewritelink-in-astro', 'explicit-anchor']) {
    assert.equal(rewriteLink(`../../../../tests/fixtures/anchors.md#${id}`, source, '/allors3/'), `${fixture}#${id}`);
  }
  assert.throws(() => rewriteLink('../../../../tests/fixtures/anchors.md#not-a-heading', source, '/allors3/'), /not-a-heading/);
});

test('fragments on repository files that are not Markdown are left alone', () => {
  assert.equal(rewriteLink('../../../../../dotnet/Core/Workspace/Tests/Tests/MergeTests.cs#L59', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/dotnet/Core/Workspace/Tests/Tests/MergeTests.cs#L59');
});
