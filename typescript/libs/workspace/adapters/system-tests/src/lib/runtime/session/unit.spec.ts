import { Pull, IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/core';

import { Fixture, name_c1C } from '../../Fixture';
import '../../Matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initSessionUnit(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function sessionUnit() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  expect(true).toBeTruthy();
}
