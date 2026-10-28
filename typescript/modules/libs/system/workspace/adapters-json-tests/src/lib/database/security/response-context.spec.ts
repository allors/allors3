import { data } from '@allors/default/workspace/meta-json';
import { WorkspaceInitialVersion } from '@allors/system/workspace/adapters';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { Fixture } from '../../fixture';

test('revocation refresh includes cached ids and ignores permission-id collisions', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = {
    i: 101,
    v: 2,
    c: fixture.m.C1.tag,
    g: [11],
    r: [21],
    ro: [],
  };
  const definition = {
    i: 21,
    c: fixture.m.C1.tag,
    t: fixture.m.C1.Name.relationType.tag,
    o: 2,
  };
  const client: IDatabaseJsonClient = {
    pull: jest
      .fn()
      .mockResolvedValueOnce({ p: [object], g: [[11, 1]], r: [[21, 1]] })
      .mockResolvedValue({ p: [object], g: [[11, 1]], r: [[21, 2]] }),
    sync: jest
      .fn()
      .mockResolvedValueOnce({
        result: { o: { selected: 101 } },
        o: [object],
        g: [{ i: 11, v: 1, p: [21] }],
        r: [{ i: 21, v: 1, p: [] }],
        p: [definition],
      })
      .mockResolvedValue({
        result: { o: { selected: 101 } },
        o: [object],
        g: [{ i: 11, v: 1, p: null }],
        r: [{ i: 21, v: 2, p: [21] }],
        p: [definition],
      }),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();
  const selected = (await session.pull({ objectId: 101 })).object('selected');
  expect(selected.strategy.canRead(fixture.m.C1.Name)).toBe(true);
  await session.pull({ objectId: 101 });
  expect(selected.strategy.canRead(fixture.m.C1.Name)).toBe(false);
  expect((client.sync as jest.Mock).mock.calls[1][0]).toEqual(
    expect.objectContaining({ o: [], g: [[11, 1]], r: [] })
  );
  expect(client.access).not.toHaveBeenCalled();
});

test('synchronization shares complete security entries and permission definitions across objects', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = {
    i: 101,
    v: 2,
    c: fixture.m.C1.tag,
    g: [11, 12],
    r: [21],
    ro: [],
  };
  const definition = {
    i: 31,
    c: fixture.m.C1.tag,
    t: fixture.m.C1.Name.relationType.tag,
    o: 2,
  };
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({
      p: [object],
      g: [
        [11, 1],
        [12, 1],
      ],
      r: [[21, 1]],
    }),
    sync: jest.fn().mockResolvedValue({
      result: { c: { selected: [101, 102] } },
      o: [object, { ...object, i: 102 }],
      g: [
        { i: 11, v: 1, p: [31, 32] },
        { i: 12, v: 1, p: [31, 32] },
      ],
      r: [{ i: 21, v: 1, p: [] }],
      p: [definition, { i: 32, c: fixture.m.C1.tag, t: null, o: 1 }],
    }),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();
  const result = await session.pull({ objectId: 101 });
  expect(
    result
      .collection('selected')
      .map((v) => v.strategy.canRead(fixture.m.C1.Name))
  ).toEqual([true, true]);
  expect(() => database.getPermission(fixture.m.C1, null, 1)).toThrow(
    'Create is not supported'
  );
  expect(client.access).not.toHaveBeenCalled();
  expect(client.permission).not.toHaveBeenCalled();
});

test.each([0, 1])(
  'an unchanged pull does not repeat synchronization or access requests: security version %s',
  async (securityVersion) => {
    const fixture = new Fixture();
    fixture.metaPopulation = new LazyMetaPopulation(data);
    const database = fixture.createDatabaseConnection();
    const object: SyncResponseObject = {
      i: 101,
      v: 2,
      c: fixture.m.C1.tag,
      g: [11],
      r: [21],
      ro: [],
    };
    const client: IDatabaseJsonClient = {
      pull: jest.fn().mockResolvedValue({
        p: [object],
        g: [[11, securityVersion]],
        r: [[21, securityVersion]],
      }),
      sync: jest.fn().mockResolvedValue({
        result: {},
        o: [object],
        g: [{ i: 11, v: securityVersion, p: [] }],
        r: [{ i: 21, v: securityVersion, p: [] }],
        p: [],
      }),
      access: jest.fn().mockResolvedValue({
        g: [{ i: 11, v: securityVersion, p: [] }],
        r: [{ i: 21, v: securityVersion, p: [] }],
      }),
      permission: jest.fn(),
      push: jest.fn(),
      invoke: jest.fn(),
    };
    database.client = client;
    const session = database.createWorkspace().createSession();

    await session.pull({ objectId: object.i });
    await session.pull({ objectId: object.i });

    expect(client.pull).toHaveBeenCalledTimes(2);
    expect(client.sync).toHaveBeenCalledTimes(1);
    expect(client.access).not.toHaveBeenCalled();
    expect(client.permission).not.toHaveBeenCalled();
  }
);

test('an invalid final object does not publish earlier contents or adopt session objects', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = { i: 101, v: 2, c: fixture.m.C1.tag, g: [], r: [], ro: [] };
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({ p: [object], g: [], r: [] }),
    sync: jest.fn().mockResolvedValue({
      result: { o: { selected: 101 } },
      o: [object, { ...object, i: 102, ro: null }],
      g: [],
      r: [],
      p: [],
    }),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();

  await expect(session.pull({ objectId: 101 })).rejects.toThrow();

  expect(database.getRecord(101)).toBeUndefined();
  expect(database.getRecord(102)).toBeUndefined();
  expect(session.instantiate(101)).toBeNull();
  expect(client.access).not.toHaveBeenCalled();
});

test.each(['data', 'security'])(
  'a %s change requires fresh contents even with a cached object',
  async (change) => {
    const fixture = new Fixture();
    fixture.metaPopulation = new LazyMetaPopulation(data);
    const database = fixture.createDatabaseConnection();
    const role = fixture.m.C1.Name.relationType.tag;
    const definition = { i: 31, c: fixture.m.C1.tag, t: role, o: 2 };
    const first = {
      i: 101,
      v: 2,
      c: fixture.m.C1.tag,
      g: [11],
      r: [],
      ro: [{ t: role, v: 'first' }],
    };
    const changed = { ...first, v: change === 'data' ? 3 : 2, ro: [] };
    const client: IDatabaseJsonClient = {
      pull: jest
        .fn()
        .mockResolvedValueOnce({ p: [first], g: [[11, 1]], r: [] })
        .mockResolvedValue({
          p: [changed],
          g: [[11, change === 'security' ? 2 : 1]],
          r: [],
        }),
      sync: jest
        .fn()
        .mockResolvedValueOnce({
          result: { o: { selected: 101 } },
          o: [first],
          g: [{ i: 11, v: 1, p: [31] }],
          r: [],
          p: [definition],
        })
        .mockResolvedValue({
          result: { o: { selected: 101 } },
          o: [changed],
          g: [{ i: 11, v: change === 'security' ? 2 : 1, p: [31] }],
          r: [],
          p: [definition],
        }),
      access: jest.fn(),
      permission: jest.fn(),
      push: jest.fn(),
      invoke: jest.fn(),
    };
    database.client = client;
    const session = database.createWorkspace().createSession();
    const initial = await session.pull({ objectId: 101 });
    expect(initial.object('selected').strategy.getRole(fixture.m.C1.Name)).toBe(
      'first'
    );
    const result = await session.pull({ objectId: 101 });
    expect(
      result.object('selected').strategy.getRole(fixture.m.C1.Name)
    ).toBeNull();
    expect(client.sync).toHaveBeenCalledTimes(2);
    expect((client.sync as jest.Mock).mock.calls[1][0].o).toEqual([]);
    expect(client.access).not.toHaveBeenCalled();
  }
);

test('an overlapping load retains advertised entries without replacing a newer current record', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const definition = {
    i: 31,
    c: fixture.m.C1.tag,
    t: fixture.m.C1.Name.relationType.tag,
    o: 2,
  };
  const first = {
    i: 101,
    v: 2,
    c: fixture.m.C1.tag,
    g: [11],
    r: [],
    ro: [{ t: fixture.m.C1.Name.relationType.tag, v: 'first' }],
  };
  const fresh = {
    ...first,
    v: 3,
    ro: [{ t: fixture.m.C1.Name.relationType.tag, v: 'fresh' }],
  };
  const added = { ...first, i: 102, ro: [] };
  let release: (response: any) => void;
  const pending = new Promise<any>((resolve) => {
    release = resolve;
  });
  const client: IDatabaseJsonClient = {
    pull: jest
      .fn()
      .mockResolvedValueOnce({ p: [first], g: [[11, 1]], r: [] })
      .mockResolvedValueOnce({ p: [first, added], g: [[11, 1]], r: [] })
      .mockResolvedValueOnce({ p: [fresh], g: [[11, 2]], r: [] }),
    sync: jest
      .fn()
      .mockResolvedValueOnce({
        result: {},
        o: [first],
        g: [{ i: 11, v: 1, p: [31] }],
        r: [],
        p: [definition],
      })
      .mockReturnValueOnce(pending)
      .mockResolvedValueOnce({
        result: {},
        o: [fresh],
        g: [{ i: 11, v: 2, p: [31] }],
        r: [],
        p: [definition],
      }),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const workspace = database.createWorkspace();
  await workspace.createSession().pull({ objectId: 101 });
  const slowSession = workspace.createSession();
  const slow = slowSession.pull([{ objectId: 101 }, { objectId: 102 }]);
  await Promise.resolve();
  const fast = workspace.createSession();
  await fast.pull({ objectId: 101 });
  const current = database.getRecord(101);
  expect((client.sync as jest.Mock).mock.calls[1][0]).toEqual(
    expect.objectContaining({
      o: [{ i: 101, v: 2, g: [11], r: [] }],
      g: [[11, 1]],
      r: [],
    })
  );
  release({
    result: { o: { selected: 101 } },
    o: [{ ...first, ro: null }, added],
    g: [{ i: 11, v: 1, p: null }],
    r: [],
    p: [],
  });
  const result = await slow;
  expect(result.object('selected').strategy.getRole(fixture.m.C1.Name)).toBe(
    'first'
  );
  expect(database.getRecord(101)).toBe(current);
  expect(fast.instantiate(101).strategy.getRole(fixture.m.C1.Name)).toBe(
    'fresh'
  );
});

test.each([
  'grant',
  'permission',
  'role',
  'selection',
  'range',
  'null object',
  'null security',
  'null permission',
  'null role',
  'create operand',
  'missing pool',
  'missing Pull pool',
  'pushed object class',
  'null named object',
  'binary encoding',
  'binary whitespace',
  'decimal encoding',
  'decimal overflow',
  'decimal precision',
  'decimal scale',
  'unique encoding',
  'date encoding',
])('invalid %s contents are rejected atomically', async (invalid) => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = { i: 101, v: 2, c: fixture.m.C1.tag, g: [11], r: [], ro: [] };
  const response: any = {
    result: { o: { selected: 101 } },
    o: [object],
    g: [{ i: 11, v: 1, p: [31] }],
    r: [],
    p: [
      {
        i: 31,
        c: fixture.m.C1.tag,
        t: fixture.m.C1.Name.relationType.tag,
        o: 2,
      },
    ],
  };
  switch (invalid) {
    case 'pushed object class':
      response.o[0].c = fixture.m.C2.tag;
      break;
    case 'grant':
      response.g[0].p = null;
      break;
    case 'permission':
      response.g[0].p = [32];
      break;
    case 'role':
      response.o[0].ro = [{ t: 'unknown', v: 'invalid' }];
      break;
    case 'selection':
      response.result.o.selected = 102;
      break;
    case 'range':
      response.o[0].g = [11, 11];
      break;
    case 'null object':
      response.o.push(null);
      break;
    case 'null security':
      response.g.push(null);
      break;
    case 'null permission':
      response.p.push(null);
      break;
    case 'null role':
      response.o[0].ro.push(null);
      break;
    case 'create operand':
      response.p.push({
        i: 31,
        c: fixture.m.C1.tag,
        t: fixture.m.C1.Name.relationType.tag,
        o: 1,
      });
      break;
    case 'missing pool':
      delete response.o;
      break;
    case 'null named object':
      response.result.o.selected = null;
      break;
    case 'binary encoding':
      response.o[0].ro = [
        { t: fixture.m.C1.C1AllorsBinary.relationType.tag, v: '***' },
      ];
      break;
    case 'decimal encoding':
      response.o[0].ro = [
        { t: fixture.m.C1.C1AllorsDecimal.relationType.tag, v: 'not-a-number' },
      ];
      break;
    case 'unique encoding':
      response.o[0].ro = [
        { t: fixture.m.C1.C1AllorsUnique.relationType.tag, v: 'not-a-guid' },
      ];
      break;
    case 'binary whitespace':
      response.o[0].ro = [
        { t: fixture.m.C1.C1AllorsBinary.relationType.tag, v: 'YW Jj' },
      ];
      break;
    case 'decimal overflow':
      response.o[0].ro = [
        {
          t: fixture.m.C1.C1AllorsDecimal.relationType.tag,
          v: '79228162514264337593543950335.9',
        },
      ];
      break;
    case 'decimal precision':
      response.o[0].ro = [
        {
          t: fixture.m.C1.C1AllorsDecimal.relationType.tag,
          v: '7922816251426433759354395033.6',
        },
      ];
      break;
    case 'decimal scale':
      response.o[0].ro = [
        {
          t: fixture.m.C1.C1AllorsDecimal.relationType.tag,
          v: '0.00000000000000000000000000001',
        },
      ];
      break;
    case 'date encoding':
      response.o[0].ro = [
        {
          t: fixture.m.C1.C1AllorsDateTime.relationType.tag,
          v: '2024-02-31T00:00:00Z',
        },
      ];
      break;
  }
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({
      p: invalid === 'missing Pull pool' ? undefined : [{ ...object, g: [11] }],
      g: [[11, 1]],
      r: [],
    }),
    sync: jest.fn().mockResolvedValue(response),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();
  const pushed =
    invalid === 'pushed object class' ? session.create(fixture.m.C1) : null;
  if (pushed) {
    (client.push as jest.Mock).mockResolvedValue({
      n: [{ w: pushed.id, d: 101 }],
    });
    expect((await session.push()).hasErrors).toBe(false);
    expect(pushed.id).toBe(101);
    expect(database.getRecord(101)).toBeUndefined();
  }
  const failure = await session.pull({ objectId: 101 }).then(
    () => null,
    (error) => error
  );
  expect(failure).toBeInstanceOf(Error);
  expect(client.sync).toHaveBeenCalledTimes(
    invalid === 'missing Pull pool' ? 0 : 1
  );
  expect(database.getRecord(101)).toBeUndefined();
  expect(session.instantiate(101)).toBe(pushed);
  if (pushed) {
    expect(pushed.strategy.cls).toBe(fixture.m.C1);
    expect(pushed.strategy.version).toBe(WorkspaceInitialVersion);
  }
  expect(
    database.getPermission(fixture.m.C1, fixture.m.C1.Name, 2)
  ).toBeUndefined();
  if (pushed) {
    response.o[0].c = fixture.m.C1.tag;
    const recovered = await session.pull({ objectId: 101 });
    expect(recovered.hasErrors).toBe(false);
    expect(recovered.object('selected')).toBe(pushed);
    expect(pushed.strategy.version).toBe(2);
    expect(database.getRecord(101).cls).toBe(fixture.m.C1);
  }
});

test.each(['data', 'security'])(
  'omitted object contents require the exact advertised %s binding',
  async (change) => {
    const fixture = new Fixture();
    fixture.metaPopulation = new LazyMetaPopulation(data);
    const database = fixture.createDatabaseConnection();
    const object = {
      i: 101,
      v: 2,
      c: fixture.m.C1.tag,
      g: [11],
      r: [],
      ro: [],
    };
    const client: IDatabaseJsonClient = {
      pull: jest
        .fn()
        .mockResolvedValueOnce({ p: [object], g: [[11, 1]], r: [] })
        .mockResolvedValue({
          p: [{ ...object, v: change === 'data' ? 3 : 2 }],
          g: [[11, change === 'security' ? 2 : 1]],
          r: [],
        }),
      sync: jest
        .fn()
        .mockResolvedValueOnce({
          result: {},
          o: [object],
          g: [{ i: 11, v: 1, p: [] }],
          r: [],
          p: [],
        })
        .mockResolvedValue({
          result: {},
          o: [{ ...object, ro: null }],
          g: [{ i: 11, v: 1, p: [] }],
          r: [],
          p: [],
        }),
      access: jest.fn(),
      permission: jest.fn(),
      push: jest.fn(),
      invoke: jest.fn(),
    };
    database.client = client;
    const session = database.createWorkspace().createSession();
    await session.pull({ objectId: 101 });
    const record = database.getRecord(101);
    await expect(session.pull({ objectId: 101 })).rejects.toThrow('advertised');
    expect((client.sync as jest.Mock).mock.calls[1][0].o).toEqual([]);
    expect(database.getRecord(101)).toBe(record);
    expect(session.instantiate(101).strategy.version).toBe(2);
  }
);

test.each(['malformed', 'network'])(
  'a %s refresh failure leaves existing session and shared cache contents intact',
  async (failureType) => {
    const fixture = new Fixture();
    fixture.metaPopulation = new LazyMetaPopulation(data);
    const database = fixture.createDatabaseConnection();
    const object = { i: 101, v: 2, c: fixture.m.C1.tag, g: [], r: [], ro: [] };
    const changed = { ...object, v: 3 };
    const client: IDatabaseJsonClient = {
      pull: jest
        .fn()
        .mockResolvedValueOnce({ p: [object], g: [], r: [] })
        .mockResolvedValueOnce({ p: [changed], g: [], r: [] })
        .mockResolvedValue({ p: [object], g: [], r: [] }),
      sync: jest
        .fn()
        .mockResolvedValueOnce({ result: {}, o: [object], g: [], r: [], p: [] })
        .mockImplementation(async () => {
          if (failureType === 'network') throw new Error('network failure');
          return {
            result: {},
            o: [changed, { ...object, i: 102, ro: null }],
            g: [],
            r: [],
            p: [],
          };
        }),
      access: jest.fn(),
      permission: jest.fn(),
      push: jest.fn(),
      invoke: jest.fn(),
    };
    database.client = client;
    const session = database.createWorkspace().createSession();
    await session.pull({ objectId: 101 });
    const record = database.getRecord(101);
    const original = session.instantiate(101);
    await expect(session.pull({ objectId: 101 })).rejects.toThrow(
      failureType === 'network' ? 'network failure' : 'advertised'
    );
    expect(database.getRecord(101)).toBe(record);
    expect(database.getRecord(102)).toBeUndefined();
    expect(session.instantiate(101)).toBe(original);
    expect(original.strategy.version).toBe(2);
    await session.pull({ objectId: 101 });
    expect(client.sync).toHaveBeenCalledTimes(2);
  }
);
