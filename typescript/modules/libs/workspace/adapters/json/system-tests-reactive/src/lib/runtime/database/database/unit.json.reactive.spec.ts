import {  initDatabaseUnit, databaseUnit } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseUnit(null, fixture.reactiveDatabaseClient, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('databaseUnit', async () => {
  await databaseUnit();
});