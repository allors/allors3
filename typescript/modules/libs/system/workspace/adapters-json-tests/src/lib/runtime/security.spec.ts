import { C1, Denied, TrimFrom, TrimTo } from '@allors/default/workspace/domain';
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

  await fixture.jsonClient.removeC1StringReadGrant(c1.id);

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
  expect(
    denied.strategy.canRead(m.Denied.DefaultWorkspaceProperty)
  ).toBeTruthy();
  expect(
    denied.strategy.canWrite(m.Denied.DefaultWorkspaceProperty)
  ).toBeFalsy();

  for (const denyRead of [true, false]) {
    if (denyRead) {
      await fixture.jsonClient.addDeniedPropertyReadRevocation(denied.id);
    } else {
      await fixture.jsonClient.removeDeniedPropertyReadRevocation(denied.id);
    }

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

test('retains objects and readable relations when a role is unreadable', async () => {
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
  expect(result.hasErrors).toBeFalsy();

  const trimFrom = result.collection<TrimFrom>(m.TrimFrom);
  expect(trimFrom).toHaveLength(4);

  const protectedRoots = trimFrom.filter(
    (v) => !v.strategy.canRead(m.TrimFrom.Name)
  );
  expect(protectedRoots).toHaveLength(2);
  for (const root of protectedRoots) {
    expect(root.Name).toBeNull();
  }

  for (const root of trimFrom) {
    expect(root.strategy.canRead(m.TrimFrom.Many2One)).toBeTruthy();
    expect(root.strategy.canRead(m.TrimFrom.Many2Manies)).toBeTruthy();
    expect(root.Many2One).not.toBeNull();
    expect(root.Many2Manies).toHaveLength(1);
    expect(root.Many2Manies[0]).toBe(root.Many2One);
  }

  const from1 = trimFrom.find((v) => v.Name === 'Untrimmed1');
  const from2 = trimFrom.find((v) => v.Name === 'Untrimmed2');

  expect(from1.Many2One.strategy.canRead(m.TrimTo.Name)).toBeFalsy();
  expect(from1.Many2One.Name).toBeNull();
  expect(from2.Many2One.strategy.canRead(m.TrimTo.Name)).toBeTruthy();
  expect(from2.Many2One.Name).toBe('Untrimmed1');

  const includedTargets = new Set(trimFrom.map((v) => v.Many2One));
  expect(includedTargets.size).toBe(2);
  expect(new Set(protectedRoots.map((v) => v.Many2One))).toEqual(
    includedTargets
  );
});

test('directly pulls an object with an unreadable role', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();
  const result = await session.pull({
    extent: { kind: 'Filter', objectType: m.TrimTo },
  });
  expect(result.hasErrors).toBeFalsy();

  const targets = result.collection<TrimTo>(m.TrimTo);
  expect(targets).toHaveLength(2);
  const protectedTarget = targets.find(
    (v) => !v.strategy.canRead(m.TrimTo.Name)
  );
  expect(protectedTarget).toBeDefined();
  expect(protectedTarget.Name).toBeNull();

  const freshSession = (await fixture.createWorkspace()).createSession();
  const directResult = await freshSession.pull({
    objectId: protectedTarget.id,
  });
  expect(directResult.hasErrors).toBeFalsy();
  const directTarget = directResult.object<TrimTo>(m.TrimTo);
  expect(directTarget).toBeDefined();
  expect(directTarget.id).toBe(protectedTarget.id);
  expect(directTarget.strategy.canRead(m.TrimTo.Name)).toBeFalsy();
  expect(directTarget.Name).toBeNull();
});

afterEach(async () => {
  await fixture?.close();
});
