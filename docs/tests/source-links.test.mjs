import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { rewriteLink } from '../integrations/source-links.mjs';

const source = fileURLToPath(new URL('../src/content/docs/internals/architecture.md', import.meta.url));

test('source links become site links at a project or domain root', () => {
  assert.equal(rewriteLink('../domains.md#declare-a-domain', source, '/allors3/'), '/allors3/domains/#declare-a-domain');
  for (const base of ['/allors3/', '/']) {
    assert.equal(rewriteLink('build-and-test.md#prerequisites', source, base), `${base}internals/build-and-test/#prerequisites`);
  }
  assert.equal(rewriteLink('../index.md', source, '/'), '/');
  assert.equal(rewriteLink('#scope', source, '/allors3/'), '#scope');
});

test('repository references resolve to the documented branch', () => {
  assert.equal(rewriteLink('../../../../../README.md#documentation', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/README.md#documentation');
  assert.equal(rewriteLink('https://example.com/guide.md', source, '/allors3/'), 'https://example.com/guide.md');
});

test('broken source links fail the build with a useful target', () => {
  assert.throws(() => rewriteLink('../missing.md', source, '/allors3/'), /missing\.md/);
});

test('repository fragments are checked against the local Markdown headings', () => {
  assert.equal(rewriteLink('../../../../../AGENTS.md#documentation', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/AGENTS.md#documentation');
  assert.throws(() => rewriteLink('../../../../../AGENTS.md#no-such-heading', source, '/allors3/'),
    /no-such-heading.*AGENTS\.md/);
  const fixture = 'https://github.com/allors/allors3/blob/main/docs/tests/fixtures/anchors.md';
  for (const id of ['added', 'added-1', 'use-rewritelink-in-astro', 'explicit-anchor']) {
    assert.equal(rewriteLink(`../../../../tests/fixtures/anchors.md#${id}`, source, '/allors3/'), `${fixture}#${id}`);
  }
  assert.throws(() => rewriteLink('../../../../tests/fixtures/anchors.md#not-a-heading', source, '/allors3/'), /not-a-heading/);
});

test('fragments on repository files that are not Markdown are left alone', () => {
  assert.equal(rewriteLink('../../../../../dotnet/System/Database/Allors.Database.Meta.Props/Props/MethodCompiler.cs#L29', source, '/allors3/'),
    'https://github.com/allors/allors3/blob/main/dotnet/System/Database/Allors.Database.Meta.Props/Props/MethodCompiler.cs#L29');
});
