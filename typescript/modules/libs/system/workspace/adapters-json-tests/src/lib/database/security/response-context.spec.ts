import { data } from '@allors/default/workspace/meta-json';
import { C1 } from '@allors/default/workspace/domain';
import { PullResponse, SyncResponse } from '@allors/system/common/protocol-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { IUnitDiff, Operations } from '@allors/system/workspace/domain';
import { Fixture } from '../../fixture';

function setup() {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const { m } = fixture;
  const client = {
    pull: jest.fn(),
    sync: jest.fn(),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client as IDatabaseJsonClient;
  const session = database.createWorkspace().createSession();
  const definitions = [
    {
      i: 31,
      c: m.C1.tag,
      t: m.C1.C1AllorsString.relationType.tag,
      o: Operations.Read,
    },
    {
      i: 32,
      c: m.C1.tag,
      t: m.C1.C1AllorsString.relationType.tag,
      o: Operations.Write,
    },
    {
      i: 21,
      c: m.C1.tag,
      t: m.C1.C1AllorsInteger.relationType.tag,
      o: Operations.Read,
    },
  ];
  client.access.mockResolvedValue({
    g: [{ i: 11, v: 2, p: [21, 31, 32] }],
    r: [{ i: 21, v: 2, p: [] }],
  });
  client.permission.mockResolvedValue({ p: definitions });
  const response = (
    version = 2,
    grantVersion = 2,
    revocationVersion = 2,
    value = 'baseline',
    denied: number[] = []
  ): SyncResponse =>
    ({
      o: [
        {
          i: 101,
          v: version,
          c: m.C1.tag,
          g: [11],
          r: [21],
          ro: [
            ...(denied.includes(31)
              ? []
              : [{ t: m.C1.C1AllorsString.relationType.tag, v: value }]),
            { t: m.C1.C1AllorsInteger.relationType.tag, v: version },
          ],
        },
      ],
      g: [{ i: 11, v: grantVersion, p: [21, 31, 32] }],
      r: [{ i: 21, v: revocationVersion, p: denied }],
      p: definitions.map((v) => ({ ...v })),
    } as unknown as SyncResponse);
  const select = (sync: SyncResponse) => ({
    p: sync.o.map(({ i, v, g, r }) => ({ i, v, g, r })),
    g: sync.g.map(({ i, v }) => [i, v]),
    r: sync.r.map(({ i, v }) => [i, v]),
    c: { C1s: [101] },
    o: { C1: 101 },
  });
  const enqueue = (sync: SyncResponse) => {
    client.pull.mockResolvedValueOnce(select(sync));
    client.sync.mockResolvedValueOnce(sync);
  };
  return { database, session, client, m, response, select, enqueue };
}

const flush = async () => {
  for (let i = 0; i < 8; ++i) {
    await Promise.resolve();
  }
};

test.each([0, 2])(
  'an unchanged pull does not repeat synchronization or request Access or Permission: security version %s',
  async (securityVersion) => {
    const { session, client, response, select, enqueue } = setup();
    const initial = response(2, securityVersion, securityVersion);
    enqueue(initial);
    await session.pull({ objectId: 101 });
    client.pull.mockResolvedValueOnce(select(initial));
    await session.pull({ objectId: 101 });
    expect(client.sync).toHaveBeenCalledTimes(1);
    expect(client.access).not.toHaveBeenCalled();
    expect(client.permission).not.toHaveBeenCalled();
  }
);

test('security-only changes refresh cached revocations independently of permission IDs', async () => {
  const { session, client, m, response, enqueue } = setup();
  enqueue(response());
  const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
  expect(object.strategy.canWrite(m.C1.C1AllorsString)).toBe(true);
  const denied = response(2, 2, 3, 'baseline', [32]);
  denied.g[0].p = null;
  denied.p = denied.p.filter((v) => v.i === 32);
  enqueue(denied);
  await session.pull({ objectId: 101 });
  expect(client.sync).toHaveBeenLastCalledWith({
    x: undefined,
    o: [101],
    g: [[11, 2]],
    r: [],
  });
  expect(object.strategy.canWrite(m.C1.C1AllorsString)).toBe(false);
  expect(object.C1AllorsInteger).toBe(2);
});

test('edited objects pin data and security and Reset adopts only their own latest received snapshot', async () => {
  const { database, session, client, m, response, enqueue } = setup();
  enqueue(response());
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  edited.C1AllorsString = 'local';
  enqueue(response(3, 2, 3, 'remote', [31, 32]));
  const result = await session.pull({ objectId: 101 });
  expect(result.hasErrors).toBe(false);
  expect(edited.C1AllorsString).toBe('local');
  expect(edited.C1AllorsInteger).toBe(2);
  expect(edited.strategy.canWrite(m.C1.C1AllorsString)).toBe(true);
  const other = database.createWorkspace().createSession();
  enqueue(response(4, 2, 4, 'other session'));
  await other.pull({ objectId: 101 });
  expect(edited.C1AllorsInteger).toBe(2);
  const calls = client.pull.mock.calls.length;
  edited.strategy.reset();
  expect(edited.strategy.version).toBe(3);
  expect(edited.C1AllorsInteger).toBe(3);
  expect(edited.C1AllorsString).toBeNull();
  expect(edited.strategy.canWrite(m.C1.C1AllorsString)).toBe(false);
  edited.C1AllorsString = 'not allowed';
  expect(edited.strategy.hasChanges).toBe(false);
  expect(client.pull).toHaveBeenCalledTimes(calls);
});

test('a disjoint refresh stays pinned and repeated Pull replaces the Reset target', async () => {
  const { session, response, enqueue } = setup();
  enqueue(response());
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  edited.C1AllorsString = 'local';
  enqueue(response(3));
  await session.pull({ objectId: 101 });
  enqueue(response(4));
  await session.pull({ objectId: 101 });
  expect(edited.C1AllorsInteger).toBe(2);
  expect(edited.strategy.version).toBe(2);
  edited.strategy.reset();
  expect(edited.C1AllorsInteger).toBe(4);
  expect(edited.C1AllorsString).toBe('baseline');
  edited.C1AllorsString = 'reapplied';
  expect(edited.C1AllorsString).toBe('reapplied');
  expect((edited.strategy.diff()[0] as IUnitDiff).originalRole).toBe(
    'baseline'
  );
});

test.each([
  'missing tables',
  'unknown object version in Pull',
  'workspace object version in Pull',
  'unknown object version in Sync',
  'workspace object version in Sync',
  'missing definition',
  'unadvertised omission',
  'unknown role',
  'duplicate role',
  'role from another class',
  'unit with object member',
  'to-one with value member',
  'to-many with object member',
  'missing object',
  'duplicate access',
  'bad result',
  'object result outside pool',
  'invalid object result ID',
  'null collection result',
  'non-array collection result',
  'invalid collection dictionary',
  'invalid object dictionary',
  'invalid value dictionary',
  'duplicate result name',
  'unsorted Pull grants',
  'duplicate Pull grants',
  'unsorted Pull revocations',
  'duplicate Pull revocations',
  'unsorted Sync grants',
  'duplicate Sync grants',
  'unsorted Sync revocations',
  'duplicate Sync revocations',
  'unsorted permissions',
  'duplicate permissions',
  'unsorted to-many IDs',
  'duplicate to-many IDs',
  'changed permission identity',
  'duplicate permission identity',
  'changed cached contents',
  'changed cached revocation contents',
  'changed object class',
])('invalid Sync adopts and publishes nothing: %s', async (kind) => {
  const { database, session, client, m, response, select, enqueue } = setup();
  enqueue(response());
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  edited.C1AllorsString = 'local';
  const invalid = response(3, 3, 3, 'remote');
  const pull = select(invalid) as unknown as PullResponse;
  let expectedMessage: string;
  let expectNoSync = false;
  switch (kind) {
    case 'unknown object version in Pull':
      pull.p[0].v = 0;
      break;
    case 'workspace object version in Pull':
      pull.p[0].v = 1;
      break;
    case 'unknown object version in Sync':
      invalid.o[0].v = 0;
      break;
    case 'workspace object version in Sync':
      invalid.o[0].v = 1;
      break;
    case 'missing tables':
      delete invalid.g;
      break;
    case 'missing definition':
      invalid.p = [];
      break;
    case 'unadvertised omission':
      invalid.g[0].p = null;
      break;
    case 'unknown role':
      invalid.o[0].ro[0].t = 'unknown';
      expectedMessage = `Sync object 101 (class '${m.C1.tag}') references unknown role tag 'unknown'. Check that the client and server use matching workspace models; regenerate your workspace if needed.`;
      break;
    case 'duplicate role':
      invalid.o[0].ro.push({ ...invalid.o[0].ro[0] });
      expectedMessage = `Sync object 101 (class '${m.C1.tag}') contains duplicate role tag '${m.C1.C1AllorsString.relationType.tag}'. Return each role only once.`;
      break;
    case 'role from another class':
      invalid.o[0].ro[0].t = m.C2.C2AllorsString.relationType.tag;
      expectedMessage = `Sync object 101 (class '${m.C1.tag}') contains role tag '${m.C2.C2AllorsString.relationType.tag}' that is not a database role of this class.`;
      break;
    case 'unit with object member':
      invalid.o[0].ro[0]['o'] = 101;
      break;
    case 'to-one with value member':
      invalid.o[0].ro.push({
        t: m.C1.C1C1Many2One.relationType.tag,
        v: 101,
      } as any);
      break;
    case 'to-many with object member':
      invalid.o[0].ro.push({
        t: m.C1.C1C1Many2Manies.relationType.tag,
        o: 101,
      } as any);
      break;
    case 'missing object':
      invalid.o = [];
      break;
    case 'duplicate access':
      invalid.g.push({ ...invalid.g[0], v: 4 });
      break;
    case 'bad result':
      pull.c.C1s = [999];
      expectNoSync = true;
      break;
    case 'object result outside pool':
      pull.o.C1 = 999;
      expectNoSync = true;
      break;
    case 'invalid object result ID':
      pull.o.C1 = -1;
      expectNoSync = true;
      break;
    case 'null collection result':
      pull.c.C1s = null;
      expectNoSync = true;
      break;
    case 'non-array collection result':
      pull.c.C1s = {} as any;
      expectNoSync = true;
      break;
    case 'invalid collection dictionary':
      pull.c = [] as any;
      expectNoSync = true;
      break;
    case 'invalid object dictionary':
      pull.o = 101 as any;
      expectNoSync = true;
      break;
    case 'invalid value dictionary':
      pull.v = [] as any;
      expectNoSync = true;
      break;
    case 'duplicate result name':
      pull.o['c1'] = 101;
      expectNoSync = true;
      break;
    case 'unsorted Pull grants':
    case 'duplicate Pull grants':
      pull.p[0].g = kind.startsWith('unsorted') ? [12, 11] : [11, 11];
      pull.g.push([12, 3]);
      expectNoSync = true;
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'unsorted Pull revocations':
    case 'duplicate Pull revocations':
      pull.p[0].r = kind.startsWith('unsorted') ? [22, 21] : [21, 21];
      pull.r.push([22, 3]);
      expectNoSync = true;
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'unsorted Sync grants':
    case 'duplicate Sync grants':
      invalid.o[0].g = kind.startsWith('unsorted') ? [12, 11] : [11, 11];
      invalid.g.push({ i: 12, v: 3, p: [] });
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'unsorted Sync revocations':
    case 'duplicate Sync revocations':
      invalid.o[0].r = kind.startsWith('unsorted') ? [22, 21] : [21, 21];
      invalid.r.push({ i: 22, v: 3, p: [] });
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'unsorted permissions':
    case 'duplicate permissions':
      invalid.g[0].p = kind.startsWith('unsorted') ? [32, 31] : [31, 31];
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'unsorted to-many IDs':
    case 'duplicate to-many IDs':
      invalid.o[0].ro.push({
        t: m.C1.C1C1Many2Manies.relationType.tag,
        c: kind.startsWith('unsorted') ? [103, 102] : [102, 102],
      } as any);
      expectedMessage = 'Response ID ranges must be sorted and distinct.';
      break;
    case 'changed permission identity':
      invalid.p[0].o = Operations.Write;
      expectedMessage = 'Sync changed the definition of permission 31.';
      break;
    case 'duplicate permission identity':
      invalid.p.push({ ...invalid.p[0], i: 99 });
      expectedMessage = `Sync permission 99 conflicts with cached permission 31 for class '${m.C1.tag}', operand '${m.C1.C1AllorsString.relationType.tag}' and operation ${Operations.Read}.`;
      break;
    case 'changed cached contents':
      invalid.g[0].v = 2;
      invalid.g[0].p = [31];
      expectedMessage = 'Sync changed the contents of grant 11 without changing version 2.';
      break;
    case 'changed cached revocation contents':
      invalid.r[0].v = 2;
      invalid.r[0].p = [32];
      expectedMessage = 'Sync changed the contents of revocation 21 without changing version 2.';
      break;
    case 'changed object class':
      invalid.o[0].c = m.C2.tag;
      invalid.o[0].ro = [];
      expectedMessage = `Sync changed the class of object 101 from '${m.C1.tag}' to '${m.C2.tag}'.`;
      break;
  }
  client.pull.mockResolvedValueOnce(pull);
  client.sync.mockResolvedValueOnce(invalid);
  const syncCount = client.sync.mock.calls.length;
  let rejectionMessage: string;
  const rejected = await session.pull({ objectId: 101 }).then(
    () => false,
    (error: Error) => {
      rejectionMessage = error.message;
      return true;
    }
  );
  expect(rejected).toBe(true);
  if (expectNoSync) {
    expect(client.sync).toHaveBeenCalledTimes(syncCount);
  }
  if (expectedMessage) {
    expect(rejectionMessage).toBe(expectedMessage);
  }
  expect(database.getRecord(101).version).toBe(2);
  expect(database.getAccess('g:11:3')).toBeUndefined();
  expect(edited.C1AllorsString).toBe('local');
  edited.strategy.reset();
  expect(edited.strategy.version).toBe(2);
  client.sync.mockReset();
  enqueue(response(4));
  await session.pull({ objectId: 101 });
  expect(edited.strategy.version).toBe(4);
});

test('published payloads and permission arrays cannot be changed through the response buffers', async () => {
  const { session, client, m, response, enqueue } = setup();
  const sync = response();
  enqueue(sync);
  const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
  sync.o[0].ro[0].v = 'mutated';
  sync.g[0].p.length = 0;
  sync.r[0].p.push(32);
  sync.p[0].t = 'unknown';
  expect(object.C1AllorsString).toBe('baseline');
  expect(object.strategy.canWrite(m.C1.C1AllorsString)).toBe(true);
});

test('same-session pulls queue through adoption while other sessions remain concurrent', async () => {
  const { database, session, client, response, select } = setup();
  const first = response();
  const second = response(3);
  let release: (value: SyncResponse) => void;
  client.pull
    .mockResolvedValueOnce(select(first))
    .mockResolvedValueOnce(select(second));
  client.sync
    .mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          release = resolve;
        })
    )
    .mockResolvedValueOnce(second);
  const pending = session.pull({ objectId: 101 });
  const queued = session.pull({ objectId: 101 });
  await flush();
  expect(client.pull).toHaveBeenCalledTimes(1);
  release(first);
  await pending;
  await queued;
  expect(session.instantiate<C1>(101).strategy.version).toBe(3);

  const other = database.createWorkspace().createSession();
  const next = response(4);
  client.pull.mockResolvedValue(select(next));
  let finish: (value: SyncResponse) => void;
  client.sync
    .mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          finish = resolve;
        })
    )
    .mockResolvedValueOnce(next);
  const waiting = session.pull({ objectId: 101 });
  await flush();
  await other.pull({ objectId: 101 });
  expect(session.instantiate<C1>(101).strategy.version).toBe(3);
  finish(next);
  await waiting;
});

test.each(['data', 'grant', 'revocation'])(
  'a delayed cache hit cannot replace another session\'s synchronized snapshot: %s',
  async (change) => {
    const { database, session, client, m, response, select, enqueue } = setup();
    const initial = response();
    enqueue(initial);
    const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
    edited.C1AllorsString = 'local';

    const additional = response(2, 2, 2, 'additional');
    additional.o[0].i = 102;
    const pull = select(initial);
    pull.p.push(...select(additional).p);
    client.pull.mockResolvedValueOnce(pull);
    let release: (value: SyncResponse) => void;
    client.sync.mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          release = resolve;
        })
    );
    const pending = session.pull({ objectId: 101 });
    await flush();
    expect(client.sync).toHaveBeenLastCalledWith({
      x: undefined,
      o: [102],
      g: [[11, 2]],
      r: [[21, 2]],
    });

    const updated =
      change === 'data'
        ? response(3, 2, 2, 'remote')
        : response(
            2,
            change === 'grant' ? 3 : 2,
            change === 'revocation' ? 3 : 2
          );
    if (change === 'grant') {
      updated.g[0].p = [21, 31];
    } else if (change === 'revocation') {
      updated.r[0].p = [32];
    }
    const other = database.createWorkspace().createSession();
    enqueue(updated);
    const refreshed = (await other.pull({ objectId: 101 })).object<C1>('C1');
    const latest = database.getRecord(101);
    expect(refreshed.strategy.version).toBe(updated.o[0].v);
    expect(refreshed.strategy.canWrite(m.C1.C1AllorsString)).toBe(
      change === 'data'
    );

    release(additional);
    const result = await pending;
    expect(result.object<C1>('C1')).toBe(edited);
    expect(edited.C1AllorsString).toBe('local');
    expect(edited.strategy.version).toBe(2);
    expect(edited.strategy.canWrite(m.C1.C1AllorsString)).toBe(true);
    session.reset();
    expect(edited.C1AllorsString).toBe('baseline');
    expect(edited.strategy.version).toBe(2);
    expect(edited.strategy.canWrite(m.C1.C1AllorsString)).toBe(true);
    expect(database.getRecord(102).getRole(m.C1.C1AllorsString)).toBe(
      'additional'
    );
    expect(database.getRecord(101) === latest).toBe(true);

    const observer = database.createWorkspace().createSession();
    const observed = observer.instantiate<C1>(101);
    expect(observed.strategy.version).toBe(updated.o[0].v);
    expect(observed.C1AllorsString).toBe(
      change === 'data' ? 'remote' : 'baseline'
    );
    expect(observed.strategy.canWrite(m.C1.C1AllorsString)).toBe(
      change === 'data'
    );
    const syncCount = client.sync.mock.calls.length;
    client.pull.mockResolvedValueOnce(select(updated));
    expect((await observer.pull({ objectId: 101 })).object<C1>('C1')).toBe(
      observed
    );
    expect(client.sync).toHaveBeenCalledTimes(syncCount);
    expect(database.getRecord(101) === latest).toBe(true);
  }
);

test.each([false, true])(
  'session Reset after Checkpoint adopts the latest snapshot even when the last edit was restored: %s',
  async (restore) => {
    const { session, client, m, response, enqueue } = setup();
    enqueue(response());
    const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
    edited.C1AllorsString = 'local';
    session.checkpoint();
    enqueue(response(3, 2, 3, 'remote', [32]));
    await session.pull({ objectId: 101 });
    session.checkpoint();
    if (restore) {
      edited.strategy.restoreRole(m.C1.C1AllorsString);
      session.checkpoint();
    }
    const calls = client.pull.mock.calls.length;
    session.reset();
    expect(edited.C1AllorsString).toBe('remote');
    expect(edited.strategy.version).toBe(3);
    expect(edited.strategy.hasChanges).toBe(false);
    expect(client.pull).toHaveBeenCalledTimes(calls);
  }
);

test('Push and Invoke wait for an earlier Pull and later Pull uses the acknowledged Push', async () => {
  const { session, client, response, select, enqueue } = setup();
  enqueue(response());
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  edited.C1AllorsString = 'local';
  const remote = response(3);
  let release: (value: SyncResponse) => void;
  client.pull.mockResolvedValueOnce(select(remote));
  client.sync.mockImplementationOnce(
    () =>
      new Promise((resolve) => {
        release = resolve;
      })
  );
  client.push.mockResolvedValue({});
  client.invoke.mockResolvedValue({});
  const pending = session.pull({ objectId: 101 });
  const pushed = session.push();
  const invoked = session.invoke([]);
  await flush();
  expect(client.push).not.toHaveBeenCalled();
  expect(client.invoke).not.toHaveBeenCalled();
  release(remote);
  await pending;
  expect(edited.C1AllorsString).toBe('local');
  await pushed;
  await invoked;
  expect(client.push.mock.calls[0][0].o[0].v).toBe(2);
  enqueue(response(4, 2, 2, 'local'));
  await session.pull({ objectId: 101 });
  expect(edited.strategy.version).toBe(4);
  expect(edited.strategy.hasChanges).toBe(false);
});

test('a Date read cannot mutate a shared immutable snapshot', async () => {
  const { database, session, m, response, select, enqueue } = setup();
  const sync = response();
  sync.p.push({
    i: 33,
    c: m.C1.tag,
    t: m.C1.C1AllorsDateTime.relationType.tag,
    o: Operations.Read,
  });
  sync.g[0].p.push(33);
  sync.o[0].ro.push({
    t: m.C1.C1AllorsDateTime.relationType.tag,
    v: '2020-01-02T03:04:05Z',
  } as any);
  enqueue(sync);
  const first = (await session.pull({ objectId: 101 })).object<C1>('C1');
  first.C1AllorsDateTime.setTime(0);
  const other = database.createWorkspace().createSession();
  // Both sessions select the same cached immutable record.
  (database.client.pull as jest.Mock).mockResolvedValueOnce(select(sync));
  const second = (await other.pull({ objectId: 101 })).object<C1>('C1');
  expect(first.C1AllorsDateTime.toISOString()).toBe('2020-01-02T03:04:05.000Z');
  expect(second.C1AllorsDateTime.toISOString()).toBe(
    '2020-01-02T03:04:05.000Z'
  );
});

test('a concurrent conflicting payload version cannot replace published cache or session snapshots', async () => {
  const { database, session, client, m, response, select, enqueue } = setup();
  enqueue(response());
  const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
  const other = database.createWorkspace().createSession();
  const invalid = response(3, 2, 2, 'inconsistent');
  client.pull.mockResolvedValueOnce(select(invalid));
  let release: (value: SyncResponse) => void;
  client.sync.mockImplementationOnce(
    () =>
      new Promise((resolve) => {
        release = resolve;
      })
  );
  const pending = session.pull({ objectId: 101 });
  await flush();
  enqueue(response(3, 2, 2, 'published'));
  await other.pull({ objectId: 101 });
  release(invalid);
  await expect(pending).rejects.toThrow(
    'Sync changed the contents of object 101 at data version 3 without changing its security versions.'
  );
  expect(database.getRecord(101).getRole(m.C1.C1AllorsString)).toBe(
    'published'
  );
  object.strategy.reset();
  expect(object.strategy.version).toBe(2);
  expect(object.C1AllorsString).toBe('baseline');
});

test('Reset refreshes cached relationships and subsequent checkpoints contain objects', async () => {
  const { session, client, m, response, enqueue } = setup();
  const related = (version: number, target: number) => {
    const sync = response(version);
    const one = m.C1.C1C1Many2One;
    const many = m.C1.C1C1Many2Manies;
    sync.p.push(
      { i: 34, c: m.C1.tag, t: one.relationType.tag, o: Operations.Read },
      { i: 35, c: m.C1.tag, t: one.relationType.tag, o: Operations.Write },
      { i: 36, c: m.C1.tag, t: many.relationType.tag, o: Operations.Read },
      { i: 37, c: m.C1.tag, t: many.relationType.tag, o: Operations.Write }
    );
    sync.g[0].p.push(34, 35, 36, 37);
    sync.o[0].ro.push(
      { t: one.relationType.tag, o: target } as any,
      { t: many.relationType.tag, c: [target] } as any
    );
    sync.o.push(
      ...[102, 103].map((i) => ({
        i,
        v: 2,
        c: m.C1.tag,
        g: [11],
        r: [21],
        ro: [],
      }))
    );
    return sync;
  };
  enqueue(related(2, 102));
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  expect(edited.C1C1Many2Manies.map((v) => v.id)).toEqual([102]);
  edited.C1C1Many2One = session.instantiate<C1>(103);
  edited.addC1C1Many2Many(session.instantiate<C1>(103));
  session.checkpoint();
  const remote = related(3, 103);
  // Other selected objects are already exact cache hits and are not requested by Sync.
  const pull = {
    p: remote.o.map(({ i, v, g, r }) => ({ i, v, g, r })),
    g: [[11, 2]],
    r: [[21, 2]],
    o: { C1: 101 },
  };
  client.pull.mockResolvedValueOnce(pull);
  remote.o = remote.o.filter((v) => v.i === 101);
  client.sync.mockResolvedValueOnce(remote);
  await session.pull({ objectId: 101 });
  edited.strategy.reset();
  expect(edited.C1C1Many2One.id).toBe(103);
  expect(edited.C1C1Many2Manies.map((v) => v.id)).toEqual([103]);
  const changes = session.checkpoint();
  for (const objects of changes.rolesByAssociationType.values()) {
    expect([...objects].every((v) => typeof v.id === 'number')).toBe(true);
  }
  expect(() => session.reset()).not.toThrow();
});

test('Pull owns named result buffers while Sync is in flight', async () => {
  const { session, client, response, select } = setup();
  const sync = response();
  sync.o.push({ ...sync.o[0], i: 102 });
  const pull = {
    ...select(sync),
    c: { C1s: [102, 101, 102] },
    o: { C1: 101, Zero: 0, Null: null },
    v: { Numbers: [1, 2] },
  };
  client.pull.mockResolvedValueOnce(pull);
  let release: (value: SyncResponse) => void;
  client.sync.mockImplementationOnce(
    () =>
      new Promise((resolve) => {
        release = resolve;
      })
  );
  const pending = session.pull({ objectId: 101 });
  await flush();
  pull.c.C1s[0] = 999;
  pull.o.C1 = 999;
  pull.v.Numbers.push(3);
  release(sync);
  const result = await pending;
  expect(result.object<C1>('C1').id).toBe(101);
  expect(result.object('Zero')).toBeNull();
  expect(result.object('Null')).toBeNull();
  expect(result.collection<C1>('C1s').map((object) => object.id)).toEqual([
    102,
    101,
    102,
  ]);
  expect(result.value('Numbers')).toEqual([1, 2]);
});

test.each(
  [
    [false, false],
    [true, false],
    [false, true],
    [true, true],
  ].flatMap(([many, reset]) =>
    ['uncached', 'cached', 'loaded'].map(
      (targets) => [many, reset, targets] as const
    )
  )
)(
  'Checkpoint reports relation changes without loading targets: many=%s, reset=%s, targets=%s',
  async (many, reset, targets) => {
    const { database, session, client, m, response, select, enqueue } = setup();
    const role = many ? m.C1.C1C1Many2Manies : m.C1.C1C1Many2One;
    const related = (version: number, target: number) => {
      const sync = response(version);
      sync.p.push({
        i: 34,
        c: m.C1.tag,
        t: role.relationType.tag,
        o: Operations.Read,
      });
      sync.g[0].p.push(34);
      sync.o[0].ro.push({
        t: role.relationType.tag,
        ...(many ? { c: [target] } : { o: target }),
      } as any);
      return sync;
    };
    enqueue(related(2, 102));
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    if (reset) {
      object.C1AllorsString = 'local';
    }
    session.checkpoint();
    if (targets !== 'uncached') {
      const targetResponse = related(2, 102);
      targetResponse.o = [102, 103].map((i) => ({
        ...targetResponse.o[0],
        i,
        ro: [],
      }));
      const targetPull = {
        ...select(targetResponse),
        c: { C1s: [102, 103] },
        o: { C1: 102 },
      };
      const other = database.createWorkspace().createSession();
      client.pull.mockResolvedValueOnce(targetPull);
      client.sync.mockResolvedValueOnce(targetResponse);
      await other.pull({ objectId: 102 });
      if (targets === 'loaded') {
        client.pull.mockResolvedValueOnce(targetPull);
        await session.pull({ objectId: 102 });
      }
    }
    const loadedIds = targets === 'loaded' ? [101, 102, 103] : [101];
    expect(session.instantiate<C1>(m.C1).map((v) => v.id).sort()).toEqual(
      loadedIds
    );
    enqueue(related(3, 103));
    await session.pull({ objectId: 101 });
    if (reset) {
      expect(object.strategy.version).toBe(2);
      expect(object.C1AllorsString).toBe('local');
      object.strategy.reset();
    }
    const changes = session.checkpoint();
    expect(session.instantiate<C1>(m.C1).map((v) => v.id).sort()).toEqual(
      loadedIds
    );
    expect(changes.associationsByRoleType.get(role)?.has(object)).toBe(true);
    if (targets === 'loaded') {
      expect(changes.rolesByAssociationType.size).toBe(1);
      expect(
        [...changes.rolesByAssociationType.get(role.associationType)]
          .map((v) => v.id)
          .sort()
      ).toEqual([102, 103]);
    } else {
      expect(changes.rolesByAssociationType.size).toBe(0);
    }
  }
);
