import {
  InvokeReadRequest,
  InvokeReadResponse,
  decodeInvokeReadRequest,
  decodeInvokeReadReceipt,
  decodeInvokeReadResponse,
} from '@allors/system/common/protocol-json';
import { InvokeReadHttpTransport } from '@allors/system/workspace/adapters-json';
import {
  PushReadRequest,
  PushReadResponse,
} from '@allors/system/common/protocol-json';
import {
  decodePushReadRequest,
  decodePushReadResponse,
  decodePushReadReceipt,
} from '@allors/system/common/protocol-json';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- Dedicated tests exercise the internal read assembler.
import {
  ReadCoordinator,
  ReadCoordinatorOwner,
} from '../../../../adapters-json/src/lib/read/read-coordinator';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { ReadFailedError } from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import {
  decodeReadRequest,
  decodeReadResponse,
  validateReadRequest,
  validateReadResponse,
} from '@allors/system/common/protocol-json';
import {
  DiscoverySuccess,
  ReadFailure,
  ReadPermissionPayload,
  ReadPull,
  ReadRecordPayload,
  ReadRequest,
  ReadResponse,
  ReadScope,
  ReadSecurityPayload,
  ReadSuccess,
  ReadValue,
} from '@allors/system/common/protocol-json';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import {
  PushReadHttpTransport,
  ReadHttpTransport,
} from '@allors/system/workspace/adapters-json';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- Immutable cache inputs are internal implementation values.
import {
  ReadStoreScope,
  ReadSecurityKey,
  ReadStoredRecord,
  ReadStoredSecurity,
  ReadStoredPermission,
} from '../../../../adapters-json/src/lib/read/read-store-values';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- Dedicated tests exercise the internal cache.
import { ReadStore } from '../../../../adapters-json/src/lib/read/read-store';

interface ContractMessages {
  coldRequest: ReadRequest;
  cachedRequest: ReadRequest;
  discovery: DiscoverySuccess;
  complete: ReadSuccess;
  failure: ReadFailure;
  unscopedFailure: ReadFailure;
  emptyComplete: ReadSuccess;
}

const fixturePath = resolve(__dirname, 'read-contract.json');
const fixture: ContractMessages = JSON.parse(readFileSync(fixturePath, 'utf8'));

function scope(): ReadScope {
  return {
    p: 'population-1',
    u: 7,
    w: 'Default',
    r: 'schema-1',
    x: 'tenant=blue',
  };
}

function pull(): ReadPull {
  return {
    l: [
      { e: null, t: 'C1', o: 101, r: null, a: null },
      { e: null, t: 'C1', o: 999, r: null, a: null },
      {
        e: { k: 1, t: 'C1', o: null, p: null, s: null },
        t: null,
        o: null,
        r: null,
        a: null,
      },
    ],
    d: [{ o: 'C1', a: null, r: 'Related' }],
  };
}

// Construct these independently of the shared fixture: a matching roundtrip alone
// would not detect two serializers agreeing on the wrong field names or encodings.
function messages(): ContractMessages {
  const values: { [name: string]: ReadValue } = {
    binary: 'AAEC/w==',
    boolean: false,
    dateTime: '2026-10-10T12:34:56Z',
    decimal: '123.4500',
    float: 1.5,
    integer: 42,
    string: 'hello',
    unique: '01234567-89ab-cdef-0123-456789abcdef',
    nothing: null,
  };

  return {
    coldRequest: {
      v: 1,
      i: 'cold-1',
      sc: null,
      x: 'tenant=blue',
      k: 'read',
      p: pull(),
      h: null,
    },
    cachedRequest: {
      v: 1,
      i: 'read-1',
      sc: scope(),
      x: 'tenant=blue',
      k: 'read',
      p: pull(),
      h: {
        s: [
          { k: 'grant', i: 11, v: 4, h: true },
          { k: 'grant', i: 11, v: 3, h: true },
          { k: 'revocation', i: 21, v: 2, h: false },
        ],
        r: [
          { i: 102, c: 'C1', v: 4294967298, g: [0], r: [2] },
          { i: 101, c: 'C1', v: 7, g: [1], r: [2] },
        ],
        p: [201],
      },
    },
    discovery: {
      i: 'discover-1',
      sc: scope(),
      k: 'discovery',
      p: [
        { i: 101, c: 'C1', v: 7, g: [0], r: [1] },
        { i: 102, c: 'C1', v: 4294967298, g: [0], r: [1] },
      ],
      s: [
        { k: 'grant', i: 11, v: 4 },
        { k: 'revocation', i: 21, v: 2 },
      ],
    },
    complete: {
      i: 'read-1',
      sc: scope(),
      k: 'complete',
      l: [
        { i: 0, o: 101, s: 'resolved' },
        { i: 1, o: 999, s: 'unavailable' },
        { i: 2, o: null, s: 'resolved' },
      ],
      c: { found: [101, 102, 103], empty: [] },
      o: { first: 101, missing: null },
      p: [
        {
          i: 101,
          c: 'C1',
          v: 7,
          g: [0, 2],
          r: [1],
          p: {
            ro: [
              ...Object.entries(values).map(([t, v]) => ({
                t,
                c: null,
                o: null,
                v,
              })),
              { t: 'one', c: null, o: 4294967297, v: null },
              { t: 'many', c: [102, 4294967297], o: null, v: null },
            ],
            r: null,
          },
        },
        {
          i: 102,
          c: 'C1',
          v: 4294967298,
          g: [0],
          r: [1],
          p: { ro: null, r: 0 },
        },
        { i: 103, c: 'C1', v: 1, g: [], r: [], p: { ro: [], r: null } },
      ],
      s: [
        { k: 'grant', i: 11, v: 4, p: { p: null, r: 0 } },
        { k: 'revocation', i: 21, v: 2, p: { p: [], r: null } },
        {
          k: 'grant',
          i: 12,
          v: 1,
          p: { p: [201, 202, 203, 204, 205], r: null },
        },
      ],
      pm: [
        { i: 201, p: { c: null, o: null, t: null, r: 0 } },
        { i: 202, p: { c: 'C1', o: 1, t: null, r: null } },
        { i: 203, p: { c: 'C1', o: 2, t: 'string', r: null } },
        { i: 204, p: { c: 'C1', o: 4, t: 'string', r: null } },
        { i: 205, p: { c: 'C1', o: 8, t: 'DoWork', r: null } },
      ],
    },
    failure: {
      i: 'read-1',
      sc: scope(),
      k: 'error',
      e: [
        {
          c: 'invalid-cache-reference',
          m: 'Remove the invalid cache reference and retry the read.',
          o: [101],
          i: 0,
        },
      ],
    },
    unscopedFailure: {
      i: 'cold-1',
      sc: null,
      k: 'error',
      e: [
        {
          c: 'unsupported-protocol',
          m: 'Use a supported read protocol revision.',
          o: null,
          i: null,
        },
      ],
    },
    emptyComplete: {
      i: 'empty-1',
      sc: scope(),
      k: 'complete',
      l: [],
      c: {},
      o: {},
      p: [],
      s: [],
      pm: [],
    },
  };
}

function responseEntryCount(response: ReadResponse): number {
  switch (response.k) {
    case 'discovery':
      return response.p.length;
    case 'complete':
      return response.l.length;
    case 'error':
      return response.e.length;
    default: {
      const exhaustive: never = response;
      return exhaustive;
    }
  }
}

const ownedConnections = new Set<DatabaseConnection>();
afterEach(async () => {
  await Promise.all(
    [...ownedConnections].map((connection) => connection.close())
  );
  ownedConnections.clear();
});

async function normalConnection(
  read: (request: ReadRequest, signal?: AbortSignal) => Promise<ReadResponse>,
  metaPopulation: LazyMetaPopulation
) {
  let bootstrap = true;
  const caches: ReadStore[] = [];
  const publish = ReadStore.prototype.publish;
  const capture = jest
    .spyOn(ReadStore.prototype, 'publish')
    .mockImplementation(function (contents) {
      caches.push(this);
      return publish.call(this, contents);
    });
  let id = 0;
  let connection: DatabaseConnection;
  try {
    connection = await DatabaseConnection.create(
      {
        name: 'Default',
        metaPopulation,
        rules: [],
        idGenerator: () => --id,
        objectFactory: {
          create: (strategy) => ({
            get id() {
              return strategy.id;
            },
            strategy,
          }),
        },
      },
      {
        read: (request, signal) =>
          bootstrap
            ? Promise.resolve(
                request.k === 'discover'
                  ? { i: request.i, k: 'discovery', sc: scope(), p: [], s: [] }
                  : {
                      i: request.i,
                      k: 'complete',
                      sc: scope(),
                      l: [],
                      c: {},
                      o: {},
                      p: [],
                      s: [],
                      pm: [],
                    }
              )
            : read(request, signal),
        pushRead: async () => {
          throw new Error('Unexpected Push');
        },
        invokeRead: async () => {
          throw new Error('Unexpected Invoke');
        },
      },
      { revision: 'schema-1', context: 'tenant=blue' }
    );
  } finally {
    capture.mockRestore();
  }
  bootstrap = false;
  ownedConnections.add(connection);
  return {
    connection,
    session: connection.createWorkspace().createSession(),
    cache: caches[0],
  };
}

describe('scoped read coordinator', () => {
  function metadata() {
    return new LazyMetaPopulation({
      c: [
        [
          'C1',
          'First',
          [],
          [
            ['name', '7'],
            ['one', 'C1'],
            ['many', 'C1'],
            ['date', '3'],
            ['integer', '6'],
            ['decimal', '4'],
            ['binary', '1'],
            ['unique', '8'],
          ],
          [['work', 'Work']],
        ],
        ['C2', 'Second', [], [['other', '7']]],
      ],
      m: [[], ['many']],
    });
  }

  function request(id = 'coordinator-1'): ReadRequest {
    return {
      v: 1,
      i: id,
      k: 'read',
      x: 'tenant=blue',
      p: { l: [{ o: 101, t: 'C1' }], d: [] },
    };
  }

  function full(sent: ReadRequest, version = 7): ReadSuccess {
    return {
      i: sent.i,
      sc: scope(),
      k: 'complete',
      l: [{ i: 0, o: 101, s: 'resolved' }],
      c: { found: [101], empty: [] },
      o: { first: 101, missing: null },
      p: [
        {
          i: 101,
          c: 'C1',
          v: version,
          g: [0],
          r: [],
          p: {
            ro: [
              { t: 'name', v: 'first' },
              { t: 'one', o: 999 },
              { t: 'many', c: [] },
              { t: 'date', v: '2026-10-10T12:34:56Z' },
            ],
          },
        },
      ],
      s: [{ k: 'grant', i: 11, v: 3, p: { p: [201, 202] } }],
      pm: [
        { i: 201, p: { c: 'C1', o: 2, t: 'name' } },
        { i: 202, p: { c: 'C1', o: 1 } },
      ],
    };
  }

  function discovery(sent: ReadRequest): DiscoverySuccess {
    const response = full(sent);
    return {
      i: sent.i,
      k: 'discovery',
      sc: response.sc,
      p: response.p.map(({ i, c, v, g, r }) => ({ i, c, v, g, r })),
      s: response.s.map(({ k, i, v }) => ({ k, i, v })),
    };
  }

  const owners = new WeakMap<
    ReadCoordinator,
    ReadCoordinatorOwner & { invalidate: jest.Mock }
  >();
  function cacheOf(reader: ReadCoordinator) {
    return owners.get(reader).store;
  }
  function coordinator(
    read: (sent: ReadRequest, signal?: AbortSignal) => Promise<ReadResponse>,
    options = {}
  ) {
    let store: ReadStore;
    const owner = {
      get store() {
        return store;
      },
      bind(value: ReadStore) {
        store = value;
      },
      assertActive: jest.fn(),
      invalidate: jest.fn(),
    };
    const reader = new ReadCoordinator(
      { read },
      {
        metaPopulation: metadata(),
        workspace: 'Default',
        revision: 'schema-1',
        context: 'tenant=blue',
        ...options,
      },
      owner
    );
    owners.set(reader, owner);
    return reader;
  }

  it.each(['cold', 'empty', 'all cached'])(
    'always discovers then completes %s reads',
    async (state) => {
      const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
        const response = full(sent);
        if (state === 'empty') {
          response.l = [];
          response.c = { empty: [] };
          response.o = {};
          response.p = [];
          response.s = [];
          response.pm = [];
        }
        if (sent.k === 'discover')
          return {
            i: sent.i,
            k: 'discovery',
            sc: response.sc,
            p: response.p.map(({ i, c, v, g, r }) => ({ i, c, v, g, r })),
            s: response.s.map(({ k, i, v }) => ({ k, i, v })),
          };
        if (sent.h?.r.length) {
          response.p[0].p = { r: 0 };
          response.s[0].p = { r: 0 };
          response.pm = response.pm.map(({ i }, index) => ({
            i,
            p: { r: index },
          }));
        }
        return response;
      });
      const owner = coordinator(read);
      const input = request();
      if (state === 'empty') input.p.l = [];
      const previous =
        state === 'all cached' ? await owner.read(input) : undefined;
      read.mockClear();
      const result = await owner.read(input);
      expect(read.mock.calls.map(([sent]) => sent.k)).toEqual([
        'discover',
        'read',
      ]);
      expect(read.mock.calls[0][0].h).toBeUndefined();
      expect(read.mock.calls[1][0].p).toEqual(input.p);
      expect(read.mock.calls[1][0].sc).toEqual(scope());
      if (previous) {
        expect(read.mock.calls[1][0].h.r).toHaveLength(1);
        expect(result.records[0]).toBe(previous.records[0]);
      } else {
        expect(read.mock.calls[1][0].h).toBeUndefined();
      }
      if (state === 'empty') {
        expect(result.records).toEqual([]);
        expect(result.collections).toEqual({ empty: [] });
      }
    }
  );

  it('accepts changed membership, versions and security in phase two without restarting', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') {
        expect(cacheOf(owner)).toBeUndefined();
        return {
          i: sent.i,
          k: 'discovery',
          sc: scope(),
          p: [{ i: 101, c: 'C1', v: 7, g: [0], r: [] }],
          s: [{ k: 'grant', i: 11, v: 3 }],
        };
      }
      expect(cacheOf(owner)).toBeUndefined();
      const response = full(sent, 8);
      response.l = [{ i: 0, s: 'resolved' }];
      response.c = { found: [101, 102] };
      response.p.push({ i: 102, c: 'C1', v: 1, g: [], r: [], p: { ro: [] } });
      response.s[0].v = 4;
      response.s[0].p = { p: [202] };
      response.pm = [response.pm[1]];
      return response;
    });
    const owner = coordinator(read);
    const input = request();
    input.p.l = [{ e: { k: 1, t: 'C1' } }];
    const result = await owner.read(input);
    expect(read.mock.calls.map(([sent]) => sent.k)).toEqual([
      'discover',
      'read',
    ]);
    expect(read.mock.calls[1][0].p).toEqual(input.p);
    expect(read.mock.calls[1][0].h).toBeUndefined();
    expect(result.collections.found).toEqual([101, 102]);
    expect(result.records[0].dataVersion).toBe(8);
    expect(result.security[0].key.version).toBe(4);
    expect(result.permissions.map(({ id }) => id)).toEqual([202]);
    expect(cacheOf(owner).getCurrentRecord(101)).toBe(result.records[0]);
  });

  it('binds a cold scope and preserves complete decoded outputs without loading relation targets', async () => {
    const read = jest.fn(async (sent: ReadRequest) =>
      sent.k === 'discover' ? discovery(sent) : full(sent)
    );
    const owner = coordinator(read);
    expect(cacheOf(owner)).toBeUndefined();
    const result = await owner.read(request());
    expect(read).toHaveBeenCalledTimes(2);
    expect(read.mock.calls[0][0].h).toBeUndefined();
    expect(result.scope.identity).toBe(cacheOf(owner).scope.identity);
    expect(result.records.map((entry) => entry.id)).toEqual([101]);
    expect(result.records[0].roles).toMatchObject({
      one: 999,
      many: [],
      date: new Date('2026-10-10T12:34:56Z'),
    });
    expect(cacheOf(owner).getCurrentRecord(999)).toBeUndefined();
    expect(result.collections).toEqual({ found: [101], empty: [] });
    expect(result.objects).toEqual({ first: 101, missing: null });
    expect('values' in result).toBe(false);
    expect(result.outcomes).toEqual([{ i: 0, o: 101, s: 'resolved' }]);
  });

  it('captures exact hints and preserves reused immutable contents after later reads', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent, sent.i === 'later' ? 8 : 7);
      if (sent.h?.r.length && sent.i !== 'later') {
        response.p[0].p = { r: 0 };
        response.s[0].p = { r: 0 };
        response.pm = response.pm.map((entry, index) => ({
          i: entry.i,
          p: { r: index },
        }));
      }
      return response;
    });
    const owner = coordinator(read);
    const first = await owner.read(request());
    const second = await owner.read(request('second'));
    expect(read.mock.calls[3][0].h).toEqual({
      r: [{ i: 101, c: 'C1', v: 7, g: [0], r: [] }],
      s: [{ k: 'grant', i: 11, v: 3, h: true }],
      p: [201, 202],
    });
    expect(second.records[0]).toBe(first.records[0]);
    expect(second.security[0]).toBe(first.security[0]);
    const later = await owner.read(request('later'));
    expect(cacheOf(owner).getCurrentRecord(101)).toBe(later.records[0]);
    expect(cacheOf(owner).getRecord(first.records[0])).toBe(first.records[0]);
    expect(second.records[0].roles.name).toBe('first');
    expect(second.records[0].dataVersion).toBe(7);
  });

  it('captures request and metadata before queued operations and serializes publication order', async () => {
    let finish: () => void;
    let entered: () => void;
    const started = new Promise<void>((resolve) => {
      entered = resolve;
    });
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      if (sent.i === 'session-1')
        await new Promise<void>((resolve) => {
          finish = resolve;
          entered();
        });
      return full(sent, sent.i === 'session-2' ? 8 : 7);
    });
    const metaPopulation = metadata();
    const owned = await normalConnection(read, metaPopulation);
    const original = { objectId: 101 };
    const next = { objectId: 101 };
    const first = owned.session.pull(original);
    const second = owned.session.pull(next);
    original.objectId = next.objectId = 999;
    metaPopulation.classes.clear();
    await started;
    expect(read).toHaveBeenCalledTimes(2);
    finish();
    expect((await first).hasErrors).toBe(false);
    expect((await second).hasErrors).toBe(false);
    expect(read.mock.calls.map(([sent]) => sent.k)).toEqual([
      'discover',
      'read',
      'discover',
      'read',
    ]);
    expect(read.mock.calls[3][0].p.l[0].o).toBe(101);
    expect(
      owned.cache.getRecord(
        101,
        'C1',
        7,
        [new ReadSecurityKey('grant', 11, 3)],
        []
      ).dataVersion
    ).toBe(7);
    expect(owned.cache.getCurrentRecord(101).dataVersion).toBe(8);
  });

  it('discovers likely headers but repeats the captured original selection for the authoritative read', async () => {
    const read = jest.fn(
      async (sent: ReadRequest): Promise<ReadResponse> =>
        sent.k === 'discover'
          ? {
              i: sent.i,
              k: 'discovery',
              sc: scope(),
              p: [{ i: 101, c: 'C1', v: 7, g: [], r: [] }],
              s: [],
            }
          : full(sent)
    );
    const owner = coordinator(read);
    const result = await owner.read(request());
    expect(read.mock.calls.map(([sent]) => sent.k)).toEqual([
      'discover',
      'read',
    ]);
    expect(read.mock.calls[1][0].p).toEqual(request().p);
    expect(result.security).toHaveLength(1);
    expect(read.mock.calls[1][0].sc).toEqual(scope());
  });

  it('retains current state after missing reused permission closure and recovers the queue after failure', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent, sent.i === 'session-2' ? 8 : 7);
      if (sent.i === 'session-2') {
        response.s[0].p = { r: 0 };
        response.pm = [];
      }
      return response;
    });
    const owned = await normalConnection(read, metadata());
    await owned.session.pull({ objectId: 101 });
    const first = owned.cache.getCurrentRecord(101);
    await expect(owned.session.pull({ objectId: 101 })).rejects.toThrow(
      /permission|closure/i
    );
    expect(owned.cache.getCurrentRecord(101)).toBe(first);
    expect((await owned.session.pull({ objectId: 101 })).hasErrors).toBe(false);
    expect(owned.cache.getCurrentRecord(101)).toBe(first);
  });

  it.each([
    'class',
    'role',
    'unit',
    'permission-class',
    'permission-operand',
    'permission-kind',
  ])(
    'rejects invalid %s metadata before binding a cold store',
    async (invalid) => {
      const owner = coordinator(async (sent) => {
        if (sent.k === 'discover') return discovery(sent);
        const response = full(sent);
        if (invalid === 'class') response.p[0].c = 'missing';
        if (invalid === 'role')
          response.p[0].p = { ro: [{ t: 'other', v: 'not applicable' }] };
        if (invalid === 'unit')
          response.p[0].p = { ro: [{ t: 'integer', v: '42' }] };
        if (invalid === 'permission-class')
          response.pm[0].p = { c: 'missing', o: 1 };
        if (invalid === 'permission-operand')
          response.pm[0].p = { c: 'C1', o: 2, t: 'other' };
        if (invalid === 'permission-kind')
          response.pm[0].p = { c: 'C1', o: 8, t: 'name' };
        return response;
      });
      await expect(owner.read(request())).rejects.toThrow(
        /metadata|role|class|operand|unit/i
      );
      expect(cacheOf(owner)).toBeUndefined();
    }
  );

  it.each(['revision', 'workspace', 'context'])(
    'rejects a changed %s scope without binding current state',
    async (field) => {
      const owner = coordinator(async (sent) => {
        if (sent.k === 'discover') return discovery(sent);
        const response = full(sent);
        if (field === 'revision') response.sc.r = 'other';
        if (field === 'workspace') response.sc.w = 'other';
        if (field === 'context') response.sc.x = '';
        return response;
      });
      await expect(owner.read(request())).rejects.toThrow(
        /scope|context|workspace|revision/i
      );
      expect(cacheOf(owner)).toBeUndefined();
    }
  );

  it.each(
    ['discover', 'read'].flatMap((phase) =>
      ['cancel', 'retire', 'failure', 'throw'].map((action) => [phase, action])
    )
  )('publishes nothing when %s encounters %s', async (phase, action) => {
    const cancellation = new AbortController();
    let closing: Promise<void>;
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k !== phase) return discovery(sent);
      if (action === 'cancel') cancellation.abort();
      if (action === 'retire') closing = owned.connection.close();
      if (action === 'throw') throw new Error('transport failed');
      if (action === 'failure')
        return {
          i: sent.i,
          k: 'error',
          e: [{ c: 'failure', m: 'Start a new read.' }],
        };
      return sent.k === 'discover' ? discovery(sent) : full(sent);
    });
    const owned = await normalConnection(read, metadata());
    const pending = owned.session.pull({ objectId: 101 }, cancellation.signal);
    if (action === 'failure') expect((await pending).hasErrors).toBe(true);
    else await expect(pending).rejects.toThrow();
    if (closing) await closing;
    expect(owned.cache.getCurrentRecord(101)).toBeUndefined();
    expect(read).toHaveBeenCalledTimes(phase === 'discover' ? 1 : 2);
  });

  it.each(['ascii', 'é', '€', '😀'])(
    'enforces the exact UTF-8 request byte boundary for %s',
    async (text) => {
      const sent = { ...request(text), sc: scope() };
      const bytes = Buffer.byteLength(
        JSON.stringify({ ...sent, k: 'discover' }),
        'utf8'
      );
      const read = jest.fn(async (input: ReadRequest) =>
        input.k === 'discover' ? discovery(input) : full(input)
      );
      const exact = coordinator(read, { maxRequestBytes: bytes });
      await exact.read(sent);

      const tooSmall = coordinator(read, { maxRequestBytes: bytes - 1 });
      await expect(tooSmall.read(sent)).rejects.toThrow(/byte limit/i);
      expect(read).toHaveBeenCalledTimes(2);
    }
  );

  it('bounds total advertised hints and request bytes while preserving full original selections', async () => {
    const read = jest.fn(async (sent: ReadRequest) =>
      sent.k === 'discover' ? discovery(sent) : full(sent)
    );
    const owner = coordinator(read, { maxHints: 2, maxRequestBytes: 700 });
    await owner.read(request());
    await owner.read(request('second'));
    const sent = read.mock.calls[3][0];
    expect(
      sent.h.r.length + sent.h.s.length + sent.h.p.length
    ).toBeLessThanOrEqual(2);
    expect(Buffer.byteLength(JSON.stringify(sent))).toBeLessThanOrEqual(700);
    expect(sent.p).toEqual(request().p);
  });

  it('rejects oversized original requests and responses before publication', async () => {
    const read = jest.fn(async (sent: ReadRequest) =>
      sent.k === 'discover' ? discovery(sent) : full(sent)
    );
    const tooSmall = coordinator(read, { maxRequestBytes: 10 });
    await expect(tooSmall.read(request())).rejects.toThrow(/size|byte|limit/i);
    expect(read).not.toHaveBeenCalled();
    const responseLimit = coordinator(read, { maxResponseBytes: 10 });
    await expect(responseLimit.read(request())).rejects.toThrow(
      /size|byte|limit/i
    );
    expect(cacheOf(responseLimit)).toBeUndefined();
  });

  it('clears only explicit unavailable targets and keeps completed result contents immutable', async () => {
    const owner = coordinator(async (sent) =>
      sent.k === 'discover'
        ? discovery(sent)
        : sent.i === 'missing'
        ? {
            i: sent.i,
            sc: scope(),
            k: 'complete',
            l: [{ i: 0, o: 101, s: 'unavailable' }],
            c: { empty: [] },
            o: { first: null },
            p: [],
            s: [],
            pm: [],
          }
        : full(sent)
    );
    const first = await owner.read(request());
    expect(() => (first.collections.found as number[]).push(5)).toThrow();
    expect(() => {
      (first.outcomes[0] as { s: string }).s = 'unavailable';
    }).toThrow();
    (first.records[0].roles.date as Date).setTime(0);
    expect(first.records[0].roles.date).toEqual(
      new Date('2026-10-10T12:34:56Z')
    );
    await owner.read(request('missing'));
    expect(cacheOf(owner).isUnavailable(101)).toBe(true);
    expect(cacheOf(owner).getCurrentRecord(101)).toBeUndefined();
    expect(first.records[0].roles.name).toBe('first');
  });

  it('advertises historical provenance without claiming missing bodies and reuses exact historical records', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') {
        const headers = discovery(sent);
        if (sent.i === 'historical') headers.s[0].v = 2;
        return headers;
      }
      const response = full(sent);
      if (sent.i === 'historical') {
        const index = sent.h.r.findIndex(
          (entry) => sent.h.s[entry.g[0]].v === 2
        );
        expect(index).toBeGreaterThanOrEqual(0);
        expect(sent.h.s[sent.h.r[index].g[0]].h).toBe(false);
        response.p[0].p = { r: index };
        response.s[0].v = 2;
      }
      return response;
    });
    const owner = coordinator(read);
    const first = await owner.read(request());
    const historical = cacheOf(owner).retainRecord(
      new ReadStoredRecord(
        first.scope,
        101,
        'C1',
        7,
        [new ReadSecurityKey('grant', 11, 2)],
        [],
        { name: 'historical' }
      )
    );
    const result = await owner.read(request('historical'));
    expect(result.records[0]).toBe(historical);
    expect(result.records[0].roles.name).toBe('historical');
    expect(first.records[0].roles.name).toBe('first');
    expect(result.security[0].key.version).toBe(2);
  });

  it('keeps captured historical bodies stable across a suspended transport failure and connection close', async () => {
    let finish: () => void;
    let entered: () => void;
    const started = new Promise<void>((resolve) => {
      entered = resolve;
    });
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      if (sent.i === 'session-2') {
        await new Promise<void>((resolve) => {
          finish = resolve;
          entered();
        });
        throw new Error('network failed');
      }
      return full(sent);
    });
    const owned = await normalConnection(read, metadata());
    await owned.session.pull({ objectId: 101 });
    const held = owned.cache.getCurrentRecord(101);
    const pending = owned.session.pull({ objectId: 101 });
    await started;
    expect(read.mock.calls[3][0].h.r).toHaveLength(1);
    let closed = false;
    const closing = owned.connection.close().then(() => {
      closed = true;
    });
    await Promise.resolve();
    expect(closed).toBe(false);
    expect(owned.cache.getRecord(held)).toBe(held);
    finish();
    await expect(pending).rejects.toThrow();
    await closing;
    expect(owned.cache.getRecord(held)).toBeUndefined();
    expect(held.roles.name).toBe('first');
  });

  it.each([
    ['date', '0000-01-01T00:00:00Z'],
    ['date', '2026-02-30T00:00:00Z'],
    ['date', '2026-10-10T24:00:00Z'],
    ['date', '2026-10-10T12:00:00+14:01'],
    ['date', '2026-10-10T12:00:00+23:00'],
    ['integer', 2147483648],
  ])(
    'rejects nonrepresentable unit %s=%s',
    async (tag: string, value: string | number) => {
      const owner = coordinator(async (sent) => {
        if (sent.k === 'discover') return discovery(sent);
        const response = full(sent);
        response.p[0].p = { ro: [{ t: tag, v: value }] };
        return response;
      });
      await expect(owner.read(request())).rejects.toThrow(/unit/i);
      expect(cacheOf(owner)).toBeUndefined();
    }
  );

  it('accepts leap day in years before 100 using the represented calendar year', async () => {
    const owner = coordinator(async (sent) => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      response.p[0].p = { ro: [{ t: 'date', v: '0004-02-29T00:00:00Z' }] };
      return response;
    });
    const result = await owner.read(request());
    expect((result.records[0].roles.date as Date).getUTCFullYear()).toBe(4);
  });

  it('does not poison retained exact keys when another staged row fails metadata validation', async () => {
    const read = jest.fn(async (sent: ReadRequest) => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent, 8);
      if (sent.i === 'broken') {
        response.p[0].p = { ro: [{ t: 'name', v: 'poison' }] };
        response.pm[0].p = { c: 'C1', o: 8, t: 'name' };
      }
      return response;
    });
    const owner = coordinator(read);
    await expect(owner.read(request('broken'))).rejects.toThrow(/operand/i);
    expect((await owner.read(request('valid'))).records[0].roles.name).toBe(
      'first'
    );
  });

  it('cancels a queued read before dispatch without disturbing the preceding publication', async () => {
    let finish: () => void;
    let entered: () => void;
    const started = new Promise<void>((resolve) => {
      entered = resolve;
    });
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      await new Promise<void>((resolve) => {
        finish = resolve;
        entered();
      });
      return full(sent);
    });
    const owned = await normalConnection(read, metadata());
    const first = owned.session.pull({ objectId: 101 });
    const cancellation = new AbortController();
    const second = owned.session.pull({ objectId: 101 }, cancellation.signal);
    cancellation.abort();
    const rejected = expect(second).rejects.toThrow(/cancel/i);
    await started;
    finish();
    expect((await first).hasErrors).toBe(false);
    await rejected;
    expect(read).toHaveBeenCalledTimes(2);
    expect(owned.cache.getCurrentRecord(101).dataVersion).toBe(7);
  });

  it('retires a known mismatched response scope without automatically replaying across identities', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      if (sent.i === 'session-2') response.sc.u = 8;
      return response;
    });
    const owned = await normalConnection(read, metadata());
    await owned.session.pull({ objectId: 101 });
    const first = owned.cache.getCurrentRecord(101);
    await expect(owned.session.pull({ objectId: 101 })).rejects.toThrow(
      /scope|closed|ready/i
    );
    expect(owned.connection.closed).toBe(true);
    await owned.connection.close();
    expect(owned.cache.getCurrentRecord(101)).toBeUndefined();
    expect(owned.cache.getRecord(first)).toBeUndefined();
    expect(first.roles.name).toBe('first');
    expect(read).toHaveBeenCalledTimes(4);
  });

  it('keeps failure diagnostics as immutable protocol data with object and selection IDs', async () => {
    const owner = coordinator(async (sent) => ({
      i: sent.i,
      k: 'error',
      sc: scope(),
      e: [{ c: 'read-failed', m: 'Start a new read.', o: [101], i: 0 }],
    }));
    try {
      await owner.read(request());
      throw new Error('Expected a failed read');
    } catch (error) {
      expect(error).toBeInstanceOf(ReadFailedError);
      const failure = error as ReadFailedError;
      expect(failure.requestId).toBe('coordinator-1');
      expect(failure.scope.userId).toBe(7);
      expect(failure.errors).toEqual([
        { c: 'read-failed', m: 'Start a new read.', o: [101], i: 0 },
      ]);
      expect(() => (failure.errors[0].o as number[]).push(102)).toThrow();
    }
    expect(owners.get(owner).invalidate).not.toHaveBeenCalled();
  });

  it('does not retire a valid generation on an invalid message carrying a different scope', async () => {
    const read = jest.fn(async (sent: ReadRequest): Promise<ReadResponse> => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      if (sent.i === 'session-2') {
        response.sc.u = 8;
        response.p[0].p = { r: 1000 };
      }
      return response;
    });
    const owned = await normalConnection(read, metadata());
    await owned.session.pull({ objectId: 101 });
    const first = owned.cache.getCurrentRecord(101);
    await expect(owned.session.pull({ objectId: 101 })).rejects.toThrow();
    expect(owned.connection.closed).toBe(false);
    expect(owned.cache.getCurrentRecord(101)).toBe(first);
    expect((await owned.session.pull({ objectId: 101 })).hasErrors).toBe(false);
  });

  it('omits empty hints when their envelope would exceed the original request byte budget', async () => {
    const sent = { ...request(), sc: scope() };
    const read = jest.fn(async (value: ReadRequest) =>
      value.k === 'discover' ? discovery(value) : full(value)
    );
    const owner = coordinator(read, {
      maxHints: 0,
      maxRequestBytes: Buffer.byteLength(
        JSON.stringify({ ...sent, k: 'discover' })
      ),
    });
    await owner.read(sent);
    await owner.read(sent);
    expect(read.mock.calls[3][0].h).toBeUndefined();
  });

  it.each([
    { maxHints: 70001 },
    { maxRequestBytes: 1048577 },
    { maxResponseBytes: 16777217 },
  ])('rejects bounds beyond protocol caps %j', (limits) => {
    expect(() =>
      coordinator(
        async (sent) => (sent.k === 'discover' ? discovery(sent) : full(sent)),
        limits
      )
    ).toThrow(/limit|hint count/i);
  });

  it('requires encoded Date values at the protocol boundary without calling JSON hooks', async () => {
    const read = jest.fn(async (sent: ReadRequest) =>
      sent.k === 'discover' ? discovery(sent) : full(sent)
    );
    const owner = coordinator(read);
    const date = new Date('2026-10-12T10:00:00Z');
    const toJSON = jest.fn(() => '2026-10-12T10:00:00Z');
    date.toJSON = toJSON;
    const sent = request();
    sent.p.l[0].a = { date: date as unknown as string };
    await expect(owner.read(sent)).rejects.toThrow(/plain JSON data/i);
    expect(toJSON).not.toHaveBeenCalled();
    expect(read).not.toHaveBeenCalled();
  });

  it('rejects caller NaN values before JSON capture can silently convert them to null', async () => {
    const read = jest.fn(async (sent: ReadRequest) =>
      sent.k === 'discover' ? discovery(sent) : full(sent)
    );
    const owner = coordinator(read);
    const sent = request();
    sent.p.l[0].a = { value: NaN as unknown as string };
    await expect(owner.read(sent)).rejects.toThrow();
    expect(read).not.toHaveBeenCalled();
  });

  it('never publishes after cancellation polling retires the captured generation', async () => {
    let retiredAttempts = 0;
    for (let threshold = 1; threshold <= 8; threshold++) {
      let delivered = false;
      let polls = 0;
      let closing: Promise<void>;
      const cancellation = new AbortController().signal;
      const owned = await normalConnection(async (sent) => {
        if (sent.k === 'discover') return discovery(sent);
        delivered = true;
        return full(sent);
      }, metadata());
      Object.defineProperty(cancellation, 'aborted', {
        get() {
          if (delivered && ++polls === threshold)
            closing = owned.connection.close();
          return false;
        },
      });
      const result = await owned.session
        .pull({ objectId: 101 }, cancellation)
        .catch(() => undefined);
      if (closing) {
        retiredAttempts++;
        expect(result).toBeUndefined();
        await closing;
        expect(owned.cache.getCurrentRecord(101)).toBeUndefined();
        expect(
          owned.cache.getRecord(
            101,
            'C1',
            7,
            [new ReadSecurityKey('grant', 11, 3)],
            []
          )
        ).toBeUndefined();
      }
    }
    expect(retiredAttempts).toBeGreaterThan(0);
  });

  it.each(['request', 'response'])(
    'rejects accessor-backed %s DTOs before invoking caller serialization',
    async (kind) => {
      const read = jest.fn(async (sent: ReadRequest) => {
        if (sent.k === 'discover') return discovery(sent);
        const response = full(sent);
        if (kind === 'response')
          Object.defineProperty(response, 'v', {
            enumerable: true,
            get: () => ({ nothing: null }),
          });
        return response;
      });
      const owner = coordinator(read);
      const sent = request();
      if (kind === 'request')
        Object.defineProperty(sent, 'x', {
          enumerable: true,
          get: () => 'tenant=blue',
        });
      await expect(owner.read(sent)).rejects.toThrow(/accessor|plain|data/i);
      expect(cacheOf(owner)).toBeUndefined();
    }
  );

  it.each([
    ['decimal', '79228162514264337593543950336'],
    ['decimal', '0.00000000000000000000000000001'],
    ['decimal', '1e2'],
    ['binary', 'not-base64'],
    ['unique', 'not-a-guid'],
  ])('rejects malformed scalar encoding %s=%s', async (tag, value) => {
    const owner = coordinator(async (sent) => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      response.p[0].p = { ro: [{ t: tag, v: value }] };
      return response;
    });
    await expect(owner.read(request())).rejects.toThrow(/unit/i);
    expect(cacheOf(owner)).toBeUndefined();
  });

  it('preserves decimal binary and unique JSON unit representations', async () => {
    const values = {
      decimal: '123.4500',
      binary: 'AAEC/w==',
      unique: '01234567-89ab-cdef-0123-456789abcdef',
    };
    const owner = coordinator(async (sent) => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      response.p[0].p = {
        ro: Object.entries(values).map(([t, v]) => ({ t, v })),
      };
      return response;
    });
    expect((await owner.read(request())).records[0].roles).toEqual(values);
  });

  it('rejects loaded relation targets whose class does not match captured role metadata', async () => {
    const owner = coordinator(async (sent) => {
      if (sent.k === 'discover') return discovery(sent);
      const response = full(sent);
      response.p[0].p = { ro: [{ t: 'one', o: 102 }] };
      response.p.push({ i: 102, c: 'C2', v: 1, g: [], r: [], p: { ro: [] } });
      return response;
    });
    await expect(owner.read(request())).rejects.toThrow(/incompatible class/i);
    expect(cacheOf(owner)).toBeUndefined();
  });
});

describe('read protocol serialization contract', () => {
  const names: Array<keyof ContractMessages> = [
    'coldRequest',
    'cachedRequest',
    'discovery',
    'complete',
    'failure',
    'unscopedFailure',
    'emptyComplete',
  ];

  it.each(names)(
    'serializes independently constructed %s to the shared fixture',
    (name) => {
      expect(JSON.parse(JSON.stringify(messages()[name]))).toEqual(
        fixture[name]
      );
      expect(JSON.parse(JSON.stringify(fixture[name]))).toEqual(
        messages()[name]
      );
    }
  );

  it('keeps an explicit cold request without cache possession claims', () => {
    const request: ReadRequest = {
      v: 1,
      i: 'cold-read',
      sc: null,
      x: null,
      k: 'read',
      p: { l: [], d: [] },
      h: null,
    };

    expect(JSON.parse(JSON.stringify(request))).toEqual({
      v: 1,
      i: 'cold-read',
      sc: null,
      x: null,
      k: 'read',
      p: { l: [], d: [] },
      h: null,
    });
  });

  it('preserves empty full bodies and zero reuse indexes as distinct alternatives', () => {
    const recordBody: ReadRecordPayload = { ro: [], r: null };
    const recordReuse: ReadRecordPayload = { ro: null, r: 0 };
    const securityBody: ReadSecurityPayload = { p: [], r: null };
    const securityReuse: ReadSecurityPayload = { p: null, r: 0 };
    const permissionBody: ReadPermissionPayload = {
      c: 'c1',
      o: 1,
      t: null,
      r: null,
    };
    const permissionReuse: ReadPermissionPayload = {
      c: null,
      o: null,
      t: null,
      r: 0,
    };

    expect(
      JSON.parse(
        JSON.stringify([
          recordBody,
          recordReuse,
          securityBody,
          securityReuse,
          permissionBody,
          permissionReuse,
        ])
      )
    ).toEqual([
      { ro: [], r: null },
      { ro: null, r: 0 },
      { p: [], r: null },
      { p: null, r: 0 },
      { c: 'c1', o: 1, t: null, r: null },
      { c: null, o: null, t: null, r: 0 },
    ]);
  });

  it('preserves historical versions, missing bodies, null outputs and wire unit values', () => {
    const decoded: ContractMessages = JSON.parse(JSON.stringify(messages()));
    expect(
      decoded.cachedRequest.h.s.map((entry) => [entry.i, entry.v, entry.h])
    ).toEqual([
      [11, 4, true],
      [11, 3, true],
      [21, 2, false],
    ]);
    expect(decoded.complete.sc).toEqual(decoded.cachedRequest.sc);
    expect(decoded.complete.i).toEqual(decoded.cachedRequest.i);
    expect(decoded.complete.l).toHaveLength(decoded.cachedRequest.p.l.length);
    expect(decoded.complete.o.missing).toBeNull();
    expect(decoded.complete.c.empty).toEqual([]);
    expect(
      decoded.complete.p[0].p.ro.find((role) => role.t === 'boolean').v
    ).toBe(false);
    expect(
      decoded.complete.p[0].p.ro.find((role) => role.t === 'nothing').v
    ).toBeNull();
    expect(
      decoded.complete.p[0].p.ro.find((role) => role.t === 'decimal').v
    ).toBe('123.4500');
    expect(
      decoded.complete.p[0].p.ro.find((role) => role.t === 'dateTime').v
    ).toBe('2026-10-10T12:34:56Z');
    expect(decoded.complete.p[1].v).toBe(4294967298);
    expect(decoded.complete.p[0].p.ro[9].o).toBe(4294967297);
    expect(
      decoded.complete.pm.slice(1).map((permission) => permission.p.o)
    ).toEqual([1, 2, 4, 8]);
    expect(decoded.complete.pm[1].p.t).toBeNull();
    expect(decoded.complete.l[0].i).toBe(0);
    expect(decoded.complete.l[1].s).toBe('unavailable');
    expect(decoded.complete.l[2].o).toBeNull();
  });

  it('also accepts omitted optional fields without inventing null members', () => {
    const request: ReadRequest = {
      v: 1,
      i: 'discover-omitted',
      k: 'discover',
      p: { l: [], d: [] },
    };
    const failure: ReadFailure = {
      i: request.i,
      k: 'error',
      e: [{ c: 'invalid-request', m: 'Supply a declarative selection.' }],
    };
    const record: ReadRecordPayload = { r: 0 };
    const security: ReadSecurityPayload = { p: [] };
    const permission: ReadPermissionPayload = { c: 'C1', o: 1 };

    for (const value of [request, failure, record, security, permission]) {
      expect(JSON.parse(JSON.stringify(value))).toEqual(value);
    }
    expect(JSON.parse(JSON.stringify(request))).not.toHaveProperty('sc');
    expect(JSON.parse(JSON.stringify(failure))).not.toHaveProperty('sc');
    expect(JSON.parse(JSON.stringify(record))).not.toHaveProperty('ro');
    expect(JSON.parse(JSON.stringify(permission))).not.toHaveProperty('t');
  });

  it('narrows response alternatives and rejects incompatible typed payloads', () => {
    const constructed = messages();
    expect(responseEntryCount(constructed.discovery)).toBe(2);
    expect(responseEntryCount(constructed.complete)).toBe(3);
    expect(responseEntryCount(constructed.failure)).toBe(1);

    // These compile-time guards do not constitute runtime validation of decoded JSON.
    // @ts-expect-error The response discriminator must identify a defined root message.
    const wrongRoot: ReadResponse = { ...constructed.complete, k: 'read' };
    // @ts-expect-error Full record roles and a reuse index are mutually exclusive.
    const mixedRecord: ReadRecordPayload = { ro: [], r: 0 };
    // @ts-expect-error Full security permissions and a reuse index are mutually exclusive.
    const mixedSecurity: ReadSecurityPayload = { p: [], r: 0 };
    // @ts-expect-error Full permission descriptors and a reuse index are mutually exclusive.
    const mixedPermission: ReadPermissionPayload = { c: 'C1', o: 1, r: 0 };
    // @ts-expect-error A create permission has no relation or method operand.
    const createOperand: ReadPermissionPayload = { c: 'C1', o: 1, t: 'string' };
    // @ts-expect-error A read permission needs its relation operand.
    const missingOperand: ReadPermissionPayload = { c: 'C1', o: 2 };
    const combinedOperation: ReadPermissionPayload = {
      c: 'C1',
      // @ts-expect-error Combined operation flags are not permission descriptors.
      o: 3,
      t: 'string',
    };
    // @ts-expect-error The new declarative pull has no procedure slot.
    const procedure: ReadPull = { ...pull(), p: {} };
    // @ts-expect-error Encoded date values use JSON strings, not mutable Date instances.
    const date: ReadValue = new Date('2026-10-10T12:34:56Z');
    void [
      wrongRoot,
      mixedRecord,
      mixedSecurity,
      mixedPermission,
      createOperand,
      missingOperand,
      combinedOperation,
      procedure,
      date,
    ];
  });
});

describe('read protocol runtime validation', () => {
  it('decodes the shared cold and cached requests without changing wire fields', () => {
    for (const request of [fixture.coldRequest, fixture.cachedRequest]) {
      expect(decodeReadRequest(JSON.stringify(request))).toEqual(request);
      expect(() => validateReadRequest(request)).not.toThrow();
    }
  });

  it('decodes complete and failure responses against their original requests', () => {
    for (const response of [fixture.complete, fixture.failure]) {
      expect(
        decodeReadResponse(JSON.stringify(response), fixture.cachedRequest)
      ).toEqual(response);
      expect(() =>
        validateReadResponse(response, fixture.cachedRequest)
      ).not.toThrow();
    }
    expect(
      decodeReadResponse(
        JSON.stringify(fixture.unscopedFailure),
        fixture.coldRequest
      )
    ).toEqual(fixture.unscopedFailure);
  });

  it('matches discovery and empty completion to the corresponding request', () => {
    const discoveryRequest: ReadRequest = {
      ...fixture.coldRequest,
      i: fixture.discovery.i,
      k: 'discover',
    };
    const emptyRequest: ReadRequest = {
      ...fixture.coldRequest,
      i: fixture.emptyComplete.i,
      p: { l: [], d: [] },
    };
    expect(
      decodeReadResponse(JSON.stringify(fixture.discovery), discoveryRequest)
    ).toEqual(fixture.discovery);
    expect(
      decodeReadResponse(JSON.stringify(fixture.emptyComplete), emptyRequest)
    ).toEqual(fixture.emptyComplete);
  });

  it.each([
    '{"v":1,"v":1,"i":"read-1","k":"read","p":{"l":[],"d":[]}}',
    '{"v":1,"i":"read-1","k":"read","p":{"l":[],"l":[],"d":[]}}',
    '{"v":1,"i":"read-1","k":"read","p":{"l":[],"d":[]},"\\u0069":"read-1"}',
    '{"v":1,"i":"read-1","k":"read","p":{"l":[],"d":[]},}',
  ])(
    'rejects malformed JSON or duplicate fields before decoding: %s',
    (json) => {
      expect(() => decodeReadRequest(json)).toThrow();
    }
  );

  it('rejects missing required values and unsupported procedure fields', () => {
    const request = JSON.parse(JSON.stringify(fixture.coldRequest));
    delete request.v;
    expect(() => decodeReadRequest(JSON.stringify(request))).toThrow(/v/);
    request.v = 1;
    request.p.p = { n: 'WriteData' };
    expect(() => decodeReadRequest(JSON.stringify(request))).toThrow(/p/);
  });

  it('rejects mixed full and reused payloads before a consumer can install them', () => {
    const response = JSON.parse(JSON.stringify(fixture.complete));
    response.p[1].p.ro = [];
    expect(() =>
      decodeReadResponse(JSON.stringify(response), fixture.cachedRequest)
    ).toThrow(/p/);
  });
});

interface ValidationChange {
  path: Array<string | number>;
  value?: unknown;
  remove?: boolean;
}

interface ValidationCase {
  name: string;
  request: keyof ContractMessages;
  response?: keyof ContractMessages;
  requestChanges?: ValidationChange[];
  changes?: ValidationChange[];
  valid: boolean;
}

const validationCases: ValidationCase[] = JSON.parse(
  readFileSync(resolve(__dirname, 'read-validation-cases.json'), 'utf8')
);

function applyValidationChanges(
  value: unknown,
  changes: ValidationChange[] = []
): void {
  for (const change of changes) {
    let parent = value as { [key: string | number]: unknown };
    for (const key of change.path.slice(0, -1)) {
      parent = parent[key] as { [key: string | number]: unknown };
    }
    const key = change.path[change.path.length - 1];
    if (change.remove) delete parent[key];
    else
      Object.defineProperty(parent, key, {
        value: change.value,
        enumerable: true,
        writable: true,
        configurable: true,
      });
  }
}

describe('shared read protocol validation cases', () => {
  it.each(validationCases)('$name', (testCase) => {
    const request = JSON.parse(JSON.stringify(fixture[testCase.request]));
    applyValidationChanges(request, testCase.requestChanges);
    const message = testCase.response
      ? JSON.parse(JSON.stringify(fixture[testCase.response]))
      : request;
    applyValidationChanges(message, testCase.changes);
    const decode = () =>
      testCase.response
        ? decodeReadResponse(JSON.stringify(message), request)
        : decodeReadRequest(JSON.stringify(message));
    if (testCase.valid) expect(decode()).toEqual(message);
    else expect(decode).toThrow(/\$.*:/);
  });

  it.each(['1.0000000000000001', '9007199254740990.5', '1e-400'])(
    'rejects a fractional version that JavaScript rounds into an integer: %s',
    (number) => {
      const value = JSON.parse(JSON.stringify(fixture.cachedRequest));
      value.h.r[0].v = 'numeric-version';
      const request = JSON.stringify(value).replace(
        '"numeric-version"',
        number
      );
      expect(() => decodeReadRequest(request)).toThrow(/integer/);
    }
  );

  it('accepts mathematically integral numeric spellings', () => {
    const json =
      '{"v":1.0,"i":"integral","k":"read","p":{"l":[{"o":1e0}],"d":[]}}';
    expect(decodeReadRequest(json).p.l[0].o).toBe(1);
  });

  it('treats prototype-like names as ordinary output and argument names', () => {
    const request = decodeReadRequest(
      '{"v":1,"i":"prototype","k":"read","p":{"l":[],"d":[]},"x":null}'
    );
    const json =
      '{"i":"prototype","k":"complete","sc":{"p":"p","u":1,"w":"w","r":"r"},"l":[],"c":{},"o":{"__proto__":null,"constructor":null},"p":[],"s":[],"pm":[]}';
    const response = decodeReadResponse(json, request) as ReadSuccess;
    expect(Object.keys(response.o)).toEqual(['__proto__', 'constructor']);
  });
});

describe('read JSON lexical validation', () => {
  it('reports the location of duplicate keys inside array and map values', () => {
    const json =
      '{"v":1,"i":"nested","k":"read","p":{"l":[{"a":{"p":1,"p":2}}],"d":[]}}';
    expect(() => decodeReadRequest(json)).toThrow('$.p.l[0].a.p:');
  });

  it('does not confuse escaped quotes and JSON punctuation in values with fields', () => {
    const request = {
      ...fixture.coldRequest,
      p: {
        l: [
          { a: { text: '"{"duplicate":1,"duplicate":2}[]:,' } },
          { a: { text: 'same field in another object' } },
        ],
        d: [],
      },
    };
    expect(decodeReadRequest(JSON.stringify(request))).toEqual(request);
  });

  it('rejects a duplicate output name even when its first value is null', () => {
    const request = {
      ...fixture.coldRequest,
      i: 'empty-1',
      p: { l: [], d: [] },
    };
    const json = JSON.stringify(fixture.emptyComplete).replace(
      '"o":{}',
      '"o":{"value":null,"value":null}'
    );
    expect(() => decodeReadResponse(json, request)).toThrow('$.o.value:');
  });
});

describe('read JSON nesting limit', () => {
  it.each([63, 64, 65])('checks total JSON nesting depth %s', (depth) => {
    // Root, pull, selections array, selection, and argument map consume five levels.
    const nested = '['.repeat(depth - 5) + '1' + ']'.repeat(depth - 5);
    const json = `{"v":1,"i":"depth","k":"read","p":{"l":[{"a":{"value":${nested}}}],"d":[]}}`;
    if (depth <= 64) expect(decodeReadRequest(json)).toEqual(JSON.parse(json));
    else expect(() => decodeReadRequest(json)).toThrow(/64/);
  });
});

describe('read JSON Unicode preservation', () => {
  it.each([
    ['high surrogate in a value', { value: '\ud800' }],
    ['low surrogate in a value', { value: '\udfff' }],
    ['high surrogate in a key', { ['\ud800']: 'value' }],
    ['low surrogate in a key', { ['\udfff']: 'value' }],
  ])('rejects a lone %s', (_name, argument) => {
    const request = {
      ...fixture.coldRequest,
      p: { l: [{ a: argument }], d: [] },
    };
    expect(() => decodeReadRequest(JSON.stringify(request))).toThrow(
      /surrogate/
    );
  });

  it('preserves valid surrogate pairs, ordinary Unicode and literal escape text', () => {
    const request = {
      ...fixture.coldRequest,
      p: {
        l: [{ a: { astral: '\ud83d\ude00', accent: 'é', literal: '\\ud800' } }],
        d: [],
      },
    };
    expect(decodeReadRequest(JSON.stringify(request))).toEqual(request);
  });
});

describe('read response original request validation', () => {
  it('rejects a cycle in a caller-mutated original request with an actionable path', () => {
    const request = JSON.parse(JSON.stringify(fixture.cachedRequest));
    const argument: { cycle?: unknown } = {};
    argument.cycle = argument;
    request.p.l[0].a = argument;
    expect(() =>
      decodeReadResponse(JSON.stringify(fixture.complete), request)
    ).toThrow(/\$request\.p\.l\[0\]\.a\.cycle:.*cycl/i);
  });

  it('rejects excessive nesting in a caller-mutated original request', () => {
    const request = JSON.parse(JSON.stringify(fixture.cachedRequest));
    let nested: unknown = 1;
    for (let depth = 0; depth < 60; depth++) nested = [nested];
    request.p.l[0].a = { value: nested };
    expect(() =>
      decodeReadResponse(JSON.stringify(fixture.complete), request)
    ).toThrow(/\$request.*64/);
  });
});

describe('read JSON source Unicode preservation', () => {
  const high = String.fromCharCode(0xd83d);
  const low = String.fromCharCode(0xde00);
  it.each([
    {
      name: 'escaped high and literal low',
      encoded: '\\uD83D' + low,
      valid: false,
    },
    {
      name: 'literal high and escaped low',
      encoded: high + '\\uDE00',
      valid: false,
    },
    { name: 'fully escaped pair', encoded: '\\uD83D\\uDE00', valid: true },
    { name: 'fully literal pair', encoded: high + low, valid: true },
  ])('$name', ({ encoded, valid }) => {
    const json = JSON.stringify(fixture.coldRequest).replace('cold-1', encoded);
    if (valid) expect(decodeReadRequest(json).i).toBe(high + low);
    else expect(() => decodeReadRequest(json)).toThrow(/surrogate/);
  });
});

describe('read HTTP transport', () => {
  const readUrl = 'https://workspace.example/allors/read';

  function response(json = JSON.stringify(fixture.complete), status = 200) {
    return {
      ok: status >= 200 && status < 300,
      status,
      text: jest.fn(async () => json),
    };
  }

  it('returns the complete detached result with one read and no follow-up request', async () => {
    const httpResponse = response();
    const fetch = jest.fn(async () => httpResponse);
    const transport = new ReadHttpTransport(readUrl, fetch);
    const result = await transport.read(fixture.cachedRequest);

    expect(result).toEqual(fixture.complete);
    expect(result).not.toBe(fixture.complete);
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(fetch).toHaveBeenCalledWith(readUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(fixture.cachedRequest),
      signal: undefined,
    });
    expect(httpResponse.text).toHaveBeenCalledTimes(1);
  });

  it('accepts strict raw requests and discovery responses', async () => {
    const request: ReadRequest = {
      ...fixture.coldRequest,
      i: fixture.discovery.i,
      k: 'discover',
    };
    const fetch = jest.fn(async () =>
      response(JSON.stringify(fixture.discovery))
    );
    const transport = new ReadHttpTransport(readUrl, fetch);
    expect(await transport.read(JSON.stringify(request))).toEqual(
      fixture.discovery
    );
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('captures context, selections, scope, and cache hints before asynchronous work', async () => {
    let completeFetch: (value: ReturnType<typeof response>) => void;
    const fetch = jest.fn<
      Promise<ReturnType<typeof response>>,
      [string, RequestInit]
    >(
      () =>
        new Promise<ReturnType<typeof response>>((resolve) => {
          completeFetch = resolve;
        })
    );
    const request: ReadRequest = JSON.parse(
      JSON.stringify(fixture.cachedRequest)
    );
    const transport = new ReadHttpTransport(readUrl, fetch);
    const pending = transport.read(request);

    request.i = 'mutated';
    request.x = 'other-context';
    request.sc.r = 'other-revision';
    request.p.l.length = 0;
    request.h.s[0].v++;
    request.h.r[0].g.length = 0;
    request.h.p.length = 0;
    completeFetch(response());

    expect(await pending).toEqual(fixture.complete);
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(JSON.parse(fetch.mock.calls[0][1].body as string)).toEqual(
      fixture.cachedRequest
    );
  });

  it.each([
    '{"v":1,"v":1,"i":"read","k":"read","p":{"l":[],"d":[]}}',
    '{"v":1,"i":"read","k":"read","p":{"l":[],"d":[],"p":{"n":"Write"}}}',
    '{"v":1,"i":"read","k":"read","p":{"l":[],"d":[]},"unknown":null}',
  ])('rejects an invalid raw request before HTTP: %s', async (json) => {
    const fetch = jest.fn(async () => response());
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(json)
    ).rejects.toThrow();
    expect(fetch).not.toHaveBeenCalled();
  });

  it('validates typed requests before serialization can discard unknown properties', async () => {
    const request = { ...fixture.cachedRequest, unknown: undefined };
    const fetch = jest.fn(async () => response());
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(request)
    ).rejects.toThrow(/unknown/);
    expect(fetch).not.toHaveBeenCalled();
  });

  it.each([
    JSON.stringify(fixture.complete).replace(
      '"k":"complete"',
      '"k":"complete","k":"complete"'
    ),
    JSON.stringify({ ...fixture.complete, unknown: null }),
    JSON.stringify({ ...fixture.complete, i: 'another-request' }),
    JSON.stringify({
      ...fixture.complete,
      sc: { ...fixture.complete.sc, x: 'other-context' },
    }),
    JSON.stringify({
      ...fixture.complete,
      sc: { ...fixture.complete.sc, p: 'other-population' },
    }),
    JSON.stringify({
      ...fixture.complete,
      p: [{ ...fixture.complete.p[1], v: 42 }],
    }),
  ])('rejects invalid raw response without a retry: %s', async (json) => {
    const fetch = jest.fn(async () => response(json));
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(fixture.cachedRequest)
    ).rejects.toThrow();
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('returns protocol failures without retrying or requesting missing bodies', async () => {
    const fetch = jest.fn(async () =>
      response(JSON.stringify(fixture.failure))
    );
    const transport = new ReadHttpTransport(readUrl, fetch);
    expect(await transport.read(fixture.cachedRequest)).toEqual(
      fixture.failure
    );
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it.each([401, 403, 429, 500, 503])(
    'rejects HTTP status %s without retrying or accepting its body',
    async (status) => {
      const httpResponse = response(undefined, status);
      const fetch = jest.fn(async () => httpResponse);
      await expect(
        new ReadHttpTransport(readUrl, fetch).read(fixture.cachedRequest)
      ).rejects.toThrow(new RegExp(`${status}`));
      expect(fetch).toHaveBeenCalledTimes(1);
      expect(httpResponse.text).not.toHaveBeenCalled();
    }
  );

  it('propagates a network failure after one attempt', async () => {
    const failure = new Error('connection interrupted');
    const fetch = jest.fn(async () => {
      throw failure;
    });
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(fixture.cachedRequest)
    ).rejects.toBe(failure);
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('propagates a body failure after one attempt', async () => {
    const failure = new Error('body interrupted');
    const fetch = jest.fn(async () => ({
      ok: true,
      status: 200,
      text: async () => {
        throw failure;
      },
    }));
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(fixture.cachedRequest)
    ).rejects.toBe(failure);
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('does not send a request that was already cancelled', async () => {
    const cancellation = new AbortController();
    cancellation.abort();
    const fetch = jest.fn(async () => response());
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(
        fixture.cachedRequest,
        cancellation.signal
      )
    ).rejects.toMatchObject({ name: 'AbortError' });
    expect(fetch).not.toHaveBeenCalled();
  });

  it('carries cancellation to HTTP and rejects a response even if HTTP ignores it', async () => {
    const cancellation = new AbortController();
    const httpResponse = response();
    const fetch = jest.fn(async (_url: string, init: RequestInit) => {
      expect(init.signal).toBe(cancellation.signal);
      cancellation.abort();
      return httpResponse;
    });
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(
        fixture.cachedRequest,
        cancellation.signal
      )
    ).rejects.toMatchObject({ name: 'AbortError' });
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(httpResponse.text).not.toHaveBeenCalled();
  });

  it('does not return success after cancellation while reading the body', async () => {
    const cancellation = new AbortController();
    const fetch = jest.fn(async () => ({
      ok: true,
      status: 200,
      text: async () => {
        cancellation.abort();
        return JSON.stringify(fixture.complete);
      },
    }));
    await expect(
      new ReadHttpTransport(readUrl, fetch).read(
        fixture.cachedRequest,
        cancellation.signal
      )
    ).rejects.toMatchObject({ name: 'AbortError' });
    expect(fetch).toHaveBeenCalledTimes(1);
  });
});

describe('scoped immutable connection store', () => {
  function storeScope(context: string | null = null) {
    return new ReadStoreScope(
      'population-1',
      7,
      'Default',
      'schema-1',
      context
    );
  }

  function candidate(
    candidateScope = storeScope(),
    roles: Readonly<Record<string, unknown>> = { name: 'first' },
    grants: readonly ReadSecurityKey[] = [new ReadSecurityKey('grant', 11, 3)],
    revocations: readonly ReadSecurityKey[] = [
      new ReadSecurityKey('revocation', 21, 2),
    ],
    classTag = 'C1',
    dataVersion = 7,
    id = 101
  ) {
    return new ReadStoredRecord(
      candidateScope,
      id,
      classTag,
      dataVersion,
      grants,
      revocations,
      roles
    );
  }

  function complete(store = new ReadStore(storeScope()), version = 3) {
    const key = new ReadSecurityKey('grant', 11, version);
    const record = store.retainRecord(
      candidate(store.scope, { name: `v${version}` }, [key], [])
    );
    const security = store.retainSecurity(
      new ReadStoredSecurity(store.scope, key, [201])
    );
    const permission = store.retainPermission(
      new ReadStoredPermission(store.scope, 201, 'C1', 1)
    );
    return { store, record, security, permission };
  }

  it('interns complete canonical identities and rejects conflicting payloads', () => {
    const store = new ReadStore(storeScope());
    const first = store.retainRecord(
      candidate(store.scope, { b: [1, 2], a: 'one' }, [
        new ReadSecurityKey('grant', 12, 1),
        new ReadSecurityKey('grant', 11, 3),
      ])
    );
    expect(
      store.retainRecord(
        candidate(store.scope, { a: 'one', b: [1, 2] }, [
          new ReadSecurityKey('grant', 11, 3),
          new ReadSecurityKey('grant', 12, 1),
          new ReadSecurityKey('grant', 11, 3),
        ])
      )
    ).toBe(first);
    expect(() =>
      store.retainRecord(
        candidate(store.scope, { a: 'one', b: [2, 1] }, first.grants)
      )
    ).toThrow(/conflict/i);
    expect(store.getRecord(candidate(store.scope, {}, first.grants))).toBe(
      first
    );
  });

  it('separates class, data version, grant membership and revocation membership', () => {
    const store = new ReadStore(storeScope());
    const original = store.retainRecord(candidate());
    const changed = [
      candidate(store.scope, {}, undefined, undefined, 'C2'),
      candidate(store.scope, {}, undefined, undefined, 'C1', 8),
      candidate(store.scope, {}, [new ReadSecurityKey('grant', 12, 3)]),
      candidate(store.scope, {}, [new ReadSecurityKey('grant', 11, 4)]),
      candidate(store.scope, {}, undefined, [
        new ReadSecurityKey('revocation', 22, 2),
      ]),
      candidate(store.scope, {}, undefined, [
        new ReadSecurityKey('revocation', 21, 3),
      ]),
      candidate(store.scope, {}, [], []),
    ].map((record) => store.retainRecord(record));
    expect(new Set([original, ...changed]).size).toBe(8);
  });

  it('owns mutable nested roles on input and output, including Date, binary and maps', () => {
    const date = new Date('2026-10-12T10:00:00Z');
    const binary = new Uint8Array([1, 2, 3]);
    const map = new Map([['value', { members: [101, 102] }]]);
    const nested = {
      values: [date, binary, map],
      dictionary: { name: 'first' },
    };
    const record = candidate(storeScope(), nested);
    date.setTime(0);
    binary[0] = 9;
    map.get('value').members.push(103);
    nested.dictionary.name = 'changed';
    const first = record.roles as typeof nested;
    expect(first.values[0]).toEqual(new Date('2026-10-12T10:00:00Z'));
    expect(first.values[1]).toEqual(new Uint8Array([1, 2, 3]));
    expect((first.values[2] as typeof map).get('value').members).toEqual([
      101, 102,
    ]);
    expect(first.dictionary.name).toBe('first');
    (first.values[0] as Date).setTime(0);
    (first.values[1] as Uint8Array)[1] = 8;
    (first.values[2] as typeof map).clear();
    first.dictionary.name = 'again';
    expect(record.roles).toEqual({
      values: [
        new Date('2026-10-12T10:00:00Z'),
        new Uint8Array([1, 2, 3]),
        new Map([['value', { members: [101, 102] }]]),
      ],
      dictionary: { name: 'first' },
    });
  });

  it('copies binary views and preserves dictionary keys without changing prototypes', () => {
    const bytes = new Uint8Array([9, 1, 2, 8]);
    const view = new DataView(bytes.buffer, 1, 2);
    const roles = JSON.parse(
      '{"__proto__":{"polluted":true},"constructor":"value"}'
    );
    roles.binary = view;
    const record = candidate(storeScope(), roles);
    bytes[1] = 7;
    const copy = record.roles;
    expect((copy.binary as DataView).getUint8(0)).toBe(1);
    (copy.binary as DataView).setUint8(0, 6);
    expect((record.roles.binary as DataView).getUint8(0)).toBe(1);
    expect(Object.prototype.hasOwnProperty.call(copy, '__proto__')).toBe(true);
    expect(copy['__proto__']).toEqual({ polluted: true });
    expect(copy['constructor']).toBe('value');
    expect(Object.prototype.hasOwnProperty.call({}, 'polluted')).toBe(false);
  });

  it('rejects unsupported and cyclic values instead of sharing mutable references', () => {
    const cyclic: Record<string, unknown> = {};
    cyclic.self = cyclic;
    expect(() => candidate(storeScope(), cyclic)).toThrow(/cyclic/i);
    class MutableUnit {
      value = 1;
    }
    expect(() => candidate(storeScope(), { unit: new MutableUnit() })).toThrow(
      /unsupported/i
    );
    expect(() => candidate(storeScope(), { callback: () => 1 })).toThrow(
      /unsupported/i
    );
  });

  it('separates provenance, security body possession and permission possession', () => {
    const store = new ReadStore(storeScope());
    const record = store.retainRecord(candidate());
    expect(store.getSecurity(record.grants[0])).toBeUndefined();
    expect(store.getPermission(201)).toBeUndefined();
    const security = store.retainSecurity(
      new ReadStoredSecurity(store.scope, record.grants[0], [201])
    );
    expect(store.getSecurity(record.grants[0])).toBe(security);
    expect(store.getPermission(201)).toBeUndefined();
    const permission = store.retainPermission(
      new ReadStoredPermission(store.scope, 201, 'C1', 1)
    );
    expect(store.getPermission(201)).toBe(permission);
    expect(store.getRecord(record)).toBe(record);
    expect(record.grants[0].version).toBe(3);
    expect(store.getCurrentRecord(record.id)).toBeUndefined();
  });

  it('interns security by kind and version and canonical permission IDs', () => {
    const store = new ReadStore(storeScope());
    const body = store.retainSecurity(
      new ReadStoredSecurity(
        store.scope,
        new ReadSecurityKey('grant', 11, 3),
        [202, 201, 202]
      )
    );
    expect(
      store.retainSecurity(
        new ReadStoredSecurity(
          store.scope,
          new ReadSecurityKey('grant', 11, 3),
          [201, 202]
        )
      )
    ).toBe(body);
    expect(() =>
      store.retainSecurity(new ReadStoredSecurity(store.scope, body.key, [201]))
    ).toThrow(/conflict/i);
    const later = store.retainSecurity(
      new ReadStoredSecurity(
        store.scope,
        new ReadSecurityKey('grant', 11, 4),
        []
      )
    );
    const revocation = store.retainSecurity(
      new ReadStoredSecurity(
        store.scope,
        new ReadSecurityKey('revocation', 11, 3),
        []
      )
    );
    expect(new Set([body, later, revocation]).size).toBe(3);
  });

  it('separates every scope component and representation-bound permission descriptors', () => {
    const store = new ReadStore(storeScope());
    const changedScopes = [
      new ReadStoreScope('population-2', 7, 'Default', 'schema-1', null),
      new ReadStoreScope('population-1', 8, 'Default', 'schema-1', null),
      new ReadStoreScope('population-1', 7, 'Other', 'schema-1', null),
      new ReadStoreScope('population-1', 7, 'Default', 'schema-2', null),
      storeScope(''),
    ];
    for (const changedScope of changedScopes) {
      expect(() => store.retainRecord(candidate(changedScope))).toThrow(
        /scope/i
      );
      expect(() =>
        store.retainSecurity(
          new ReadStoredSecurity(
            changedScope,
            new ReadSecurityKey('grant', 11, 3),
            []
          )
        )
      ).toThrow(/scope/i);
      expect(() =>
        store.retainPermission(
          new ReadStoredPermission(changedScope, 201, 'C1', 1)
        )
      ).toThrow(/scope/i);
    }
    const first = store.retainPermission(
      new ReadStoredPermission(store.scope, 201, 'C1', 1)
    );
    expect(
      store.retainPermission(
        new ReadStoredPermission(store.scope, 201, 'C1', 1)
      )
    ).toBe(first);
    expect(() =>
      store.retainPermission(
        new ReadStoredPermission(store.scope, 201, 'C2', 1)
      )
    ).toThrow(/conflict/i);
    const revised = new ReadStore(changedScopes[3]);
    expect(
      revised.retainPermission(
        new ReadStoredPermission(revised.scope, 201, 'NewTag', 1)
      ).classTag
    ).toBe('NewTag');
  });

  it('keeps retained snapshots stable while current record and security indexes advance', () => {
    const old = complete();
    const original = old.store.publish({
      records: [old.record],
      security: [old.security],
      permissions: [old.permission],
    });
    const newer = complete(old.store, 4);
    const current = old.store.publish({
      records: [newer.record],
      security: [newer.security],
      permissions: [newer.permission],
    });

    expect(old.store.getCurrentRecord(101)).toBe(newer.record);
    expect(old.store.getCurrentSecurity('grant', 11)).toBe(newer.security);
    expect(old.store.getCurrentPermission(201)).toBe(newer.permission);
    expect(original.records[0].roles).toEqual({ name: 'v3' });
    expect(original.security[0].key.version).toBe(3);
    expect(current.records[0]).toBe(newer.record);

    expect(old.store.getRecord(old.record)).toBe(old.record);
    expect(old.store.getSecurity(old.security.key)).toBe(old.security);

    expect(old.store.getRecord(newer.record)).toBe(newer.record);
    expect(old.store.getSecurity(newer.security.key)).toBe(newer.security);
    expect(old.store.getPermission(201)).toBe(newer.permission);
  });

  it('preserves outside-read membership and separates unavailability from historical contents', () => {
    const old = complete();
    const retained = old.store.publish({
      records: [old.record],
      security: [old.security],
      permissions: [old.permission],
    });
    const other = old.store.retainRecord(
      candidate(old.store.scope, { name: 'other' }, [], [], 'C1', 1, 102)
    );
    old.store.publish({ records: [other] });
    expect(old.store.getCurrentRecord(101)).toBe(old.record);
    old.store.publish({ unavailable: [101] });
    expect(old.store.isUnavailable(101)).toBe(true);
    expect(old.store.getCurrentRecord(101)).toBeUndefined();
    expect(old.store.getCurrentRecord(102)).toBe(other);
    expect(retained.records[0]).toBe(old.record);
    old.store.publish({
      records: [old.record],
      security: [old.security],
      permissions: [old.permission],
    });
    expect(old.store.isUnavailable(101)).toBe(false);
  });

  it('validates full publication before updating any current index', () => {
    const old = complete();
    old.store.publish({
      records: [old.record],
      security: [old.security],
      permissions: [old.permission],
    });
    const newer = complete(old.store, 4);
    expect(() =>
      old.store.publish({
        records: [newer.record],
        security: [newer.security],
        unavailable: [999],
      })
    ).toThrow(/permission/i);
    expect(() =>
      old.store.publish({
        records: [newer.record],
        permissions: [newer.permission],
      })
    ).toThrow(/security/i);
    expect(() =>
      old.store.publish({
        records: [newer.record],
        security: [newer.security],
        permissions: [newer.permission],
        unavailable: [101],
      })
    ).toThrow(/unavailable/i);
    expect(old.store.getCurrentRecord(101)).toBe(old.record);
    expect(old.store.getCurrentSecurity('grant', 11)).toBe(old.security);
    expect(old.store.isUnavailable(999)).toBe(false);
  });

  it('rejects foreign-scope and noncanonical publication without changing current indexes', () => {
    const store = new ReadStore(storeScope());
    const other = complete(new ReadStore(storeScope('other')));
    expect(() => store.retainRecord(other.record)).toThrow(/scope/i);
    expect(() =>
      store.publish({
        records: [other.record],
        security: [other.security],
        permissions: [other.permission],
      })
    ).toThrow(/retain|owned|store|scope/i);
    const own = complete(store);
    expect(() =>
      store.publish({
        records: [candidate(store.scope)],
        security: [own.security],
        permissions: [own.permission],
      })
    ).toThrow(/retain|owned|store|security/i);
    expect(store.getCurrentRecord(101)).toBeUndefined();
  });

  it('clears owner lookup after close while immutable snapshots remain inspectable', async () => {
    const meta = new LazyMetaPopulation({
      c: [['C1', 'First', [], [['name', '7']]]],
      m: [],
    });
    const owned = await normalConnection(async () => {
      throw new Error('Unexpected read');
    }, meta);
    const held = complete(owned.cache);
    const contents = owned.cache.publish({
      records: [held.record],
      security: [held.security],
      permissions: [held.permission],
    });
    await owned.connection.close();
    expect(() => owned.connection.assertActive()).toThrow(
      /closed|closing|ready/i
    );
    expect(owned.cache.getCurrentRecord(101)).toBeUndefined();
    expect(owned.cache.getCurrentSecurity('grant', 11)).toBeUndefined();
    expect(owned.cache.getCurrentPermission(201)).toBeUndefined();
    expect(owned.cache.getRecord(held.record)).toBeUndefined();
    expect(owned.cache.getSecurity(held.security.key)).toBeUndefined();
    expect(owned.cache.getPermission(201)).toBeUndefined();
    expect(contents.records[0].roles).toEqual({ name: 'v3' });
    expect(held.record.roles).toEqual({ name: 'v3' });
  });

  it('rejects invalid database IDs, versions, security kinds and permission shapes', () => {
    for (const id of [0, -1, 1.1, Number.MAX_SAFE_INTEGER + 1]) {
      expect(() => candidate(storeScope(), {}, [], [], 'C1', 0, id)).toThrow();
      expect(() => new ReadSecurityKey('grant', id, 0)).toThrow();
    }
    expect(() => candidate(storeScope(), {}, [], [], 'C1', -1)).toThrow();
    expect(() => new ReadSecurityKey('grant', 11, Number.NaN)).toThrow();
    expect(() =>
      candidate(
        storeScope(),
        {},
        [new ReadSecurityKey('revocation', 21, 2)],
        []
      )
    ).toThrow(/grant/i);
    expect(
      () => new ReadStoredPermission(storeScope(), 201, 'C1', 1, 'role')
    ).toThrow();
    expect(
      () => new ReadStoredPermission(storeScope(), 201, 'C1', 2)
    ).toThrow();
    expect(candidate(storeScope(), {}, [], [], 'C1', 0).dataVersion).toBe(0);
  });

  it('rejects excessive nesting with an actionable error', () => {
    let value: unknown = 1;
    for (let depth = 0; depth < 64; depth++) value = { nested: value };
    expect(() => candidate(storeScope(), { deep: value })).toThrow(
      /depth|nesting/i
    );
  });

  it('keeps historical bodies while current security and permissions replace outside-read authority', () => {
    const old = complete();
    const outside = old.store.retainRecord(
      candidate(old.store.scope, {}, [old.security.key], [], 'C1', 7, 102)
    );
    const original = old.store.publish({
      records: [old.record, outside],
      security: [old.security],
      permissions: [old.permission],
    });
    const nextKey = new ReadSecurityKey('grant', 11, 4);
    const nextRecord = old.store.retainRecord(
      candidate(old.store.scope, { name: 'new' }, [nextKey], [])
    );
    const nextSecurity = old.store.retainSecurity(
      new ReadStoredSecurity(old.store.scope, nextKey, [202])
    );
    const nextPermission = old.store.retainPermission(
      new ReadStoredPermission(old.store.scope, 202, 'C1', 2, 'name')
    );
    old.store.publish({
      records: [nextRecord],
      security: [nextSecurity],
      permissions: [nextPermission],
    });

    expect(old.store.getCurrentRecord(102)).toBe(outside);
    expect(outside.grants[0].version).toBe(3);
    expect(old.store.getCurrentSecurity('grant', 11)).toBe(nextSecurity);
    expect(old.store.getRecord(old.record)).toBe(old.record);
    expect(old.store.getSecurity(old.security.key)).toBe(old.security);
    expect(old.store.getPermission(201)).toBe(old.permission);
    expect(old.store.getCurrentPermission(201)).toBeUndefined();
    expect(old.store.getPermission(202)).toBe(nextPermission);
    expect(original.security[0]).toBe(old.security);
    expect(original.permissions[0]).toBe(old.permission);
  });

  it('rejects extra publication bodies and ambiguous current choices without updating indexes', () => {
    const held = complete();
    const unrelated = held.store.retainSecurity(
      new ReadStoredSecurity(
        held.store.scope,
        new ReadSecurityKey('grant', 12, 1),
        []
      )
    );
    const extraPermission = held.store.retainPermission(
      new ReadStoredPermission(held.store.scope, 202, 'C1', 1)
    );
    const conflicting = held.store.retainSecurity(
      new ReadStoredSecurity(
        held.store.scope,
        new ReadSecurityKey('grant', 11, 4),
        [201]
      )
    );
    const base = {
      records: [held.record],
      security: [held.security],
      permissions: [held.permission],
    };
    expect(() =>
      held.store.publish({ ...base, security: [held.security, unrelated] })
    ).toThrow(/unreferenced/i);
    expect(() =>
      held.store.publish({
        ...base,
        permissions: [held.permission, extraPermission],
      })
    ).toThrow(/unreferenced/i);
    expect(() =>
      held.store.publish({ ...base, security: [held.security, conflicting] })
    ).toThrow(/conflict/i);
    expect(held.store.getCurrentRecord(101)).toBeUndefined();
    expect(held.store.getCurrentSecurity('grant', 11)).toBeUndefined();
    expect(held.store.getCurrentPermission(201)).toBeUndefined();
  });

  it('protects immutable wrappers and completed contents held by ordinary references', () => {
    const held = complete();
    const contents = held.store.publish({
      records: [held.record],
      security: [held.security],
      permissions: [held.permission],
    });
    expect(() =>
      Object.defineProperty(held.record, 'id', { value: 102 })
    ).toThrow();
    expect(() =>
      Object.defineProperty(held.store.scope, 'context', { value: 'changed' })
    ).toThrow();
    expect(() =>
      (held.record.grants as ReadSecurityKey[]).push(
        new ReadSecurityKey('grant', 12, 1)
      )
    ).toThrow();
    expect(() => (contents.records as ReadStoredRecord[]).pop()).toThrow();
    held.store.clear();
    expect(held.store.getRecord(held.record)).toBeUndefined();
    expect(contents.records[0]).toBe(held.record);
    expect(contents.security[0]).toBe(held.security);
  });

  it('looks up exact metadata without constructing a record payload', () => {
    const held = complete();
    expect(held.store.getRecord(101, 'C1', 7, [held.security.key], [])).toBe(
      held.record
    );
    expect(
      held.store.getRecord(101, 'C2', 7, [held.security.key], [])
    ).toBeUndefined();
    expect(held.store.getRecord(101, 'C1', 7, [], [])).toBeUndefined();
  });

  it('rejects owner closure during declaration capture before dispatch', async () => {
    const read = jest.fn(async () => {
      throw new Error('Unexpected dispatch');
    });
    const meta = new LazyMetaPopulation({
      c: [['C1', 'First', [], [['name', '7']]]],
      m: [],
    });
    const owned = await normalConnection(read, meta);
    let closing: Promise<void>;
    const input = {
      get objectId() {
        closing ??= owned.connection.close();
        return 101;
      },
    };
    await expect(owned.session.pull(input)).rejects.toThrow();
    await closing;
    expect(read).not.toHaveBeenCalled();
    expect(owned.cache.getCurrentRecord(101)).toBeUndefined();
  });

  it('rejects duplicate publication entries without updating current indexes', () => {
    const held = complete();
    const base = {
      records: [held.record],
      security: [held.security],
      permissions: [held.permission],
    };
    expect(() =>
      held.store.publish({ ...base, records: [held.record, held.record] })
    ).toThrow(/duplicate|conflict/i);
    expect(() =>
      held.store.publish({ ...base, security: [held.security, held.security] })
    ).toThrow(/duplicate|conflict/i);
    expect(() =>
      held.store.publish({
        ...base,
        permissions: [held.permission, held.permission],
      })
    ).toThrow(/duplicate|conflict/i);
    expect(held.store.getCurrentRecord(101)).toBeUndefined();
  });

  it('validates the captured security permission inventory including sparse arrays', () => {
    expect(
      () =>
        new ReadStoredSecurity(
          storeScope(),
          new ReadSecurityKey('grant', 11, 3),
          new Array<number>(1)
        )
    ).toThrow(/permission/i);
    const input = [201];
    input[Symbol.iterator] = function* () {
      yield 0;
    };
    expect(
      () =>
        new ReadStoredSecurity(
          storeScope(),
          new ReadSecurityKey('grant', 11, 3),
          input
        )
    ).toThrow(/permission/i);
  });

  it('copies ArrayBuffer contents without invoking a caller-controlled copy method', () => {
    const buffer = new ArrayBuffer(2);
    new Uint8Array(buffer).set([1, 2]);
    buffer.slice = () => buffer;
    const record = candidate(storeScope(), { buffer });
    new Uint8Array(buffer)[0] = 9;
    expect([...new Uint8Array(record.roles.buffer as ArrayBuffer)]).toEqual([
      1, 2,
    ]);
    new Uint8Array(record.roles.buffer as ArrayBuffer)[1] = 8;
    expect([...new Uint8Array(record.roles.buffer as ArrayBuffer)]).toEqual([
      1, 2,
    ]);
  });

  it('preserves dictionary ordering equivalence without conflating Map representation', () => {
    const store = new ReadStore(storeScope());
    const first = store.retainRecord(
      candidate(store.scope, {
        values: new Map([
          ['b', 2],
          ['a', 1],
        ]),
      })
    );
    expect(
      store.retainRecord(
        candidate(store.scope, {
          values: new Map([
            ['a', 1],
            ['b', 2],
          ]),
        })
      )
    ).toBe(first);
    expect(() =>
      store.retainRecord(candidate(store.scope, { values: { a: 1, b: 2 } }))
    ).toThrow(/conflict/i);
  });
});

describe('PushRead protocol', () => {
  it('PushRead HTTP captures one request and returns raw independent outcomes even when cancellation follows acknowledgement', async () => {
    const request: PushReadRequest = {
      v: 1,
      i: 'save',
      sc: scope(),
      x: 'tenant=blue',
      push: {},
    };
    const cancellation = new AbortController();
    const response = JSON.stringify({
      i: 'save',
      sc: scope(),
      push: { k: 'committed', n: [] },
      changed: { malformed: true },
    });
    const fetch = jest.fn(async (_url, init) => {
      expect(init.body).toBe(JSON.stringify(request));
      expect(init.signal).toBe(cancellation.signal);
      cancellation.abort();
      return { ok: true, status: 200, text: async () => response };
    });
    expect(
      await new PushReadHttpTransport('/allors/push-read', fetch).pushRead(
        request,
        cancellation.signal
      )
    ).toBe(response);
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('PushRead HTTP rejects pre-dispatch cancellation and oversized requests without transport', async () => {
    const request: PushReadRequest = {
      v: 1,
      i: 'save',
      sc: scope(),
      x: 'tenant=blue',
      push: {},
    };
    const fetch = jest.fn();
    const transport = new PushReadHttpTransport('/allors/push-read', fetch);
    const cancellation = new AbortController();
    cancellation.abort();
    await expect(
      transport.pushRead(request, cancellation.signal)
    ).rejects.toMatchObject({ name: 'AbortError' });
    request.push.n = [
      { w: -1, t: 'C1', r: [{ t: 'string', u: 'x'.repeat(1024 * 1024) }] },
    ];
    await expect(transport.pushRead(request)).rejects.toThrow(/byte limit/i);
    expect(fetch).not.toHaveBeenCalled();
  });

  it.each([401, 503])(
    'PushRead HTTP does not retry status %s or admit its body',
    async (status) => {
      const request: PushReadRequest = {
        v: 1,
        i: 'save',
        sc: scope(),
        x: 'tenant=blue',
        push: {},
      };
      const text = jest.fn(async () => '{}');
      const fetch = jest.fn(async () => ({ ok: false, status, text }));
      await expect(
        new PushReadHttpTransport('/allors/push-read', fetch).pushRead(request)
      ).rejects.toThrow(String(status));
      expect(fetch).toHaveBeenCalledTimes(1);
      expect(text).not.toHaveBeenCalled();
    }
  );

  it('serializes independently constructed save and result messages to the shared fixture', () => {
    const expected = JSON.parse(
      readFileSync(resolve(__dirname, 'push-read-contract.json'), 'utf8')
    );
    const captured = scope();
    const request: PushReadRequest = {
      v: 1,
      i: 'save-1',
      sc: captured,
      x: 'tenant=blue',
      returnChanged: true,
      push: {
        n: [{ w: -1, t: 'C1', r: [{ t: 'number', u: -3 }] }],
        o: [{ d: 101, v: 7, r: [] }],
      },
      discover: { l: [{ o: 101 }], d: [] },
    };
    const response: PushReadResponse = {
      i: 'save-1',
      sc: captured,
      push: { k: 'committed', n: [{ w: -1, d: 102 }] },
      changed: {
        i: 'save-1',
        sc: captured,
        k: 'changed',
        p: [
          {
            i: 101,
            c: 'C1',
            v: 8,
            g: [0],
            r: [],
            p: { ro: [{ t: 'number', v: -3 }] },
          },
        ],
        s: [{ k: 'grant', i: 11, v: 4, p: { p: [201] } }],
        pm: [{ i: 201, p: { c: 'C1', o: 2, t: 'number' } }],
        u: [102],
      },
      discovery: { i: 'save-1', sc: captured, k: 'discovery', p: [], s: [] },
    };
    expect(JSON.parse(JSON.stringify(request))).toEqual(expected.request);
    expect(JSON.parse(JSON.stringify(response))).toEqual(expected.response);
  });

  const messages = JSON.parse(
    readFileSync(resolve(__dirname, 'push-read-contract.json'), 'utf8')
  );
  const cases: Record<
    string,
    Array<{ name: string; json: string }>
  > = JSON.parse(
    readFileSync(resolve(__dirname, 'push-read-validation-cases.json'), 'utf8')
  );
  it('decodes the shared request and independent outcomes', () => {
    expect(decodePushReadRequest(JSON.stringify(messages.request))).toEqual(
      messages.request
    );
    expect(
      decodePushReadResponse(
        JSON.stringify(messages.response),
        messages.request
      )
    ).toEqual(messages.response);
    expect(
      decodePushReadReceipt(JSON.stringify(messages.response), messages.request)
    ).toEqual({
      i: messages.response.i,
      sc: messages.response.sc,
      push: messages.response.push,
    });
  });
  for (const [kind, entries] of Object.entries(cases))
    for (const entry of entries) {
      it(`${kind}: ${entry.name}`, () => {
        const action = () =>
          kind.includes('Request')
            ? decodePushReadRequest(entry.json)
            : kind.includes('Receipt') || kind === 'receiptOnlyResponses'
            ? decodePushReadReceipt(entry.json, messages.request)
            : decodePushReadResponse(entry.json, messages.request);
        if (kind.startsWith('invalid')) expect(action).toThrow();
        else expect(action).not.toThrow();
        if (kind === 'receiptOnlyResponses')
          expect(() =>
            decodePushReadResponse(entry.json, messages.request)
          ).toThrow();
      });
    }
});

// Invoke has its own receipt and never acknowledges local Push changes.
describe('protected atomic Invoke protocol', () => {
  const messages: {
    request: InvokeReadRequest;
    response: InvokeReadResponse;
    rejected: InvokeReadResponse;
    unknown: InvokeReadResponse;
  } = JSON.parse(
    readFileSync(resolve(__dirname, 'invoke-read-contract.json'), 'utf8')
  );
  const cases: Record<
    string,
    { name: string; json: string; request?: InvokeReadRequest }[]
  > = JSON.parse(
    readFileSync(
      resolve(__dirname, 'invoke-read-validation-cases.json'),
      'utf8'
    )
  );

  it('serializes independently constructed invocation and result messages to the shared fixture', () => {
    const request: InvokeReadRequest = {
      v: 1,
      i: 'invoke-1',
      sc: scope(),
      x: 'tenant=blue',
      l: [
        { i: 101, v: 7, m: 'JustDoIt' },
        { i: 102, v: 8, m: 'JustDoIt' },
      ],
      refresh: true,
    };
    const response: InvokeReadResponse = {
      i: 'invoke-1',
      sc: scope(),
      invoke: { k: 'committed' },
      changed: {
        i: 'invoke-1',
        sc: scope(),
        k: 'changed',
        p: [
          {
            i: 101,
            c: 'C1',
            v: 8,
            g: [0],
            r: [],
            p: { ro: [{ t: 'name', v: 'after' }] },
          },
        ],
        s: [{ k: 'grant', i: 11, v: 1, p: { p: [31] } }],
        pm: [{ i: 31, p: { c: 'C1', o: 2, t: 'name' } }],
        u: [102],
      },
    };
    expect(JSON.parse(JSON.stringify(request))).toEqual(messages.request);
    expect(JSON.parse(JSON.stringify(response))).toEqual(messages.response);
  });

  it('decodes the shared invocation request and independent outcomes', () => {
    expect(decodeInvokeReadRequest(JSON.stringify(messages.request))).toEqual(
      messages.request
    );
    for (const response of [
      messages.response,
      messages.rejected,
      messages.unknown,
    ]) {
      expect(
        decodeInvokeReadResponse(JSON.stringify(response), messages.request)
      ).toEqual(response);
      expect(
        decodeInvokeReadReceipt(JSON.stringify(response), messages.request)
          .invoke
      ).toEqual(response.invoke);
    }
  });

  for (const [kind, entries] of Object.entries(cases)) {
    for (const entry of entries) {
      it(`InvokeRead ${kind}: ${entry.name}`, () => {
        const request = entry.request ?? messages.request;
        if (kind === 'validRequests')
          expect(() => decodeInvokeReadRequest(entry.json)).not.toThrow();
        else if (kind === 'invalidRequests')
          expect(() => decodeInvokeReadRequest(entry.json)).toThrow();
        else if (kind === 'validResponses') {
          expect(() =>
            decodeInvokeReadReceipt(entry.json, request)
          ).not.toThrow();
          expect(() =>
            decodeInvokeReadResponse(entry.json, request)
          ).not.toThrow();
        } else if (kind === 'invalidReceipts')
          expect(() => decodeInvokeReadReceipt(entry.json, request)).toThrow();
        else {
          expect(() =>
            decodeInvokeReadReceipt(entry.json, request)
          ).not.toThrow();
          expect(() => decodeInvokeReadResponse(entry.json, request)).toThrow();
        }
      });
    }
  }

  it('InvokeRead HTTP captures one request and returns raw outcomes when cancellation follows receipt', async () => {
    const abort = new AbortController();
    const raw = JSON.stringify(messages.response);
    const fetch = jest.fn(async (_url: string, init: RequestInit) => {
      expect(init.signal).toBe(abort.signal);
      expect(init.body).toBe(JSON.stringify(messages.request));
      abort.abort();
      return { ok: true, status: 200, text: async () => raw };
    });
    const transport = new InvokeReadHttpTransport(
      'https://example.test/invoke-read',
      fetch
    );
    expect(await transport.invokeRead(messages.request, abort.signal)).toBe(
      raw
    );
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it('InvokeRead HTTP rejects pre-dispatch cancellation and oversized requests without transport', async () => {
    const fetch = jest.fn();
    const transport = new InvokeReadHttpTransport(
      'https://example.test/invoke-read',
      fetch
    );
    const abort = new AbortController();
    abort.abort();
    await expect(
      transport.invokeRead(messages.request, abort.signal)
    ).rejects.toThrow(/cancel/i);
    await expect(
      transport.invokeRead({ ...messages.request, i: 'x'.repeat(1024 * 1024) })
    ).rejects.toThrow(/byte limit/i);
    expect(fetch).not.toHaveBeenCalled();
  });

  it.each([401, 503])(
    'InvokeRead HTTP does not retry status %s or admit its body',
    async (status) => {
      const text = jest.fn(async () => JSON.stringify(messages.response));
      const fetch = jest.fn(async () => ({ ok: false, status, text }));
      const transport = new InvokeReadHttpTransport(
        'https://example.test/invoke-read',
        fetch
      );
      await expect(transport.invokeRead(messages.request)).rejects.toThrow(
        String(status)
      );
      expect(fetch).toHaveBeenCalledTimes(1);
      expect(text).not.toHaveBeenCalled();
    }
  );

  it('InvokeRead HTTP rejects oversized responses without another transport attempt', async () => {
    const fetch = jest.fn(async () => ({
      ok: true,
      status: 200,
      text: async () => ' '.repeat(16 * 1024 * 1024 + 1),
    }));
    const transport = new InvokeReadHttpTransport(
      'https://example.test/invoke-read',
      fetch
    );
    await expect(transport.invokeRead(messages.request)).rejects.toThrow(
      /byte limit/i
    );
    expect(fetch).toHaveBeenCalledTimes(1);
  });
});
