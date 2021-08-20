import { initDatabaseUnit, databaseUnit } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseUnit(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('unit', async () => {
  await databaseUnit();
});
