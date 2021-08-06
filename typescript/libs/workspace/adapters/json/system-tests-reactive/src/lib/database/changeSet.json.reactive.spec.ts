import { initChangeSet, creatingChangeSetAfterCreatingSession } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initChangeSet(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('creatingChangeSetAfterCreatingSession', async () => {
  await creatingChangeSetAfterCreatingSession();
});

