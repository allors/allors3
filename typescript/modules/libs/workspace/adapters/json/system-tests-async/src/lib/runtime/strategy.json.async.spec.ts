import { initStrategy, addCompositesWrongObjectType, setUnitWrongObjectType, setCompositeRoleWrongObjectType, addCompositesRoleWrongRoleType, setCompositeRoleWrongRoleType } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await initStrategy(fixture.asyncDatabaseClient, null, fixture.databaseConnection.createWorkspace(), (login) => fixture.client.login(login));
});

test('setUnitWrongObjectType', async () => {
  await setUnitWrongObjectType();
});

test('setCompositeRoleWrongObjectType', async () => {
  await setCompositeRoleWrongObjectType();
});

test('setCompositeRoleWrongRoleType', async () => {
  await setCompositeRoleWrongRoleType();
});

test('addCompositesWrongObjectType', async () => {
  await addCompositesWrongObjectType();
});

test('addCompositesRoleWrongRoleType', async () => {
  await addCompositesRoleWrongRoleType();
});
