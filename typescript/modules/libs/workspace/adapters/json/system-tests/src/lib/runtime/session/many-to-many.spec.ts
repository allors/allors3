import { Fixture } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('sessionManyToManySetRole', async () => {
  const { m, client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);
  const c1b = session.create<C1>(m.C1);

  c1a.addSessionC1Many2Many(c1b);

  expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
  expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);

  await client.pushAsync(session);

  expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
  expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);

  await client.pullAsync(session, { object: c1a });

  expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
  expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);
});

// test('databaseManyToManySetRoleToNull', async () => {
//   await databaseManyToManySetRoleToNull();
// });

// test('databaseManyToManyRemoveRole', async () => {
//   await databaseManyToManyRemoveRole();
// });

// test('databaseManyToManyRemoveNullRole', async () => {
//   await databaseManyToManyRemoveNullRole();
// });
