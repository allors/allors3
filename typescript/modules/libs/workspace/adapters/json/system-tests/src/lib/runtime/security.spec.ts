import { C1, Denied } from '@allors/workspace/domain/default';
import { Pull } from '@allors/workspace/domain/system';
import { Origin } from '@allors/workspace/meta/system';
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
        expect(denied.strategy.canRead(roleType)).toBeTruthy();
      }
    }
  }
});
