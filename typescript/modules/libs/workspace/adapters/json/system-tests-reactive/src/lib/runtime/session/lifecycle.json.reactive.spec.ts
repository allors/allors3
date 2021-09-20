import { sessionCrossSessionShouldThrowError, initSessionLifecycle, sessionInstantiateOtherSession, sessionPullOtherSessionShouldThrowError } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initSessionLifecycle(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
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

