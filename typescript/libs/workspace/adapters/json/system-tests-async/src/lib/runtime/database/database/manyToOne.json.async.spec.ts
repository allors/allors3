import { databaseManyToOneRemoveRole, databaseManyToOneSetRole, initDatabaseManyToOne } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../../../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initDatabaseManyToOne(
    fixture.asyncDatabaseClient,
    null,
    fixture.databaseConnection.createWorkspace(),
    (login) => fixture.client.login(login),
    () => fixture.databaseConnection.createWorkspace(),
    () => fixture.createDatabaseConnection().createWorkspace()
  );
});

test('databaseManyToOneSetRole', async () => {
  await databaseManyToOneSetRole();
});

test('databaseManyToOneRemoveRole', async () => {
  await databaseManyToOneRemoveRole();
});
