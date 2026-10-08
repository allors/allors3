import {
  CacheKey,
  createIdGenerator,
  DatabaseConnection,
  MemoryCache,
  Operations,
  Pull,
  RecordChangedEvent,
  WorkspaceInitialVersion,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { Fixture, name_c1A, name_c1B, name_c1C } from '../fixture';

// The contract of the connection, exercised without a session, as the .NET ConnectionTests
// do: a pull by the query model answers ids and leaves records behind, a record answers its
// roles as values and its permissions against the grants and revocations of the user, and a
// push and an invoke take ids and versions.
let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

const upper = (name: string) => name.toUpperCase();

test('pullByExtentAnswersIdsAndLeavesRecords', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const result = await connection.pull([
    { extent: { kind: 'Filter', objectType: m.C1 } },
  ]);

  expect(result.hasErrors).toBeFalsy();
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const ids = result.collections.get(upper(m.C1.pluralName));
  expect(ids.length).toBe(4);
  expect([...ids].sort()).toEqual([...result.pool].sort());

  for (const id of ids) {
    expect(id).toBeGreaterThan(0);

    const record = connection.getRecord(id);
    expect(record).toBeDefined();
    expect(record.id).toBe(id);
    expect(record.cls).toBe(m.C1);
    expect(record.version).toBeGreaterThan(WorkspaceInitialVersion);
  }
});

test('recordAnswersRolesAsValues', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const result = await connection.pull([
    { extent: { kind: 'Filter', objectType: m.C1 } },
  ]);
  const idByName = new Map(
    result.collections
      .get(upper(m.C1.pluralName))
      .map((id) => [connection.getRecord(id).getRole(m.C1.Name) as string, id])
  );

  const c1A = connection.getRecord(idByName.get(name_c1A));
  expect(c1A.getRole(m.C1.C1AllorsString)).toBeFalsy();
  expect(c1A.getRole(m.C1.C1C1One2One)).toBe(idByName.get(name_c1B));

  const c1B = connection.getRecord(idByName.get(name_c1B));
  expect(c1B.getRole(m.C1.C1AllorsString)).toBe('ᴀbra');
  expect(c1B.getRole(m.C1.C1AllorsBoolean)).toBe(true);

  const c1C = connection.getRecord(idByName.get(name_c1C));
  expect(c1C.getRole(m.C1.C1C1Many2Manies)).toEqual(
    [idByName.get(name_c1B), idByName.get(name_c1C)].sort((a, b) => a - b)
  );
});

test('permissionsAreAnsweredPerRecord', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const result = await connection.pull([
    { extent: { kind: 'Filter', objectType: m.C1 } },
  ]);

  const read = connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read);
  const write = connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Write);
  expect(read).not.toBe(0);
  expect(write).not.toBe(0);

  for (const id of result.collections.get(upper(m.C1.pluralName))) {
    const record = connection.getRecord(id);
    expect(record.isPermitted(read)).toBeTruthy();
    expect(record.isPermitted(write)).toBeTruthy();
  }
});

test('withoutAccessControlNothingIsPermitted', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('noacl');

  const result = await connection.pull([
    { extent: { kind: 'Filter', objectType: m.C1 } },
  ]);

  const read = connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read);
  expect(read).toBe(0);

  for (const id of result.collections.get(upper(m.C1.pluralName))) {
    expect(connection.getRecord(id).isPermitted(read)).toBeFalsy();
  }
});

test('revocationDeniesTheWrite', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const result = await connection.pull([
    { extent: { kind: 'Filter', objectType: m.Denied } },
  ]);

  const read = connection.getPermission(
    m.Denied,
    m.Denied.DefaultWorkspaceProperty,
    Operations.Read
  );
  const write = connection.getPermission(
    m.Denied,
    m.Denied.DefaultWorkspaceProperty,
    Operations.Write
  );
  expect(read).not.toBe(0);
  expect(write).not.toBe(0);

  const ids = result.collections.get(upper(m.Denied.pluralName));
  expect(ids.length).toBeGreaterThan(0);

  for (const id of ids) {
    const record = connection.getRecord(id);
    expect(record.isPermitted(read)).toBeTruthy();
    expect(record.isPermitted(write)).toBeFalsy();
  }
});

test('pushNewObjectAnswersItsDatabaseId', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const workspaceId = createIdGenerator()();

  const pushed = await connection.push(
    [
      {
        workspaceId,
        cls: m.C1,
        roles: [{ roleType: m.C1.C1AllorsString, value: 'pushed' }],
      },
    ],
    null
  );

  expect(pushed.hasErrors).toBeFalsy();
  const id = pushed.databaseIdByWorkspaceId.get(workspaceId);
  expect(id).toBeGreaterThan(0);

  const result = await connection.pull([{ objectId: id }]);

  expect(result.hasErrors).toBeFalsy();
  expect(result.pool).toContain(id);

  const record = connection.getRecord(id);
  expect(record.cls).toBe(m.C1);
  expect(record.getRole(m.C1.C1AllorsString)).toBe('pushed');
});

test('pushChangedObjectTakesTheVersionItWasChangedFrom', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: { kind: 'Equals', propertyType: m.C1.Name, value: name_c1A },
    },
  };
  const ids = (await connection.pull([pull])).collections.get(upper(m.C1.pluralName));
  expect(ids.length).toBe(1);
  const id = ids[0];
  const before = connection.getRecord(id);

  const pushed = await connection.push(null, [
    {
      id,
      version: before.version,
      roles: [{ roleType: m.C1.C1AllorsString, value: 'X' }],
    },
  ]);

  expect(pushed.hasErrors).toBeFalsy();

  await connection.pull([pull]);
  const after = connection.getRecord(id);
  expect(after).not.toBe(before);
  expect(after.version).toBeGreaterThan(before.version);
  expect(after.getRole(m.C1.C1AllorsString)).toBe('X');

  const stale = await connection.push(null, [
    {
      id,
      version: before.version,
      roles: [{ roleType: m.C1.C1AllorsString, value: 'Y' }],
    },
  ]);

  expect(stale.hasErrors).toBeTruthy();
  expect(stale.versionErrors).toContain(id);
});

test('invokeRunsTheMethod', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.Organisation } };
  const id = (await connection.pull([pull])).collections.get(
    upper(m.Organisation.pluralName)
  )[0];
  const before = connection.getRecord(id);
  expect(before.getRole(m.Organisation.JustDidIt)).not.toBe(true);

  const invoked = await connection.invoke([
    { id, version: before.version, methodType: m.Organisation.JustDoIt },
  ]);

  expect(invoked.hasErrors).toBeFalsy();

  await connection.pull([pull]);
  expect(connection.getRecord(id).getRole(m.Organisation.JustDidIt)).toBe(true);
});

test('recordChangedIsRaisedWhenAPullReplacesARecord', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: { kind: 'Equals', propertyType: m.C1.Name, value: name_c1A },
    },
  };
  const id = (await connection.pull([pull])).collections.get(upper(m.C1.pluralName))[0];
  const version = connection.getRecord(id).version;

  const changed: RecordChangedEvent[] = [];
  connection.recordChanged.subscribe((e) => changed.push(e));

  await connection.pull([pull]);
  expect(changed).toEqual([]);

  const pushed = await connection.push(null, [
    {
      id,
      version,
      roles: [{ roleType: m.C1.C1AllorsString, value: 'changed' }],
    },
  ]);
  expect(pushed.hasErrors).toBeFalsy();
  expect(changed).toEqual([]);

  await connection.pull([pull]);

  expect(changed.length).toBe(1);
  expect(changed[0].id).toBe(id);
  expect(changed[0].record).toBe(connection.getRecord(id));
  expect(changed[0].record.version).toBeGreaterThan(version);
});

test('theConnectionsOfOneUserShareACache', async () => {
  const { m } = fixture;
  const template = fixture.createConnection('administrator');

  const cache = new MemoryCache(template.workspaceName, template.metaPopulation);
  const first = fixture.createConnection('administrator', { cache });
  const second = fixture.createConnection('administrator', { cache });

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await first.pull([pull]);

  expect(first.cache).toBe(cache);
  expect(second.cache).toBe(cache);
  expect(result.pool.length).toBeGreaterThan(0);
  for (const id of result.pool) {
    expect(second.getRecord(id)).toBeDefined();
    expect(second.getRecord(id)).toBe(first.getRecord(id));
  }

  const changed: RecordChangedEvent[] = [];
  second.recordChanged.subscribe((e) => changed.push(e));

  await second.pull([pull]);

  expect(changed).toEqual([]);
});

test('theCacheTellsEveryConnectionWhenAnotherConnectionReplacesARecord', async () => {
  const { m } = fixture;
  const template = fixture.createConnection('administrator');

  const cache = new MemoryCache(template.workspaceName, template.metaPopulation);
  const first = fixture.createConnection('administrator', { cache });
  const second = fixture.createConnection('administrator', { cache });

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: { kind: 'Equals', propertyType: m.C1.Name, value: name_c1A },
    },
  };
  const id = (await first.pull([pull])).collections.get(upper(m.C1.pluralName))[0];
  const before = first.getRecord(id);

  const changed: RecordChangedEvent[] = [];
  cache.recordChanged.subscribe((e) => changed.push(e));

  const pushed = await first.push(null, [
    {
      id,
      version: before.version,
      roles: [{ roleType: m.C1.C1AllorsString, value: 'shared' }],
    },
  ]);
  expect(pushed.hasErrors).toBeFalsy();
  expect(changed).toEqual([]);

  await second.pull([pull]);

  expect(changed.length).toBe(1);
  expect(changed[0].id).toBe(id);
  expect(changed[0].record).toBe(first.getRecord(id));
  expect(changed[0].record.version).toBeGreaterThan(before.version);
  expect(first.getRecord(id).getRole(m.C1.C1AllorsString)).toBe('shared');
});

test('aCacheOfAnotherWorkspaceNameIsRefused', () => {
  const template = fixture.createConnection('administrator');

  const cache = new MemoryCache('Other', template.metaPopulation);

  expect(() => fixture.createConnection('administrator', { cache })).toThrow(
    /Other.*Default|Default.*Other/
  );
});

test('aCacheOfAnotherMetaPopulationIsRefused', () => {
  const template = fixture.createConnection('administrator');

  const cache = new MemoryCache(template.workspaceName, new LazyMetaPopulation(data));

  expect(() => fixture.createConnection('administrator', { cache })).toThrow(
    /meta population/
  );
});

test('theConnectionLearnsTheDatabaseAndTheUserFromTheFirstResponse', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  expect(connection.databaseId).toBeNull();
  expect(connection.userId).toBeNull();
  expect(connection.cache.key).toBeNull();
  expect(connection.metaFingerprint).toMatch(/^[0-9a-f]{16}$/);

  await connection.pull([{ extent: { kind: 'Filter', objectType: m.C1 } }]);

  expect(connection.databaseId).toBeTruthy();
  expect(connection.userId).toBeGreaterThan(0);
  expect(
    connection.cache.key.equals(
      new CacheKey(
        connection.databaseId,
        connection.userId,
        connection.workspaceName,
        connection.metaFingerprint
      )
    )
  ).toBe(true);

  const other = fixture.createConnection('noacl');
  await other.pull([{ extent: { kind: 'Filter', objectType: m.C1 } }]);

  expect(other.databaseId).toBe(connection.databaseId);
  expect(other.userId).not.toBe(connection.userId);
});

test('theCacheRefusesAConnectionOfAnotherUser', async () => {
  const { m } = fixture;
  const template = fixture.createConnection('administrator');

  const cache = new MemoryCache(template.workspaceName, template.metaPopulation);
  const administrator = fixture.createConnection('administrator', { cache });
  const noacl = fixture.createConnection('noacl', { cache });

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await administrator.pull([pull]);

  await expect(noacl.pull([pull])).rejects.toThrow(
    new RegExp(`user ${administrator.userId}.*user \\d+`)
  );

  for (const id of result.pool) {
    expect(administrator.getRecord(id)).toBeDefined();
  }
});

test('theServerRefusesAConnectionForAnotherWorkspaceName', async () => {
  const { m } = fixture;
  const template = fixture.createConnection('administrator');

  const connection = new DatabaseConnection(
    'Other',
    template.metaPopulation,
    fixture.createTransport('administrator')
  );

  await expect(
    connection.pull([{ extent: { kind: 'Filter', objectType: m.C1 } }])
  ).rejects.toThrow(/'Other'.*'Default'|'Default'.*'Other'/);

  expect(connection.databaseId).toBeNull();
});

test('aGrantWhoseVersionChangedIsRequestedAgain', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.C1 } };
  const result = await connection.pull([pull]);
  const write = connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Write);
  const read = connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read);
  expect(result.pool.length).toBeGreaterThan(0);
  for (const id of result.pool) {
    expect(connection.getRecord(id).isPermitted(write)).toBe(true);
  }

  await fixture.removeAdministratorPermission(m.C1.C1AllorsString, Operations.Write);

  await connection.pull([pull]);

  for (const id of result.pool) {
    expect(connection.getRecord(id).isPermitted(write)).toBe(false);
    expect(connection.getRecord(id).isPermitted(read)).toBe(true);
  }
});

test('aRevocationWhoseVersionChangedIsRequestedAgain', async () => {
  const { m } = fixture;
  const connection = fixture.createConnection('administrator');

  const pull: Pull = { extent: { kind: 'Filter', objectType: m.Denied } };
  const result = await connection.pull([pull]);
  const read = connection.getPermission(
    m.Denied,
    m.Denied.DefaultWorkspaceProperty,
    Operations.Read
  );
  const write = connection.getPermission(
    m.Denied,
    m.Denied.DefaultWorkspaceProperty,
    Operations.Write
  );
  expect(result.pool.length).toBeGreaterThan(0);
  for (const id of result.pool) {
    expect(connection.getRecord(id).isPermitted(read)).toBe(true);
    expect(connection.getRecord(id).isPermitted(write)).toBe(false);
  }

  await fixture.denyPermission(m.Denied.DefaultWorkspaceProperty, Operations.Read);

  await connection.pull([pull]);

  for (const id of result.pool) {
    expect(connection.getRecord(id).isPermitted(read)).toBe(false);
  }
});
