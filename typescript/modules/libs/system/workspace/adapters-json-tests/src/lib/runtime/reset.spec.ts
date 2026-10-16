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

  expect(c1a.C1AllorsString).toBe('X');
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

  expect(c1a.C1AllorsString).toBe('Y');
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

  expect(c1a.C1C1One2One).toBeNull();
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

  expect(c1a.C1C1One2One).toBeNull();
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

  expect(c1a.C1C1Many2One).toBe(c1b);
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

  expect(c1a.C1C1Many2One).toBeNull();
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

  expect(c1a.C1C1One2Manies).toEqual([c1b]);
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

  expect(c1a.C1C1Many2Manies).toEqual([c1b]);
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

  expect(c1a.C1C1Many2Manies).toEqual([]);
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

afterEach(async () => {
  await fixture?.close();
});

test.each([
  'unit',
  'one-to-one',
  'many-to-one',
  'one-to-many',
  'many-to-many',
] as const)(
  'Reset after skipped Push retains the accepted %s baseline',
  async (kind) => {
    const session = fixture.workspace.createSession();
    const a = await fixture.pullC1(session, name_c1A);
    const b = await fixture.pullC1(session, name_c1B);
    const before =
      kind === 'unit'
        ? a.C1AllorsString
        : kind === 'one-to-one'
        ? a.C1C1One2One
        : kind === 'many-to-one'
        ? a.C1C1Many2One
        : kind === 'one-to-many'
        ? [...a.C1C1One2Manies]
        : [...a.C1C1Many2Manies];
    if (kind === 'unit') a.C1AllorsString = 'saved';
    else if (kind === 'one-to-one') a.C1C1One2One = null;
    else if (kind === 'many-to-one') a.C1C1Many2One = b;
    else if (kind === 'one-to-many') a.addC1C1One2Many(b);
    else a.addC1C1Many2Many(b);
    const result = await session.push({ refresh: false });
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('not-requested');
    a.strategy.reset();
    const value = () =>
      kind === 'unit'
        ? a.C1AllorsString
        : kind === 'one-to-one'
        ? a.C1C1One2One
        : kind === 'many-to-one'
        ? a.C1C1Many2One
        : kind === 'one-to-many'
        ? a.C1C1One2Manies
        : a.C1C1Many2Manies;
    expect(value()).toEqual(before);
    await session.pull({ object: a });
    expect(value()).toEqual(
      kind === 'unit'
        ? 'saved'
        : kind === 'one-to-one'
        ? null
        : kind === 'many-to-one'
        ? b
        : [b]
    );
  }
);
