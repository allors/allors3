import {
  DatabaseConnection,
  metaFingerprint,
  metaPopulationFingerprint,
  Operations,
  Pull,
} from '@allors/system/workspace/connection';
import { Request } from '@allors/system/common/protocol-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeTransport } from '../fakes/fake-transport';

// The envelope at the client: the connection sends its workspace name and meta fingerprint
// with every request, learns the database and the user from the first response, faults
// when either changes, and refuses a response for another workspace or another meta.
describe('DatabaseConnection and the envelope', () => {
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
    server.addGrant(10, 100);
    server.addObject(1, m.C1, 10).withRole(m.C1.C1AllorsString, 'one');

    pull = [{ extent: { kind: 'Filter', objectType: m.C1 } }];
  });

  it('fingerprints the sorted tags of the meta population', () => {
    const connection = createConnection();

    const tags = [
      ...[...m.composites].map((v) => v.tag),
      ...[...m.relationTypes].map((v) => v.tag),
      ...[...m.methodTypes].map((v) => v.tag),
    ];

    expect(connection.metaFingerprint).toBe(metaFingerprint(tags));
    expect(connection.metaFingerprint).toBe(metaPopulationFingerprint(m));
    expect(connection.metaFingerprint).toBe(
      metaPopulationFingerprint(new LazyMetaPopulation(data))
    );
  });

  it('sends the workspace name and the fingerprint with every request', async () => {
    const connection = createConnection();

    await connection.pull(pull);
    await connection.push(null, null);
    await connection.invoke([]);

    const server = transport.server;
    const requests: Request[] = [
      ...server.pullRequests,
      ...server.syncRequests,
      ...server.accessRequests,
      ...server.permissionRequests,
      ...server.pushRequests,
      ...server.invokeRequests,
    ];
    expect(requests.length).toBe(6);

    for (const request of requests) {
      expect(request._w).toBe(workspaceName);
      expect(request._f).toBe(connection.metaFingerprint);
    }
  });

  it('learns the database and the user from the first response', async () => {
    const connection = createConnection();

    expect(connection.databaseId).toBeNull();
    expect(connection.userId).toBeNull();

    await connection.pull(pull);

    expect(connection.databaseId).toBe('fake');
    expect(connection.userId).toBe(1);
  });

  it('faults when the user changes', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    const record = connection.getRecord(1);
    expect(record).toBeDefined();
    expect(record.isPermitted(100)).toBe(true);

    transport.server.userId = 2;

    await expect(connection.pull(pull)).rejects.toThrow(/user 1.*user 2/);

    expect(connection.getRecord(1)).toBeUndefined();
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(0);
    expect(record.isPermitted(100)).toBe(false);
    expect(connection.userId).toBe(1);

    // The connection stays faulted, whatever the server answers next.
    transport.server.userId = 1;
    await expect(connection.pull(pull)).rejects.toThrow(/user 2/);
    await expect(connection.push(null, null)).rejects.toThrow();
    await expect(connection.invoke([])).rejects.toThrow();
    expect(transport.server.pullRequests.length).toBe(2);
    expect(transport.server.pushRequests).toEqual([]);
    expect(transport.server.invokeRequests).toEqual([]);
  });

  it('faults when the database changes', async () => {
    const connection = createConnection();
    await connection.pull(pull);

    transport.server.databaseId = 'other';

    await expect(connection.pull(pull)).rejects.toThrow(
      /'fake'.*'other'|'other'.*'fake'/
    );
    expect(connection.getRecord(1)).toBeUndefined();
  });

  it('refuses a response for another workspace name before anything is stored', async () => {
    transport.server.workspaceName = 'Other';
    const connection = createConnection();

    await expect(connection.pull(pull)).rejects.toThrow(
      /'Other'.*'Default'|'Default'.*'Other'/
    );

    expect(connection.getRecord(1)).toBeUndefined();
    expect(connection.databaseId).toBeNull();
    expect(transport.server.syncRequests).toEqual([]);

    // Not a fault: the same connection serves once the server serves its workspace.
    transport.server.workspaceName = workspaceName;
    await connection.pull(pull);
    expect(connection.getRecord(1)).toBeDefined();
  });

  it('refuses a response with another fingerprint before anything is stored', async () => {
    transport.server.metaFingerprint = '0000000000000000';
    const connection = createConnection();

    await expect(connection.pull(pull)).rejects.toThrow(
      new RegExp(
        `0000000000000000.*${connection.metaFingerprint}|${connection.metaFingerprint}.*0000000000000000`
      )
    );

    expect(connection.getRecord(1)).toBeUndefined();
    expect(transport.server.syncRequests).toEqual([]);
  });

  it('refuses a response without the envelope', async () => {
    transport.server.databaseId = null;
    const connection = createConnection();

    await expect(connection.pull(pull)).rejects.toThrow(/database/);
    expect(connection.getRecord(1)).toBeUndefined();
  });

  it('keeps the views of separate users in separate connections', async () => {
    const first = createConnection();
    await first.pull(pull);

    transport.server.userId = 2;
    transport.server.objects.get(1).withRole(m.C1.C1AllorsString, 'user two');
    transport.server.grants.get(10).permissions = [];
    const second = createConnection();
    await second.pull(pull);

    expect(first.userId).toBe(1);
    expect(second.userId).toBe(2);
    expect(first.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('one');
    expect(second.getRecord(1).getRole(m.C1.C1AllorsString)).toBe('user two');
    expect(first.getRecord(1).isPermitted(100)).toBe(true);
    expect(second.getRecord(1).isPermitted(100)).toBe(false);
    expect(transport.server.syncRequests.length).toBe(2);
  });
});
