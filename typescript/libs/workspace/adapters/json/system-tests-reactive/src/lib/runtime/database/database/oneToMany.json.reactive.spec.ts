import { initDatabaseOneToMany, databaseOneToManySetRole, databaseOneToManySetRoleToNull, databaseOneToManyRemoveRole, databaseOneToManyRemoveNullRole } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseOneToMany(
    null,
    fixture.reactiveDatabaseClient,
    fixture.databaseConnection.createWorkspace(),
    (login) => fixture.client.login(login),
    () => fixture.databaseConnection.createWorkspace(),
    () => fixture.createDatabaseConnection().createWorkspace()
  );
});

test('databaseOneToManySetRole', async () => {
  await databaseOneToManySetRole();
});

test('databaseOneToManySetRoleToNull', async () => {
  await databaseOneToManySetRoleToNull();
});

test('databaseOneToManyRemoveRole', async () => {
  await databaseOneToManyRemoveRole();
});

test('databaseOneToManyRemoveNullRole', async () => {
  await databaseOneToManyRemoveNullRole();
});


