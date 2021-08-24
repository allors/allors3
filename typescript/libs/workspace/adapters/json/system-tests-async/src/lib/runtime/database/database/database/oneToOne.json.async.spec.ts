import { databaseOneToOneRemoveRole, databaseOneToOneSetRole, initDatabaseOneToOne } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseOneToOne(
    fixture.asyncDatabaseClient,
    null,
    fixture.databaseConnection.createWorkspace(),
    (login) => fixture.client.login(login),
    () => fixture.databaseConnection.createWorkspace(),
    () => fixture.createDatabaseConnection().createWorkspace()
  );
});

test('databaseOneToOneSetRole', async () => {
  await databaseOneToOneSetRole();
});

test('databaseOneToOneRemoveRole', async () => {
  await databaseOneToOneRemoveRole();
});
