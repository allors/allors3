import { Pull, IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';

import { Fixture } from '../../Fixture';
import '../../Matchers';
import { C1 } from '@allors/workspace/domain/default';

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

  // // New Object
  // {
  //   const c1 = session.create<C1>(m.C1);

  //   expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeFalsy();

  //   c1.C1AllorsString = 'I am changed!';

  //   expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeTruthy();
  // }

  // // New Object with push
  // {
  //   const c1 = session.create<C1>(m.C1);

  //   c1.C1AllorsString = 'I am changed!';

  //   await client.pushAsync(session);

  //   expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeTruthy();
  // }

  // New Object with push and pull
  {
    const c1 = session.create<C1>(m.C1);

    c1.C1AllorsString = 'I am changed!';

    const x = await client.pushAsync(session);
    await client.pullAsync(session, { object: c1 });

    expect(c1.strategy.hasChangedRole(m.C1.C1AllorsString)).toBeFalsy();
  }
}
