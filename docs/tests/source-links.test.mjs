import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { rewriteLink } from '../integrations/source-links.mjs';

const source = fileURLToPath(new URL('../src/content/docs/internals/workspace-session-state.md', import.meta.url));

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
