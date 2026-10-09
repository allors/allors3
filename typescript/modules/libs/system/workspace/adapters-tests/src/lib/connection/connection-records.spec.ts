import {
  DatabaseConnection,
  Operations,
  Pull,
  RecordChangedEvent,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeTransport } from '../fakes/fake-transport';

// The connection's own records and permissions, over the fake transport: what a pull
// receives, isolation between connections, version guards and completed record events.
describe('DatabaseConnection and its records', () => {
  const workspaceName = 'Default';

  let m: M;
  let transport: FakeTransport;
  let pull: Pull[];

  const createConnection = () =>
    new DatabaseConnection(workspaceName, m, transport);

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

  it('holds records and permissions after a pull', async () => {
    const connection = createConnection();

    const result = await connection.pull(pull);

    expect(result.pool).toEqual([1, 2]);

    const record = connection.getRecord(1);
    expect(record.cls).toBe(m.C1);
    expect(record.getRole(m.C1.C1AllorsString)).toBe('one');
    expect(record.grantIds).toEqual([10]);
    expect(record.revocationIds).toBeUndefined();

    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(100);
    expect(record.isPermitted(100)).toBe(true);
    expect(record.isPermitted(101)).toBe(true);
    expect(record.isPermitted(102)).toBe(false);
    expect(connection.getRecord(3)).toBeUndefined();
  });

  it('keeps records and permissions separate between connections of one user', async () => {
    const first = createConnection();
    const second = createConnection();

    await first.pull(pull);

    expect(second.getRecord(1)).toBeUndefined();
    expect(
      second.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(0);

    await second.pull(pull);

    expect(second.getRecord(1)).not.toBe(first.getRecord(1));
    expect(second.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('one');
    expect(transport.server.syncRequests.length).toBe(2);
    expect(transport.server.accessRequests.length).toBe(2);
    expect(transport.server.permissionRequests.length).toBe(2);

    const grant = transport.server.grants.get(10);
    grant.version++;
    grant.permissions = [100];
    await second.pull(pull);

    expect(first.getRecord(1).isPermitted(101)).toBe(true);
    expect(second.getRecord(1).isPermitted(101)).toBe(false);
  });

  it('raises recordChanged only for the connection whose record is replaced', async () => {
    const first = createConnection();
    const second = createConnection();
    const firstChanges: RecordChangedEvent[] = [];
    const secondChanges: RecordChangedEvent[] = [];
    first.recordChanged.subscribe((e) => firstChanges.push(e));
    second.recordChanged.subscribe((e) => secondChanges.push(e));

    await first.pull(pull);
    await second.pull(pull);
    expect(firstChanges).toEqual([]);
    expect(secondChanges).toEqual([]);

    const object = transport.server.objects.get(1);
    object.version++;
    object.withRole(m.C1.C1AllorsString, 'changed');

    await second.pull(pull);

    expect(firstChanges).toEqual([]);
    expect(secondChanges.length).toBe(1);
    expect(secondChanges[0].id).toBe(1);
    expect(secondChanges[0].record).toBe(second.getRecord(1));
    expect(first.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('one');
    expect(second.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('changed');
  });

  it('raises recordChanged after the new grants and permissions are available', async () => {
    const connection = createConnection();
    await connection.pull(pull);

    const permissionsAtChange: number[] = [];
    const permittedAtChange: boolean[] = [];
    connection.recordChanged.subscribe((e) => {
      permissionsAtChange.push(
        connection.getPermission(m.C1, m.C1.ClassMethod, Operations.Execute)
      );
      permittedAtChange.push(e.record.isPermitted(102));
    });

    transport.server.addPermission(
      102,
      m.C1,
      m.C1.ClassMethod,
      Operations.Execute
    );
    transport.server.addGrant(11, 102);
    const object = transport.server.objects.get(1);
    object.version++;
    object.grants = [11];
    await connection.pull(pull);

    expect(permissionsAtChange).toEqual([102]);
    expect(permittedAtChange).toEqual([true]);
  });

  it('keeps the newer record when an older sync response arrives', async () => {
    const connection = createConnection();
    const object = transport.server.objects.get(1);
    object.version = 2;
    object.withRole(m.C1.C1AllorsString, 'newer');
    await connection.pull(pull);
    const newer = connection.getRecord(1);
    const changes: RecordChangedEvent[] = [];
    connection.recordChanged.subscribe((e) => changes.push(e));

    object.version = 1;
    object.withRole(m.C1.C1AllorsString, 'older');
    await connection.pull(pull);

    expect(transport.server.syncRequests.length).toBe(2);
    expect(connection.getRecord(1)).toBe(newer);
    expect(connection.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('newer');
    expect(changes).toEqual([]);
  });

  it('replaces a record at the same version when its revocations change', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    const before = connection.getRecord(1);
    const permissionsAtChange: boolean[] = [];
    connection.recordChanged.subscribe((e) =>
      permissionsAtChange.push(e.record.isPermitted(101))
    );

    transport.server.addRevocation(20, 101);
    transport.server.objects.get(1).revocations = [20];
    await connection.pull(pull);

    const after = connection.getRecord(1);
    expect(after).not.toBe(before);
    expect(after.version).toBe(before.version);
    expect(after.revocationIds).toEqual([20]);
    expect(after.isPermitted(100)).toBe(true);
    expect(after.isPermitted(101)).toBe(false);
    expect(permissionsAtChange).toEqual([false]);
  });

  it('finds a permission by class, operand type and operation', async () => {
    transport.server.addPermission(
      102,
      m.C1,
      m.C1.ClassMethod,
      Operations.Execute
    );
    transport.server.grants.get(10).permissions = [100, 101, 102];
    const connection = createConnection();
    await connection.pull(pull);

    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(100);
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Write)
    ).toBe(101);
    expect(
      connection.getPermission(m.C1, m.C1.ClassMethod, Operations.Execute)
    ).toBe(102);
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsInteger, Operations.Read)
    ).toBe(0);
    expect(
      connection.getPermission(m.C2, m.C1.C1AllorsString, Operations.Read)
    ).toBe(0);
  });
});
