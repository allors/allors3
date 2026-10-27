import { data } from '@allors/default/workspace/meta-json';
import { C1 } from '@allors/default/workspace/domain';
import { SyncResponse } from '@allors/system/common/protocol-json';
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

test('an unchanged pull does not repeat synchronization or request Access or Permission', async () => {
  const { session, client, response, select, enqueue } = setup();
  const initial = response();
  enqueue(initial);
  await session.pull({ objectId: 101 });
  client.pull.mockResolvedValueOnce(select(initial));
  await session.pull({ objectId: 101 });
  expect(client.sync).toHaveBeenCalledTimes(1);
  expect(client.access).not.toHaveBeenCalled();
  expect(client.permission).not.toHaveBeenCalled();
});

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
  'missing definition',
  'unadvertised omission',
  'unknown role',
  'missing object',
  'duplicate access',
  'bad result',
  'duplicate result name',
  'changed permission identity',
  'duplicate permission identity',
  'changed cached contents',
])('invalid Sync adopts and publishes nothing: %s', async (kind) => {
  const { database, session, client, m, response, select, enqueue } = setup();
  enqueue(response());
  const edited = (await session.pull({ objectId: 101 })).object<C1>('C1');
  edited.C1AllorsString = 'local';
  const invalid = response(3, 3, 3, 'remote');
  const pull = select(invalid);
  switch (kind) {
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
      break;
    case 'missing object':
      invalid.o = [];
      break;
    case 'duplicate access':
      invalid.g.push({ ...invalid.g[0], v: 4 });
      break;
    case 'bad result':
      pull.c.C1s = [999];
      break;
    case 'duplicate result name':
      pull.o['c1'] = 101;
      break;
    case 'changed permission identity':
      invalid.p[0].o = Operations.Write;
      break;
    case 'duplicate permission identity':
      invalid.p.push({ ...invalid.p[0], i: 99 });
      break;
    case 'changed cached contents':
      invalid.g[0].v = 2;
      invalid.g[0].p = [31];
      break;
  }
  client.pull.mockResolvedValueOnce(pull);
  client.sync.mockResolvedValueOnce(invalid);
  const rejected = await session.pull({ objectId: 101 }).then(
    () => false,
    () => true
  );
  expect(rejected).toBe(true);
  expect(database.getRecord(101).version).toBe(2);
  expect(edited.C1AllorsString).toBe('local');
  edited.strategy.reset();
  expect(edited.strategy.version).toBe(2);
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
  expect(
    await pending.then(
      () => false,
      () => true
    )
  ).toBe(true);
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
  const pull = { ...select(sync), v: { Numbers: [1, 2] } };
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
  expect(result.collection<C1>('C1s').map((object) => object.id)).toEqual([
    101,
  ]);
  expect(result.value('Numbers')).toEqual([1, 2]);
});

test('Checkpoint reports a to-one change even when neither target is loaded', async () => {
  const { session, m, response, enqueue } = setup();
  const related = (version: number, target: number) => {
    const sync = response(version);
    sync.p.push({
      i: 34,
      c: m.C1.tag,
      t: m.C1.C1C1Many2One.relationType.tag,
      o: Operations.Read,
    });
    sync.g[0].p.push(34);
    sync.o[0].ro.push({
      t: m.C1.C1C1Many2One.relationType.tag,
      o: target,
    } as any);
    return sync;
  };
  enqueue(related(2, 102));
  const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
  session.checkpoint();
  enqueue(related(3, 103));
  await session.pull({ objectId: 101 });
  const changes = session.checkpoint();
  expect(
    changes.associationsByRoleType.get(m.C1.C1C1Many2One)?.has(object)
  ).toBe(true);
});
