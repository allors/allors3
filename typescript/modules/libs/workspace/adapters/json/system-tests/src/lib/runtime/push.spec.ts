import { WorkspaceInitialVersion } from '@allors/workspace/adapters/system';
import { C1, Person } from '@allors/workspace/domain/default';
import { IObject } from '@allors/workspace/domain/system';
import { Fixture, name_c1A } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('pushNewObject', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  for (const roleType of m.C1.roleTypes) {
    expect(newObject.strategy.canRead(roleType)).toBeTruthy();
    expect(newObject.strategy.existRole(roleType)).toBeFalsy();
  }

  for (const associationType of m.C1.associationTypes) {
    if (associationType.isOne) {
      const association = newObject.strategy.getCompositeAssociation<IObject>(associationType);
      expect(association).toBeNull();
    } else {
      const association = newObject.strategy.getCompositesAssociation<IObject>(associationType);
      expect(association.length).toBe(0);
    }
  }
});

test('pushAndPullNewObject', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: newObject }]);

  for (const roleType of m.C1.roleTypes) {
    const x = newObject.strategy.canRead(roleType);
    if (!x) {
      console.debug();
    }

    expect(newObject.strategy.canRead(roleType)).toBeTruthy();
    expect(newObject.strategy.existRole(roleType)).toBeFalsy();
  }

  for (const associationType of m.C1.associationTypes) {
    if (associationType.isOne) {
      const association = newObject.strategy.getCompositeAssociation<IObject>(associationType);
      expect(association).toBeNull();
    } else {
      const association = newObject.strategy.getCompositesAssociation<IObject>(associationType);
      expect(association.length).toBe(0);
    }
  }
});

test('pushNewObjectWithChangedRoles', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);
  newObject.C1AllorsString = 'A new object';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: newObject }]);

  expect(newObject.C1AllorsString).toBe('A new object');
});

test('pushExistingObjectWithChangedRoles', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: c1a }]);

  expect(c1a.C1AllorsString).toBe('X');
});

test('changesBeforeCheckpointShouldBePushed', async () => {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const c1a_1 = await fixture.pullC1(session1, name_c1A);

  c1a_1.C1AllorsString = 'X';

  const changeSet = session1.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);

  const pushResult = await client.pushAsync(session1);

  expect(pushResult.hasErrors).toBeFalsy();

  const result = await client.pullAsync(session2, [{ object: c1a_1 }]);
  const c1a_2 = result.object<C1>(m.C1);

  expect(c1a_2.C1AllorsString).toBe('X');
});

test('pushShouldUpdateId', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  expect(person.id < 0);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  expect(person.id > 0);
});

test('pushShouldNotUpdateVersion', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  expect(person.strategy.version).toBe(WorkspaceInitialVersion);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  expect(person.strategy.version).toBe(WorkspaceInitialVersion);
});

test('pushShouldDerive', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: person }]);

  expect(person.DomainFullName).toBe('Johny Doey');
});

test('pushTwice', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1x = session.create<C1>(m.C1);
  const c1y = session.create<C1>(m.C1);
  c1x.C1C1Many2One = c1y;

  let pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();
});
