import { C1 } from '@allors/default/workspace/domain';
import {
  ICompositeDiff,
  ICompositesDiff,
  IUnitDiff,
  Pull,
  ResultError,
} from '@allors/system/workspace/domain';
import { Fixture, name_c1A, name_c1B, name_c1C, name_c1D } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('databaseUnitDiff', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await session.push();

  result = await session.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(1);

  const diff = diffs[0] as IUnitDiff;

  expect(diff.originalRole).toBe('X');
  expect(diff.changedRole).toBe('Y');
  expect(diff.relationType.roleType).toBe(m.C1.C1AllorsString);
});

test('databaseUnitDiffAfterReset', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await session.push();

  result = await session.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  c1a_2.strategy.reset();

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(0);
});

test('databaseUnitDiffAfterDoubleReset', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await session.push();

  result = await session.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  c1a_2.strategy.reset();
  c1a_2.strategy.reset();

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(0);
});

test('databaseMultipleUnitDiff', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';
  c1a_1.C1AllorsInteger = 1;

  await session.push();

  result = await session.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';
  c1a_2.C1AllorsInteger = 2;

  const diffs = c1a_2.strategy.diff() as IUnitDiff[];

  expect(diffs.length).toBe(2);

  const stringDiff = diffs.find(
    (v) => v.relationType.roleType === m.C1.C1AllorsString
  );

  expect(stringDiff.originalRole).toBe('X');
  expect(stringDiff.changedRole).toBe('Y');

  const intDiff = diffs.find(
    (v) => v.relationType.roleType === m.C1.C1AllorsInteger
  );

  expect(intDiff.originalRole).toBe(1);
  expect(intDiff.changedRole).toBe(2);
});

test('databaseDiffRetainsBaselineWhenAnotherSessionRefreshes', async () => {
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

  source.C1AllorsString = 'remote';
  source.C1AllorsInteger = 2;
  source.C1C1Many2One = remoteTarget;
  source.addC1C1Many2Many(remoteTarget);
  expect((await remote.push()).hasErrors).toBe(false);
  expect((await remote.pull(pull)).hasErrors).toBe(false);
  expect(source.strategy.version).not.toBe(version);

  // The shared connection has the new record, but editing has not pulled it.
  expect(edited.strategy.version).toBe(version);
  expect(edited.C1AllorsInteger).toBe(1);
  expect(edited.C1AllorsString).toBe('local');
  expect(edited.C1C1Many2One).toBe(localTarget);
  expect(edited.C1C1Many2Manies.map((v) => v.id).sort()).toEqual(
    [baselineTarget.id, localTarget.id].sort()
  );
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

  // TypeScript rejects the promise; .NET returns the failed push result.
  const pushed = await editing.push().catch((error: unknown) => error);
  expect(pushed).toBeInstanceOf(ResultError);
  const rejected = (pushed as ResultError).result;
  expect(rejected.hasErrors).toBe(true);
  expect(rejected.versionErrors).toEqual([edited]);
  expect(edited.strategy.version).toBe(version);
  expect(edited.C1AllorsString).toBe('local');
  expect(edited.strategy.diff()).toHaveLength(3);

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
  expect(edited.C1AllorsString).toBe('remote');
  expect(edited.C1AllorsInteger).toBe(2);
  expect(edited.C1C1Many2One.id).toBe(remoteTarget.id);
  expect(edited.C1C1Many2Manies.map((v) => v.id).sort()).toEqual(
    source.C1C1Many2Manies.map((v) => v.id).sort()
  );
  expect(edited.strategy.diff()).toHaveLength(0);
});
