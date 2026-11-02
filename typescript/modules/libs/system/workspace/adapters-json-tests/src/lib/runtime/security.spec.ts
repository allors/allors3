import {
  C1,
  C2,
  Denied,
  Organisation,
  TrimFrom,
} from '@allors/default/workspace/domain';
import {
  AccessRequest,
  InvokeRequest,
  PermissionRequest,
  PullRequest,
  PushRequest,
  SyncRequest,
} from '@allors/system/common/protocol-json';
import {
  DatabaseConnection,
  IDatabaseJsonClient,
} from '@allors/system/workspace/adapters-json';
import { IObject, ISession, Pull } from '@allors/system/workspace/domain';
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

/**
 * Forwards to the fixture's client, counts the access requests and can hold the first request to an
 * endpoint until the test releases it, which interleaves two pulls deterministically.
 */
class ControlledClient implements IDatabaseJsonClient {
  accessRequests = 0;

  /** Resolves once the held endpoint receives its first request. */
  requested: Promise<void>;

  release: () => void;

  private endpoint?: 'access' | 'permission';
  private onRequested: () => void;
  private released: Promise<void>;

  constructor(private readonly client: IDatabaseJsonClient) {}

  hold(endpoint: 'access' | 'permission') {
    this.endpoint = endpoint;
    this.requested = new Promise((resolve) => (this.onRequested = resolve));
    this.released = new Promise((resolve) => (this.release = resolve));
  }

  pull(pullRequest: PullRequest) {
    return this.client.pull(pullRequest);
  }

  sync(syncRequest: SyncRequest) {
    return this.client.sync(syncRequest);
  }

  push(pushRequest: PushRequest) {
    return this.client.push(pushRequest);
  }

  invoke(invokeRequest: InvokeRequest) {
    return this.client.invoke(invokeRequest);
  }

  async access(accessRequest: AccessRequest) {
    this.accessRequests++;
    await this.holdFirst('access');
    return this.client.access(accessRequest);
  }

  async permission(permissionRequest: PermissionRequest) {
    await this.holdFirst('permission');
    return this.client.permission(permissionRequest);
  }

  private async holdFirst(endpoint: 'access' | 'permission') {
    if (this.endpoint === endpoint) {
      this.endpoint = undefined;
      this.onRequested();
      await this.released;
    }
  }
}

function controlledSession(): [ControlledClient, ISession] {
  const client = new ControlledClient(fixture.jsonClient);
  const database = new DatabaseConnection(
    fixture.databaseConnection.configuration,
    client
  );
  return [client, database.createWorkspace().createSession()];
}

function expectReadable(objects: IObject[]) {
  expect(objects.length).toBeGreaterThan(0);

  for (const object of objects) {
    for (const roleType of object.strategy.cls.roleTypes) {
      expect(object.strategy.canRead(roleType)).toBeTruthy();
    }
  }
}

test('concurrentColdPullsOfDifferentObjects', async () => {
  const { m } = fixture;
  const [client, session] = controlledSession();

  // The first pull syncs the C1s, registers their grants and waits for the permissions.
  client.hold('permission');
  const first = session.pull({ extent: { kind: 'Filter', objectType: m.C1 } });
  await client.requested;

  // The second pull syncs the C2s, which share those grants.
  const second = await session.pull({
    extent: { kind: 'Filter', objectType: m.C2 },
  });
  expectReadable(second.collection<C2>(m.C2));

  client.release();
  expectReadable((await first).collection<C1>(m.C1));
});

test('concurrentColdPullsOfTheSameObjects', async () => {
  const { m } = fixture;
  const [client, session] = controlledSession();
  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };

  // The first pull syncs the C1s, registers their grants and waits for the permissions.
  client.hold('permission');
  const first = session.pull(pull);
  await client.requested;

  // The second pull finds the C1s synced, with their grants registered but their permissions pending.
  const second = await session.pull(pull);
  expectReadable(second.collection<C1>(m.C1));

  client.release();
  expectReadable((await first).collection<C1>(m.C1));
});

test('knownRevocationIsNotRequestedAgain', async () => {
  const { m } = fixture;
  const [client, session] = controlledSession();

  const result = await session.pull({
    extent: { kind: 'Filter', objectType: m.Organisation },
  });
  const [organisation1, organisation2] = result.collection<Organisation>(
    m.Organisation
  );

  // Both organisations now carry the same revocation.
  await session.invoke([
    organisation1.ToggleCanWrite,
    organisation2.ToggleCanWrite,
  ]);

  // The first pull syncs organisation1 and requests the revocation.
  await session.pull({ object: organisation1 });
  client.accessRequests = 0;

  // The second pull syncs organisation2 and finds the revocation known.
  await session.pull({ object: organisation2 });

  expect(client.accessRequests).toBe(0);
  expect(organisation2.strategy.canRead(m.Organisation.Name)).toBeTruthy();
  expect(organisation2.strategy.canWrite(m.Organisation.Name)).toBeFalsy();
});

test('concurrentPullsOfAnObjectWithANewRevocation', async () => {
  const { m } = fixture;
  const [client, session] = controlledSession();

  const result = await session.pull({
    extent: { kind: 'Filter', objectType: m.Organisation },
  });
  const organisation = result.collection<Organisation>(m.Organisation)[0];
  await session.invoke(organisation.ToggleCanWrite);

  // The first pull syncs the organisation with its new revocation and waits for the access response.
  client.hold('access');
  const first = session.pull({ object: organisation });
  await client.requested;

  // The second pull finds the organisation synced, with its revocation not registered yet.
  await session.pull({ object: organisation });
  expect(organisation.strategy.canWrite(m.Organisation.Name)).toBeFalsy();

  client.release();
  await first;
  expect(organisation.strategy.canWrite(m.Organisation.Name)).toBeFalsy();
});
