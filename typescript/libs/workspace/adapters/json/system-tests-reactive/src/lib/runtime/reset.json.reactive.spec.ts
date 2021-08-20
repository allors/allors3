import {
  initReset,
  resetMany2ManyAfterPush,
  resetMany2ManyRemoveAfterPush,
  resetMany2ManyWithoutPush,
  resetMany2OneAfterPush,
  resetMany2OneRemoveAfterPush,
  resetMany2OneWithoutPush,
  resetOne2ManyAfterPush,
  resetOne2ManyRemoveAfterPush,
  resetOne2ManyWithoutPush,
  resetOne2OneAfterPush,
  resetOne2OneRemoveAfterPush,
  resetUnitAfterDoublePush,
  resetUnitAfterPush,
  resetUnitWithoutPush,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initReset(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('resetUnitWithoutPush', async () => {
  await resetUnitWithoutPush();
});

test('resetUnitAfterPush', async () => {
  await resetUnitAfterPush();
});

test('resetUnitAfterDoublePush', async () => {
  await resetUnitAfterDoublePush();
});

test('resetOne2OneAfterPush', async () => {
  await resetOne2OneAfterPush();
});

test('resetOne2OneRemoveAfterPush', async () => {
  await resetOne2OneRemoveAfterPush();
});

test('resetMany2OneWithoutPush', async () => {
  await resetMany2OneWithoutPush();
});

test('resetMany2OneAfterPush', async () => {
  await resetMany2OneAfterPush();
});

test('resetMany2OneRemoveAfterPush', async () => {
  await resetMany2OneRemoveAfterPush();
});

test('resetOne2ManyWithoutPush', async () => {
  await resetOne2ManyWithoutPush();
});

test('resetOne2ManyAfterPush', async () => {
  await resetOne2ManyAfterPush();
});

test('resetOne2ManyRemoveAfterPush', async () => {
  await resetOne2ManyRemoveAfterPush();
});

test('resetMany2ManyWithoutPush', async () => {
  await resetMany2ManyWithoutPush();
});

test('resetMany2ManyAfterPush', async () => {
  await resetMany2ManyAfterPush();
});

test('resetMany2ManyRemoveAfterPush', async () => {
  await resetMany2ManyRemoveAfterPush();
});
