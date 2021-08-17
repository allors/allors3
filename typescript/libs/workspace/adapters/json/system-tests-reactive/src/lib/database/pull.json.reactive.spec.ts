import { initPull, andGreaterThanLessThan, associationMany2ManyContainedIn, associationMany2ManyContains } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initPull(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('andGreaterThanLessThan', async () => {
  await andGreaterThanLessThan();
});

test('associationMany2ManyContainedIn', async () => {
  await associationMany2ManyContainedIn();
});

test('associationMany2ManyContains', async () => {
  await associationMany2ManyContains();
});

