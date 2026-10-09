import { C1 } from '@allors/default/workspace/domain';
import {
  ICompositeDiff,
  ICompositesDiff,
  IUnitDiff,
  Pull,
} from '@allors/system/workspace/domain';
import { Fixture, name_c1A, name_c1B, name_c1C, name_c1D } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('databaseMergeError', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.Name,
        value: name_c1A,
      },
    },
  };

  let result = await session1.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  result = await session2.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';
  c1a_2.C1AllorsString = 'Y';

  await session2.push();

  result = await session1.pull([pull]);

  expect(result.hasErrors).toBeTruthy();
  expect(result.mergeErrors.length).toBe(1);

  const mergeError = result.mergeErrors[0];

  expect(mergeError.strategy).toBe(c1a_1.strategy);
});

test.each(['disjoint', 'string', 'to-one', 'to-many'])(
  'databaseEditingBaselineAfterPull: %s',
  async (remoteEdit) => {
    const { workspace, m } = fixture;
    const remote = workspace.createSession();
    const editing = workspace.createSession();
    const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
    let objects = (await remote.pull(pull)).collection<C1>(m.C1);
    const source = objects.find((v) => v.Name === name_c1A);
    const originalTarget = objects.find((v) => v.Name === name_c1B);
    const remoteTarget = objects.find((v) => v.Name === name_c1D);
    source.C1AllorsString = 'baseline';
    source.C1AllorsInteger = 1;
    source.C1C1Many2One = originalTarget;
    source.C1C1Many2Manies = [originalTarget];
    expect((await remote.push()).hasErrors).toBe(false);
    expect((await remote.pull(pull)).hasErrors).toBe(false);

    objects = (await editing.pull(pull)).collection<C1>(m.C1);
    const edited = objects.find((v) => v.Name === name_c1A);
    const baselineTarget = objects.find((v) => v.Name === name_c1B);
    const localTarget = objects.find((v) => v.Name === name_c1C);
    const version = edited.strategy.version;
    edited.C1AllorsString = 'local';
    edited.C1C1Many2One = localTarget;
    edited.addC1C1Many2Many(localTarget);

    source.C1AllorsInteger = 2;
    switch (remoteEdit) {
      case 'string':
        source.C1AllorsString = 'remote';
        break;
      case 'to-one':
        source.C1C1Many2One = remoteTarget;
        break;
      case 'to-many':
        source.addC1C1Many2Many(remoteTarget);
        break;
    }

    expect((await remote.push()).hasErrors).toBe(false);
    expect((await remote.pull(pull)).hasErrors).toBe(false);
    expect(source.strategy.version).not.toBe(version);
    const result = await editing.pull(pull);
    const conflict = remoteEdit !== 'disjoint';
    expect(result.hasErrors).toBe(conflict);
    expect(result.mergeErrors ?? []).toHaveLength(conflict ? 1 : 0);
    if (conflict) {
      expect(result.mergeErrors[0].strategy).toBe(edited.strategy);
    }

    // A conflicting role rejects the entire replacement record for this object.
    expect(edited.strategy.version).toBe(
      conflict ? version : source.strategy.version
    );
    expect(edited.C1AllorsInteger).toBe(conflict ? 1 : 2);
    expect(edited.C1AllorsString).toBe('local');
    expect(edited.C1C1Many2One).toBe(localTarget);
    expect(edited.C1C1Many2Manies.map((v) => v.id).sort()).toEqual(
      [baselineTarget.id, localTarget.id].sort()
    );
    expect(edited.strategy.hasChanges).toBe(true);
    const diffs = edited.strategy.diff();
    expect(diffs).toHaveLength(3);
    const unit = diffs.find(
      (v) => v.relationType === m.C1.C1AllorsString.relationType
    ) as IUnitDiff;
    expect(unit.originalRole).toBe('baseline');
    expect(unit.changedRole).toBe('local');
    const one = diffs.find(
      (v) => v.relationType === m.C1.C1C1Many2One.relationType
    ) as ICompositeDiff;
    expect(one.originalRole).toBe(baselineTarget);
    expect(one.changedRole).toBe(localTarget);
    const many = diffs.find(
      (v) => v.relationType === m.C1.C1C1Many2Manies.relationType
    ) as ICompositesDiff;
    expect(many.originalRoles).toEqual([baselineTarget]);
    expect(many.changedRoles.map((v) => v.id).sort()).toEqual(
      [baselineTarget.id, localTarget.id].sort()
    );

    if (!conflict) {
      // Push must now use the accepted version, preserving the remote integer.
      expect((await editing.push()).hasErrors).toBe(false);
      expect((await editing.pull(pull)).hasErrors).toBe(false);
      expect(edited.strategy.diff()).toHaveLength(0);
      expect((await remote.pull(pull)).hasErrors).toBe(false);
      expect(source.C1AllorsString).toBe('local');
      expect(source.C1AllorsInteger).toBe(2);
      expect(source.C1C1Many2One.id).toBe(localTarget.id);
      expect(source.C1C1Many2Manies.map((v) => v.id).sort()).toEqual(
        [baselineTarget.id, localTarget.id].sort()
      );
      return;
    }

    edited.strategy.reset();
    expect(edited.strategy.version).toBe(version);
    expect(edited.C1AllorsString).toBe('baseline');
    expect(edited.C1AllorsInteger).toBe(1);
    expect(edited.C1C1Many2One).toBe(baselineTarget);
    expect(edited.C1C1Many2Manies).toEqual([baselineTarget]);
    expect(edited.strategy.diff()).toHaveLength(0);
    expect(edited.strategy.hasChanges).toBe(false);

    expect((await editing.pull(pull)).hasErrors).toBe(false);
    expect(edited.strategy.version).toBe(source.strategy.version);
    expect(edited.C1AllorsString).toBe(source.C1AllorsString);
    expect(edited.C1AllorsInteger).toBe(2);
    expect(edited.C1C1Many2One.id).toBe(source.C1C1Many2One.id);
    expect(edited.C1C1Many2Manies.map((v) => v.id).sort()).toEqual(
      source.C1C1Many2Manies.map((v) => v.id).sort()
    );
    expect(edited.strategy.diff()).toHaveLength(0);
  }
);

test.each(['dateTime', 'binary'])(
  'databaseUnitEqualityDuringDisjointPull: %s',
  async (kind) => {
    const { workspace, m } = fixture;
    const remote = workspace.createSession();
    const editing = workspace.createSession();
    const source = await fixture.pullC1(remote, name_c1A);
    const pull: Pull = { object: source };
    const binary = kind === 'binary';
    const roleType = binary ? m.C1.C1AllorsBinary : m.C1.C1AllorsDateTime;
    const baseline = binary ? 'AQI=' : new Date('2020-01-02T03:04:05Z');
    const local = binary ? 'AwQ=' : new Date('2021-01-02T03:04:05Z');
    source.strategy.setUnitRole(roleType, baseline);
    source.C1AllorsInteger = 1;
    expect((await remote.push()).hasErrors).toBe(false);
    expect((await remote.pull(pull)).hasErrors).toBe(false);
    const edited = await fixture.pullC1(editing, name_c1A);
    const version = edited.strategy.version;
    edited.strategy.setUnitRole(roleType, local);
    source.C1AllorsInteger = 2;
    expect((await remote.push()).hasErrors).toBe(false);
    expect((await remote.pull(pull)).hasErrors).toBe(false);
    expect(source.strategy.getUnitRole(roleType)).toEqual(baseline);

    const result = await editing.pull({ object: edited });
    expect(source.strategy.version).not.toBe(version);
    expect(result.hasErrors).toBe(false);
    expect(result.mergeErrors ?? []).toHaveLength(0);
    expect(edited.strategy.version).toBe(source.strategy.version);
    expect(edited.C1AllorsInteger).toBe(2);
    expect(edited.strategy.getUnitRole(roleType)).toEqual(local);
    const diffs = edited.strategy.diff();
    expect(diffs).toHaveLength(1);
    const diff = diffs[0] as IUnitDiff;
    expect(diff.relationType).toBe(roleType.relationType);
    expect(diff.originalRole).toEqual(baseline);
    expect(diff.changedRole).toEqual(local);
    edited.strategy.reset();
    expect(edited.strategy.getUnitRole(roleType)).toEqual(baseline);
    expect(edited.strategy.diff()).toHaveLength(0);
    expect((await editing.pull({ object: edited })).hasErrors).toBe(false);
    expect(edited.strategy.version).toBe(source.strategy.version);
    expect(edited.C1AllorsInteger).toBe(2);
    expect(edited.strategy.getUnitRole(roleType)).toEqual(baseline);
  }
);

test.each([
  ['dateTime', 'unchanged-null'],
  ['binary', 'unchanged-null'],
  ['dateTime', 'null-to-value'],
  ['binary', 'null-to-value'],
  ['dateTime', 'value-to-null'],
  ['binary', 'value-to-null'],
  ['dateTime', 'different-value'],
  ['binary', 'different-value'],
  ['dateTime', 'local-removal'],
  ['binary', 'local-removal'],
  ['binary', 'different-length'],
])('databaseUnitValueEqualityDuringPull: %s, %s', async (kind, remoteEdit) => {
  const { workspace, m } = fixture;
  const remote = workspace.createSession();
  const editing = workspace.createSession();
  const source = await fixture.pullC1(remote, name_c1A);
  const binary = kind === 'binary';
  const roleType = binary ? m.C1.C1AllorsBinary : m.C1.C1AllorsDateTime;
  const initialValue = binary ? 'AQI=' : new Date('2020-01-02T03:04:05Z');
  const baseline =
    remoteEdit === 'unchanged-null' || remoteEdit === 'null-to-value'
      ? null
      : initialValue;
  const local =
    remoteEdit === 'local-removal'
      ? null
      : binary
      ? 'AwQ='
      : new Date('2021-01-02T03:04:05Z');
  const remoteValue =
    remoteEdit === 'different-value'
      ? binary
        ? 'AQM='
        : new Date('2022-01-02T03:04:05Z')
      : remoteEdit === 'different-length'
      ? 'AQID'
      : remoteEdit === 'value-to-null'
      ? null
      : remoteEdit === 'null-to-value'
      ? initialValue
      : baseline;
  const conflict =
    remoteEdit !== 'unchanged-null' && remoteEdit !== 'local-removal';

  source.strategy.setUnitRole(roleType, baseline);
  source.C1AllorsInteger = 1;
  expect((await remote.push()).hasErrors).toBe(false);
  expect((await remote.pull({ object: source })).hasErrors).toBe(false);
  const edited = await fixture.pullC1(editing, name_c1A);
  const version = edited.strategy.version;
  expect(edited.strategy.getUnitRole(roleType)).toEqual(baseline);
  edited.strategy.setUnitRole(roleType, local);

  source.strategy.setUnitRole(roleType, remoteValue);
  source.C1AllorsInteger = 2;
  expect((await remote.push()).hasErrors).toBe(false);
  expect((await remote.pull({ object: source })).hasErrors).toBe(false);
  expect(source.strategy.version).not.toBe(version);
  expect(source.strategy.getUnitRole(roleType)).toEqual(remoteValue);

  const result = await editing.pull({ object: edited });
  expect(result.hasErrors).toBe(conflict);
  expect(result.mergeErrors ?? []).toHaveLength(conflict ? 1 : 0);
  if (conflict) {
    expect(result.mergeErrors[0].strategy).toBe(edited.strategy);
  }
  expect(edited.strategy.version).toBe(
    conflict ? version : source.strategy.version
  );
  expect(edited.C1AllorsInteger).toBe(conflict ? 1 : 2);
  expect(edited.strategy.getUnitRole(roleType)).toEqual(local);
  const diffs = edited.strategy.diff();
  expect(diffs).toHaveLength(1);
  const diff = diffs[0] as IUnitDiff;
  expect(diff.relationType).toBe(roleType.relationType);
  expect(diff.originalRole ?? null).toEqual(baseline);
  expect(diff.changedRole).toEqual(local);

  edited.strategy.reset();
  expect(edited.strategy.version).toBe(
    conflict ? version : source.strategy.version
  );
  expect(edited.strategy.getUnitRole(roleType)).toEqual(baseline);
  expect(edited.strategy.diff()).toHaveLength(0);
  expect((await editing.pull({ object: edited })).hasErrors).toBe(false);
  expect(edited.strategy.version).toBe(source.strategy.version);
  expect(edited.C1AllorsInteger).toBe(2);
  expect(edited.strategy.getUnitRole(roleType)).toEqual(remoteValue);
  expect(edited.strategy.diff()).toHaveLength(0);
});
