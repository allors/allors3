import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';

import { Fixture } from '../../../fixture'
import '../../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initWorkspaceUnit(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function workspaceUnit() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  expect(true).toBeTruthy();
}
