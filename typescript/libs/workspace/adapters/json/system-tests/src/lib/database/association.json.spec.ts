import { initAssociation, databaseGetOne2Many } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initAssociation(fixture.database, (login) => fixture.client.login(login));
});

test('databaseGetOne2Many', async () => {
  await databaseGetOne2Many();
});
