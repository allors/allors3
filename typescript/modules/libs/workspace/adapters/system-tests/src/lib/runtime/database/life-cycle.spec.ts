import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1, C2 } from '@allors/workspace/domain/default';

import { Fixture, name_c1A, name_c2A } from '../../fixture'
import '../../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDatabaseLifecycle(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function databasePullSameSessionNotPushedException() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1 = session.create<C1>(m.C1);

  let hasErrors = false;
  try {
    await client.pullAsync(session, { object: c1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
}

export async function databasPullOtherSessionNotPushedException() {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c1 = session1.create<C1>(m.C1);

  let hasErrors = false;
  try {
    await client.pullAsync(session2, { object: c1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
}

export async function databaseStrategyHasChanges() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  // New Object with Unit Role
  {
    const c1 = session.create<C1>(m.C1);

    expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeFalsy();

    c1.C1AllorsString = 'I am changed!';

    expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeTruthy();
  }

  // New Object with Unit Role with push
  {
    const c1 = session.create<C1>(m.C1);

    c1.C1AllorsString = 'I am changed!';

    await client.pushAsync(session);

    expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeTruthy();
  }

  // New Object with Unit Role with push and pull
  {
    const c1 = session.create<C1>(m.C1);

    c1.C1AllorsString = 'I am changed!';

    await client.pushAsync(session);
    await client.pullAsync(session, { object: c1 });

    expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeFalsy();
  }

  // New Object with with Composite Role to New Object
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }
  // New Object with Composite Role to New Object with push
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }

  // New Object with Composite Role to New Object with push and pull
  {
    const c1 = session.create<C1>(m.C1);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);
    await client.pullAsync(session, { object: c1 });

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();
  }

   // Existing Object with with Composite Role to New Object
   {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }
  // Existing Object with Composite Role to New Object with push
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }

  // Existing Object with Composite Role to New Object with push and pull
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = session.create<C2>(m.C2);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);
    await client.pullAsync(session, { object: c1 });

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();
  }

  // Existing Object with with Composite Role to Existing Object
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();

    c1.C1C2One2One = c2;

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }
  // Existing Object with Composite Role to Existing Object with push
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeTruthy();
  }

  // Existing Object with Composite Role to Existing Object with push and pull
  {
    const c1 = await fixture.pullC1(session, name_c1A);
    const c2 = await fixture.pullC2(session, name_c2A);

    c1.C1C2One2One = c2;

    await client.pushAsync(session);
    await client.pullAsync(session, { object: c1 });

    expect(c1.strategy.hasChangedRole(m.C1.C1C2One2One)).toBeFalsy();
  }
}
