import {
  DatabaseConnection,
  Operations,
  Pull,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeTransport } from '../fakes/fake-transport';

// A pull advertises the version of every grant and revocation of the objects it answers;
// the connection requests the ones it holds at another version again, even when no object
// changed, so that a changed permission set reaches the client.
describe('DatabaseConnection and the versions of grants and revocations', () => {
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
    server.addPermission(102, m.C1, m.C1.C1AllorsInteger, Operations.Read);
    server.addGrant(10, 100, 101);
    server.addRevocation(20, 101);
    server.addObject(1, m.C1, 10).withRole(m.C1.C1AllorsString, 'one');
    server.addObject(2, m.C1, 10).withRole(m.C1.C1AllorsString, 'two').revocations = [20];

    pull = [{ extent: { kind: 'Filter', objectType: m.C1 } }];
  });

  it('does not request an unchanged grant again', async () => {
    const connection = createConnection();

    await connection.pull(pull);
    await connection.pull(pull);

    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(1);
    expect(transport.server.permissionRequests.length).toBe(1);
  });

  it('requests a grant whose version changed again', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    expect(connection.getRecord(1).isPermitted(101)).toBe(true);

    const grant = transport.server.grants.get(10);
    grant.version++;
    grant.permissions = [100];

    await connection.pull(pull);

    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(2);
    expect(transport.server.accessRequests[1].g).toEqual([10]);
    expect(connection.cache.getGrant(10).version).toBe(2);
    expect(connection.getRecord(1).isPermitted(100)).toBe(true);
    expect(connection.getRecord(1).isPermitted(101)).toBe(false);
  });

  it('requests a revocation whose version changed again', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    expect(connection.getRecord(2).isPermitted(100)).toBe(true);
    expect(connection.getRecord(2).isPermitted(101)).toBe(false);

    const revocation = transport.server.revocations.get(20);
    revocation.version++;
    revocation.permissions = [100, 101];

    await connection.pull(pull);

    expect(transport.server.syncRequests.length).toBe(1);
    expect(transport.server.accessRequests.length).toBe(2);
    expect(transport.server.accessRequests[1].r).toEqual([20]);
    expect(connection.getRecord(2).isPermitted(100)).toBe(false);
    expect(connection.getRecord(1).isPermitted(100)).toBe(true);
  });

  it('requests the permissions a changed grant names for the first time', async () => {
    const connection = createConnection();
    await connection.pull(pull);
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsInteger, Operations.Read)
    ).toBe(0);

    const grant = transport.server.grants.get(10);
    grant.version++;
    grant.permissions = [100, 101, 102];

    await connection.pull(pull);

    expect(transport.server.permissionRequests.length).toBe(2);
    expect(transport.server.permissionRequests[1].p).toEqual([102]);
    expect(
      connection.getPermission(m.C1, m.C1.C1AllorsInteger, Operations.Read)
    ).toBe(102);
    expect(connection.getRecord(1).isPermitted(102)).toBe(true);
  });

  it('requests a changed grant once for all the objects that name it', async () => {
    const connection = createConnection();
    await connection.pull(pull);

    const grant = transport.server.grants.get(10);
    grant.version++;

    await connection.pull(pull);

    const request =
      transport.server.accessRequests[transport.server.accessRequests.length - 1];
    expect(request.g).toEqual([10]);
    expect(request.r == null || request.r.length === 0).toBe(true);
  });
});
