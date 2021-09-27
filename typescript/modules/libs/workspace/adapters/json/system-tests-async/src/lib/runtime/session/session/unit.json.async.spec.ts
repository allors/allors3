import { initSessionUnit, sessionUnit } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../fixture'

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initSessionUnit(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('sessionUnit', async () => {
  await sessionUnit();
});
