import { C1, C2 } from '@allors/default/workspace/domain';
import exp = require('constants');
import { Fixture, name_c1A, name_c1C, name_c2A, name_c2C } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('pullSameSessionNotPushedException', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1 = session.create<C1>(m.C1);

  let hasErrors = false;
  try {
    await session.pull({ object: c1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('databasPullOtherSessionNotPushedException', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c1 = session1.create<C1>(m.C1);

  let hasErrors = false;
  try {
    await session2.pull({ object: c1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('databasPullOtherSession', async () => {
  const { workspace } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c1a_1 = await fixture.pullC1(session1, name_c1A);

  expect(() => session2.instantiate(c1a_1)).toThrow(
    `Object ${c1a_1.id} is not in this session. Pull it before instantiating it.`
  );
});

test('databaseRoleToObjectPulledByOtherSession', async () => {
  const { workspace } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c2c_1 = await fixture.pullC2(session1, name_c2C);
  const c1c_2 = await fixture.pullC1(session2, name_c1C);

  expect(() => session2.instantiate(c2c_1.id)).toThrow(
    `Object ${c2c_1.id} is not in this session. Pull it before instantiating it.`
  );
  expect(() => c1c_2.C1C2One2One).toThrow(
    `Object ${c2c_1.id} in role C1.C1C2One2One is not in this session. Include the role in the Pull, or pull the object, before reading it.`
  );
});

test('databasPullOtherSessionNonExistingId', async () => {
  const { workspace } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  await fixture.pullC1(session1, name_c1A);

  expect(() => session2.instantiate(Number.MAX_SAFE_INTEGER)).toThrow(
    `Object ${Number.MAX_SAFE_INTEGER} is not in this session. Pull it before instantiating it.`
  );
});

test('databaseStrategyHasChanges', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  // New Object with Unit Role
  {
    const c1 = session.create<C1>(m.C1);

    expect(c1.strategy.hasChanged(m.C1.C1AllorsString)).toBeFalsy();

    c1.C1AllorsString = 'I am changed!';

    expect(c1.strategy.hasChanged(m.C1.C1AllorsString)).toBeTruthy();
  }

  // New Object with Unit Role with push
  {
    const c1 = session.create<C1>(m.C1);

    c1.C1AllorsString = 'I am changed!';

    await session.push();

    expect(c1.strategy.hasChanged(m.C1.C1AllorsString)).toBeTruthy();
  }

  // New Object with Unit Role with push and pull
  {
    const c1 = session.create<C1>(m.C1);

    c1.C1AllorsString = 'I am changed!';

    await session.push();
    await session.pull({ object: c1 });

    expect(c1.strategy.hasChanged(m.C1.C1AllorsString)).toBeFalsy();
  }

  // New Object with with Composite Role to New Object
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }
  // New Object with Composite Role to New Object with push
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await session.push();

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }

  // New Object with Composite Role to New Object with push and pull
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await session.push();
    await session.pull({ object: c1 });

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();
  }

  // Existing Object with with Composite Role to New Object
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }
  // Existing Object with Composite Role to New Object with push
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await session.push();

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }

  // Existing Object with Composite Role to New Object with push and pull
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await session.push();
    await session.pull({ object: c1 });

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();
  }

  // Existing Object with with Composite Role to Existing Object
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }
  // Existing Object with Composite Role to Existing Object with push
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    c1.C1C2One2One = c2;

    await session.push();

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeTruthy();
  }

  // Existing Object with Composite Role to Existing Object with push and pull
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    c1.C1C2One2One = c2;

    await session.push();
    await session.pull({ object: c1 });

    expect(c1.strategy.hasChanged(m.C1.C1C2One2One)).toBeFalsy();
  }
});
