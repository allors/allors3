import { C1 } from '@allors/default/workspace/domain';
import { IUnitDiff, Pull } from '@allors/system/workspace/domain';
import { Fixture } from '../fixture';
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
        value: 'C1A',
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
        value: 'C1A',
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
        value: 'C1A',
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
        value: 'C1A',
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
