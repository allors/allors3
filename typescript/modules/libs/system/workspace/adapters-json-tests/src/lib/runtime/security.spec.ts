import { C1, Denied, TrimFrom } from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
import { Origin } from '@allors/system/workspace/meta';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('withAccessControl', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await session.pull(pull);

  const c1s = result.collection<C1>(m.C1);

  for (const c1 of c1s) {
    for (const roleType of c1.strategy.cls.roleTypes) {
      expect(c1.strategy.canRead(roleType)).toBeTruthy();
      expect(c1.strategy.canWrite(roleType)).toBeTruthy();
    }
  }
});

test('withAccessControlAsAgent', async () => {
  await fixture.login('agent');

  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await session.pull(pull);

  const c1s = result.collection<C1>(m.C1);
  expect(c1s.length).toBeGreaterThan(0);

  for (const c1 of c1s) {
    for (const roleType of c1.strategy.cls.roleTypes) {
      expect(c1.strategy.canRead(roleType)).toBeTruthy();
      expect(c1.strategy.canWrite(roleType)).toBeTruthy();
    }
  }
});

test('withoutAccessControl', async () => {
  await fixture.login('noacl');

  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await session.pull(pull);

  const c1s = result.collection<C1>(m.C1);
  for (const c1 of c1s) {
    for (const roleType of c1.strategy.cls.roleTypes) {
      if (roleType.relationType.origin === Origin.Database) {
        expect(c1.strategy.canRead(roleType)).toBeFalsy();
        expect(c1.strategy.canWrite(roleType)).toBeFalsy();
      } else {
        expect(c1.strategy.canRead(roleType)).toBeTruthy();
        expect(c1.strategy.canWrite(roleType)).toBeTruthy();
      }
    }
  }
});

test('withoutPermissions', async () => {
  await fixture.login('noperm');

  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await session.pull(pull);

  const c1s = result.collection<C1>(m.C1);
  for (const c1 of c1s) {
    for (const roleType of c1.strategy.cls.roleTypes) {
      if (roleType.relationType.origin === Origin.Database) {
        expect(c1.strategy.canRead(roleType)).toBeFalsy();
        expect(c1.strategy.canWrite(roleType)).toBeFalsy();
      } else {
        expect(c1.strategy.canRead(roleType)).toBeTruthy();
        expect(c1.strategy.canWrite(roleType)).toBeTruthy();
      }
    }
  }
});

test('deniedPermissions', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.Denied } };
  const result = await session.pull(pull);

  const denieds = result.collection<Denied>(m.Denied);
  for (const denied of denieds) {
    for (const roleType of denied.strategy.cls.roleTypes) {
      if (roleType.relationType.origin === Origin.Database) {
        expect(denied.strategy.canRead(roleType)).toBeTruthy();
        expect(denied.strategy.canWrite(roleType)).toBeFalsy();
      } else {
        expect(denied.strategy.canRead(roleType)).toBeTruthy();
        expect(denied.strategy.canWrite(roleType)).toBeTruthy();
      }
    }
  }
});

test('refreshes a cached grant when an object is synchronized', async () => {
  await fixture.login('administrator');
  const { workspace, m } = fixture;
  const session = workspace.createSession();
  const result = await session.pull({
    extent: { kind: 'Filter', objectType: m.C1 },
  });
  expect(result.hasErrors).toBeFalsy();
  const c1s = result.collection<C1>(m.C1);
  expect(c1s.length).toBeGreaterThan(0);
  const c1 = c1s[0];
  expect(c1.strategy.canRead(m.C1.C1AllorsString)).toBeTruthy();

  const mutation = await session.call({
    name: 'TestPermissionRefresh',
    objects: { target: c1 },
    values: { action: 'removeGrantRead' },
  });
  expect(mutation.hasErrors).toBeFalsy();

  const refreshed = await session.pull({ object: c1 });
  expect(refreshed.hasErrors).toBeFalsy();
  expect(refreshed.object<C1>(m.C1)).toBe(c1);
  expect(c1.strategy.canRead(m.C1.C1AllorsString)).toBeFalsy();
});

test('refreshes a cached revocation when an object is synchronized', async () => {
  await fixture.login('administrator');
  const { workspace, m } = fixture;
  const session = workspace.createSession();
  const result = await session.pull({
    extent: { kind: 'Filter', objectType: m.Denied },
  });
  expect(result.hasErrors).toBeFalsy();
  const denieds = result.collection<Denied>(m.Denied);
  expect(denieds).toHaveLength(1);
  const denied = denieds[0];
  expect(denied.strategy.canRead(m.Denied.DefaultWorkspaceProperty)).toBeTruthy();
  expect(denied.strategy.canWrite(m.Denied.DefaultWorkspaceProperty)).toBeFalsy();

  for (const denyRead of [true, false]) {
    const mutation = await session.call({
      name: 'TestPermissionRefresh',
      objects: { target: denied },
      values: {
        action: denyRead ? 'addReadRevocation' : 'removeReadRevocation',
      },
    });
    expect(mutation.hasErrors).toBeFalsy();

    const refreshed = await session.pull({ object: denied });
    expect(refreshed.hasErrors).toBeFalsy();
    expect(refreshed.object<Denied>(m.Denied)).toBe(denied);
    expect(denied.strategy.canRead(m.Denied.DefaultWorkspaceProperty)).toBe(
      !denyRead
    );
    expect(
      denied.strategy.canWrite(m.Denied.DefaultWorkspaceProperty)
    ).toBeFalsy();
  }
});

test('trim', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.TrimFrom,
    },
    results: [
      {
        include: [
          {
            propertyType: m.TrimFrom.Many2One,
          },
          {
            propertyType: m.TrimFrom.Many2Manies,
          },
        ],
      },
    ],
  };
  const result = await session.pull(pull);

  const trimFrom = result.collection<TrimFrom>(m.TrimFrom);

  expect(trimFrom.length).toBe(2);

  const from1 = trimFrom.find((v) => v.Name === 'Untrimmed1');
  const from2 = trimFrom.find((v) => v.Name === 'Untrimmed2');

  expect(from1.Many2One).toBeNull();
  expect(from2.Many2One).toBeDefined();

  expect(from1.Many2Manies.length).toBe(0);
  expect(from2.Many2Manies.length).toBe(1);
});
