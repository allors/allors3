import { databaseMergeError, initMerge } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initMerge(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('databaseMergeError', async () => {
  await databaseMergeError();
});
