import { sessionInstantiateOtherSession, initSessionLifecycle, sessionPullOtherSessionShouldThrowError, sessionCrossSessionShouldThrowError } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initSessionLifecycle(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('sessionInstantiateOtherSession', async () => {
  await sessionInstantiateOtherSession();
});

test('sessionPullOtherSessionShouldThrowError', async () => {
  await sessionPullOtherSessionShouldThrowError();
});

test('sessionCrossSessionShouldThrowError', async () => {
  await sessionCrossSessionShouldThrowError();
});
