import assert from 'node:assert/strict';
import test from 'node:test';
import { diff, getFrames, scenarios, visibleValues } from '../src/examples/workspace-session-state.ts';

const baseline = { string: 'baseline', integer: 1, toOne: 'c1B', toMany: ['c1B'] };
const local = { string: 'local', toOne: 'c1C', toMany: ['c1B', 'c1C'] };
const remote = { string: 'remote', integer: 2, toOne: 'c1D', toMany: ['c1B', 'c1D'] };
const localDiff = [
  { role: 'string', original: 'baseline', changed: 'local' },
  { role: 'toOne', original: 'c1B', changed: 'c1C' },
  { role: 'toMany', original: ['c1B'], changed: ['c1B', 'c1C'] },
];

function record(version, values) {
  return { version, values };
}

function scenario(id) {
  const found = scenarios.find((entry) => entry.id === id);
  assert.ok(found, `Missing prepared scenario: ${id}`);
  return found;
}

function frames(id) {
  return Object.fromEntries(getFrames(scenario(id)).map((frame) => [frame.id, frame]));
}

function assertDiff(state, expected) {
  const byRole = (entries) => Object.fromEntries(entries.map((entry) => [entry.role, entry]));
  assert.deepEqual(byRole(diff(state)), byRole(expected));
  assert.equal(diff(state).length, expected.length);
}

test('prepared scenarios identify every step and expose explanations without running the controls', () => {
  assert.deepEqual(scenarios.map((entry) => entry.id).sort(), [
    'disjoint', 'permissions', 'push-reset', 'remote-refresh',
    'string-conflict', 'to-many-conflict', 'to-one-conflict',
  ]);
  for (const entry of scenarios) {
    assert.ok(entry.title.trim());
    assert.ok(entry.summary.trim());
    const sequence = getFrames(entry);
    assert.equal(sequence[0].id, 'start');
    assert.equal(new Set(sequence.map((frame) => frame.id)).size, sequence.length);
    for (const frame of sequence) {
      assert.ok(frame.title.trim(), `${entry.id}/${frame.id} needs a title`);
      assert.ok(frame.explanation.trim(), `${entry.id}/${frame.id} needs an explanation`);
    }
  }
});

// DatabaseDiffRetainsBaselineWhenAnotherSessionRefreshes and its TypeScript counterpart.
test('another session refreshes the connection without replacing the editing record or diff originals', () => {
  const sequence = frames('remote-refresh');
  assert.deepEqual(Object.keys(sequence), [
    'start', 'local-edit', 'remote-push', 'remote-pull', 'push', 'reset', 'recovery-pull',
  ]);
  assert.deepEqual(sequence.start.state.retained, record(1, baseline));
  assert.deepEqual(sequence.start.state.local, {});
  assert.equal(sequence.start.state.pushed, false);
  assert.equal(sequence.start.outcome, 'ready');

  const pushed = sequence['remote-push'];
  assert.equal(pushed.outcome, 'remote-pushed');
  assert.deepEqual(pushed.state.database, record(2, remote));
  assert.deepEqual(pushed.state.connection, record(1, baseline));

  const refreshed = sequence['remote-pull'];
  assert.equal(refreshed.outcome, 'refreshed');
  assert.deepEqual(refreshed.state.connection, record(2, remote));
  assert.deepEqual(refreshed.state.retained, record(1, baseline));
  assert.deepEqual(refreshed.state.local, local);
  assert.deepEqual(visibleValues(refreshed.state), { ...baseline, ...local });
  assertDiff(refreshed.state, localDiff);

  assert.equal(sequence.push.outcome, 'version-error');
  assert.deepEqual(sequence.push.state, refreshed.state);
  assert.equal(sequence.push.state.pushed, false);

  assert.equal(sequence.reset.outcome, 'reset');
  assert.deepEqual(sequence.reset.state.retained, record(1, baseline));
  assert.deepEqual(sequence.reset.state.connection, record(2, remote));
  assert.deepEqual(visibleValues(sequence.reset.state), baseline);
  assert.deepEqual(sequence.reset.state.local, {});
  assertDiff(sequence.reset.state, []);

  const recovered = sequence['recovery-pull'];
  assert.equal(recovered.outcome, 'accepted');
  assert.deepEqual(recovered.state.retained, record(2, remote));
  assert.deepEqual(visibleValues(recovered.state), remote);
  assertDiff(recovered.state, []);
});

// DatabaseEditingBaselineAfterPull("disjoint") and databaseEditingBaselineAfterPull: disjoint.
test('a disjoint editing Pull accepts the integer and version while preserving all three local edits', () => {
  const sequence = frames('disjoint');
  const remoteValues = { ...baseline, integer: 2 };
  const accepted = sequence['editing-pull'];
  assert.equal(accepted.outcome, 'accepted');
  assert.deepEqual(accepted.state.retained, record(2, remoteValues));
  assert.deepEqual(accepted.state.local, local);
  assert.deepEqual(visibleValues(accepted.state), { ...remoteValues, ...local });
  assertDiff(accepted.state, localDiff);

  const pushed = sequence.push;
  assert.equal(pushed.outcome, 'pushed');
  assert.equal(pushed.state.pushed, true);
  assert.deepEqual(pushed.state.database, record(3, { ...remoteValues, ...local }));
  assert.deepEqual(pushed.state.connection, record(2, remoteValues));
  assert.deepEqual(pushed.state.retained, record(2, remoteValues));
  assertDiff(pushed.state, localDiff);

  const recovered = sequence['recovery-pull'];
  assert.equal(recovered.outcome, 'accepted');
  assert.equal(recovered.state.pushed, false);
  assert.deepEqual(recovered.state.retained, record(3, { ...remoteValues, ...local }));
  assert.deepEqual(recovered.state.local, {});
  assertDiff(recovered.state, []);
});

// The three conflicting rows of DatabaseEditingBaselineAfterPull in both runtimes.
for (const [id, remoteChange] of [
  ['string-conflict', { string: 'remote' }],
  ['to-one-conflict', { toOne: 'c1D' }],
  ['to-many-conflict', { toMany: ['c1B', 'c1D'] }],
]) {
  test(`${id}: one conflicting role rejects the entire record until Reset followed by Pull`, () => {
    const sequence = frames(id);
    const remoteValues = { ...baseline, integer: 2, ...remoteChange };
    const rejected = sequence['editing-pull'];
    assert.equal(rejected.outcome, 'merge-error');
    assert.deepEqual(rejected.state.connection, record(2, remoteValues));
    assert.deepEqual(rejected.state.retained, record(1, baseline));
    assert.deepEqual(rejected.state.local, local);
    assert.deepEqual(visibleValues(rejected.state), { ...baseline, ...local });
    assertDiff(rejected.state, localDiff);

    assert.deepEqual(sequence.reset.state.retained, record(1, baseline));
    assert.deepEqual(visibleValues(sequence.reset.state), baseline);
    assert.deepEqual(sequence.reset.state.local, {});
    assertDiff(sequence.reset.state, []);

    const recovered = sequence['recovery-pull'];
    assert.equal(recovered.outcome, 'accepted');
    assert.deepEqual(recovered.state.retained, record(2, remoteValues));
    assert.deepEqual(visibleValues(recovered.state), remoteValues);
    assertDiff(recovered.state, []);
  });
}

// ResetUnitAfterPushTest / resetUnitAfterPush use a null string baseline and local value "X".
test('Reset after successful Push exposes the last pulled value and preserves the pushed flag until Pull', () => {
  const sequence = frames('push-reset');
  const original = { ...baseline, string: null };
  const changed = { ...original, string: 'X' };
  assert.deepEqual(sequence.start.state.retained, record(1, original));
  assert.deepEqual(sequence['local-edit'].state.local, { string: 'X' });

  assert.equal(sequence.push.outcome, 'pushed');
  assert.equal(sequence.push.state.pushed, true);
  assert.deepEqual(sequence.push.state.database, record(2, changed));
  assert.deepEqual(sequence.push.state.connection, record(1, original));
  assert.deepEqual(sequence.push.state.retained, record(1, original));
  assert.deepEqual(visibleValues(sequence.push.state), changed);
  assertDiff(sequence.push.state, [{ role: 'string', original: null, changed: 'X' }]);

  assert.equal(sequence.reset.state.pushed, true);
  assert.deepEqual(sequence.reset.state.retained, record(1, original));
  assert.deepEqual(sequence.reset.state.connection, record(1, original));
  assert.deepEqual(visibleValues(sequence.reset.state), original);
  assertDiff(sequence.reset.state, []);

  const recovered = sequence['recovery-pull'];
  assert.equal(recovered.state.pushed, false);
  assert.deepEqual(recovered.state.connection, record(2, changed));
  assert.deepEqual(recovered.state.retained, record(2, changed));
  assert.deepEqual(visibleValues(recovered.state), changed);
  assertDiff(recovered.state, []);
});

test('the known-grant illustration changes the live permission map without accepting the newer data record', () => {
  const sequence = frames('permissions');
  const before = sequence['remote-pull'];
  const after = sequence['permission-refresh'];
  assert.equal(before.state.grantAllowsRead, true);
  assert.equal(after.outcome, 'permission-refreshed');
  assert.equal(after.state.grantAllowsRead, false);
  assert.deepEqual(after.state.retained, record(1, baseline));
  assert.deepEqual(after.state.connection, record(2, { ...baseline, integer: 2 }));
  assert.deepEqual(after.state, { ...before.state, grantAllowsRead: false });
  assert.deepEqual(visibleValues(after.state), visibleValues(before.state));
  assert.deepEqual(diff(after.state), diff(before.state));
});

test('revisiting scenarios and changing returned frames cannot leak state into other steps or visits', () => {
  for (const entry of scenarios) {
    const original = structuredClone(getFrames(entry));
    const visited = getFrames(entry);
    const final = visited.at(-1);
    final.state.retained.values.toMany.push('unrelated');
    final.state.connection.values.string = 'unrelated';
    final.state.local.string = 'unrelated';
    final.state.grantAllowsRead = !final.state.grantAllowsRead;
    assert.deepEqual(visited[0], original[0], `${entry.id}: the initial frame changed`);
    for (const other of scenarios) getFrames(other);
    assert.deepEqual(getFrames(entry), original, `${entry.id}: a new visit changed`);
  }
});
