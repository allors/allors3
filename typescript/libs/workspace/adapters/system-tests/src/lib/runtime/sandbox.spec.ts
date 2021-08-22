import { C1 } from '@allors/workspace/domain/core';
import { IAsyncDatabaseClient, IReactiveDatabaseClient, IWorkspace } from '@allors/workspace/domain/system';

import { Fixture } from '../Fixture';
import '../Matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initSandbox(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function sandbox() {
  const { workspace, m, asyncClient } = fixture;
  const session = workspace.createSession();
  
  const c1x = session.create<C1>(m.C1);
  const c1y_2 = session.create<C1>(m.C1);

  await asyncClient.pushAsync(session);
  const result = await asyncClient.pullAsync(session, { object: c1y_2 });
  const c1y_1 = result.objects.values().next().value;

  if (!c1x.canWriteC1C1Many2Manies) {
    await asyncClient.pullAsync(session, { object: c1x });
  }

  c1x.addC1C1Many2Many(c1y_1);

  expect(c1x.C1C1Many2Manies.length).toBe(1);
  expect(c1x.C1C1Many2Manies).toContain(c1y_1);

  expect(c1y_1.C1sWhereC1C1Many2Many.length).toBe(1);
  expect(c1y_1.C1sWhereC1C1Many2Many).toContain(c1x);
}
