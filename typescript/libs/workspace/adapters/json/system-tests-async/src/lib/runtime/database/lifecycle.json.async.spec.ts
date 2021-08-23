import { initDatabaseLifecycle,  databasePullSameSessionNotPushedException, databasPullOtherSessionNotPushedException } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseLifecycle(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('pullSameSessionNotPushedException', async () => {
  await databasePullSameSessionNotPushedException();
});

test('databasPullOtherSessionNotPushedException', async () => {
  await databasPullOtherSessionNotPushedException();
});
