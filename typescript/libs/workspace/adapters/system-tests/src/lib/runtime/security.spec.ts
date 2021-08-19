import { IAsyncDatabaseClient, IReactiveDatabaseClient, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { C1, Denied } from '@allors/workspace/domain/core';

import { Fixture, name_c1A, name_c1B } from '../Fixture';
import '../Matchers';
import { RelationType } from '../../../../../meta/system/src/lib/RelationType';
import { Origin } from '@allors/workspace/meta/system';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initSecurity(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function withAccessControl() {
  const { workspace, client, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await client.pullAsync(session, pull);

  const c1s = result.collection<C1>(m.C1);

  for (const c1 of c1s) {
    for (const roleType of c1.strategy.cls.roleTypes) {
      expect(c1.strategy.canRead(roleType)).toBeTruthy();
      expect(c1.strategy.canWrite(roleType)).toBeTruthy();
    }
  }
}

export async function withoutAccessControl() {
  fixture.login('noacl');

  const { workspace, client, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await client.pullAsync(session, pull);

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
}

export async function withoutPermissions() {
  fixture.login('noperm');

  const { workspace, client, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await client.pullAsync(session, pull);

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
}

export async function deniedPermissions() {
  const { workspace, client, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.Denied } };
  const result = await client.pullAsync(session, pull);

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
}
