import { C1 } from '@allors/default/workspace/domain';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('isDeleted', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a_1 = session.create<C1>(m.C1);

  c1a_1.strategy.delete();

  expect(c1a_1.strategy.isDeleted).toBe(true);
});

test('instantiate', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c1a_1 = session1.create<C1>(m.C1);

  c1a_1.strategy.delete();

  const c1a_2 = session1.instantiate(c1a_1.id);

  expect(c1a_2).toBeNull();

  const c1a_3 = session2.instantiate(c1a_1.id);

  expect(c1a_3).toBeNull();
});

test('roles', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1b;
  c1a.C1C1Many2One = c1b;
  c1a.addC1C1One2Many(c1b);
  c1a.addC1C1Many2Many(c1b);

  c1a.strategy.delete();

  expect(c1b.C1WhereC1C1One2One).toBeNull();
  expect(c1b.C1sWhereC1C1Many2One.length).toBe(0);
  expect(c1b.C1WhereC1C1One2Many).toBeNull();
  expect(c1b.C1sWhereC1C1Many2Many.length).toBe(0);
});

test('associations', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);
  const c1b = session.create<C1>(m.C1);

  c1b.C1C1One2One = c1a;
  c1b.C1C1Many2One = c1a;
  c1b.addC1C1One2Many(c1a);
  c1b.addC1C1Many2Many(c1a);

  c1a.strategy.delete();

  expect(c1b.C1C1One2One).toBeNull();
  expect(c1b.C1C1Many2One).toBeNull();
  expect(c1b.C1C1One2Manies.length).toBe(0);
  expect(c1b.C1C1Many2Manies.length).toBe(0);
});
