import { initDatabaseLifecycle, databasPullOtherSessionNotPushedException, databasePullSameSessionNotPushedException, databaseStrategyHasChanges } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../fixture'

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseLifecycle(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('databasePullSameSessionNotPushedException', async () => {
  await databasePullSameSessionNotPushedException();
});

test('databasPullOtherSessionNotPushedException', async () => {
  await databasPullOtherSessionNotPushedException();
});

test('databaseStrategyHasChanges', async () => {
  await databaseStrategyHasChanges();
});
