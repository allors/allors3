import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull } from '@allors/workspace/domain/system';
import { Fixture } from '../Fixture';
import '../Matchers';
import '@allors/workspace/domain/core';
import { C1 } from '@allors/workspace/domain/core';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initChangeSet(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function changeSetConstruction() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const changeSet = session.checkpoint();

  expect(changeSet.instantiated.size).toBe(0);
}

export async function changeSetInstantiated() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.Name,
        value: 'c1A',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);
  const c1s = result.collection<C1>('C1s');
  const c1a = c1s[0];

  const changeSet = session.checkpoint();

  expect(changeSet.instantiated.size).toBe(1);
  const instantiated = changeSet.instantiated.values().next().value;
  expect(instantiated).toBe(c1a.strategy);
}

export async function changeSetAfterPush() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.Name,
        value: 'c1A',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);
  const c1s = result.collection<C1>('C1s');
  const c1a = c1s[0];

  c1a.C1AllorsString = 'X';

  await client.pushAsync(session);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  const changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
}

export async function changeSetPushChangeNoPush() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.Name,
        value: 'c1A',
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  let changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();

  result = await client.pullAsync(session, [pull]);

  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
}
