import {
  callMultiple,
  callMultipleIsolated,
  callSingle,
  initMethod,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initMethod(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('callSingle', async () => {
  await callSingle();
});

test('callMultiple', async () => {
  await callMultiple();
});

test('callMultipleIsolated', async () => {
  await callMultipleIsolated();
});
