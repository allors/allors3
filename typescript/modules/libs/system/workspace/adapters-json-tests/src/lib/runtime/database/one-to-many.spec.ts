import { C1 } from '@allors/default/workspace/domain';
import { Fixture } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('databaseOneToManyAddMovesRoleFromPreviousAssociation', async () => {
  const { m, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);
  const c1b = session.create<C1>(m.C1);
  const c1c = session.create<C1>(m.C1);
  const c1d = session.create<C1>(m.C1);

  c1a.addC1C1One2Many(c1b);
  c1a.addC1C1One2Many(c1c);

  await session.push();
  await session.pull([
    { object: c1a },
    { object: c1b },
    { object: c1c },
    { object: c1d },
  ]);

  c1d.addC1C1One2Many(c1b);

  expect(c1a.C1C1One2Manies).toEqual([c1c]);
  expect(c1d.C1C1One2Manies).toEqual([c1b]);
  expect(c1b.C1WhereC1C1One2Many).toBe(c1d);

  await session.push();
  await session.pull([{ object: c1a }, { object: c1d }]);

  expect(c1a.C1C1One2Manies).toEqual([c1c]);
  expect(c1d.C1C1One2Manies).toEqual([c1b]);
  expect(c1b.C1WhereC1C1One2Many).toBe(c1d);
});
