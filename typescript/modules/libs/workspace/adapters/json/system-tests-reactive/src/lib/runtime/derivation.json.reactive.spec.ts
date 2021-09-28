import { initDerivation, sessionFullName } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDerivation(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('sessionFullName', async () => {
  await sessionFullName();
});
