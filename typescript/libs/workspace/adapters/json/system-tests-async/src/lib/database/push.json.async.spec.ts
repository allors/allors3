import {
  initPush,
  pushNewObject,
  pushAndPullNewObject,
  pushNewObjectWithChangedRoles,
  pushExistingObjectWithChangedRoles,
  changesBeforeCheckpointShouldBePushed,
  pushShouldUpdateId,
  pushShouldNotUpdateVersion,
  pushShouldDerive,
  pushTwice,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initPush(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('pushNewObject', async () => {
  await pushNewObject();
});

test('pushAndPullNewObject', async () => {
  await pushAndPullNewObject();
});

test('pushNewObjectWithChangedRoles', async () => {
  await pushNewObjectWithChangedRoles();
});

test('pushExistingObjectWithChangedRoles', async () => {
  await pushExistingObjectWithChangedRoles();
});

test('changesBeforeCheckpointShouldBePushed', async () => {
  await changesBeforeCheckpointShouldBePushed();
});

test('pushShouldUpdateId', async () => {
  await pushShouldUpdateId();
});

test('pushShouldUpdateId', async () => {
  await pushShouldUpdateId();
});

test('pushShouldNotUpdateVersion', async () => {
  await pushShouldNotUpdateVersion();
});

test('pushShouldDerive', async () => {
  await pushShouldDerive();
});

test('pushTwice', async () => {
  await pushTwice();
});
