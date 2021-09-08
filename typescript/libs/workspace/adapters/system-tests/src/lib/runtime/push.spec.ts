import { IAsyncDatabaseClient, IObject, IReactiveDatabaseClient, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { Fixture, name_c1A, name_c1B, name_c1C, name_c1D, name_c2A, name_c2B, name_c2C, name_c2D } from '../Fixture';
import '../Matchers';
import '@allors/workspace/domain/default';
import { C1, C2, Person } from '@allors/workspace/domain/default';
import { Origin } from '@allors/workspace/meta/system';
import { WorkspaceInitialVersion } from '@allors/workspace/adapters/system';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initPush(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function pushNewObject() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  for (const roleType of m.C1.roleTypes) {
    expect(newObject.strategy.CanRead(roleType)).toBeTruthy();
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
}

export async function pushAndPullNewObject() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: newObject }]);

  for (const roleType of m.C1.roleTypes) {
    const x = newObject.strategy.CanRead(roleType);
    if (!x) {
      console.debug();
    }

    expect(newObject.strategy.CanRead(roleType)).toBeTruthy();
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
}

export async function pushNewObjectWithChangedRoles() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const newObject = session.create<C1>(m.C1);
  newObject.C1AllorsString = 'A new object';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: newObject }]);

  expect(newObject.C1AllorsString).toBe('A new object');
}

export async function pushExistingObjectWithChangedRoles() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: c1a }]);

  expect(c1a.C1AllorsString).toBe('X');
}

export async function changesBeforeCheckpointShouldBePushed() {
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
}

export async function pushShouldUpdateId() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  expect(person.id < 0);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  expect(person.id > 0);
}

export async function pushShouldNotUpdateVersion() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  expect(person.strategy.version).toBe(WorkspaceInitialVersion);

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  expect(person.strategy.version).toBe(WorkspaceInitialVersion);
}

export async function pushShouldDerive() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const person = session.create<Person>(m.Person);
  person.FirstName = 'Johny';
  person.LastName = 'Doey';

  const pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  await client.pullAsync(session, [{ object: person }]);

  expect(person.DomainFullName).toBe('Johny Doey');
}

export async function pushTwice() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1x = session.create<C1>(m.C1);
  const c1y = session.create<C1>(m.C1);
  c1x.C1C1Many2One = c1y;

  let pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();

  pushResult = await client.pushAsync(session);

  expect(pushResult.hasErrors).toBeFalsy();
}
