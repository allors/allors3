import { C1 } from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
import { Fixture } from '../fixture';
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
        value: 'C1A',
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
