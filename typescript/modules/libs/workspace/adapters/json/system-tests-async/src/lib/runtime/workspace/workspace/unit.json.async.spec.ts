import { initWorkspaceUnit, workspaceUnit } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initWorkspaceUnit(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('workspaceUnit', async () => {
  await workspaceUnit();
});
