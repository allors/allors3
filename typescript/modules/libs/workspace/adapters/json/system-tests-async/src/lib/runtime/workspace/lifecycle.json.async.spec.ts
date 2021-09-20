import { initWorkspaceLifecycle, workspaceCrossSession, workspaceInstantiateOtherSession, workspacePullOtherSessionShouldThrowError } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initWorkspaceLifecycle(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('workspaceInstantiateOtherSession', async () => {
  await workspaceInstantiateOtherSession();
});

test('workspacePullOtherSessionShouldThrowError', async () => {
  await workspacePullOtherSessionShouldThrowError();
});

test('workspaceCrossSession', async () => {
  await workspaceCrossSession();
});
