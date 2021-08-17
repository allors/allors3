import {
  initProcedure,
  nonExistingProcedure,
  testUnitSamplesWithNulls,
  testUnitSamplesWithValues,
} from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initProcedure(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('testUnitSamplesWithNulls', async () => {
  await testUnitSamplesWithNulls();
});

test('testUnitSamplesWithValues', async () => {
  await testUnitSamplesWithValues();
});

test('nonExistingProcedure', async () => {
  await nonExistingProcedure();
});

