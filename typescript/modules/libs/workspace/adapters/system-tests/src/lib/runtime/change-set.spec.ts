import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1, SC1 } from '@allors/workspace/domain/default';

import { Fixture } from '../fixture';
import '../matchers';

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

export async function changeSetPushChangePush() {
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

  await client.pushAsync(session);

  changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
}

export async function changeSetAfterPushWithNoChanges() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  const created = changeSet.created.values().next().value;
  expect(created).toBeDefined();
  expect(created).toBe(c1a.strategy);

  await client.pushAsync(session);

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
}

export async function changeSetAfterPushWithPull() {
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
  const c1a = result.collection<C1>('C1s')[0];

  c1a.C1AllorsString = 'X';

  await client.pushAsync(session);

  await client.pullAsync(session, [pull]);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  const changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
}

export async function changeSetAfterPushWithPullWithNoChanges() {
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
  const c1a = result.collection<C1>('C1s')[0];

  await client.pushAsync(session);
  await client.pullAsync(session, [pull]);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
}

export async function changeSetAfterPushOne2One() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1x = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1x;

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).size).toBe(1);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1x.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values().next().value).toBe(c1x.strategy);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetIncludeAfterPushOne2One() {
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
    results: [{ include: [{ propertyType: m.C1.C1C1One2One }] }],
  };

  const result = await client.pullAsync(session, [pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = c1a.C1C1One2One;
  const c1x = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1x;

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  const created = [...changeSet.created];
  const associations = [...changeSet.associationsByRoleType.get(m.C1.C1C1One2One)];
  const roles = [...changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType)];

  expect(associations.length).toBe(1);
  expect(roles.length).toBe(2);

  expect(created).toContain(c1x.strategy);
  expect(associations).toContain(c1a.strategy);
  expect(roles).toContain(c1b.strategy);
  expect(roles).toContain(c1x.strategy);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetAfterPushOne2OneWithPreviousIncluded() {
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
    results: [
      {
        include: [
          {
            propertyType: m.C1.C1C1One2One,
          },
        ],
      },
    ],
  };

  const result = await client.pullAsync(session, [pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);
  const previous = c1a.C1C1One2One;

  c1a.C1C1One2One = c1b;

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  const created = [...changeSet.created.values()];
  const associationsC1C1One2One = [...changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values()];
  const rolesC1C1One2One = [...changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values()];

  expect(created).toEqual([c1b.strategy]);
  expect(associationsC1C1One2One).toEqual([c1a.strategy]);
  expect(rolesC1C1One2One).toEqual([previous.strategy, c1b.strategy]);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetAfterPushOne2OneRemove() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1b;

  await client.pushAsync(session);
  await client.pullAsync(session, [pull]);
  session.checkpoint();

  c1a.C1C1One2One = null;

  await client.pushAsync(session);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values().next().value).toBe(c1b.strategy);
}

export async function changeSetAfterPushMany2One() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1Many2One = c1b;

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2One.associationType).values().next().value).toBe(c1b.strategy);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetAfterPushMany2OneRemove() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1Many2One = c1b;

  await client.pushAsync(session);
  await client.pullAsync(session, [pull]);
  session.checkpoint();

  c1a.C1C1Many2One = null;

  await client.pushAsync(session);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2One.associationType).values().next().value).toBe(c1b.strategy);
}

export async function changeSetAfterPushOne2Many() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1One2Many(c1b);

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2Manies.associationType).values().next().value).toBe(c1b.strategy);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetAfterPushOne2ManyRemove() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1One2Many(c1b);

  await client.pushAsync(session);
  await client.pullAsync(session, [pull]);
  session.checkpoint();

  c1a.C1C1One2Manies = null;

  await client.pushAsync(session);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2Manies.associationType).values().next().value).toBe(c1b.strategy);
}

export async function changeSetAfterPushMany2Many() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1Many2Many(c1b);

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);

  await client.pushAsync(session);
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetAfterPushMany2ManyRemove() {
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
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1Many2Many(c1b);

  await client.pushAsync(session);
  await client.pullAsync(session, [pull]);
  session.checkpoint();

  c1a.C1C1Many2Manies = null;

  await client.pushAsync(session);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);
}

export async function changeSetAfterPullInNewSessionButNoPush() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  await client.pullAsync(session, []);

  const changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetBeforeAndAfterResetWithSessionObject() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const sc1a = session.create<SC1>(m.SC1);

  sc1a.SessionAllorsString = 'X';

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(0);

  expect(changeSet.associationsByRoleType.get(m.SC1.SessionAllorsString).values().next().value).toBe(sc1a.strategy);

  sc1a.strategy.reset();

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
}

export async function changeSetBeforeAndAfterResetWithChangeSessionObject() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const sc1a = session.create<SC1>(m.SC1);

  sc1a.SessionAllorsString = 'X';

  await client.pushAsync(session);

  let changeSet = session.checkpoint();

  sc1a.SessionAllorsString = 'Y';

  changeSet = session.checkpoint();

  sc1a.strategy.reset();

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);

  expect(sc1a.SessionAllorsString).toBe('Y');
}

export async function changeSetAfterDoubleReset() {
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

  session.checkpoint();

  c1a_1.C1AllorsString = 'X';

  await client.pushAsync(session);
  result = await client.pullAsync(session, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  await client.pushAsync(session);

  c1a_2.strategy.reset();
  c1a_2.strategy.reset();

  const changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(0);

  expect(changeSet.associationsByRoleType.get(m.C1.C1AllorsString).values().next().value).toBe(c1a_2.strategy);
}
