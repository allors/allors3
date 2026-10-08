import {
  DatabaseConnection,
  MemoryCache,
  Operations,
  Pull,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeTransport } from '../fakes/fake-transport';
import { MemoryPersistenceProvider } from '../fakes/memory-persistence-provider';

// The persistence provider behind the cache: what a pull stores, what a connection with an
// empty cache restores instead of asking the server, and what it still asks the server
// because the persisted entry is not the one the pull advertises.
describe('DatabaseConnection and its persistence provider', () => {
  const workspaceName = 'Default';

  let m: M;
  let transport: FakeTransport;
  let provider: MemoryPersistenceProvider;
  let pull: Pull[];

  const createConnection = () =>
    new DatabaseConnection(workspaceName, m, transport, {
      cache: new MemoryCache(workspaceName, m),
      persistence: provider,
    });

  beforeEach(() => {
    m = new LazyMetaPopulation(data) as unknown as M;
    transport = new FakeTransport(m);
    provider = new MemoryPersistenceProvider();

    const server = transport.server;
    server.addPermission(100, m.C1, m.C1.C1AllorsString, Operations.Read);
    server.addPermission(101, m.C1, m.C1.C1AllorsString, Operations.Write);
    server.addPermission(102, m.C1, m.C1.C1AllorsInteger, Operations.Read);
    server.addGrant(10, 100, 101);
    server.addRevocation(20, 101);
    server
      .addObject(1, m.C1, 10)
      .withRole(m.C1.C1AllorsString, 'one')
      .withRole(m.C1.C1C1Many2Manies, [2]);
    server.addObject(2, m.C1, 10).withRole(m.C1.C1AllorsString, 'two').revocations = [20];

    pull = [{ extent: { kind: 'Filter', objectType: m.C1 } }];
  });

  it('stores what the server sent under the key of the connection after a pull', async () => {
    const connection = createConnection();

    await connection.pull(pull);

    const key = connection.cache.key;
    expect(provider.keys.length).toBe(1);
    expect(provider.keys[0].equals(key)).toBe(true);
    expect(provider.storeCount).toBe(1);

    const objects = provider.objects(key);
    expect([...objects.keys()].sort()).toEqual([1, 2]);
    expect(objects.get(1).c).toBe(m.C1.tag);
    expect(objects.get(1).v).toBe(1);
    expect(objects.get(1).g).toEqual([10]);
    expect(
      objects
        .get(1)
        .ro.some(
          (v) => v.t === m.C1.C1AllorsString.relationType.tag && v.v === 'one'
        )
    ).toBe(true);
    expect(objects.get(2).r).toEqual([20]);

    expect([...provider.grants(key).keys()]).toEqual([10]);
    expect(provider.grants(key).get(10).p).toEqual([100, 101]);
    expect([...provider.revocations(key).keys()]).toEqual([20]);
    expect([...provider.permissions(key).keys()].sort()).toEqual([100, 101]);
  });

  it('restores from the provider instead of asking the server when its cache is empty', async () => {
    const first = createConnection();
    await first.pull(pull);

    const second = createConnection();
    const result = await second.pull(pull);

    expect(result.pool).toEqual([1, 2]);
    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(1);
    expect(transport.server.permissionRequests.length).toBe(1);
    expect(provider.storeCount).toBe(1);

    const record = second.getRecord(1);
    expect(record.cls).toBe(m.C1);
    expect(record.version).toBe(1);
    expect(record.getRole(m.C1.C1AllorsString)).toBe('one');
    expect(record.getRole(m.C1.C1C1Many2Manies)).toEqual([2]);
    expect(record.grantIds).toEqual([10]);
    expect(record.isPermitted(100)).toBe(true);
    expect(record.isPermitted(101)).toBe(true);
    expect(second.getRecord(2).isPermitted(101)).toBe(false);
    expect(
      second.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(100);

    // What memory holds is not asked of the provider again.
    const loads = provider.loadCount;
    await second.pull(pull);
    expect(provider.loadCount).toBe(loads);
  });

  it('restores a persisted object only at the version and access the pull advertises', async () => {
    const first = createConnection();
    await first.pull(pull);

    const object = transport.server.objects.get(1);
    object.version++;
    object.withRole(m.C1.C1AllorsString, 'changed');

    const second = createConnection();
    await second.pull(pull);

    expect(transport.server.syncRequests.length).toBe(2);
    expect(transport.server.syncRequests[1].o).toEqual([1]);
    expect(second.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('changed');
    expect(second.getRecord(2).getRole(m.C1.C1AllorsString)).toBe('two');
    expect(transport.server.accessRequests.length).toBe(1);

    // The provider holds the new version now.
    expect(provider.objects(second.cache.key).get(1).v).toBe(2);

    const third = createConnection();
    await third.pull(pull);
    expect(transport.server.syncRequests.length).toBe(2);
  });

  it('restores a persisted grant only at the version the pull advertises', async () => {
    const first = createConnection();
    await first.pull(pull);

    const grant = transport.server.grants.get(10);
    grant.version++;
    grant.permissions = [100, 102];

    const second = createConnection();
    await second.pull(pull);

    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(2);
    expect(transport.server.accessRequests[1].g).toEqual([10]);
    expect(transport.server.permissionRequests.length).toBe(2);
    expect(transport.server.permissionRequests[1].p).toEqual([102]);

    expect(second.getRecord(1).isPermitted(100)).toBe(true);
    expect(second.getRecord(1).isPermitted(101)).toBe(false);
    expect(second.getRecord(1).isPermitted(102)).toBe(true);
    expect(provider.grants(second.cache.key).get(10).v).toBe(2);
    expect(provider.permissions(second.cache.key).has(102)).toBe(true);
  });

  it('forgets the cache and the persisted view on clear', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    const key = connection.cache.key;

    await connection.clear();

    expect(provider.clearCount).toBe(1);
    expect(provider.keys).toEqual([]);
    expect(connection.getRecord(1)).toBeUndefined();
    expect(connection.cache.key).toBeNull();

    await connection.pull(pull);
    expect(transport.server.syncRequests.length).toBe(2);
    expect(connection.cache.key.equals(key)).toBe(true);
  });

  it('forgets the persisted view on a fault too', async () => {
    const connection = createConnection();
    await connection.pull(pull);

    transport.server.userId = 2;
    await expect(connection.pull(pull)).rejects.toThrow();

    expect(provider.clearCount).toBe(1);
    expect(provider.keys).toEqual([]);
  });

  it('works as before without a provider', async () => {
    const connection = new DatabaseConnection(workspaceName, m, transport);

    await connection.pull(pull);
    await connection.clear();

    expect(provider.keys).toEqual([]);
    expect(connection.getRecord(1)).toBeUndefined();
  });
});
