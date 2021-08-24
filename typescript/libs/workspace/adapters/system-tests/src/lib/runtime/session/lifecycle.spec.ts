import { Pull, IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/core';

import { Fixture } from '../../Fixture';
import '../../Matchers';
import { C1, SC1, SessionOrganisation } from '@allors/workspace/domain/core';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initSessionLifecycle(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function sessionInstantiateOtherSession() {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);

  const objectSession2 = session2.instantiate(objectSession1);

  expect(objectSession2).toBeNull();
}

export async function sessionPullOtherSessionShouldThrowError() {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);

  let hasErrors = false;
  try {
    await client.pullAsync(session2, { object: objectSession1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
}

export async function sessionCrossSessionShouldThrowError() {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);
  const objectSession2 = session2.create<SC1>(m.SC1);

  let hasErrors = false;
  try {
    objectSession1.addSC1SC1Many2Many(objectSession2);
  } catch(error) {
    hasErrors = true;
    expect(error.message).toBe('Strategy is from a different session')
  }

  expect(hasErrors).toBeTruthy();
}
