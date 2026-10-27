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
  return { version, values, grantVersion: 1, grantAllowsRead: true };
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
  assert.deepEqual(refreshed.state.latestReceived, record(1, baseline));
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
test('disjoint edits pin the active snapshot until Reset, then chosen edits can be reapplied', () => {
  const sequence = frames('disjoint');
  const remoteValues = { ...baseline, integer: 2 };
  const accepted = sequence['editing-pull'];
  assert.equal(accepted.outcome, 'pinned');
  assert.deepEqual(accepted.state.retained, record(1, baseline));
  assert.deepEqual(accepted.state.latestReceived, record(2, remoteValues));
  assert.deepEqual(accepted.state.local, local);
  assert.deepEqual(visibleValues(accepted.state), { ...baseline, ...local });
  assertDiff(accepted.state, localDiff);

  assert.deepEqual(sequence.reset.state.retained, record(2, remoteValues));
  assert.deepEqual(sequence.reset.state.connection, accepted.state.connection);
  assert.deepEqual(sequence.reset.state.database, accepted.state.database);
  assertDiff(sequence.reset.state, []);
  assert.deepEqual(visibleValues(sequence.reapply.state), { ...remoteValues, ...local });

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
  test(`${id}: edits pin the whole snapshot and Reset adopts the already-received replacement`, () => {
    const sequence = frames(id);
    const remoteValues = { ...baseline, integer: 2, ...remoteChange };
    const rejected = sequence['editing-pull'];
    assert.equal(rejected.outcome, 'pinned');
    assert.deepEqual(rejected.state.connection, record(2, remoteValues));
    assert.deepEqual(rejected.state.retained, record(1, baseline));
    assert.deepEqual(rejected.state.latestReceived, record(2, remoteValues));
    assert.deepEqual(rejected.state.local, local);
    assert.deepEqual(visibleValues(rejected.state), { ...baseline, ...local });
    assertDiff(rejected.state, localDiff);

    assert.deepEqual(sequence.reset.state.retained, record(2, remoteValues));
    assert.deepEqual(visibleValues(sequence.reset.state), remoteValues);
    assert.deepEqual(sequence.reset.state.local, {});
    assertDiff(sequence.reset.state, []);

    assert.equal(sequence['recovery-pull'], undefined);
    assert.deepEqual(sequence.reset.state.connection, rejected.state.connection);
    assert.deepEqual(sequence.reset.state.database, rejected.state.database);
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

test('security-only changes remain pinned with data until Reset adopts the session’s latest snapshot', () => {
  const sequence = frames('permissions');
  const before = sequence['local-edit'];
  const after = sequence['permission-refresh'];
  assert.equal(before.state.retained.grantAllowsRead, true);
  assert.equal(after.outcome, 'permission-refreshed');
  assert.equal(after.state.retained.grantAllowsRead, true);
  assert.equal(after.state.connection.grantAllowsRead, false);
  assert.deepEqual(after.state.retained, record(1, baseline));
  assert.deepEqual(after.state.latestReceived, record(1, baseline));
  assert.deepEqual(visibleValues(after.state), visibleValues(before.state));
  assert.deepEqual(diff(after.state), diff(before.state));
  const received = sequence['editing-pull'];
  assert.equal(received.outcome, 'pinned');
  assert.equal(received.state.latestReceived.version, 1);
  assert.equal(received.state.latestReceived.grantVersion, 2);
  assert.equal(received.state.latestReceived.grantAllowsRead, false);
  assert.deepEqual(received.state.retained, before.state.retained);
  assert.deepEqual(sequence.reset.state.retained, received.state.latestReceived);
  assert.equal(sequence.reset.state.retained.grantAllowsRead, false);
  assert.equal(visibleValues(sequence.reset.state).string, null);
  assertDiff(sequence.reset.state, []);
});

test('revisiting scenarios and changing returned frames cannot leak state into other steps or visits', () => {
  for (const entry of scenarios) {
    const original = structuredClone(getFrames(entry));
    const visited = getFrames(entry);
    const final = visited.at(-1);
    final.state.retained.values.toMany.push('unrelated');
    final.state.connection.values.string = 'unrelated';
    final.state.local.string = 'unrelated';
    final.state.retained.grantAllowsRead = !final.state.retained.grantAllowsRead;
    final.state.latestReceived.values.string = 'unrelated';
    assert.deepEqual(visited[0], original[0], `${entry.id}: the initial frame changed`);
    for (const other of scenarios) getFrames(other);
    assert.deepEqual(getFrames(entry), original, `${entry.id}: a new visit changed`);
  }
});
