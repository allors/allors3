import { initAssociation, databaseGetOne2Many, databaseGetOne2One } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initAssociation(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('databaseGetOne2Many', async () => {
  await databaseGetOne2Many();
});

test('databaseGetOne2One', async () => {
  await databaseGetOne2One();
});
