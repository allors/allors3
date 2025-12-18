import { C1 } from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
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

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
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
  expect(created).toBe(c1a);

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
  expect(
    changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size
  ).toBe(1);

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values()
      .next().value
  ).toBe(c1x);

  c1a.C1C1One2One = c1y;

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(changeSet.associationsByRoleType.get(m.C1.C1C1One2One).size).toBe(1);
  expect(
    changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size
  ).toBe(2);

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values()
  ).toContain(c1x);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values()
  ).toContain(c1y);
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
  expect(
    changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType).size
  ).toBe(1);

  expect(changeSet.created.values().next().value).toBe(c1x);
  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values()
      .next().value
  ).toBe(c1x);

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
  const associations = [
    ...changeSet.associationsByRoleType.get(m.C1.C1C1One2One),
  ];
  const roles = [
    ...changeSet.rolesByAssociationType.get(m.C1.C1C1One2One.associationType),
  ];

  expect(associations.length).toBe(1);
  expect(roles.length).toBe(2);

  expect(created).toContain(c1x);
  expect(associations).toContain(c1a);
  expect(roles).toContain(c1b);
  expect(roles).toContain(c1x);

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
  const associationsC1C1One2One = [
    ...changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values(),
  ];
  const rolesC1C1One2One = [
    ...changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values(),
  ];

  expect(created).toEqual([c1b]);
  expect(associationsC1C1One2One).toEqual([c1a]);
  expect(rolesC1C1One2One).toEqual([previous, c1b]);

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

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2One).values().next().value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2One.associationType)
      .values()
      .next().value
  ).toBe(c1b);
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

  expect(changeSet.created.values().next().value).toBe(c1b);
  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2One.associationType)
      .values()
      .next().value
  ).toBe(c1b);

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

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2One).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2One.associationType)
      .values()
      .next().value
  ).toBe(c1b);
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

  expect(changeSet.created.values().next().value).toBe(c1b);
  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);

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

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1One2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1One2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);
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

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);

  c1a.removeC1C1Many2Many(c1b);

  changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(1);

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);
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

  expect(changeSet.created.values().next().value).toBe(c1b);
  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);

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

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1C1Many2Manies).values().next()
      .value
  ).toBe(c1a);
  expect(
    changeSet.rolesByAssociationType
      .get(m.C1.C1C1Many2Manies.associationType)
      .values()
      .next().value
  ).toBe(c1b);
});

test('changeSetAfterPullInNewSessionButNoPush', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  await session.pull([]);

  const changeSet = session.checkpoint();

  expect(changeSet.created.size).toBe(0);
  expect(changeSet.associationsByRoleType.size).toBe(0);
  expect(changeSet.rolesByAssociationType.size).toBe(0);
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
  expect(changeSet.associationsByRoleType.size).toBe(1);
  expect(changeSet.rolesByAssociationType.size).toBe(0);

  expect(
    changeSet.associationsByRoleType.get(m.C1.C1AllorsString).values().next()
      .value
  ).toBe(c1a_2);
});
