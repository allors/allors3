import { Fixture, name_c1C } from '../../../fixture';
import { Pull } from '@allors/system/workspace/domain';
import '../../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('unit', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.Name,
        value: 'c2C',
      },
    },
    results: [
      {
        include: [
          {
            propertyType: m.C2.C1WhereC1C2One2Many,
          },
        ],
      },
    ],
  };

  const result = await session.pull([pull]);

  const c2s = result.collection('C2s');

  const c2C = c2s[0];

  const c1WhereC1C2One2Many = c2C.C1WhereC1C2One2Many;

  expect(c1WhereC1C2One2Many).toBeDefined();
  expect(c1WhereC1C2One2Many.Name).toBe(name_c1C);
});
