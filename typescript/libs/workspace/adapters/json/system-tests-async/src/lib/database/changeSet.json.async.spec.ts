import { initChangeSet, changeSetConstruction, changeSetInstantiated, changeSetAfterPush, changeSetPushChangeNoPush } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initChangeSet(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('changeSetConstruction', async () => {
  await changeSetConstruction();
});

test('changeSetInstantiated', async () => {
  await changeSetInstantiated();
});

test('changeSetAfterPush', async () => {
  await changeSetAfterPush();
});

test('changeSetPushChangeNoPush', async () => {
  await changeSetPushChangeNoPush();
});
