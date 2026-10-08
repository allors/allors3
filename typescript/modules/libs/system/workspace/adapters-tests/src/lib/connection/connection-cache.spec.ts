import {
  DatabaseConnection,
  ICache,
  MemoryCache,
  Operations,
  Pull,
  RecordChangedEvent,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeRecord } from '../fakes/fake-record';
import { FakeTransport } from '../fakes/fake-transport';

// The connection and its cache, over the fake transport: what a pull leaves in the cache,
// the connections of one user sharing a cache, and the checks a shared cache makes.
describe('DatabaseConnection and its cache', () => {
  const workspaceName = 'Default';

  let m: M;
  let transport: FakeTransport;
  let pull: Pull[];

  const createConnection = (cache?: ICache) =>
    new DatabaseConnection(workspaceName, m, transport, { cache });

  beforeEach(() => {
    m = new LazyMetaPopulation(data) as unknown as M;
    transport = new FakeTransport(m);

    const server = transport.server;
    server.addPermission(100, m.C1, m.C1.C1AllorsString, Operations.Read);
    server.addPermission(101, m.C1, m.C1.C1AllorsString, Operations.Write);
    server.addGrant(10, 100, 101);
    server.addObject(1, m.C1, 10).withRole(m.C1.C1AllorsString, 'one');
    server.addObject(2, m.C1, 10).withRole(m.C1.C1AllorsString, 'two');

    pull = [{ extent: { kind: 'Filter', objectType: m.C1 } }];
  });

  it('leaves records, grants and permissions in the cache after a pull', async () => {
    const connection = createConnection();

    const result = await connection.pull(pull);

    expect(result.pool).toEqual([1, 2]);

    const record = connection.cache.getRecord(1);
    expect(connection.getRecord(1)).toBe(record);
    expect(record.cls).toBe(m.C1);
    expect(record.getRole(m.C1.C1AllorsString)).toBe('one');
    expect(record.grantIds).toEqual([10]);
    expect(record.revocationIds).toBeUndefined();

    expect(connection.cache.getGrant(10).id).toBe(10);
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(100);
    expect(record.isPermitted(100)).toBe(true);
    expect(record.isPermitted(101)).toBe(true);
    expect(record.isPermitted(102)).toBe(false);
  });

  it('has a memory cache of its own without a cache', () => {
    const connection = createConnection();
    const other = createConnection();

    expect(connection.cache).toBeInstanceOf(MemoryCache);
    expect(connection.cache).not.toBe(other.cache);
    expect(connection.cache.workspaceName).toBe(workspaceName);
    expect(connection.cache.metaPopulation).toBe(m);
  });

  it('shares a cache between the connections of one user', async () => {
    const cache = new MemoryCache(workspaceName, m);
    const first = createConnection(cache);
    const second = createConnection(cache);

    await first.pull(pull);

    expect(second.cache).toBe(cache);
    expect(second.getRecord(1)).toBe(first.getRecord(1));
    expect(transport.server.syncRequests.length).toBe(1);

    await second.pull(pull);

    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(1);
    expect(transport.server.permissionRequests.length).toBe(1);
  });

  it('tells every connection through the cache when a record is replaced', async () => {
    const cache = new MemoryCache(workspaceName, m);
    const first = createConnection(cache);
    const second = createConnection(cache);

    await first.pull(pull);

    const cacheChanges: RecordChangedEvent[] = [];
    const firstChanges: RecordChangedEvent[] = [];
    const secondChanges: RecordChangedEvent[] = [];
    cache.recordChanged.subscribe((e) => cacheChanges.push(e));
    first.recordChanged.subscribe((e) => firstChanges.push(e));
    second.recordChanged.subscribe((e) => secondChanges.push(e));

    const object = transport.server.objects.get(1);
    object.version++;
    object.withRole(m.C1.C1AllorsString, 'changed');

    await second.pull(pull);

    expect(cacheChanges.length).toBe(1);
    expect(cacheChanges[0].id).toBe(1);
    expect(cacheChanges[0].record).toBe(first.getRecord(1));
    expect(first.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('changed');

    expect(firstChanges).toEqual([]);
    expect(secondChanges.length).toBe(1);
  });

  it('keeps the newer record when an older one arrives from another connection', async () => {
    const cache = new MemoryCache(workspaceName, m);
    const first = createConnection(cache);
    const second = createConnection(cache);

    await first.pull(pull);

    const object = transport.server.objects.get(1);
    object.version++;
    object.withRole(m.C1.C1AllorsString, 'newer');
    await second.pull(pull);
    const newer = cache.getRecord(1);

    // A sync response built before the change arrives late: the cache keeps the newer record.
    expect(cache.setRecord(new FakeRecord(m.C1, 1, newer.version - 1))).toBe(false);

    expect(cache.getRecord(1)).toBe(newer);
    expect(first.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('newer');
  });

  it('refuses a cache of another workspace name', () => {
    const cache = new MemoryCache('Other', m);

    expect(() => createConnection(cache)).toThrow(/Other.*Default|Default.*Other/);
  });

  it('refuses a cache of another meta population', () => {
    const cache = new MemoryCache(
      workspaceName,
      new LazyMetaPopulation(data) as unknown as M
    );

    expect(() => createConnection(cache)).toThrow(/meta population/);
  });
});
