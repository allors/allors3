import { Fixture, name_c1A, name_c1B } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('resetUnitWithoutPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBeNull();
});

test('resetUnitAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBeNull();
});

test('resetUnitAfterDoublePush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  await session.push();
  await session.pull({ object: c1a });

  c1a.C1AllorsString = 'Y';

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBe('X');
});

test('resetOne2OneWithoutPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  expect(c1a.C1C1One2One).toBe(c1b);

  c1a.C1C1One2One = null;

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBe(c1b);
});

test('resetOne2OneAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  const previous = c1a.C1C1One2One;

  expect(previous).toBe(c1b);

  c1a.C1C1One2One = null;

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBe(previous);
});

test('resetOne2OneRemoveAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1One2One = c1b;

  await session.push();
  await session.pull({ object: c1a });

  c1a.C1C1One2One = null;

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBe(c1b);
});

test('resetMany2OneWithoutPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBeNull();
});

test('resetMany2OneAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBeNull();
});

test('resetMany2OneRemoveAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  await session.push();
  await session.pull({ object: c1a });

  c1a.C1C1Many2One = null;

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBe(c1b);
});

test('resetOne2ManyWithoutPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(0);
});

test('resetOne2ManyAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(0);
});

test('resetOne2ManyRemoveAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  await session.push();
  await session.pull({ object: c1a });

  c1a.removeC1C1One2Many(c1b);

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(1);
  expect(c1a.C1C1One2Manies).toContain(c1b);
});

test('resetMany2ManyWithoutPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(0);
});

test('resetMany2ManyAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(0);
});

test('resetMany2ManyRemoveAfterPush', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  await session.push();
  await session.pull({ object: c1a });

  c1a.removeC1C1Many2Many(c1b);

  await session.push();

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(1);
  expect(c1a.C1C1Many2Manies).toContain(c1b);
});

test('sessionResetWithPulls', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1AllorsString = 'X';
  c1a.C1C1One2One = c1b;
  c1a.C1C1Many2One = c1b;
  c1a.addC1C1One2Many(c1b);
  c1a.addC1C1Many2Many(c1b);

  await session.push();
  await session.pull({ object: c1a });

  session.reset();

  await session.pull({ object: c1b });

  session.reset();

  await session.pull({ extent: { kind: 'Filter', objectType: m.C2 } });

  session.reset();
});
