import { C1, SC1 } from '@allors/workspace/domain/default';
import { Pull } from '@allors/workspace/domain/system';
import { Fixture } from '../fixture';
import '../matchers';
let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('changeSetConstruction', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const changeSet = session.checkpoint();

  expect(changeSet.instantiated.size).toBe(0);
});

test('changeSetInstantiated', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1s = result.collection<C1>('C1s');
  const c1a = c1s[0];

  const changeSet = session.checkpoint();

  expect(changeSet.instantiated.size).toBe(1);
  const instantiated = changeSet.instantiated.values().next().value;
  expect(instantiated).toBe(c1a.strategy);
});

test('changeSetAfterPush', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1s = result.collection<C1>('C1s');
  const c1a = c1s[0];

  c1a.C1AllorsString = 'X';

  await session.push();

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  const changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
});

test('changeSetPushChangeNoPush', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  let changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();

  result = await session.pull([pull]);

  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
});

test('changeSetPushChangePush', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  let changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();

  result = await session.pull([pull]);

  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  await session.push();

  changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
});

test('changeSetAfterPushWithNoChanges', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = session.create<C1>(m.C1);

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  const created = changeSet.created.values().next().value;
  expect(created).toBeDefined();
  expect(created).toBe(c1a.strategy);

  await session.push();

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
});

test('changeSetAfterPushWithPull', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];

  c1a.C1AllorsString = 'X';

  await session.push();

  await session.pull([pull]);

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  const changed = changeSet.associationsByRoleType.get(m.C1.C1AllorsString);
  expect(changed).toBeDefined();
});

test('changeSetAfterPushWithPullWithNoChanges', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];

  await session.push();
  await session.pull([pull]);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
});

test('changeSetOne2One', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1x = session.create<C1>(m.C1);
  const c1y = session.create<C1>(m.C1);

  session.checkpoint();

  c1a.C1C1One2One = c1x;

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).size).toBe(1);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values().next().value).toBe(c1x.strategy);

  c1a.C1C1One2One = c1y;

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).size).toBe(1);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values().next().value).toBe(c1y.strategy);
});

test('changeSetAfterPushOne2One', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1x = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1x;

  await session.push();

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

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetIncludeAfterPushOne2One', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = c1a.C1C1One2One;
  const c1x = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1x;

  await session.push();

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

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetAfterPushOne2OneWithPreviousIncluded', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);
  const previous = c1a.C1C1One2One;

  c1a.C1C1One2One = c1b;

  await session.push();

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

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetAfterPushOne2OneRemove', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1One2One = c1b;

  await session.push();
  await session.pull([pull]);
  session.checkpoint();

  c1a.C1C1One2One = null;

  await session.push();

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).values().next().value).toBe(c1b.strategy);
});

test('changeSetAfterPushMany2One', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1Many2One = c1b;

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2One.associationType).values().next().value).toBe(c1b.strategy);

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetAfterPushMany2OneRemove', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.C1C1Many2One = c1b;

  await session.push();
  await session.pull([pull]);
  session.checkpoint();

  c1a.C1C1Many2One = null;

  await session.push();

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2One.associationType).values().next().value).toBe(c1b.strategy);
});

test('changeSetAfterPushOne2Many', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1One2Many(c1b);

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2Manies.associationType).values().next().value).toBe(c1b.strategy);

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetAfterPushOne2ManyRemove', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1One2Many(c1b);

  await session.push();
  await session.pull([pull]);
  session.checkpoint();

  c1a.C1C1One2Manies = null;

  await session.push();

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1One2Manies.associationType).values().next().value).toBe(c1b.strategy);
});

test('changeSetMany2Many', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  session.checkpoint();

  c1a.addC1C1Many2Many(c1b);

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);

  c1a.removeC1C1Many2Many(c1b);

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);
});

test('changeSetAfterPushMany2Many', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1Many2Many(c1b);

  await session.push();

  let changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(1);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1b.strategy);
  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);

  await session.push();
  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetAfterPushMany2ManyRemove', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);
  const c1a = result.collection<C1>('C1s')[0];
  const c1b = session.create<C1>(m.C1);

  c1a.addC1C1Many2Many(c1b);

  await session.push();
  await session.pull([pull]);
  session.checkpoint();

  c1a.C1C1Many2Manies = null;

  await session.push();

  const changeSet = session.checkpoint();

  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next().value).toBe(c1a.strategy);
  expect(changeSet.rolesByAssociationType.get(m.C1.C1C1Many2Manies.associationType).values().next().value).toBe(c1b.strategy);
});

test('changeSetAfterPullInNewSessionButNoPush', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  await session.pull([]);

  const changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
});

test('changeSetBeforeAndAfterResetWithSessionObject', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const sc1a = session.create<SC1>(m.SC1);

  sc1a.SessionAllorsString = 'X';

  await session.push();

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
});

test('changeSetBeforeAndAfterResetWithChangeSessionObject', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const sc1a = session.create<SC1>(m.SC1);

  sc1a.SessionAllorsString = 'X';

  await session.push();

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
});

test('changeSetAfterDoubleReset', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  session.checkpoint();

  c1a_1.C1AllorsString = 'X';

  await session.push();
  result = await session.pull([pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  await session.push();

  c1a_2.strategy.reset();
  c1a_2.strategy.reset();

  const changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.instantiated.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(0);

  expect(changeSet.associationsByRoleType.get(m.C1.C1AllorsString).values().next().value).toBe(c1a_2.strategy);
});
