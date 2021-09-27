import {
  initChangeSet,
  changeSetConstruction,
  changeSetInstantiated,
  changeSetAfterPush,
  changeSetPushChangeNoPush,
  changeSetPushChangePush,
  changeSetAfterPushWithNoChanges,
  changeSetAfterPushWithPull,
  changeSetAfterPushWithPullWithNoChanges,
  changeSetAfterPushOne2One,
  changeSetAfterPushOne2OneRemove,
  changeSetAfterPushMany2One,
  changeSetAfterPushMany2OneRemove,
  changeSetAfterPushOne2Many,
  changeSetAfterPushOne2ManyRemove,
  changeSetAfterPushMany2Many,
  changeSetAfterPushMany2ManyRemove,
  changeSetAfterPullInNewSessionButNoPush,
  changeSetBeforeAndAfterResetWithSessionObject,
  changeSetBeforeAndAfterResetWithChangeSessionObject,
  changeSetAfterDoubleReset,
  changeSetAfterPushOne2OneWithPreviousIncluded,
  changeSetIncludeAfterPushOne2One,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../fixture';

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

test('changeSetPushChangePush', async () => {
  await changeSetPushChangePush();
});

test('changeSetAfterPushWithNoChanges', async () => {
  await changeSetAfterPushWithNoChanges();
});

test('changeSetAfterPushWithPull', async () => {
  await changeSetAfterPushWithPull();
});

test('changeSetAfterPushWithPullWithNoChanges', async () => {
  await changeSetAfterPushWithPullWithNoChanges();
});

test('changeSetAfterPushOne2One', async () => {
  await changeSetAfterPushOne2One();
});

test('changeSetIncludeAfterPushOne2One', async () => {
  await changeSetIncludeAfterPushOne2One();
});

test('changeSetAfterPushOne2OneWithPreviousIncluded', async () => {
  await changeSetAfterPushOne2OneWithPreviousIncluded();
});

test('changeSetAfterPushOne2OneRemove', async () => {
  await changeSetAfterPushOne2OneRemove();
});

test('changeSetAfterPushMany2One', async () => {
  await changeSetAfterPushMany2One();
});

test('changeSetAfterPushMany2OneRemove', async () => {
  await changeSetAfterPushMany2OneRemove();
});

test('changeSetAfterPushOne2Many', async () => {
  await changeSetAfterPushOne2Many();
});

test('changeSetAfterPushOne2ManyRemove', async () => {
  await changeSetAfterPushOne2ManyRemove();
});

test('changeSetAfterPushMany2Many', async () => {
  await changeSetAfterPushMany2Many();
});

test('changeSetAfterPushMany2ManyRemove', async () => {
  await changeSetAfterPushMany2ManyRemove();
});

test('changeSetAfterPullInNewSessionButNoPush', async () => {
  await changeSetAfterPullInNewSessionButNoPush();
});

test('changeSetBeforeAndAfterResetWithSessionObject', async () => {
  await changeSetBeforeAndAfterResetWithSessionObject();
});

test('changeSetBeforeAndAfterResetWithChangeSessionObject', async () => {
  await changeSetBeforeAndAfterResetWithChangeSessionObject();
});

test('changeSetAfterDoubleReset', async () => {
  await changeSetAfterDoubleReset();
});
