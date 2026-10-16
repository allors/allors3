// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- This test exercises the retained wire adapter without adding a normal public export.
import { PullResult as WirePullResult } from '../../../../../adapters-json/src/lib/database/pull/pull-result';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- This test exercises the retained legacy decoder without exposing a normal shared cache.
import { DatabaseRecord } from '../../../../../adapters-json/src/lib/database/database-record';
import {
  DefaultNumberRanges,
  Grant,
  Revocation,
} from '@allors/system/workspace/adapters';
import { Origin } from '@allors/system/workspace/meta';
import { data } from '@allors/default/workspace/meta-json';
import { C1 } from '@allors/default/workspace/domain';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import {
  Workspace,
  DatabaseConnection,
  Session as JsonSession,
  IDatabaseJsonClient,
  ResponseContext,
  pullToJson,
} from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { Fixture } from '../../fixture';
import {
  DiscoverySuccess,
  PullRequest,
  PullResponse,
  ReadRequest,
  ReadResponse,
  ReadSuccess,
} from '@allors/system/common/protocol-json';
import { Strategy as CommonStrategy } from '@allors/system/workspace/adapters';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- Verify internal cache publication through the normal connection lifecycle.
import { ReadStore } from '../../../../../adapters-json/src/lib/read/read-store';
import { IPullResult, ISession, Pull } from '@allors/system/workspace/domain';
import { FetchClient } from '../../fetch-client';

describe('scoped test owner lifecycle', () => {
  const fixtures = new Set<Fixture>();
  afterEach(async () => {
    await Promise.all([...fixtures].map((fixture) => fixture.close()));
    fixtures.clear();
  });
  const scope = {
    workspace: 'Default',
    revision: 'owner-revision',
    context: 'owner-context',
  };

  function setupOwner() {
    const fixture = new Fixture();
    fixtures.add(fixture);
    fixture.metaPopulation = new LazyMetaPopulation(data);
    const read = jest.fn(
      async (request: ReadRequest): Promise<ReadResponse> => {
        const sc = {
          p: 'owner-population',
          u: 1,
          w: scope.workspace,
          r: scope.revision,
          x: scope.context,
        };
        return request.k === 'discover'
          ? { i: request.i, k: 'discovery', sc, p: [], s: [] }
          : {
              i: request.i,
              k: 'complete',
              sc,
              l: [],
              c: {},
              o: {},
              p: [],
              s: [],
              pm: [],
            };
      }
    );
    const transport = {
      read,
      invokeRead: jest.fn().mockImplementation(async (request) => ({
        i: request.i,
        sc: request.sc,
        invoke: { k: 'committed' },
        ...(request.refresh
          ? {
              changed: {
                i: request.i,
                sc: request.sc,
                k: 'changed',
                p: [],
                s: [],
                pm: [],
                u: [],
              },
            }
          : {}),
      })),
      pushRead: jest.fn().mockImplementation(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: {
          k: 'committed',
          n: (request.push.n ?? []).map((entry, index) => ({
            w: entry.w,
            d: 202 + index,
          })),
        },
        ...(request.returnChanged
          ? {
              changed: {
                i: request.i,
                sc: request.sc,
                k: 'changed',
                p: [],
                s: [],
                pm: [],
                u: [],
              },
            }
          : {}),
      })),
      dispose: jest.fn(),
    };
    return { fixture, transport };
  }

  it.each(['read', 'pushRead', 'invokeRead'] as const)(
    'normal factory rejects missing %s transport capability before bootstrap',
    async (capability) => {
      const { fixture, transport } = setupOwner();
      const required: [
        Record<string, never> extends Pick<
          import('@allors/system/workspace/adapters-json').DatabaseConnectionTransport,
          'read'
        >
          ? false
          : true,
        Record<string, never> extends Pick<
          import('@allors/system/workspace/adapters-json').DatabaseConnectionTransport,
          'pushRead'
        >
          ? false
          : true,
        Record<string, never> extends Pick<
          import('@allors/system/workspace/adapters-json').DatabaseConnectionTransport,
          'invokeRead'
        >
          ? false
          : true
      ] = [true, true, true];
      expect(required).toEqual([true, true, true]);
      for (const value of [undefined, 1]) {
        const invalid = { ...transport, [capability]: value };
        await expect(
          DatabaseConnection.create(
            fixture.createConfiguration(),
            invalid as unknown as Parameters<
              typeof DatabaseConnection.create
            >[1],
            scope
          )
        ).rejects.toThrow(capability);
      }
      expect(transport.read).not.toHaveBeenCalled();
      expect(transport.pushRead).not.toHaveBeenCalled();
      expect(transport.invokeRead).not.toHaveBeenCalled();
      expect(transport.dispose).not.toHaveBeenCalled();
    }
  );

  it('normal factory captures configuration options and all transport functions before awaiting bootstrap', async () => {
    const { fixture, transport } = setupOwner();
    const configuration = fixture.createConfiguration();
    const initialRules = [...configuration.rules];
    const options = { ...scope, maxRequestBytes: 1024, maxHints: 2 };
    let release: () => void;
    let dispatched: () => void;
    const started = new Promise<void>((resolve) => (dispatched = resolve));
    const originalRead = transport.read.getMockImplementation();
    transport.read.mockImplementationOnce(
      (request) =>
        new Promise((resolve) => {
          release = () => resolve(originalRead(request));
          dispatched();
        })
    );
    const originalPush = transport.pushRead;
    const originalInvoke = transport.invokeRead;
    const createWorkspace = jest.spyOn(
      DatabaseConnection.prototype,
      'createWorkspace'
    );
    const createSession = jest.spyOn(Workspace.prototype, 'createSession');
    const objectFactory = jest.spyOn(configuration.objectFactory, 'create');
    let connection: DatabaseConnection;
    try {
      const creating = DatabaseConnection.create(
        configuration,
        transport,
        options
      );
      await started;
      configuration.name = 'changed';
      configuration.rules.length = 0;
      options.revision = 'changed';
      options.context = 'changed';
      options.maxRequestBytes = 1;
      transport.read = jest
        .fn()
        .mockRejectedValue(new Error('replacement read'));
      transport.pushRead = jest
        .fn()
        .mockRejectedValue(new Error('replacement push'));
      transport.invokeRead = jest
        .fn()
        .mockRejectedValue(new Error('replacement invoke'));
      release();
      connection = await creating;
      expect(connection.configuration.name).toBe('Default');
      expect(connection.configuration.rules).toEqual(initialRules);
      expect(connection.context).toBe(scope.context);
      expect(createWorkspace).not.toHaveBeenCalled();
      expect(createSession).not.toHaveBeenCalled();
      expect(objectFactory).not.toHaveBeenCalled();
      const session = connection.createWorkspace().createSession();
      expect((await session.push()).outcome).toBe('committed');
      expect((await session.invoke([])).outcome).toBe('committed');
      expect(originalPush).toHaveBeenCalledTimes(1);
      expect(originalInvoke).toHaveBeenCalledTimes(1);
      expect(originalPush.mock.calls[0][0].sc).toMatchObject({
        w: 'Default',
        r: scope.revision,
        x: scope.context,
      });
      expect(transport.read).not.toHaveBeenCalled();
      expect(transport.pushRead).not.toHaveBeenCalled();
      expect(transport.invokeRead).not.toHaveBeenCalled();
    } finally {
      await connection?.close();
      createWorkspace.mockRestore();
      createSession.mockRestore();
      objectFactory.mockRestore();
    }
  });

  it.each(['discovery', 'completion'] as const)(
    'normal factory cancellation during %s publishes no connection and leaves transport borrowed',
    async (phase) => {
      const { fixture, transport } = setupOwner();
      const abort = new AbortController();
      let release: () => void;
      let dispatched: () => void;
      const started = new Promise<void>((resolve) => (dispatched = resolve));
      const original = transport.read.getMockImplementation();
      if (phase === 'completion')
        transport.read.mockImplementationOnce(original);
      transport.read.mockImplementationOnce(
        (request) =>
          new Promise((resolve) => {
            release = () => resolve(original(request));
            dispatched();
          })
      );
      const close = jest.spyOn(JsonSession.prototype, 'close');
      try {
        const creating = DatabaseConnection.create(
          fixture.createConfiguration(),
          transport,
          scope,
          abort.signal
        );
        const failure = creating.catch((error: Error) => error);
        await started;
        abort.abort();
        release();
        expect(await failure).toBeInstanceOf(Error);
        expect(transport.read).toHaveBeenCalledTimes(
          phase === 'discovery' ? 1 : 2
        );
        expect(close).not.toHaveBeenCalled();
        expect(transport.pushRead).not.toHaveBeenCalled();
        expect(transport.invokeRead).not.toHaveBeenCalled();
        expect(transport.dispose).not.toHaveBeenCalled();
      } finally {
        close.mockRestore();
      }
    }
  );

  it.each(['discovery', 'completion'] as const)(
    'normal factory rejects malformed %s without publishing a connection',
    async (phase) => {
      const { fixture, transport } = setupOwner();
      const original = transport.read.getMockImplementation();
      if (phase === 'completion')
        transport.read.mockImplementationOnce(original);
      transport.read.mockImplementationOnce(async (request) => {
        const response = await original(request);
        return { ...response, sc: { ...response.sc, w: 'wrong-workspace' } };
      });
      await expect(
        DatabaseConnection.create(
          fixture.createConfiguration(),
          transport,
          scope
        )
      ).rejects.toThrow();
      expect(transport.read).toHaveBeenCalledTimes(
        phase === 'discovery' ? 1 : 2
      );
      expect(transport.dispose).not.toHaveBeenCalled();
      expect(transport.pushRead).not.toHaveBeenCalled();
      expect(transport.invokeRead).not.toHaveBeenCalled();
    }
  );

  it('normal factory rejects pre-cancelled initialization without sending', async () => {
    const { fixture, transport } = setupOwner();
    const abort = new AbortController();
    abort.abort();
    await expect(
      DatabaseConnection.create(
        fixture.createConfiguration(),
        transport,
        scope,
        abort.signal
      )
    ).rejects.toThrow(/cancel/i);
    expect(transport.read).not.toHaveBeenCalled();
    expect(transport.dispose).not.toHaveBeenCalled();
  });

  it('early Session close is idempotent while retaining values and diffs', async () => {
    const { session, database, fixture } = await setup();
    const sibling = database.createWorkspace().createSession();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    session.checkpoint();
    object.Name = 'retained';
    session.close();
    session.close();
    expect(session.closed).toBe(true);
    expect(sibling.closed).toBe(false);
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('retained');
    expect(object.strategy.diff()).toHaveLength(1);
    expect(() => session.create(fixture.m.C1)).toThrow(/closed/i);
    await database.close();
    expect(sibling.closed).toBe(true);
    expect('unregisterSession' in database).toBe(false);
  });

  it.each([
    'read transport',
    'read application',
    'notification',
    'Session cleanup',
  ] as const)(
    'normal close finishes cleanup after %s failure',
    async (kind) => {
      const { session, database, read } = await setup();
      const sibling = database.createWorkspace().createSession();
      let closing: Promise<void>;
      if (kind === 'Session cleanup') {
        const close = session.close.bind(session);
        jest.spyOn(session, 'close').mockImplementation(() => {
          close();
          throw new Error('Session cleanup failed');
        });
        closing = database.close();
        await expect(closing).rejects.toThrow('Session cleanup failed');
        expect(database.close()).toBe(closing);
        controlledConnections.delete(database);
        const failures: unknown[] = [0, null, undefined, false, '', NaN];
        const outcomes: PromiseSettledResult<void>[] = [];
        for (const failure of failures) {
          const source = await setup();
          const workspace = source.database.createWorkspace();
          class FailingSession extends JsonSession {
            override close(): void {
              super.close();
              throw failure;
            }
          }
          class LaterFailingSession extends JsonSession {
            override close(): void {
              super.close();
              throw new Error('Later cleanup failure');
            }
          }
          const failing = new FailingSession(workspace);
          const later = new LaterFailingSession(workspace);
          const remaining = workspace.createSession();
          const first = source.database.close();
          expect(source.database.close()).toBe(first);
          const [outcome] = await Promise.allSettled([first]);
          outcomes.push(outcome);
          controlledConnections.delete(source.database);
          expect(source.database.closed).toBe(true);
          expect(failing.closed).toBe(true);
          expect(later.closed).toBe(true);
          expect(remaining.closed).toBe(true);
        }
        expect(outcomes.map((outcome) => outcome.status)).toEqual(
          failures.map(() => 'rejected')
        );
        outcomes.forEach((outcome, index) => {
          expect((outcome as PromiseRejectedResult).reason).toBe(
            failures[index]
          );
        });
      } else {
        if (kind === 'notification') await session.pull({ objectId: 101 });
        const closeAndFail = () => {
          closing = database.close();
          throw new Error(`${kind} failed`);
        };
        if (kind === 'read transport')
          read.mockImplementationOnce(closeAndFail);
        if (kind === 'read application') {
          jest
            .spyOn(database.configuration.objectFactory, 'create')
            .mockImplementationOnce(closeAndFail);
        } else if (kind === 'notification') {
          const object = session.instantiate<C1>(101);
          const state = (object.strategy as CommonStrategy).DatabaseOriginState;
          jest
            .spyOn(state, 'acceptRecord')
            .mockImplementationOnce(closeAndFail);
        }
        await expect(session.pull({ objectId: 101 })).rejects.toThrow();
        await closing;
      }
      expect(database.closed).toBe(true);
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      await expect(sibling.pull([])).rejects.toThrow(/closed/i);
    }
  );

  it('normal close and failed creation never dispose borrowed transports', async () => {
    const { fixture, transport } = setupOwner();
    const connection = await DatabaseConnection.create(
      fixture.createConfiguration(),
      transport,
      scope
    );
    await connection.close();
    expect(transport.dispose).not.toHaveBeenCalled();
    await transport.read({
      v: 1,
      i: 'host',
      k: 'read',
      x: scope.context,
      p: { l: [], d: [] },
    });
    transport.read.mockRejectedValueOnce(new Error('bootstrap failed'));
    await expect(
      DatabaseConnection.create(fixture.createConfiguration(), transport, scope)
    ).rejects.toThrow('bootstrap failed');
    expect(transport.dispose).not.toHaveBeenCalled();
    transport.dispose();
    expect(transport.dispose).toHaveBeenCalledTimes(1);
  });

  it('normal constructors cannot bypass readiness or Session registration', async () => {
    const { fixture, transport } = setupOwner();
    const noPublicConstructor: typeof DatabaseConnection extends new (
      ...args: never[]
    ) => unknown
      ? false
      : true = true;
    const noPublicScoped: Extract<
      keyof typeof import('@allors/system/workspace/adapters-json'),
      | 'ScopedDatabaseConnection'
      | 'ScopedSession'
      | 'ScopedSessionOwner'
      | 'ReadStore'
      | 'ReadStorePin'
      | 'ReadCoordinator'
      | 'CompletedRead'
    > extends never
      ? true
      : false = true;
    const noPublicRegistry: Extract<
      keyof DatabaseConnection,
      'registerSession' | 'unregisterSession'
    > extends never
      ? true
      : false = true;
    expect([noPublicConstructor, noPublicScoped, noPublicRegistry]).toEqual([
      true,
      true,
      true,
    ]);
    const initializing = Reflect.construct(DatabaseConnection, [
      fixture.createConfiguration(),
      transport,
      scope,
    ]) as DatabaseConnection;
    try {
      expect(() => new Workspace(initializing)).toThrow(/ready/i);
      expect(
        () =>
          new JsonSession({ database: initializing } as unknown as Workspace)
      ).toThrow(/ready/i);
      expect(transport.read).not.toHaveBeenCalled();
    } finally {
      await initializing.close();
    }
    const owner = await fixture.createConnection(scope, transport);
    const workspace = new Workspace(owner);
    const session = new JsonSession(workspace);
    const sibling = workspace.createSession();
    expect('owner' in session).toBe(false);
    expect('registerSession' in owner).toBe(false);
    expect('unregisterSession' in owner).toBe(false);
    const closing = owner.close();
    expect(() => new Workspace(owner)).toThrow(/closed|ready/i);
    expect(() => new JsonSession(workspace)).toThrow(/closed|ready/i);
    expect(() => workspace.createSession()).toThrow(/closed|ready/i);
    await closing;
    expect(session.closed).toBe(true);
    expect(sibling.closed).toBe(true);
  });

  it('normal connection close shares one completion and closes Sessions from every workspace without saving', async () => {
    const { fixture, transport } = setupOwner();
    const owner = await fixture.createConnection(scope, transport);
    const session = owner.createWorkspace().createSession();
    const sibling = owner.createWorkspace().createSession();
    const created = session.create<C1>(fixture.m.C1);
    created.Name = 'retained edit';
    let release: () => void;
    let arrived: () => void;
    const started = new Promise<void>((resolve) => (arrived = resolve));
    const response = transport.read.getMockImplementation();
    transport.read.mockImplementationOnce(
      (request) =>
        new Promise((resolve) => {
          release = () => resolve(response(request));
          arrived();
        })
    );
    const reading = session.pull([]).catch((error: Error) => error);
    await started;
    const queued = sibling.pull([]).catch((error: Error) => error);
    const first = owner.close();
    try {
      expect(owner.close()).toBe(first);
      let completed = false;
      void first.then(() => (completed = true));
      await Promise.resolve();
      expect(completed).toBe(false);
      expect(session.closed).toBe(false);
      expect(() => owner.createWorkspace()).toThrow(/closed|ready/i);
      release();
      expect(await reading).toBeInstanceOf(Error);
      expect(await queued).toBeInstanceOf(Error);
      await first;
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      expect(
        (created.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBe('retained edit');
      expect(created.strategy.diff()).toHaveLength(1);
      expect(transport.read).toHaveBeenCalledTimes(3);
      expect(transport.pushRead).not.toHaveBeenCalled();
      expect(transport.dispose).not.toHaveBeenCalled();
    } finally {
      release();
      await first;
    }
  });

  it('bootstraps once without legacy construction and closes temporary and tracked Sessions', async () => {
    const { fixture, transport } = setupOwner();
    const create = jest.spyOn(JsonSession.prototype, 'close');
    const close = jest.spyOn(JsonSession.prototype, 'close');
    try {
      const owner = await fixture.createConnection(scope, transport);
      expect(close).not.toHaveBeenCalled();
      expect(create).not.toHaveBeenCalled();
      expect(transport.read.mock.calls.map(([request]) => request.k)).toEqual([
        'discover',
        'read',
      ]);
      expect(
        transport.read.mock.calls.every(([request]) => request.p.l.length === 0)
      ).toBe(true);
      const session = owner.createWorkspace().createSession();
      const sibling = owner.createWorkspace().createSession();
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'immediate push';
      await session.push();
      expect(created.id).toBe(202);
      expect(transport.read).toHaveBeenCalledTimes(2);
      expect(transport.pushRead).toHaveBeenCalledTimes(1);
      await fixture.closeConnection(owner);
      await fixture.closeConnection(owner);
      expect(owner.closed).toBe(true);
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      expect(transport.dispose).toHaveBeenCalledTimes(1);
      expect(
        (created.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBe('immediate push');
      expect(() => owner.createWorkspace().createSession()).toThrow(
        /closed|ready/i
      );
      expect(() => session.create(fixture.m.C1)).toThrow(/closed/i);
      const independentTransport = setupOwner().transport;
      const independent = await fixture.createConnection(
        scope,
        independentTransport
      );
      try {
        expect(transport.read).toHaveBeenCalledTimes(2);
        expect(independentTransport.read).toHaveBeenCalledTimes(2);
        expect(
          independent.createWorkspace().createSession().instantiate(202)
        ).toBeNull();
      } finally {
        await fixture.closeConnection(independent);
      }
    } finally {
      close.mockRestore();
      create.mockRestore();
    }
  });

  it.each([
    ['error', 'discover'],
    ['throw', 'discover'],
    ['error', 'read'],
    ['throw', 'read'],
  ])(
    'closes and retires failed bootstrap (%s during %s) without mutation or retry',
    async (failure, stage) => {
      const { fixture, transport } = setupOwner();
      if (stage === 'read') {
        transport.read.mockImplementationOnce(
          transport.read.getMockImplementation()
        );
      }
      if (failure === 'error') {
        transport.read.mockImplementationOnce(async (request) => ({
          i: request.i,
          k: 'error',
          e: [{ c: 'setup', m: 'Bootstrap rejected' }],
        }));
      } else {
        transport.read.mockRejectedValueOnce(
          new Error('Bootstrap disconnected')
        );
      }
      const close = jest.spyOn(JsonSession.prototype, 'close');
      try {
        await expect(
          fixture.createConnection(scope, transport)
        ).rejects.toThrow(/Bootstrap/);
        expect(close).not.toHaveBeenCalled();
        expect(transport.read).toHaveBeenCalledTimes(stage === 'read' ? 2 : 1);
        expect(transport.invokeRead).not.toHaveBeenCalled();
        expect(transport.pushRead).not.toHaveBeenCalled();
        expect(transport.dispose).toHaveBeenCalledTimes(1);
      } finally {
        close.mockRestore();
      }
    }
  );

  it.each(['push-skip', 'invoke', 'push-refresh'] as const)(
    'drains dispatched %s before cleanup and rejects queued work without replay',
    async (mode) => {
      const method = mode === 'invoke' ? 'invokeRead' : 'pushRead';
      const { fixture, transport } = setupOwner();
      const owner = await fixture.createConnection(scope, transport);
      const session = owner.createWorkspace().createSession();
      const sibling = owner.createWorkspace().createSession();
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'retained edit';
      const localId = created.id;
      let finish: () => void;
      let dispatched: () => void;
      const started = new Promise<void>((resolve) => (dispatched = resolve));
      transport[method].mockImplementationOnce(
        (request) =>
          new Promise((resolve) => {
            finish = () =>
              resolve(
                method === 'pushRead'
                  ? {
                      i: request.i,
                      sc: request.sc,
                      push: { k: 'committed', n: [{ w: localId, d: 202 }] },
                      ...(request.returnChanged
                        ? {
                            changed: {
                              i: request.i,
                              sc: request.sc,
                              k: 'changed',
                              p: [],
                              s: [],
                              pm: [],
                              u: [],
                            },
                          }
                        : {}),
                    }
                  : {
                      i: request.i,
                      sc: request.sc,
                      invoke: { k: 'committed' },
                      changed: {
                        i: request.i,
                        sc: request.sc,
                        k: 'changed',
                        p: [],
                        s: [],
                        pm: [],
                        u: [],
                      },
                    }
              );
            dispatched();
          })
      );
      const pending =
        method === 'pushRead'
          ? session.push({ refresh: mode === 'push-refresh' })
          : session.invoke([]);
      const outcome = pending.then(
        (value) => ({ value }),
        (error: Error) => ({ error })
      );
      await started;
      const queued = sibling.push().then(
        () => undefined,
        (error: Error) => error
      );
      let disposed = false;
      const disposal = fixture.closeConnection(owner).then(() => {
        disposed = true;
      });
      await Promise.resolve();
      expect(owner.closed).toBe(true);
      expect(disposed).toBe(false);
      expect(session.closed).toBe(false);
      expect(sibling.closed).toBe(false);
      expect(transport.dispose).not.toHaveBeenCalled();
      expect(() => owner.createWorkspace().createSession()).toThrow(
        /closed|ready/i
      );
      finish();
      const settled = await outcome;
      if (method === 'pushRead') {
        expect('value' in settled && settled.value).toEqual(
          expect.objectContaining({
            outcome: 'committed',
            reconciliationRequired: true,
          })
        );
      } else {
        expect('value' in settled && settled.value).toMatchObject({
          outcome: 'committed',
          application: {
            acknowledgement: 'not-applicable',
            refresh: 'skipped',
          },
          refresh: { status: 'succeeded' },
          reconciliationRequired: false,
        });
      }
      expect((await queued).message).toMatch(/closed|retired/i);
      await disposal;
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      expect(transport.dispose).toHaveBeenCalledTimes(1);
      expect(transport[method]).toHaveBeenCalledTimes(1);
      expect(transport.read).toHaveBeenCalledTimes(2);
      expect(created.id).toBe(localId);
      expect(
        (created.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBe('retained edit');
      expect(created.strategy.diff()).toHaveLength(1);
    }
  );

  it('captures one credential and endpoint for all scoped transport methods', async () => {
    const { fixture, transport } = setupOwner();
    fixture.jsonClient = new FetchClient('http://owner.example/allors/');
    fixture.jsonClient.testUserId = 'original-user';
    const fetch = jest.fn(async (url: string, init: RequestInit) => {
      const request = JSON.parse(init.body as string);
      const body = url.endsWith('/read')
        ? await transport.read(request)
        : url.endsWith('/push-read')
        ? await transport.pushRead(request)
        : await transport.invokeRead(request);
      return { ok: true, status: 200, text: async () => JSON.stringify(body) };
    });
    const fixed = fixture.jsonClient.createTransport(fetch);
    const owner = await fixture.createConnection(scope, fixed);
    fixture.jsonClient.testUserId = 'replacement-user';
    fixture.jsonClient.baseUrl = 'http://replacement.example/allors/';
    try {
      const session = owner.createWorkspace().createSession();
      await session.pull([]);
      await session.push();
      await session.invoke([]);
      await session.push({ refresh: false });
      expect(new Set(fetch.mock.calls.map(([url]) => url))).toEqual(
        new Set([
          'http://owner.example/allors/read',
          'http://owner.example/allors/invoke-read',
          'http://owner.example/allors/push-read',
        ])
      );
      expect(
        fetch.mock.calls.every(
          ([, init]) =>
            new Headers(init.headers).get('X-Allors-TestUser') ===
            'original-user'
        )
      ).toBe(true);
    } finally {
      await fixture.closeConnection(owner);
    }
    await expect(
      fixed.invokeRead({
        v: 1,
        i: 'disposed',
        sc: {
          p: 'owner-population',
          u: 1,
          w: scope.workspace,
          r: scope.revision,
          x: scope.context,
        },
        x: scope.context,
        l: [],
        refresh: true,
      })
    ).rejects.toThrow(/disposed/i);
  });
});

const controlledConnections = new Set<DatabaseConnection>();
afterEach(async () => {
  const connections = [...controlledConnections];
  controlledConnections.clear();
  await Promise.all(connections.map((connection) => connection.close()));
});

async function ready(
  configuration: Parameters<typeof DatabaseConnection.create>[0],
  transport: Parameters<typeof DatabaseConnection.create>[1],
  options: Parameters<typeof DatabaseConnection.create>[2]
): Promise<DatabaseConnection> {
  let bootstrap = true;
  const read = transport.read.bind(transport);
  const connection = await DatabaseConnection.create(
    configuration,
    {
      ...transport,
      read: (request, cancellation) => {
        if (!bootstrap) return read(request, cancellation);
        const sc = {
          p: 'population',
          u: 1,
          w: configuration.name,
          r: options.revision,
          x: options.context ?? null,
        };
        return Promise.resolve(
          request.k === 'discover'
            ? { i: request.i, k: 'discovery', sc, p: [], s: [] }
            : {
                i: request.i,
                k: 'complete',
                sc,
                l: [],
                c: {},
                o: {},
                p: [],
                s: [],
                pm: [],
              }
        );
      },
    },
    options
  );
  bootstrap = false;
  controlledConnections.add(connection);
  return connection;
}

async function expectFailedBootstrap(
  configuration: Parameters<typeof DatabaseConnection.create>[0],
  mutations: Partial<Parameters<typeof DatabaseConnection.create>[1]> = {}
) {
  const read = jest.fn(async () => {
    throw new Error('Bootstrap rejected');
  });
  const pushRead = mutations.pushRead ?? jest.fn();
  const invokeRead = mutations.invokeRead ?? jest.fn();
  await expect(
    DatabaseConnection.create(
      configuration,
      { read, pushRead, invokeRead },
      { revision: 'test-revision', context: 'scoped' }
    )
  ).rejects.toThrow(/Bootstrap/);
  expect(read).toHaveBeenCalledTimes(1);
  expect(pushRead).not.toHaveBeenCalled();
  expect(invokeRead).not.toHaveBeenCalled();
}

function discovery(request: ReadRequest): DiscoverySuccess {
  return {
    i: request.i,
    k: 'discovery',
    sc: {
      p: 'population',
      u: 1,
      w: 'Default',
      r: 'test-revision',
      x: 'scoped',
    },
    p: [],
    s: [],
  };
}

async function setup() {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const configuration = fixture.createConfiguration();
  const read = jest.fn();
  const push = jest.fn().mockResolvedValue({});
  const pushRead = jest.fn().mockImplementation(async (request) => ({
    i: request.i,
    sc: request.sc,
    push: { k: 'committed', n: [] },
    ...(request.returnChanged
      ? {
          changed: {
            i: request.i,
            sc: request.sc,
            k: 'changed',
            p: [],
            s: [],
            pm: [],
            u: [],
          },
        }
      : {}),
  }));
  const invoke = jest.fn().mockImplementation(async (request) => ({
    i: request.i,
    sc: request.sc,
    invoke: { k: 'committed' },
    ...(request.refresh
      ? {
          changed: {
            i: request.i,
            sc: request.sc,
            k: 'changed',
            p: [],
            s: [],
            pm: [],
            u: [],
          },
        }
      : {}),
  }));
  const database = await ready(
    configuration,
    { read, pushRead, invokeRead: invoke },
    {
      revision: 'test-revision',
      context: 'scoped',
    }
  );
  const session = database.createWorkspace().createSession();
  function response(
    request: ReadRequest,
    version = 1,
    name = 'original'
  ): ReadSuccess {
    return {
      i: request.i,
      k: 'complete',
      sc: {
        p: 'population',
        u: 1,
        w: 'Default',
        r: 'test-revision',
        x: 'scoped',
      },
      l: request.p.l.map((p, i) => ({
        i,
        ...(p.o ? { o: p.o } : {}),
        s: 'resolved',
      })),
      c: {},
      o: { C1: 101 },
      p: [
        {
          i: 101,
          c: fixture.m.C1.tag,
          v: version,
          g: [0],
          r: [],
          p: { ro: [{ t: fixture.m.C1.Name.relationType.tag, v: name }] },
        },
      ],
      s: [{ k: 'grant', i: 11, v: 1, p: { p: [31, 32] } }],
      pm: [
        {
          i: 31,
          p: {
            c: fixture.m.C1.tag,
            o: 2,
            t: fixture.m.C1.Name.relationType.tag,
          },
        },
        {
          i: 32,
          p: {
            c: fixture.m.C1.tag,
            o: 4,
            t: fixture.m.C1.Name.relationType.tag,
          },
        },
      ],
    };
  }
  read.mockImplementation(async (request) =>
    request.k === 'discover' ? discovery(request) : response(request)
  );
  return {
    fixture,
    database,
    session,
    read,
    push,
    pushRead,
    invoke,
    response,
  };
}

async function setupPushRead(
  limits: { maxRequestBytes?: number; maxResponseBytes?: number } = {}
) {
  const existing = await setup();
  const pushRead = jest.fn();
  const database = await ready(
    existing.database.configuration,
    {
      read: existing.read,
      invokeRead: existing.invoke,
      pushRead,
    },
    {
      revision: 'test-revision',
      context: 'scoped',
      ...limits,
    }
  );
  const session = database.createWorkspace().createSession();
  pushRead.mockImplementation(async (request) => {
    const full = existing.response(
      { ...request, p: { l: [], d: [] } },
      2,
      'saved'
    );
    return {
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [] },
      ...(request.returnChanged
        ? {
            changed: {
              i: request.i,
              sc: request.sc,
              k: 'changed',
              p: full.p,
              s: full.s,
              pm: full.pm,
              u: [],
            },
          }
        : {}),
      ...(request.discover
        ? {
            discovery: {
              ...discovery(request),
              p: full.p.map(({ p, ...header }) => header),
              s: full.s.map(({ p, ...header }) => header),
            },
          }
        : {}),
    };
  });
  return { ...existing, database, session, pushRead };
}

describe('scoped Session integration', () => {
  it('ordinary Push defaults to affected refresh and Reset uses its accepted baseline', async () => {
    const { session, pushRead, read, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const saved = await session.push();
    expect(pushRead).toHaveBeenCalledTimes(1);
    expect(pushRead.mock.calls[0][0].returnChanged).toBe(true);
    expect(saved.application.errors.map((error) => error.message)).toEqual([]);
    expect(saved).toMatchObject({
      outcome: 'committed',
      refresh: { status: 'succeeded' },
      application: { acknowledgement: 'applied', refresh: 'applied' },
      reconciliationRequired: false,
    });
    expect('pushRead' in session).toBe(false);
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    object.Name = 'later';
    session.reset();
    expect(object.Name).toBe('saved');
    expect(read).toHaveBeenCalledTimes(2);
  });

  it.each([
    'validation',
    'version',
    'access',
    'missing',
    'derivation',
  ] as const)(
    'ordinary Push returns expected %s rejection diagnostics without ResultError',
    async (kind) => {
      const { session, pushRead, push, fixture, database } =
        await setupPushRead();
      await session.pull([]);
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'retained';
      const id = created.id;
      const failure =
        kind === 'validation'
          ? { _e: 'Correct the submitted value' }
          : kind === 'version'
          ? { _v: [999] }
          : kind === 'access'
          ? { _a: [id, 999] }
          : kind === 'missing'
          ? { _m: [999] }
          : {
              _d: [
                {
                  m: 'Correct the name',
                  r: [
                    { i: id, r: fixture.m.C1.Name.relationType.tag },
                    { i: 999, r: 'unloaded-role' },
                  ],
                },
              ],
            };
      push.mockResolvedValueOnce(failure);
      pushRead.mockImplementationOnce(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: { k: 'rejected', ...failure },
      }));
      const create = jest.spyOn(database.configuration.objectFactory, 'create');
      try {
        let failed = false;
        const saved = await session.push().catch(() => {
          failed = true;
          return undefined;
        });
        expect(failed).toBe(false);
        expect(saved).toMatchObject({
          outcome: 'rejected',
          reconciliationRequired: false,
        });
        expect(saved.hasErrors).toBe(true);
        expect(saved.errorMessage.trim().length).toBeGreaterThan(0);
        if (kind === 'validation') {
          expect(saved.diagnostics.errorMessage).toBe(
            'Correct the submitted value'
          );
          expect(saved.errorMessage).toContain('Correct the submitted value');
        } else if (kind === 'version') {
          expect(saved.diagnostics.versionErrorIds).toEqual([999]);
        } else if (kind === 'access') {
          expect(saved.diagnostics.accessErrorIds).toEqual([id, 999]);
        } else if (kind === 'missing') {
          expect(saved.diagnostics.missingErrorIds).toEqual([999]);
        } else {
          expect(saved.diagnostics.derivationErrors).toEqual([
            {
              message: 'Correct the name',
              relations: [
                {
                  objectId: id,
                  relationTypeTag: fixture.m.C1.Name.relationType.tag,
                },
                { objectId: 999, relationTypeTag: 'unloaded-role' },
              ],
            },
          ]);
          expect(saved.errorMessage).toContain('Correct the name');
        }
        expect(created.id).toBe(id);
        expect(created.Name).toBe('retained');
        expect(session.pushToDatabaseTracker.created.has(created)).toBe(true);
        expect(create).not.toHaveBeenCalled();
        expect(pushRead).toHaveBeenCalledTimes(1);
        expect(push).not.toHaveBeenCalled();
        created.Name = 'corrected';
      } finally {
        create.mockRestore();
      }
    }
  );

  it.each(['empty options', 'refresh false', 'AbortSignal'])(
    'normal Push accepts supported %s arguments without a legacy endpoint',
    async (kind) => {
      const { session, pushRead, push, fixture } = await setup();
      const object = session.create<C1>(fixture.m.C1);
      object.Name = 'retained';
      const localId = object.id;
      pushRead.mockImplementationOnce(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: { k: 'committed', n: [{ w: request.push.n[0].w, d: 202 }] },
        ...(request.returnChanged
          ? {
              changed: {
                i: request.i,
                sc: request.sc,
                k: 'changed',
                p: [],
                s: [],
                pm: [],
                u: [],
              },
            }
          : {}),
      }));
      const args =
        kind === 'AbortSignal'
          ? [undefined, new AbortController().signal]
          : [kind === 'empty options' ? {} : { refresh: false }];
      const saved = await Reflect.apply(session.push, session, args);
      expect(saved).toMatchObject({
        outcome: 'committed',
        creationMappings: [{ workspaceId: localId, databaseId: 202 }],
        refresh: {
          status: kind === 'refresh false' ? 'not-requested' : 'succeeded',
        },
        reconciliationRequired: false,
      });
      expect(pushRead).toHaveBeenCalledTimes(1);
      expect(push).not.toHaveBeenCalled();
      expect(object.id).toBe(202);
      expect(object.Name).toBe('retained');
    }
  );

  it('committed Push preserves independent read evidence when acknowledgement cannot apply', async () => {
    const { session, pushRead } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementationOnce(async (request) => {
      const response = await send(request);
      session.close();
      return response;
    });
    const saved = await Reflect.apply(session.push, session, [
      { discover: [] },
    ]);
    expect(saved).toMatchObject({
      outcome: 'committed',
      refresh: { status: 'succeeded' },
      discovery: { status: 'succeeded' },
      application: { acknowledgement: 'failed', refresh: 'skipped' },
      reconciliationRequired: true,
    });
    expect(saved.pendingQuery).toBeUndefined();
    expect(saved.application.errors.map((error) => error.stage)).toEqual([
      'acknowledgement',
      'discovery',
    ]);
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('refresh failure permits an unrelated explicit mutation without replay', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [] },
      changed: {
        i: request.i,
        sc: request.sc,
        k: 'error',
        e: [{ c: 'read-failed', m: 'Read the affected objects again' }],
      },
    }));
    const saved = await session.push();
    expect(saved).toMatchObject({
      outcome: 'committed',
      refresh: { status: 'failed' },
      reconciliationRequired: false,
    });
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    const created = session.create<C1>(fixture.m.C1);
    created.Name = 'unrelated';
    const id = created.id;
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [{ w: id, d: 202 }] },
    }));
    await expect(
      Reflect.apply(session.push, session, [{ refresh: false }])
    ).resolves.toMatchObject({ outcome: 'committed' });
    expect(created.id).toBe(202);
    expect(pushRead).toHaveBeenCalledTimes(2);
    expect(pushRead.mock.calls[1][0].push.o ?? []).toEqual([]);
    expect(pushRead.mock.calls[1][0].push.n.map((entry) => entry.w)).toEqual([
      id,
    ]);
  });

  it('Push result diagnostics and mappings remain owned after closure', async () => {
    const { session, pushRead, fixture, database } = await setupPushRead();
    await session.pull([]);
    const object = session.create<C1>(fixture.m.C1);
    const id = object.id;
    const diagnostics = {
      k: 'rejected',
      _a: [id, 999],
      _d: [
        {
          m: 'Correct name',
          r: [
            { i: 999, r: 'unloaded-role' },
            { i: id, r: fixture.m.C1.tag },
          ],
        },
      ],
    };
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: diagnostics,
    }));
    const rejected = await session.push();
    const mappings = [{ w: id, d: 202 }];
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: mappings },
    }));
    const committed = await Reflect.apply(session.push, session, [
      { refresh: false },
    ]);
    diagnostics._a[0] = 555;
    diagnostics._d[0].r[0].i = 555;
    mappings[0].d = 555;
    session.close();
    const create = jest.spyOn(database.configuration.objectFactory, 'create');
    try {
      expect(rejected).toHaveProperty('diagnostics.accessErrorIds', [id, 999]);
      expect(rejected).toHaveProperty(
        'diagnostics.derivationErrors.0.relations.0.objectId',
        999
      );
      expect(rejected.accessErrors).toEqual([object]);
      expect(rejected.derivationErrors[0].roles.length).toBe(0);
      expect(committed.creationMappings).toEqual([
        { workspaceId: id, databaseId: 202 },
      ]);
      expect(() =>
        committed.creationMappings.push({ workspaceId: -99, databaseId: 999 })
      ).toThrow();
      expect(create).not.toHaveBeenCalled();
    } finally {
      create.mockRestore();
    }
  });

  it.each(['missing', 'duplicate', 'held-ID collision'] as const)(
    'normal Push rejects %s mappings before any local remap',
    async (kind) => {
      for (const duplicateWorkspace of kind === 'duplicate'
        ? [true, false]
        : [false]) {
        for (const missingResponse of kind === 'missing'
          ? [true, false]
          : [false]) {
          const { session, pushRead, fixture } = await setup();
          const held = session.create<C1>(fixture.m.C1);
          pushRead.mockImplementationOnce(async (request) => ({
            i: request.i,
            sc: request.sc,
            push: { k: 'committed', n: [{ w: request.push.n[0].w, d: 101 }] },
          }));
          await session.push({ refresh: false });
          const first = session.create<C1>(fixture.m.C1);
          const second = session.create<C1>(fixture.m.C1);
          first.Name = 'first';
          second.Name = 'second';
          const ids = [first.id, second.id];
          const tracker = session.pushToDatabaseTracker.created;
          pushRead.mockImplementationOnce(async (request) => {
            if (missingResponse) return null;
            const n =
              kind === 'missing'
                ? [{ w: ids[0], d: 202 }]
                : [
                    { w: ids[0], d: 202 },
                    {
                      w: duplicateWorkspace ? ids[0] : ids[1],
                      d:
                        kind === 'held-ID collision'
                          ? held.id
                          : duplicateWorkspace
                          ? 203
                          : 202,
                    },
                  ];
            request.push.n[0].w = -999;
            return {
              i: request.i,
              sc: request.sc,
              push: { k: 'committed', n },
            };
          });
          const result = await session.push({ refresh: false });
          expect(result.outcome).toBe(
            kind !== 'held-ID collision' ? 'unknown' : 'committed'
          );
          expect(result.reconciliationRequired).toBe(true);
          expect(result.application.acknowledgement).toBe(
            kind !== 'held-ID collision' ? 'not-applicable' : 'failed'
          );
          expect([first.id, second.id]).toEqual(ids);
          expect(session.pushToDatabaseTracker.created).toBe(tracker);
          expect([...tracker]).toEqual([first, second]);
          expect([first.Name, second.Name]).toEqual(['first', 'second']);
          expect(pushRead).toHaveBeenCalledTimes(2);
        }
      }
    }
  );

  it('protected Push applies changed values before return and leaves checkpoints owned by the caller', async () => {
    const { session, pushRead, read, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    session.checkpoint();
    object.Name = 'submitted';
    const saved = await session.push({ refresh: true });
    expect(saved.outcome).toBe('committed');
    expect(saved.application.refresh).toBe('applied');
    expect(saved.reconciliationRequired).toBe(false);
    expect(object.Name).toBe('saved');
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    expect(pushRead).toHaveBeenCalledTimes(1);
    expect(read).toHaveBeenCalledTimes(2);
    expect(
      session.checkpoint().associationsByRoleType.get(fixture.m.C1.Name)
    ).toContain(object);
  });

  it('protected Push pauses editing and overlapping operations while inspection and another Session remain usable', async () => {
    const { session, database, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    let finish: () => void;
    pushRead.mockImplementation(
      (request) =>
        new Promise((resolve) => {
          finish = () =>
            resolve({
              i: request.i,
              sc: request.sc,
              push: { k: 'rejected', e: [{ c: 'test', m: 'Rejected' }] },
            });
        })
    );
    const saving = session.push({ refresh: false });
    const strategy = object.strategy as CommonStrategy;
    for (const edit of [
      () => {
        object.Name = 'later';
      },
      () => session.create(fixture.m.C1),
      () => session.reset(),
      () => session.checkpoint(),
      () => strategy.reset(),
      () => strategy.restoreRole(fixture.m.C1.Name),
      () => strategy.delete(),
      () => strategy.DatabaseOriginState.restoreRole(fixture.m.C1.Name),
    ])
      expect(edit).toThrow(/save.*progress/i);
    expect(object.Name).toBe('submitted');
    expect(strategy.diff().length).toBeGreaterThan(0);
    expect(
      database.createWorkspace().createSession().create(fixture.m.C1)
    ).toBeDefined();
    await expect(session.pull({ objectId: 101 })).rejects.toThrow(
      /save.*progress/i
    );
    await expect(session.push()).rejects.toThrow(/save.*progress/i);
    await expect(session.invoke([])).rejects.toThrow(/save.*progress/i);
    await expect(session.push({ refresh: false })).rejects.toThrow(
      /save.*progress/i
    );
    finish();
    expect((await saving).outcome).toBe('rejected');
    object.Name = 'allowed';
    expect(object.Name).toBe('allowed');
  });

  it('protected Push completes its captured query only on demand and once with merge conflicts preserved', async () => {
    const { session, read, response } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const query = { objectId: 101 };
    const saved = await session.push({
      refresh: true,
      discover: query,
    });
    expect(saved.discovery.status).toBe('succeeded');
    expect(read).toHaveBeenCalledTimes(2);
    query.objectId = 999;
    object.Name = 'intervening';
    read.mockImplementation(async (request) =>
      response(request, 3, 'concurrent')
    );
    const final = await saved.pendingQuery.complete();
    expect(read).toHaveBeenCalledTimes(3);
    expect(read.mock.calls[2][0].k).toBe('read');
    expect(read.mock.calls[2][0].p.l[0].o).toBe(101);
    expect(read.mock.calls[2][0].h.r[0].v).toBe(2);
    expect(final.mergeErrors).toContain(object);
    expect(object.Name).toBe('intervening');
    await expect(saved.pendingQuery.complete()).rejects.toThrow(/once|used/i);
  });

  it('protected Push retains committed receipt and changed records when discovery is malformed', async () => {
    const { session, pushRead } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => ({
      ...(await send(request)),
      discovery: { bad: true },
    }));
    const saved = await session.push({
      refresh: true,
      discover: { objectId: 101 },
    });
    expect(saved.outcome).toBe('committed');
    expect(saved.application.refresh).toBe('applied');
    expect(saved.discovery.status).toBe('failed');
    expect(saved.pendingQuery).toBeUndefined();
    expect(object.Name).toBe('saved');
  });

  it('protected Push retains write blocks after malformed refresh and requires reconciliation after unknown transport outcome', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    pushRead.mockImplementation(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [] },
      changed: { bad: true },
    }));
    const saved = await session.push({ refresh: true });
    expect(saved.outcome).toBe('committed');
    expect(saved.refresh.status).toBe('failed');
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    await session.pull({ objectId: 101 });
    pushRead.mockRejectedValue(new Error('connection lost'));
    const unknown = await session.push({ refresh: false });
    expect(unknown.outcome).toBe('unknown');
    expect(unknown.reconciliationRequired).toBe(true);
    expect(() => session.create(fixture.m.C1)).toThrow(/reconcil/i);
    await expect(session.push()).rejects.toThrow(/reconcil/i);
    expect(object.Name).toBe('original');
    await session.pull({ objectId: 101 });
    expect(() => session.create(fixture.m.C1)).toThrow(/reconcil/i);
  });

  it('protected Push rejects unsupported transports and earlier in-flight operations before dispatch', async () => {
    const legacy = await setup();
    await expect(
      DatabaseConnection.create(
        legacy.database.configuration,
        {
          read: legacy.read,
          invokeRead: legacy.invoke,
        } as unknown as Parameters<typeof DatabaseConnection.create>[1],
        { revision: 'test-revision', context: 'scoped' }
      )
    ).rejects.toThrow(/pushRead/i);
    expect(legacy.read).not.toHaveBeenCalled();
    await legacy.session.pull({ objectId: 101 });
    const { session, read, pushRead } = await setupPushRead();
    let finish: () => void;
    read.mockImplementationOnce(
      (request) =>
        new Promise((resolve) => {
          finish = () => resolve(discovery(request));
        })
    );
    const reading = session.pull({ objectId: 101 });
    await expect(session.push({ refresh: false })).rejects.toThrow(
      /operation|progress/i
    );
    expect(pushRead).not.toHaveBeenCalled();
    await Promise.resolve();
    session.close();
    finish();
    await expect(reading).rejects.toThrow(/closed/i);
  });

  it.each(['changed', 'discovery'] as const)(
    'protected Push preserves independent outcomes when raw %s JSON has duplicate properties',
    async (part) => {
      const { session, pushRead } = await setupPushRead();
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      const send = pushRead.getMockImplementation();
      pushRead.mockImplementation(async (request) => {
        const response = await send(request);
        const json = JSON.stringify(response);
        const value = JSON.stringify(response[part]);
        return json.replace(
          value,
          value.replace('"i":', '"i":"duplicate","i":')
        );
      });
      const result = await session.push({
        refresh: true,
        discover: { objectId: 101 },
      });
      expect(result.outcome).toBe('committed');
      expect(result[part === 'changed' ? 'refresh' : 'discovery'].status).toBe(
        'failed'
      );
      if (part === 'changed') expect(result.discovery.status).toBe('succeeded');
      else {
        expect(result.application.refresh).toBe('applied');
        expect(object.Name).toBe('saved');
      }
    }
  );

  it('protected Push validates changed classes before replacing any Session baseline', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const response = await send(request);
      response.changed.p = [
        { i: 101, c: fixture.m.C2.tag, v: 2, g: [], r: [], p: { ro: [] } },
      ];
      response.changed.s = [];
      response.changed.pm = [];
      return response;
    });
    const saved = await session.push({ refresh: true });
    expect(saved.outcome).toBe('committed');
    expect(saved.refresh.status).toBe('failed');
    expect(object.strategy.cls).toBe(fixture.m.C1);
    expect(object.strategy.canRead(fixture.m.C1.Name)).toBe(true);
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('submitted');
  });

  it('protected Push maps creations in place and applies refreshed security while preserving their prior checkpoint', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    await session.pull([]);
    const object = session.create<C1>(fixture.m.C1);
    const localId = object.id;
    object.Name = 'created';
    session.checkpoint();
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const response = await send(request);
      response.push.n = [{ w: localId, d: 102 }];
      response.changed.p[0].i = 102;
      return response;
    });
    const saved = await session.push({ refresh: true });
    expect(saved.outcome).toBe('committed');
    expect(saved.application.refresh).toBe('applied');
    expect(object.id).toBe(102);
    expect(session.instantiate(102)).toBe(object);
    expect(session.instantiate(localId)).toBeNull();
    expect(object.Name).toBe('saved');
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    expect(
      session.checkpoint().associationsByRoleType.get(fixture.m.C1.Name)
    ).toContain(object);
    expect(session.pushToDatabaseTracker.created).toBeNull();
  });

  it.each(['session', 'shared'] as const)(
    'protected Push validates all mappings before any remap when the %s already owns an ID',
    async (collision) => {
      const { session, database, pushRead, fixture } = await setupPushRead();
      if (collision === 'session') await session.pull({ objectId: 101 });
      else
        await database
          .createWorkspace()
          .createSession()
          .pull({ objectId: 101 });
      const first = session.create<C1>(fixture.m.C1);
      const second = session.create<C1>(fixture.m.C1);
      const ids = [first.id, second.id];
      pushRead.mockImplementation(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: {
          k: 'committed',
          n: [
            { w: first.id, d: 102 },
            { w: second.id, d: 101 },
          ],
        },
      }));
      const saved = await session.push({ refresh: false });
      expect(saved.outcome).toBe('committed');
      expect(saved.reconciliationRequired).toBe(true);
      expect(saved.application.errors[0].message).toMatch(/collid/i);
      expect([first.id, second.id]).toEqual(ids);
      expect(session.saving).toBe(false);
      expect(() => session.reset()).toThrow(/reconcil/i);
    }
  );

  it.each(['duplicate', 'missing'] as const)(
    'protected Push rejects %s mappings without partial identity updates or replay',
    async (kind) => {
      const { session, pushRead, fixture } = await setupPushRead();
      await session.pull([]);
      const first = session.create<C1>(fixture.m.C1);
      const second = session.create<C1>(fixture.m.C1);
      const ids = [first.id, second.id];
      pushRead.mockImplementation(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: {
          k: 'committed',
          n:
            kind === 'missing'
              ? [{ w: first.id, d: 102 }]
              : [
                  { w: first.id, d: 102 },
                  { w: second.id, d: 102 },
                ],
        },
      }));
      const saved = await session.push({ refresh: false });
      expect(saved.outcome).toBe('unknown');
      expect(saved.reconciliationRequired).toBe(true);
      expect([first.id, second.id]).toEqual(ids);
      expect(pushRead).toHaveBeenCalledTimes(1);
    }
  );

  it('protected Push without refreshed bodies acknowledges mappings and preserves the pushed write block until Pull', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const saved = await session.push({ refresh: false });
    expect(saved.outcome).toBe('committed');
    expect(saved.refresh.status).toBe('not-requested');
    expect(saved.pendingQuery).toBeUndefined();
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    expect(object.Name).toBe('submitted');
    await session.pull({ objectId: 101 });
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('protected Push guards every direct origin and Strategy editing entrypoint including Session-origin roles', async () => {
    const { session, fixture, pushRead } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const strategy = object.strategy as CommonStrategy;
    const state = strategy.DatabaseOriginState;
    const local = session.sessionOriginState;
    const unit = fixture.m.C1.Name;
    const sessionRole = [...fixture.m.C1.roleTypes].find(
      (role) => role.origin === Origin.Session
    );
    const one = [...fixture.m.C1.roleTypes].find(
      (role) => role.objectType.isComposite && role.isOne
    );
    const many = [...fixture.m.C1.roleTypes].find(
      (role) => role.objectType.isComposite && role.isMany
    );
    const saving = session.push({ refresh: false });
    const actions = [
      () => strategy.setUnitRole(sessionRole, null),
      () => strategy.restoreRole(sessionRole),
      () => strategy.setCompositeRole(one, null),
      () => strategy.setCompositesRole(many, []),
      () => strategy.addCompositesRole(many, object),
      () => strategy.removeCompositesRole(many, object),
      () => state.setUnitRole(unit, null),
      () => state.setCompositeRole(one, null),
      () => state.setCompositesRole(many, []),
      () => state.addCompositesRole(many, object),
      () => state.removeCompositesRole(many, object),
      () => state.reset(),
      () => state.checkpoint(null),
      () => local.setUnitRole(object, sessionRole, null),
      () => local.setCompositeRole(object, one, null),
      () => local.setCompositesRole(object, many, []),
      () => local.addCompositesRole(object, many, object),
      () => local.removeCompositesRole(object, many, object),
      () => local.checkpoint(null),
      () => session.activate([]),
    ];
    actions.forEach((action) => expect(action).toThrow(/save.*progress/i));
    await saving;
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('protected Push leaves no gate during the UI gap and advertises inventory obtained from another Session at completion', async () => {
    const { session, database, read, response } = await setupPushRead();
    await session.pull({ objectId: 101 });
    const saved = await session.push({
      refresh: true,
      discover: { objectId: 101 },
    });
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : response(request, 3, 'newer')
    );
    await database.createWorkspace().createSession().pull({ objectId: 101 });
    await saved.pendingQuery.complete();
    const request = read.mock.calls[4][0];
    expect(request.k).toBe('read');
    expect(
      request.h.r.some((record) => record.i === 101 && record.v === 3)
    ).toBe(true);
    expect(read).toHaveBeenCalledTimes(5);
  });

  it('protected Push rejects failed bootstrap and bundled new-object references before dispatch and releases its pause', async () => {
    const { session, fixture, pushRead } = await setupPushRead();
    await expectFailedBootstrap(fixture.createConfiguration(), { pushRead });
    await session.pull([]);
    const object = session.create<C1>(fixture.m.C1);
    await expect(
      session.push({ refresh: false, discover: { object } })
    ).rejects.toThrow(/positive/i);
    expect(session.saving).toBe(false);
    object.Name = 'still editable';
    expect(object.Name).toBe('still editable');
    expect(pushRead).not.toHaveBeenCalled();
  });

  it('protected Push rejects cancellation before dispatch but preserves a received committed acknowledgement after cancellation', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const before = new AbortController();
    before.abort();
    await expect(
      session.push({ refresh: false }, before.signal)
    ).rejects.toMatchObject({
      name: 'AbortError',
    });
    expect(pushRead).not.toHaveBeenCalled();
    expect(session.reconciliationRequired).toBe(false);
    object.Name = 'submitted';
    const after = new AbortController();
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      after.abort();
      return result;
    });
    const saved = await session.push(
      { refresh: true, discover: { objectId: 101 } },
      after.signal
    );
    expect(saved.outcome).toBe('committed');
    expect(saved.refresh.status).toBe('succeeded');
    expect(saved.application.refresh).toBe('skipped');
    expect(saved.discovery.status).toBe('succeeded');
    expect(saved.pendingQuery).toBeUndefined();
    expect(saved.reconciliationRequired).toBe(false);
    expect(session.saving).toBe(false);
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
  });

  it.each(['close', 'retire'] as const)(
    'protected Push preserves commitment when the %s happens during dispatch and prevents pending completion after lifetime ends',
    async (lifetime) => {
      const first = await setupPushRead();
      await first.session.pull([]);
      const send = first.pushRead.getMockImplementation();
      first.pushRead.mockImplementation(async (request) => {
        const result = await send(request);
        if (lifetime === 'close') first.session.close();
        else void first.database.close();
        return result;
      });
      const saved = await first.session.push({ refresh: false });
      expect(saved.outcome).toBe('committed');
      expect(saved.reconciliationRequired).toBe(true);
      expect(saved.application.errors[0].message).toMatch(/closed|retired/i);
      expect(first.session.saving).toBe(false);
      const second = await setupPushRead();
      await second.session.pull([]);
      const waiting = await second.session.push({
        refresh: false,
        discover: [],
      });
      if (lifetime === 'close') second.session.close();
      else void second.database.close();
      await expect(waiting.pendingQuery.complete()).rejects.toThrow(
        /closed|retired/i
      );
      expect(second.read).toHaveBeenCalledTimes(2);
    }
  );

  it('protected Push propagates changed unavailable authority while keeping existing values inspectable', async () => {
    const { session, pushRead, fixture } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      result.changed = { ...result.changed, p: [], s: [], pm: [], u: [101] };
      return result;
    });
    const result = await session.push({ refresh: true });
    expect(result.application.refresh).toBe('applied');
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('original');
  });

  it('protected Push rejects discovery classes absent from captured metadata before exposing pendingQuery', async () => {
    const { session, pushRead } = await setupPushRead();
    await session.pull([]);
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      result.discovery.p[0].c = 'absent';
      return result;
    });
    const saved = await session.push({ refresh: false, discover: [] });
    expect(saved.outcome).toBe('committed');
    expect(saved.discovery.status).toBe('failed');
    expect(saved.pendingQuery).toBeUndefined();
  });

  it('protected Push rejects a new object passed as a bundled query argument before transport', async () => {
    const { session, fixture, pushRead } = await setupPushRead();
    await session.pull([]);
    const created = session.create<C1>(fixture.m.C1);
    await expect(
      session.push({
        refresh: false,
        discover: { objectId: 101, arguments: { target: created } },
      })
    ).rejects.toThrow(/positive|database|unsaved/i);
    expect(pushRead).not.toHaveBeenCalled();
    expect(session.saving).toBe(false);
  });

  it('protected Push retires a generation when a valid independent failure reports a changed scope without losing commitment', async () => {
    const { session, database, pushRead } = await setupPushRead();
    await session.pull([]);
    pushRead.mockImplementation(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [] },
      changed: {
        i: request.i,
        sc: { ...request.sc, p: 'changed-population' },
        k: 'error',
        e: [{ c: 'scope', m: 'Scope changed' }],
      },
    }));
    const result = await session.push({ refresh: true });
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('failed');
    expect(database.closed).toBe(true);
  });

  it('protected Push enforces configured request and raw-response byte bounds without replay', async () => {
    const bounded = await setupPushRead({ maxRequestBytes: 1024 });
    const object = (await bounded.session.pull({ objectId: 101 })).object<C1>(
      'C1'
    );
    object.Name = 'x'.repeat(2048);
    await expect(bounded.session.push({ refresh: false })).rejects.toThrow(
      /byte limit/i
    );
    expect(bounded.pushRead).not.toHaveBeenCalled();
    expect(bounded.session.saving).toBe(false);
    const large = await setupPushRead({ maxResponseBytes: 1024 });
    await large.session.pull([]);
    large.pushRead.mockImplementation(async (request) =>
      JSON.stringify({
        i: request.i,
        sc: request.sc,
        push: { k: 'unknown', e: [{ c: 'test', m: 'x'.repeat(2048) }] },
      })
    );
    const result = await large.session.push({ refresh: false });
    expect(result.outcome).toBe('unknown');
    expect(result.diagnostics.errors[0].message).toMatch(/byte limit/i);
    expect(large.pushRead).toHaveBeenCalledTimes(1);
  });

  it('protected Push bounds the combined object response before publishing either individually valid read part', async () => {
    const { session, pushRead } = await setupPushRead({
      maxResponseBytes: 1200,
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      result.changed.p[0].p.ro[0].v = 'x'.repeat(250);
      for (let id = 102; id < 108; id++)
        result.discovery.p.push({ ...result.discovery.p[0], i: id });
      expect(JSON.stringify(result.changed).length).toBeLessThan(1200);
      expect(JSON.stringify(result.discovery).length).toBeLessThan(1200);
      expect(JSON.stringify(result).length).toBeGreaterThan(1200);
      return result;
    });
    const saved = await session.push({ refresh: true, discover: [] });
    expect(saved.outcome).toBe('unknown');
    expect(saved.reconciliationRequired).toBe(true);
    expect(saved.diagnostics.errors[0].message).toMatch(/byte limit/i);
    expect(object.Name).toBe('submitted');
  });

  it('protected Push keeps the editing pause active in object factories and dispatches no pending read during the UI gap', async () => {
    const { session, database, read, pushRead, fixture } =
      await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      result.changed.p.push({
        i: 102,
        c: fixture.m.C2.tag,
        v: 1,
        g: [],
        r: [],
        p: { ro: [] },
      });
      return result;
    });
    const factory = database.configuration.objectFactory;
    const create = factory.create.bind(factory);
    const spy = jest.spyOn(factory, 'create').mockImplementation((strategy) => {
      expect(() => {
        object.Name = 'reentrant';
      }).toThrow(/save.*progress/i);
      expect(() => session.checkpoint()).toThrow(/save.*progress/i);
      return create(strategy);
    });
    try {
      const saved = await session.push({
        refresh: true,
        discover: [],
      });
      expect(saved.application.refresh).toBe('applied');
      expect(object.Name).toBe('saved');
      expect(session.instantiate(102)).not.toBeNull();
    } finally {
      spy.mockRestore();
    }
    pushRead.mockImplementation(send);
    const dispatchedReads = read.mock.calls.length;
    const saved = await session.push({ refresh: false, discover: [] });
    expect(saved.pendingQuery).toBeDefined();
    expect(read).toHaveBeenCalledTimes(dispatchedReads);
    await database.close();
    await expect(saved.pendingQuery.complete()).rejects.toThrow(
      /closed|retired/i
    );
    expect(read).toHaveBeenCalledTimes(dispatchedReads);
  });

  it('protected Push permits edits while explicit final completion is in flight and merges against the acknowledged baseline', async () => {
    const { session, read, response } = await setupPushRead();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const saved = await session.push({
      refresh: true,
      discover: { objectId: 101 },
    });
    let finish: () => void;
    let started: () => void;
    const dispatched = new Promise<void>((resolve) => {
      started = resolve;
    });
    read.mockImplementationOnce(
      (request) =>
        new Promise((resolve) => {
          finish = () => resolve(response(request, 3, 'saved'));
          started();
        })
    );
    const completing = saved.pendingQuery.complete();
    await dispatched;
    object.Name = 'during final read';
    await expect(session.push({ refresh: false })).rejects.toThrow(
      /operation.*progress/i
    );
    finish();
    const final = await completing;
    expect(final.mergeErrors ?? []).toHaveLength(0);
    expect(object.Name).toBe('during final read');
    expect(object.strategy.version).toBe(3);
  });

  it('protected Push rejects both read parts when one object response part alone exceeds the configured cap', async () => {
    const { session, pushRead } = await setupPushRead({
      maxResponseBytes: 1200,
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'submitted';
    const send = pushRead.getMockImplementation();
    pushRead.mockImplementation(async (request) => {
      const result = await send(request);
      result.changed.p[0].p.ro[0].v = 'x'.repeat(2000);
      return result;
    });
    const saved = await session.push({ refresh: true, discover: [] });
    expect(saved.outcome).toBe('unknown');
    expect(saved.reconciliationRequired).toBe(true);
    expect(saved.diagnostics.errors[0].message).toMatch(/byte limit/i);
    expect(object.Name).toBe('submitted');
  });

  it('protected Push never maps a new object onto an ID already known unavailable in the bound population', async () => {
    const { session, database, pushRead, read, response, fixture } =
      await setupPushRead();
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            l: [{ i: 0, o: 999, s: 'unavailable' }],
            p: [],
            s: [],
            pm: [],
            o: {},
          }
    );
    await database.createWorkspace().createSession().pull({ objectId: 999 });
    const object = session.create<C1>(fixture.m.C1);
    const id = object.id;
    pushRead.mockImplementation(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [{ w: id, d: 999 }] },
    }));
    const saved = await session.push({ refresh: false });
    expect(saved.outcome).toBe('committed');
    expect(saved.reconciliationRequired).toBe(true);
    expect(object.id).toBe(id);
  });

  it('protected Push requires a complete serializable object envelope before trusting its receipt', async () => {
    const { session, pushRead } = await setupPushRead();
    await session.pull([]);
    const getter = jest.fn(() => []);
    pushRead.mockImplementation(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [] },
      changed: Object.defineProperty({}, 'p', {
        enumerable: true,
        get: getter,
      }),
    }));
    const result = await session.push({ refresh: true });
    expect(result.outcome).toBe('unknown');
    expect(result.reconciliationRequired).toBe(true);
    expect(getter).not.toHaveBeenCalled();
  });

  it('discovers then completes one protected read, owns its baseline, and forbids cross-Session cache instantiation', async () => {
    const { fixture, database, session, read } = await setup();
    const result = await session.pull({ objectId: 101 });
    const object = result.object<C1>('c1');
    expect(object.Name).toBe('original');
    expect(result.scope.context).toBe('scoped');
    expect(result.outcomes).toEqual([{ i: 0, o: 101, s: 'resolved' }]);
    expect(read).toHaveBeenCalledTimes(2);
    expect(
      database.createWorkspace().createSession().instantiate(101)
    ).toBeNull();
    session.close();
    session.close();
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('original');
    await expect(session.pull({ objectId: 101 })).rejects.toThrow(/closed/i);
  });

  it('keeps conflicts and reports unavailable inputs without losing local edits', async () => {
    const { fixture, session, read, pushRead, response } = await setup();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'local';
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : response(request, 2, 'remote')
    );
    expect((await session.pull({ objectId: 101 })).mergeErrors).toEqual([
      object,
    ]);
    expect(object.Name).toBe('local');
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            l: [{ i: 0, o: 101, s: 'unavailable' }],
            p: [],
            s: [],
            pm: [],
            o: { C1: null },
          }
    );
    const unavailable = await session.pull({ objectId: 101 });
    expect(unavailable.hasErrors).toBe(false);
    expect(unavailable.object('C1')).toBeNull();
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    await expect(session.push()).rejects.toThrow(/unavailable/i);
    expect(pushRead).not.toHaveBeenCalled();
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('local');
  });

  it('requires successful factory bootstrap for mutations and never retries transport failure', async () => {
    const { fixture, session, read, pushRead } = await setup();
    await expectFailedBootstrap(fixture.createConfiguration(), { pushRead });
    expect(pushRead).not.toHaveBeenCalled();
    await session.pull({ objectId: 101 });
    pushRead.mockRejectedValue(new Error('possibly committed'));
    const result = await session.push();
    expect(result.outcome).toBe('unknown');
    expect(result.reconciliationRequired).toBe(true);
    expect(result.diagnostics.errors[0].cause.message).toBe(
      'possibly committed'
    );
    await expect(session.push()).rejects.toThrow(/reconcil/i);
    expect(pushRead).toHaveBeenCalledTimes(1);
    expect(read).toHaveBeenCalledTimes(2);
  });

  it('exposes no standalone result-value API on wire adaptation or normal pull results', async () => {
    const { session, read, response } = await setup();
    const client: jest.Mocked<IDatabaseJsonClient> = {
      pull: jest.fn().mockResolvedValue({
        c: { empty: [] },
        o: { missing: null },
        v: { retired: '42' },
        p: [],
        g: [],
        r: [],
      }),
      sync: jest.fn(),
      push: jest.fn(),
      invoke: jest.fn(),
      access: jest.fn(),
      permission: jest.fn(),
    };
    const legacyResult = new WirePullResult(
      session,
      await client.pull({ x: session.context, l: [] })
    );
    read.mockImplementation(async (request: ReadRequest) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            c: { empty: [] },
            o: { C1: 101, missing: null },
          }
    );
    const scopedResult = await session.pull({ objectId: 101 });

    const noPullResultValues: 'values' extends keyof IPullResult
      ? false
      : true = true;
    const noPullResultValue: 'value' extends keyof IPullResult ? false : true =
      true;
    const noPullResponseValues: 'v' extends keyof PullResponse ? false : true =
      true;
    const noReadSuccessValues: 'v' extends keyof ReadSuccess ? false : true =
      true;
    expect([
      noPullResultValues,
      noPullResultValue,
      noPullResponseValues,
      noReadSuccessValues,
    ]).toEqual([true, true, true, true]);
    expect(legacyResult.object('missing')).toBeNull();
    expect(legacyResult.collection('empty')).toEqual([]);
    expect(scopedResult.object('C1').id).toBe(101);
    expect(scopedResult.object('missing')).toBeNull();
    expect(scopedResult.collection('empty')).toEqual([]);
    expect(client.pull).toHaveBeenCalledTimes(1);
    expect(client.sync).not.toHaveBeenCalled();
    expect(client.push).not.toHaveBeenCalled();
    expect(client.invoke).not.toHaveBeenCalled();
    expect(client.access).not.toHaveBeenCalled();
    expect(client.permission).not.toHaveBeenCalled();
    expect(read).toHaveBeenCalledTimes(2);
    expect([
      'values' in legacyResult,
      'value' in legacyResult,
      'values' in scopedResult,
      'value' in scopedResult,
    ]).toEqual([false, false, false, false]);
  });

  it('exposes no procedure call on normal Sessions', async () => {
    const { session, read } = await setup();

    const noSessionCall: 'call' extends keyof ISession ? false : true = true;
    const noPullProcedure: 'p' extends keyof PullRequest ? false : true = true;
    expect(noSessionCall).toBe(true);
    expect(noPullProcedure).toBe(true);
    expect('call' in JsonSession.prototype).toBe(false);
    expect('Call' in JsonSession.prototype).toBe(false);
    expect('call' in session).toBe(false);
    expect(read).not.toHaveBeenCalled();
  });

  it('rejects context changes and zero IDs before transport', async () => {
    const { session, read } = await setup();
    await expect(session.pull({ objectId: 0 })).rejects.toThrow(/ID|id/i);
    session.context = 'different';
    await expect(session.pull({ objectId: 101 })).rejects.toThrow(/context/i);
    expect(read).not.toHaveBeenCalled();
  });

  it.each([
    ['extentRef string', 'extentRef', '01234567-89ab-cdef-0123-456789abcdef'],
    ['extentRef null', 'extentRef', null],
    ['selectRef string', 'selectRef', '01234567-89ab-cdef-0123-456789abcdef'],
    ['selectRef null', 'selectRef', null],
  ])(
    'rejects removed declaration references before read or save transport (%s)',
    async (_name, key, value) => {
      const { fixture, session, read, pushRead } = await setupPushRead();
      const pull = {
        extent: { kind: 'Filter', objectType: fixture.m.C1 },
        results: [{ name: 'objects' }],
      };
      if (key === 'extentRef') pull[key] = value;
      else pull.results[0][key] = value;
      const invalid = pull as unknown as Pull;

      await expect(
        session.pull(invalid).then(() => 'dispatched')
      ).rejects.toThrow(/unsupported/i);
      expect(read).not.toHaveBeenCalled();
      await session.pull([]);
      read.mockClear();
      const object = session.create<C1>(fixture.m.C1);
      await expect(
        session.push({ refresh: false, discover: invalid })
      ).rejects.toThrow(/unsupported/i);
      expect(read).not.toHaveBeenCalled();
      expect(pushRead).not.toHaveBeenCalled();
      expect(session.reconciliationRequired).toBe(false);
      expect(session.saving).toBe(false);
      object.Name = 'still editable';
      expect(object.Name).toBe('still editable');
    }
  );

  it('serializes inline pulls and results without prepared-reference keys', async () => {
    const { fixture } = await setup();
    const pull = pullToJson({
      extent: {
        kind: 'Filter',
        objectType: fixture.m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: fixture.m.C1.Name,
          parameter: 'name',
        },
      },
      arguments: { name: 'original' },
      results: [
        {
          name: 'related',
          select: {
            propertyType: fixture.m.C1.C1WhereC1C1One2One,
            include: [{ propertyType: fixture.m.C1.C1C1One2One }],
          },
        },
        {
          name: 'objects',
          include: [{ propertyType: fixture.m.C1.C1C1One2One }],
        },
      ],
    });

    expect(Object.keys(pull)).not.toContain('er');
    expect(pull.e.t).toBe(fixture.m.C1.tag);
    expect(pull.e.p.p).toBe('name');
    expect(pull.a).toEqual({ name: 'original' });
    expect(pull.r.map((result) => result.n)).toEqual(['related', 'objects']);
    expect(pull.r[0].s.a).toBe(
      fixture.m.C1.C1WhereC1C1One2One.relationType.tag
    );
    expect(pull.r[0].s.i[0].r).toBe(fixture.m.C1.C1C1One2One.relationType.tag);
    expect(pull.r[1].i[0].r).toBe(fixture.m.C1.C1C1One2One.relationType.tag);
    for (const result of pull.r) {
      expect(Object.keys(result)).not.toContain('r');
    }
  });

  it('returns structured immutable failures without instantiating diagnostic IDs', async () => {
    const { session, read } = await setup();
    read.mockImplementation(async (request) => ({
      i: request.i,
      k: 'error',
      e: [
        {
          c: 'invalid-query',
          m: 'Supply a supported inline query.',
          o: [999],
          i: 0,
        },
      ],
    }));
    const result = await session.pull({ objectId: 101 });
    expect(result.hasErrors).toBe(true);
    expect(result.readErrors[0].o).toEqual([999]);
    expect(Object.isFrozen(result.readErrors[0].o)).toBe(true);
    expect(session.instantiate(999)).toBeNull();
  });

  it('rejects new-object permission shortcuts after retirement while keeping values inspectable', async () => {
    const { fixture, database, session } = await setup();
    const object = session.create<C1>(fixture.m.C1);
    object.Name = 'local';
    void database.close();
    expect(() => object.strategy.canRead(fixture.m.C1.Name)).toThrow(
      /retired/i
    );
    expect(
      (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('local');
    expect(object.strategy.diff().length).toBe(1);
  });

  it('observes readable-role changes at the same version and retains immutable checkpoints until owner cleanup', async () => {
    const { fixture, database, session, read, response } = await setup();
    const stores: ReadStore[] = [];
    const publish = ReadStore.prototype.publish;
    const spy = jest
      .spyOn(ReadStore.prototype, 'publish')
      .mockImplementation(function (contents) {
        stores.push(this);
        return publish.call(this, contents);
      });
    try {
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      const store = stores[0];
      const original = store.getCurrentRecord(101);
      session.checkpoint();
      read.mockImplementation(async (request) => {
        if (request.k === 'discover') return discovery(request);
        const result = response(request);
        result.p[0].p = { ro: [] };
        result.s[0].v = 2;
        result.s[0].p = { p: [32] };
        result.pm = [result.pm[1]];
        return result;
      });
      await session.pull({ objectId: 101 });
      expect(
        (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBeUndefined();
      expect(store.getRecord(original)).toBe(original);
      const changes = session.checkpoint();
      expect(changes.associationsByRoleType.get(fixture.m.C1.Name)).toContain(
        object
      );
      expect(store.getRecord(original)).toBe(original);
      const latest = store.getCurrentRecord(101);
      await database.close();
      expect(store.getRecord(original)).toBeUndefined();
      expect(store.getRecord(latest)).toBeUndefined();
      expect(original.roles[fixture.m.C1.Name.relationType.tag]).toBe(
        'original'
      );
      expect(latest.roles[fixture.m.C1.Name.relationType.tag]).toBeUndefined();
      expect(session.closed).toBe(true);
    } finally {
      spy.mockRestore();
    }
  });

  it('uses exact returned records while another Session advances authority and shared bodies', async () => {
    const { fixture, database, session, read, response } = await setup();
    const original = (await session.pull({ objectId: 101 })).object<C1>('C1');
    original.Name = 'local';
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request, 2, 'other');
      result.s[0].v = 2;
      result.s[0].p = { p: [31] };
      result.pm = [result.pm[0]];
      result.pm.push({ i: 33, p: { c: fixture.m.C1.tag, o: 1 } });
      result.s[0].p = { p: [31, 33] };
      return result;
    });
    const other = database.createWorkspace().createSession();
    expect((await other.pull({ objectId: 101 })).object<C1>('C1').Name).toBe(
      'other'
    );
    expect(original.Name).toBe('local');
    expect(original.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    expect(original.strategy.diff()[0]).toEqual(
      expect.objectContaining({
        originalRole: 'original',
        changedRole: 'local',
      })
    );
  });

  it('stages factories before application and rejects reentrant retirement without importing objects', async () => {
    const { database, session } = await setup();
    const factory = database.configuration.objectFactory;
    const create = factory.create.bind(factory);
    const spy = jest.spyOn(factory, 'create').mockImplementation((strategy) => {
      const object = create(strategy);
      void database.close();
      return object;
    });
    try {
      await expect(session.pull({ objectId: 101 })).rejects.toThrow(/retired/i);
      expect(session.instantiate(101)).toBeNull();
    } finally {
      spy.mockRestore();
    }
  });

  it('rejects close after publication before Session adaptation and clears cache after draining', async () => {
    const { database, session } = await setup();
    const originalPublish = ReadStore.prototype.publish;
    const observed: ReadStore[] = [];
    const spy = jest
      .spyOn(ReadStore.prototype, 'publish')
      .mockImplementation(function (publication) {
        observed.push(this);
        const result = originalPublish.call(this, publication);
        void database.close();
        return result;
      });
    try {
      await expect(session.pull({ objectId: 101 })).rejects.toThrow(/retired/i);
      expect(session.instantiate(101)).toBeNull();
      await database.close();
      expect(database.closed).toBe(true);
      expect(observed[0].getCurrentRecord(101)).toBeUndefined();
      expect(observed[0].recent).toEqual({});
    } finally {
      spy.mockRestore();
    }
  });

  it('captures declarations and dependencies before waiting and orders mutations through response application', async () => {
    const { fixture, database, session, read, pushRead } = await setup();
    await session.pull([]);
    let resolve: (value: ReadResponse) => void;
    let firstRequest: ReadRequest;
    let entered: () => void;
    const started = new Promise<void>((done) => {
      entered = done;
    });
    read.mockImplementationOnce((request) => {
      firstRequest = request;
      entered();
      return new Promise<ReadResponse>((done) => {
        resolve = done;
      });
    });
    const first = session.pull({ objectId: 101 });
    await started;
    const other = database.createWorkspace().createSession();
    const dependency = {
      objectType: fixture.m.C1,
      propertyType: fixture.m.C1.Name,
    };
    other.activate([
      {
        objectType: fixture.m.C1,
        roleType: fixture.m.C1.Name,
        dependencies: [dependency],
        derive: () => 'derived',
      },
    ]);
    const selection: Pull = {
      objectId: 101,
      results: [{ name: 'captured' }],
      arguments: { value: 'before' },
    };
    const second = other.pull(selection);
    await expect(other.push()).rejects.toThrow(/operation.*progress/i);
    const saver = database.createWorkspace().createSession();
    const mutation = saver.push({ refresh: false });
    dependency.propertyType = fixture.m.C1.C1AllorsString;
    selection.objectId = 202;
    selection.results[0].name = 'changed';
    selection.arguments.value = 'after';
    expect(pushRead).not.toHaveBeenCalled();
    resolve(discovery(firstRequest));
    await Promise.all([first, second, mutation]);
    expect(read.mock.calls[5][0].p.l[0]).toEqual(
      expect.objectContaining({
        o: 101,
        a: { value: 'before' },
        r: [expect.objectContaining({ n: 'captured' })],
      })
    );
    expect(pushRead).toHaveBeenCalledTimes(1);
    expect(read.mock.calls[5][0].p.d).toEqual([
      { o: fixture.m.C1.tag, r: fixture.m.C1.Name.relationType.tag },
    ]);
    expect(other.instantiate<C1>(101).Name).toBe('original');
    expect(fixture.m.C1).toBeDefined();
  });

  it('does not apply mutation responses after retirement and does not dispatch queued work', async () => {
    const { fixture, database, session, pushRead } = await setup();
    await session.pull({ objectId: 101 });
    const created = session.create<C1>(fixture.m.C1);
    const localId = created.id;
    let resolve: (value: unknown) => void;
    let entered: () => void;
    const started = new Promise<void>((done) => {
      entered = done;
    });
    let request;
    pushRead.mockImplementationOnce((value) => {
      request = value;
      entered();
      return new Promise((done) => {
        resolve = done;
      });
    });
    const running = session.push({ refresh: false });
    await started;
    const queued = database
      .createWorkspace()
      .createSession()
      .push({ refresh: false });
    void database.close();
    resolve({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [{ w: localId, d: 202 }] },
    });
    await expect(running).resolves.toMatchObject({
      outcome: 'committed',
      application: { acknowledgement: 'failed' },
      reconciliationRequired: true,
    });
    await expect(queued).rejects.toThrow(/retired/i);
    expect(created.id).toBe(localId);
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('captures native dates before queued Push and reports uncertain transport outcomes', async () => {
    const { fixture, database, session, read, pushRead } = await setup();
    await session.pull({ objectId: 101 });
    const created = session.create<C1>(fixture.m.C1);
    const date = new Date('2026-10-12T10:00:00Z');
    created.C1AllorsDateTime = date;
    let resolve: (value: ReadResponse) => void;
    let request: ReadRequest;
    let entered: () => void;
    const started = new Promise<void>((done) => {
      entered = done;
    });
    read.mockImplementationOnce((value) => {
      request = value;
      entered();
      return new Promise((done) => {
        resolve = done;
      });
    });
    const otherPull = database
      .createWorkspace()
      .createSession()
      .pull({ objectId: 101 });
    await started;
    pushRead.mockRejectedValueOnce(new Error('connection dropped'));
    const mutation = session.push({ refresh: false });
    date.setTime(0);
    resolve(discovery(request));
    await otherPull;
    await expect(mutation).resolves.toMatchObject({
      outcome: 'unknown',
      reconciliationRequired: true,
    });
    expect(pushRead.mock.calls[0][0].push.n[0].r).toContainEqual({
      t: fixture.m.C1.C1AllorsDateTime.relationType.tag,
      u: '2026-10-12T10:00:00.000Z',
    });
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('rejects unsupported Exists parameters and custom extents before any transport', async () => {
    const { fixture, session, read } = await setup();
    await expect(
      session.pull({
        extent: {
          kind: 'Filter',
          objectType: fixture.m.C1,
          predicate: {
            kind: 'Exists',
            propertyType: fixture.m.C1.Name,
            parameter: 'ignored',
          },
        },
      })
    ).rejects.toThrow(/Exists/i);
    await expect(
      session.pull({
        objectType: fixture.m.C1,
        extent: { kind: 'Custom' } as unknown as Pull['extent'],
      })
    ).rejects.toThrow(/legacy/i);
    expect(read).not.toHaveBeenCalled();
  });

  it('rejects unknown executable fields and declaration accessors without invoking them', async () => {
    const { session, read } = await setup();
    await expect(
      session.pull({
        objectId: 101,
        procedure: { name: 'hidden' },
      } as unknown as Pull)
    ).rejects.toThrow(/legacy|unsupported/i);
    const getter = jest.fn(() => 101);
    await expect(
      session.pull(
        Object.defineProperty({}, 'objectId', { enumerable: true, get: getter })
      )
    ).rejects.toThrow(/accessor/i);
    expect(getter).not.toHaveBeenCalled();
    expect(read).not.toHaveBeenCalled();
  });

  it('captures native Date arguments without invoking custom serialization hooks', async () => {
    const { session, read } = await setup();
    const date = new Date('2026-10-12T10:00:00Z');
    const toJSON = jest.fn(() => 'custom JSON');
    const toISOString = jest.fn(() => 'custom ISO');
    date.toJSON = toJSON;
    date.toISOString = toISOString;
    await session.pull({ objectId: 101, arguments: { date } });
    expect(read.mock.calls[0][0].p.l[0].a.date).toBe(
      '2026-10-12T10:00:00.000Z'
    );
    expect(toJSON).not.toHaveBeenCalled();
    expect(toISOString).not.toHaveBeenCalled();
  });

  it('preserves native Date equality and ownership across exact record checkpoints', async () => {
    const { fixture, session, read, response } = await setup();
    const dateRole = fixture.m.C1.C1AllorsDateTime;
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request);
      result.p[0].p = {
        ro: [{ t: dateRole.relationType.tag, v: '2026-10-12T10:00:00Z' }],
      };
      return result;
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const state = (object.strategy as CommonStrategy).DatabaseOriginState;
    const heldDate = state.getUnitRole(dateRole) as Date;
    heldDate.setTime(0);
    expect((state.getUnitRole(dateRole) as Date).toISOString()).toBe(
      '2026-10-12T10:00:00.000Z'
    );
    session.checkpoint();
    await session.pull({ objectId: 101 });
    expect(session.checkpoint().associationsByRoleType.size).toBe(0);
  });

  it('compares a prior local relation checkpoint with IDs in a newly accepted record', async () => {
    const { fixture, session, read, response } = await setup();
    const role = fixture.m.C1.C1C2Many2One;
    let version = 1;
    let target = 102;
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request, version);
      result.p[0].p = { ro: [{ t: role.relationType.tag, o: target }] };
      for (const id of [102, 103, 104])
        result.p.push({
          i: id,
          c: fixture.m.C2.tag,
          v: 1,
          g: [],
          r: [],
          p: { ro: [] },
        });
      result.pm = [
        { i: 31, p: { c: fixture.m.C1.tag, o: 2, t: role.relationType.tag } },
        { i: 32, p: { c: fixture.m.C1.tag, o: 4, t: role.relationType.tag } },
      ];
      return result;
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    session.checkpoint();
    object.C1C2Many2One = session.instantiate(103);
    session.checkpoint();
    await session.push({ refresh: false });
    version = 2;
    target = 104;
    await session.pull({ objectId: 101 });
    const changes = session.checkpoint();
    expect(changes.rolesByAssociationType.get(role.associationType)).toEqual(
      new Set([session.instantiate(103), session.instantiate(104)])
    );
  });

  it('checkpoints unloaded relation IDs without importing another Session cache or losing changes', async () => {
    const { fixture, database, session, read, response } = await setup();
    const role = fixture.m.C1.C1C2Many2Manies;
    let version = 1;
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request, version);
      result.p[0].p = {
        ro: [{ t: role.relationType.tag, c: version === 1 ? [102] : [103] }],
      };
      result.pm = [
        { i: 31, p: { c: fixture.m.C1.tag, o: 2, t: role.relationType.tag } },
        { i: 32, p: { c: fixture.m.C1.tag, o: 4, t: role.relationType.tag } },
      ];
      return result;
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    session.checkpoint();
    version = 2;
    await session.pull({ objectId: 101 });
    const changes = session.checkpoint();
    expect(changes.associationsByRoleType.get(role)).toContain(object);
    expect(changes.rolesByAssociationType.size).toBe(0);
    expect(session.instantiate(102)).toBeNull();
    expect(() => object.C1C2Many2Manies).toThrow(
      /include, dependency, or Pull/i
    );
    expect(
      database.createWorkspace().createSession().instantiate(101)
    ).toBeNull();
  });

  it('closes clean active operations and Session-origin permission shortcuts while leaving diffs inspectable', async () => {
    const { fixture, session } = await setup();
    const object = session.create<C1>(fixture.m.C1);
    const role = [...fixture.m.C1.roleTypes].find(
      (role) => role.origin === Origin.Session
    );
    expect(role).toBeDefined();
    session.close();
    expect(() => session.checkpoint()).toThrow(/closed/i);
    expect(() => session.reset()).toThrow(/closed/i);
    expect(() => object.strategy.canRead(role)).toThrow(/closed/i);
    expect(() => object.strategy.canWrite(role)).toThrow(/closed/i);
    expect(() => object.strategy.setRole(role, null)).toThrow(/closed/i);
    expect(() => object.strategy.diff()).not.toThrow();
  });

  it('checks current revocations before grants and restores unavailable objects on a later included read', async () => {
    const { fixture, session, read, response } = await setup();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'local';
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request, 2, 'remote');
      result.p[0].r = [1];
      result.s.push({ k: 'revocation', i: 21, v: 1, p: { p: [32] } });
      return result;
    });
    expect((await session.pull({ objectId: 101 })).mergeErrors).toContain(
      object
    );
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            l: [{ i: 0, o: 101, s: 'unavailable' }],
            p: [],
            s: [],
            pm: [],
            o: { C1: null },
          }
    );
    await session.pull({ objectId: 101 });
    expect(object.strategy.canRead(fixture.m.C1.Name)).toBe(false);
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : response(request, 3, 'recovered')
    );
    expect((await session.pull({ objectId: 101 })).mergeErrors).toContain(
      object
    );
    expect(object.strategy.canRead(fixture.m.C1.Name)).toBe(true);
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    expect(object.Name).toBe('local');
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            l: [{ i: 0, s: 'resolved' }],
            p: [],
            s: [],
            pm: [],
            o: {},
            c: { C1s: [] },
          }
    );
    await session.pull({
      extent: { kind: 'Filter', objectType: fixture.m.C1 },
    });
    expect(object.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
    expect(session.instantiate(101)).toBe(object);
  });

  it('cancels queued reads before dispatch but preserves published read application after cancellation', async () => {
    const { database, session, read } = await setup();
    const cancellation = new AbortController();
    cancellation.abort();
    await expect(
      session.pull({ objectId: 101 }, cancellation.signal)
    ).rejects.toThrow(/cancel/i);
    expect(read).not.toHaveBeenCalled();
    const after = new AbortController();
    const originalPublish = ReadStore.prototype.publish;
    const spy = jest
      .spyOn(ReadStore.prototype, 'publish')
      .mockImplementation(function (publication) {
        const pin = originalPublish.call(this, publication);
        after.abort();
        return pin;
      });
    try {
      const result = await session.pull({ objectId: 101 }, after.signal);
      expect(result.object<C1>('C1').Name).toBe('original');
      expect(database.closed).toBe(false);
    } finally {
      spy.mockRestore();
    }
  });
  it('retains correlated read diagnostics as the cause when a failure retires the generation', async () => {
    const { database, session, read } = await setup();
    await session.pull({ objectId: 101 });
    read.mockImplementation(async (request) => ({
      i: request.i,
      k: 'error',
      sc: { ...request.sc, p: 'replacement' },
      e: [{ c: 'scope-changed', m: 'Reconfigure', o: [999], i: 0 }],
    }));
    let failure: Error & { cause?: { errors: unknown[] } };
    try {
      await session.pull({ objectId: 101 });
    } catch (error) {
      failure = error;
    }
    expect(failure.message).toMatch(/retired/i);
    expect(failure.cause.errors).toEqual([
      { c: 'scope-changed', m: 'Reconfigure', o: [999], i: 0 },
    ]);
    expect(database.closed).toBe(true);
  });

  it('reports malformed post-dispatch responses as uncertain outcomes without replay', async () => {
    const getter = jest.fn(() => []);
    for (const response of [
      null,
      Object.defineProperty({}, 'push', { enumerable: true, get: getter }),
    ]) {
      const { session, pushRead } = await setup();
      await session.pull({ objectId: 101 });
      pushRead.mockResolvedValueOnce(response);
      const result = await session.push();
      expect(result.outcome).toBe('unknown');
      expect(result.reconciliationRequired).toBe(true);
      await expect(session.push()).rejects.toThrow(/reconcil/i);
      expect(getter).not.toHaveBeenCalled();
      expect(pushRead).toHaveBeenCalledTimes(1);
    }
  });

  it.each(['malformed', 'retired'])(
    'reports a %s mutation result with independent commitment and its direct cause',
    async (kind) => {
      const { database, session, pushRead } = await setup();
      await session.pull({ objectId: 101 });
      const send = pushRead.getMockImplementation();
      pushRead.mockImplementationOnce(async (request) => {
        const response = await send(request);
        if (kind === 'retired') void database.close();
        return kind === 'malformed' ? null : response;
      });
      const saved = await session.push();
      expect(saved.outcome).toBe(
        kind === 'malformed' ? 'unknown' : 'committed'
      );
      expect(saved.reconciliationRequired).toBe(true);
      const failure =
        kind === 'malformed'
          ? saved.diagnostics.errors[0]
          : saved.application.errors[0];
      expect(failure.cause).toBeInstanceOf(Error);
      expect(failure.cause.message).toMatch(
        kind === 'malformed' ? /object|response/i : /retired/i
      );
      expect(failure.cause.message).not.toMatch(/may have committed/i);
      if (kind === 'retired') {
        expect(saved.application.acknowledgement).toBe('failed');
        expect(saved.refresh.status).toBe('succeeded');
        expect(saved.application.refresh).toBe('skipped');
      }
      expect(pushRead).toHaveBeenCalledTimes(1);
    }
  );

  it('denies authority when an active revocation descriptor is unavailable', async () => {
    const { fixture, session, read, response } = await setup();
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request);
      result.p[0].r = [1];
      result.s.push({ k: 'revocation', i: 21, v: 1, p: { p: [32] } });
      return result;
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const original = ReadStore.prototype.getCurrentPermission;
    const spy = jest
      .spyOn(ReadStore.prototype, 'getCurrentPermission')
      .mockImplementation(function (id) {
        return id === 32 ? undefined : original.call(this, id);
      });
    try {
      expect(object.strategy.canRead(fixture.m.C1.Name)).toBe(false);
    } finally {
      spy.mockRestore();
    }
  });

  it('preserves scoped declaration arguments empty results and parameter fields without I/O mutation', async () => {
    const { fixture, session, read } = await setup();
    const selection: Pull = {
      extent: {
        kind: 'Filter',
        objectType: fixture.m.C1,
        predicate: {
          kind: 'Instanceof',
          objectType: fixture.m.C1,
          parameter: 'class',
        },
      },
      arguments: { class: fixture.m.C1 },
      results: [],
    };
    await session.pull(selection);
    expect(read.mock.calls[0][0].p.l[0]).toEqual(
      expect.objectContaining({
        a: { class: fixture.m.C1.tag },
        r: [],
        e: expect.objectContaining({
          p: expect.objectContaining({ p: 'class' }),
        }),
      })
    );
  });

  it.each(['unit', 'objects'])(
    'preserves prototype-like argument names and typed values (%s)',
    async (kind) => {
      const { fixture, session, read } = await setup();
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      read.mockClear();
      const date = new Date('2026-10-12T10:00:00Z');
      const arguments_: Pull['arguments'] = JSON.parse(
        '{"__proto__":"wanted-name","constructor":"constructor-value","toString":"string-value"}'
      );
      if (kind === 'objects') arguments_['__proto__'] = [object];
      Object.assign(arguments_, {
        object,
        objects: [object],
        class: fixture.m.C1,
        date,
        number: 7,
        boolean: false,
        empty: null,
      });
      const pending = session.pull({
        extent: {
          kind: 'Filter',
          objectType: fixture.m.C1,
          predicate:
            kind === 'objects'
              ? {
                  kind: 'ContainedIn',
                  propertyType: fixture.m.C1.C1I12Many2One,
                  parameter: '__proto__',
                }
              : {
                  kind: 'Equals',
                  propertyType: fixture.m.C1.Name,
                  parameter: '__proto__',
                },
        },
        arguments: arguments_,
      });
      arguments_['__proto__'] = 'changed';
      date.setUTCFullYear(2000);
      await pending;
      const selection = (read.mock.calls[0][0] as ReadRequest).p.l[0];
      expect(selection.e.p.p).toBe('__proto__');
      expect(Object.keys(selection.a)).toEqual(Object.keys(arguments_));
      expect(selection.a['__proto__']).toEqual(
        kind === 'objects' ? [101] : 'wanted-name'
      );
      expect(selection.a).toEqual(
        expect.objectContaining({
          constructor: 'constructor-value',
          toString: 'string-value',
          object: 101,
          objects: [101],
          class: fixture.m.C1.tag,
          date: '2026-10-12T10:00:00.000Z',
          number: 7,
          boolean: false,
          empty: null,
        })
      );
      expect(read).toHaveBeenCalledTimes(2);
      session.close();
    }
  );

  it('preserves prototype-like collection names in scoped results', async () => {
    const { fixture, session, read, response } = await setup();
    const names = ['__proto__', 'constructor', 'toString', 'ordinary'];
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            o: {},
            c: Object.fromEntries([
              ...names.map((name) => [name, [101]]),
              ['empty', []],
            ]),
          }
    );
    const result = await session.pull({
      extent: { kind: 'Filter', objectType: fixture.m.C1 },
      results: [...names, 'empty'].map((name) => ({ name })),
    });
    expect(result.hasErrors).toBe(false);
    expect([...result.collections.keys()]).toEqual(
      [...names, 'empty'].map((name) => name.toUpperCase())
    );
    for (const name of names) {
      const collection = result.collection<C1>(name);
      expect(collection.map((object) => object.id)).toEqual([101]);
      expect(collection[0]).toBe(session.instantiate(101));
    }
    expect(result.collection('empty')).toEqual([]);
    expect(read).toHaveBeenCalledTimes(2);
    session.close();
  });

  function invokeResponse(
    request: import('@allors/system/common/protocol-json').InvokeReadRequest,
    source: Awaited<ReturnType<typeof setup>>,
    name = 'original'
  ) {
    const full = source.response(
      { ...request, k: 'read', p: { l: [], d: [] } },
      2,
      name
    );
    return {
      i: request.i,
      sc: request.sc,
      invoke: { k: 'committed' as const },
      ...(request.refresh
        ? {
            changed: {
              i: request.i,
              sc: request.sc,
              k: 'changed' as const,
              p: full.p,
              s: full.s,
              pm: full.pm,
              u: [],
            },
          }
        : {}),
    };
  }

  it('protected Invoke captures method order versions scope and refresh before queued dispatch', async () => {
    const source = await setup();
    const { fixture, session, database, read, invoke } = source;
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const sibling = database.createWorkspace().createSession();
    let release: () => void;
    let started: () => void;
    const dispatched = new Promise<void>((resolve) => (started = resolve));
    read.mockImplementationOnce(async (request) => {
      started();
      await new Promise<void>((resolve) => (release = resolve));
      return discovery(request);
    });
    const prior = sibling.pull([]);
    await dispatched;
    const wrapper = { object, methodType: fixture.m.Organisation.JustDoIt };
    const methods = [wrapper, wrapper];
    const options = { refresh: false };
    const pending = session.invoke(methods, options);
    methods.length = 0;
    wrapper.methodType = fixture.m.Organisation.Delete;
    options.refresh = true;
    release();
    await prior;
    const result = await pending;
    expect(result.outcome).toBe('committed');
    expect(invoke.mock.calls[0][0]).toMatchObject({
      l: [
        { i: 101, v: 1, m: fixture.m.Organisation.JustDoIt.tag },
        { i: 101, v: 1, m: fixture.m.Organisation.JustDoIt.tag },
      ],
      refresh: false,
      x: 'scoped',
      sc: {
        p: 'population',
        u: 1,
        w: 'Default',
        r: 'test-revision',
        x: 'scoped',
      },
    });
    expect(invoke.mock.calls[0][0]).not.toHaveProperty('o');
  });

  it('protected Invoke refresh preserves unsaved edits creations and checkpoint ownership without acknowledging Push', async () => {
    const source = await setup();
    const { fixture, session, read, invoke, pushRead } = source;
    const response = source.response;
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const full = response(request);
      full.s[0].p.p.push(33, 34, 35, 36);
      for (const [id, role, operation] of [
        [33, fixture.m.C1.C1AllorsString, 2],
        [34, fixture.m.C1.C1AllorsString, 4],
        [35, fixture.m.C1.C1I1Many2One, 2],
        [36, fixture.m.C1.C1I1Many2One, 4],
      ] as const)
        full.pm.push({
          i: id,
          p: { c: fixture.m.C1.tag, o: operation, t: role.relationType.tag },
        });
      return full;
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'retained';
    const created = session.create<C1>(fixture.m.C1);
    created.Name = 'new';
    object.C1I1Many2One = created;
    const checkpoint = session.checkpoint();
    const localId = created.id;
    const pushed = jest.spyOn(
      (object.strategy as CommonStrategy).DatabaseOriginState,
      'onPushed'
    );
    invoke.mockImplementationOnce(async (request) => {
      const full = await read({ ...request, k: 'read', p: { l: [], d: [] } });
      const result = invokeResponse(request, source);
      result.changed.p = full.p;
      result.changed.s = full.s;
      result.changed.pm = full.pm;
      result.changed.p[0].v = 2;
      result.changed.p[0].p.ro.push({
        t: fixture.m.C1.C1AllorsString.relationType.tag,
        v: 'remote',
      });
      return result;
    });
    const result = await session.invoke({
      object,
      methodType: fixture.m.Organisation.JustDoIt,
    });
    expect(result.outcome).toBe('committed');
    expect(result.application.acknowledgement).toBe('not-applicable');
    expect(object.Name).toBe('retained');
    expect(object.C1AllorsString).toBe('remote');
    expect(object.C1I1Many2One).toBe(created);
    expect(created.id).toBe(localId);
    expect(pushed).not.toHaveBeenCalled();
    expect(checkpoint).toBeDefined();
    expect(object.strategy.diff().length).toBeGreaterThan(0);
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'rejected', _e: 'retained submission' },
    }));
    await session.push({ refresh: false });
    const submitted = pushRead.mock.calls[0][0];
    expect(submitted.push.n[0].w).toBe(localId);
    expect(submitted.push.o[0].r).toEqual(
      expect.arrayContaining([
        { t: fixture.m.C1.Name.relationType.tag, u: 'retained' },
        { t: fixture.m.C1.C1I1Many2One.relationType.tag, c: localId },
      ])
    );
  });

  it('protected Invoke reports conflicting refresh without erasing the old baseline or unsaved edits', async () => {
    const source = await setup();
    const { fixture, session, invoke } = source;
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'local';
    const before = object.strategy.diff();
    invoke.mockImplementationOnce(async (request) =>
      invokeResponse(request, source, 'remote')
    );
    const result = await session.invoke({
      object,
      methodType: fixture.m.Organisation.JustDoIt,
    });
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('succeeded');
    expect(result.application.refresh).toBe('conflicted');
    expect(result.reconciliationRequired).toBe(false);
    expect(object.Name).toBe('local');
    expect(object.strategy.diff()).toEqual(before);
    session.reset();
    expect(object.Name).toBe('original');
  });

  it.each([
    'malformed shape',
    'unknown metadata',
    'wrong scope',
    'read failure',
    'missing changed',
  ])(
    'protected Invoke classifies %s refresh failure independently of its committed receipt',
    async (kind) => {
      const source = await setup();
      const { fixture, session, invoke } = source;
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      object.Name = 'retained';
      invoke.mockImplementationOnce(async (request) => {
        const result = invokeResponse(request, source);
        if (kind === 'malformed shape')
          return { ...result, changed: { invalid: true } };
        if (kind === 'unknown metadata') result.changed.p[0].c = 'absent-class';
        if (kind === 'wrong scope')
          result.changed.sc = { ...result.changed.sc, u: 99 };
        if (kind === 'read failure')
          return {
            ...result,
            changed: {
              i: request.i,
              k: 'error',
              e: [{ c: 'refresh-failed', m: 'Pull again.' }],
            },
          };
        if (kind === 'missing changed') delete result.changed;
        return result;
      });
      const result = await session.invoke({
        object,
        methodType: fixture.m.Organisation.JustDoIt,
      });
      expect(result.outcome).toBe('committed');
      expect(result.refresh.status).toBe('failed');
      expect(result.refresh.errors.length).toBeGreaterThan(0);
      expect(result.reconciliationRequired).toBe(false);
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(object.Name).toBe('retained');
      object.Name = 'still editable';
      expect(invoke).toHaveBeenCalledTimes(1);
    }
  );

  it.each([
    'disconnect',
    'post-send abort without receipt',
    'malformed receipt',
    'mismatched receipt',
  ])(
    'protected Invoke reports unknown after %s and blocks further mutation without replay',
    async (kind) => {
      const source = await setup();
      const { fixture, session, invoke, pushRead } = source;
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      object.Name = 'local';
      const abort = new AbortController();
      invoke.mockImplementationOnce(async (request) => {
        if (kind === 'disconnect') throw new Error('connection lost');
        if (kind === 'post-send abort without receipt') {
          abort.abort();
          throw new Error('aborted after send');
        }
        if (kind === 'malformed receipt')
          return { i: request.i, invoke: { k: 'maybe' } };
        return { ...invokeResponse(request, source), i: 'different' };
      });
      const method = { object, methodType: fixture.m.Organisation.JustDoIt };
      const result = await session.invoke(method, undefined, abort.signal);
      expect(result.outcome).toBe('unknown');
      expect(result.reconciliationRequired).toBe(true);
      await expect(session.invoke(method)).rejects.toThrow(/reconcil/i);
      await expect(session.push()).rejects.toThrow(/reconcil/i);
      for (const edit of [
        () => {
          object.Name = 'changed';
        },
        () => session.create(fixture.m.C1),
        () => session.reset(),
        () => session.checkpoint(),
      ])
        expect(edit).toThrow(/reconcil/i);
      await session.pull({ objectId: 101 });
      expect(session.reconciliationRequired).toBe(true);
      expect(object.Name).toBe('local');
      expect(result.diagnostics.errors[0].message).toMatch(/reconcil/i);
      expect(invoke).toHaveBeenCalledTimes(1);
      expect(pushRead).not.toHaveBeenCalled();
    }
  );

  it.each(['cancel', 'close', 'retire'])(
    'protected Invoke retains receipt and read classification after %s prevents local application',
    async (kind) => {
      const source = await setup();
      const { fixture, session, database, invoke } = source;
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      object.Name = 'local';
      const abort = new AbortController();
      invoke.mockImplementationOnce(async (request) => {
        if (kind === 'cancel') abort.abort();
        else if (kind === 'close') session.close();
        else void database.close();
        return invokeResponse(request, source);
      });
      const result = await session.invoke(
        { object, methodType: fixture.m.Organisation.JustDoIt },
        undefined,
        abort.signal
      );
      expect(result.outcome).toBe('committed');
      expect(result.refresh.status).toBe('succeeded');
      expect(result.application.refresh).toBe('skipped');
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(result.reconciliationRequired).toBe(false);
      if (kind === 'cancel')
        expect(result.application.errors[0].message).toMatch(
          /Invoke cancelled/
        );
      expect(
        (object.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBe('local');
    }
  );

  it('protected Invoke holds the same-Session editing gate through capture dispatch and refresh', async () => {
    const source = await setup();
    const { fixture, session, database, invoke } = source;
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const created = session.create<C1>(fixture.m.C1);
    const sibling = database.createWorkspace().createSession();
    const pendingChecks: Promise<unknown>[] = [];
    const checkGate = () => {
      expect(session.saving).toBe(true);
      expect(object.Name).toBe('original');
      for (const edit of [
        () => {
          object.Name = 'bad';
        },
        () => session.create(fixture.m.C1),
        () => session.reset(),
        () => session.checkpoint(),
        () => created.strategy.delete(),
      ])
        expect(edit).toThrow(/save.*progress/i);
      pendingChecks.push(
        expect(session.pull([])).rejects.toThrow(/save.*progress/i)
      );
      pendingChecks.push(
        expect(session.push()).rejects.toThrow(/save.*progress/i)
      );
      pendingChecks.push(
        expect(session.invoke([])).rejects.toThrow(/save.*progress/i)
      );
      sibling.create<C1>(fixture.m.C1).Name = 'sibling';
    };
    const method = {
      get object() {
        checkGate();
        return object;
      },
      methodType: fixture.m.Organisation.JustDoIt,
    };
    const factory = database.configuration.objectFactory;
    const create = factory.create.bind(factory);
    jest.spyOn(factory, 'create').mockImplementation((strategy) => {
      if (strategy.session === session) checkGate();
      return create(strategy);
    });
    invoke.mockImplementationOnce(async (request) => {
      checkGate();
      const result = invokeResponse(request, source);
      result.changed.p.push({ ...result.changed.p[0], i: 102 });
      return result;
    });
    const result = await session.invoke(method);
    await Promise.all(pendingChecks);
    expect(result.outcome).toBe('committed');
    expect(result.application.refresh).toBe('applied');
    expect(session.saving).toBe(false);
    object.Name = 'editable again';
  });

  it('protected Invoke rejects failed bootstrap unsupported transports foreign or new targets and pre-dispatch cancellation', async () => {
    const source = await setup();
    const { fixture, session, database, read, invoke } = source;
    await expectFailedBootstrap(fixture.createConfiguration(), {
      invokeRead: invoke,
    });
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const method = { object, methodType: fixture.m.Organisation.JustDoIt };
    const foreign = database.createWorkspace().createSession();
    await expect(foreign.invoke(method)).rejects.toThrow(
      /this scoped Session/i
    );
    await expect(
      session.invoke({
        object: session.create(fixture.m.C1),
        methodType: method.methodType,
      })
    ).rejects.toThrow(/committed/i);
    await expect(
      session.invoke({
        object: { id: 0, strategy: object.strategy },
        methodType: method.methodType,
      })
    ).rejects.toThrow(/committed/i);
    await expect(
      session.invoke({
        object: { id: object.id, strategy: object.strategy },
        methodType: method.methodType,
      })
    ).rejects.toThrow(/this scoped Session/i);
    await expect(session.invoke(null)).rejects.toThrow(/committed/i);
    const abort = new AbortController();
    abort.abort();
    await expect(
      session.invoke(method, undefined, abort.signal)
    ).rejects.toThrow(/cancel/i);
    const beforeMissing = read.mock.calls.length;
    await expect(
      DatabaseConnection.create(
        database.configuration,
        { read, pushRead: source.pushRead } as unknown as Parameters<
          typeof DatabaseConnection.create
        >[1],
        { revision: 'test-revision', context: 'scoped' }
      )
    ).rejects.toThrow(/invokeRead/i);
    expect(read).toHaveBeenCalledTimes(beforeMissing);
    expect(invoke).not.toHaveBeenCalled();
    expect(session.saving).toBe(false);
  });

  it('protected Invoke marks partial local application failure for reconciliation and preserves commit', async () => {
    const source = await setup();
    const { fixture, session, invoke } = source;
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    object.Name = 'retained';
    const state = (object.strategy as CommonStrategy).DatabaseOriginState;
    const accept = state.acceptRecord.bind(state);
    jest.spyOn(state, 'acceptRecord').mockImplementation((record) => {
      accept(record);
      throw new Error('publication failed');
    });
    invoke.mockImplementationOnce(async (request) =>
      invokeResponse(request, source)
    );
    const result = await session.invoke({
      object,
      methodType: fixture.m.Organisation.JustDoIt,
    });
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('succeeded');
    expect(result.application.refresh).toBe('failed');
    expect(result.application.acknowledgement).toBe('not-applicable');
    expect(result.reconciliationRequired).toBe(true);
    expect(result.application.errors[0].message).toMatch(/publication failed/);
    expect(state.getUnitRole(fixture.m.C1.Name)).toBe('retained');
    expect(invoke).toHaveBeenCalledTimes(1);
  });

  it('protected Invoke keeps copied raw diagnostics after Session close without materializing unknown IDs', async () => {
    const { fixture, session, invoke } = await setup();
    await session.pull([]);
    const diagnostics = {
      k: 'rejected',
      _d: [
        {
          m: null,
          r: [
            { i: -999, r: 'unknown-role' },
            { i: 888, r: 'unknown-role' },
          ],
        },
      ],
    };
    invoke.mockImplementationOnce(async (request) => ({
      i: request.i,
      invoke: diagnostics,
    }));
    const instantiate = jest.spyOn(session, 'instantiate');
    const result = await session.invoke([]);
    session.close();
    diagnostics._d[0].r[0].i = 777;
    expect(result.diagnostics.derivationErrors[0].relations[0].objectId).toBe(
      -999
    );
    expect(result.diagnostics.derivationErrors[0].message).toBeNull();
    expect(
      Object.isFrozen(result.diagnostics.derivationErrors[0].relations)
    ).toBe(true);
    expect(result.derivationErrors[0].roles).toEqual([]);
    expect(instantiate).not.toHaveBeenCalled();
    expect(fixture.m.C1).toBeDefined();
  });

  it('normal Invoke accepts refresh and cancellation and returns expected rejection', async () => {
    const { session, invoke } = await setup();
    invoke.mockImplementation(async (request) => ({
      i: request.i,
      sc: request.sc,
      invoke: { k: 'rejected', _e: 'rejected', _v: [999] },
    }));
    for (const options of [{}, { refresh: true }, { refresh: false }, null]) {
      const result = await session.invoke([], options);
      expect(result.outcome).toBe('rejected');
      expect(result.diagnostics.versionErrorIds).toEqual([999]);
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(result.refresh.status).toBe(
        options?.refresh === false ? 'not-requested' : 'unavailable'
      );
    }
    const result = await session.invoke(
      [],
      undefined,
      new AbortController().signal
    );
    expect(result.outcome).toBe('rejected');
    expect(result.diagnostics.versionErrorIds).toEqual([999]);
    const abort = new AbortController();
    abort.abort();
    await expect(session.invoke([], undefined, abort.signal)).rejects.toThrow(
      /cancel/i
    );
    expect(invoke).toHaveBeenCalledTimes(5);
  });

  it.each(['version', 'access', 'missing', 'derivation', 'runtime'])(
    'protected Invoke returns expected %s rejections as inspectable mutation results',
    async (kind) => {
      const { fixture, session, invoke } = await setup();
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      object.Name = 'unsaved';
      const diagnostic =
        kind === 'version'
          ? { _v: [101] }
          : kind === 'access'
          ? { _a: [101] }
          : kind === 'missing'
          ? { _m: [101] }
          : kind === 'derivation'
          ? {
              _d: [
                {
                  m: 'invalid',
                  r: [{ i: 101, r: fixture.m.C1.Name.relationType.tag }],
                },
              ],
            }
          : { _e: 'method failed' };
      invoke.mockImplementationOnce(async (request) => ({
        i: request.i,
        sc: request.sc,
        invoke: { k: 'rejected', ...diagnostic },
      }));
      const result = await session.invoke({
        object,
        methodType: fixture.m.Organisation.JustDoIt,
      });
      expect(result.outcome).toBe('rejected');
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(result.reconciliationRequired).toBe(false);
      expect(result.hasErrors).toBe(true);
      expect(object.Name).toBe('unsaved');
      expect(invoke).toHaveBeenCalledTimes(1);
    }
  );

  it('protected Invoke rejects retired and unsupported options at runtime as well as in public types', async () => {
    const { fixture, session, invoke } = await setup();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const method = { object, methodType: fixture.m.Organisation.JustDoIt };
    for (const options of [
      { isolated: true },
      { continueOnError: true },
      { returnChanged: true },
      { discover: [] },
      { extra: true },
      { refresh: null },
      { refresh: 1 },
    ]) {
      let rejected = false;
      try {
        await session.invoke(method, options as never);
      } catch {
        rejected = true;
      }
      expect(rejected).toBe(true);
    }
    const getter = jest.fn(() => true);
    await expect(
      session.invoke(
        method,
        Object.defineProperty({}, 'refresh', { get: getter, enumerable: true })
      )
    ).rejects.toThrow();
    expect(getter).not.toHaveBeenCalled();
    expect(invoke).not.toHaveBeenCalled();
    const publicTypes: [
      Extract<
        keyof import('@allors/system/workspace/domain').InvokeOptions,
        'isolated' | 'continueOnError' | 'returnChanged'
      > extends never
        ? true
        : false,
      import('@allors/system/workspace/domain').IInvokeResult extends import('@allors/system/workspace/domain').IMutationResult
        ? true
        : false
    ] = [true, true];
    expect(publicTypes).toEqual([true, true]);
    for (const options of [undefined, {}, { refresh: undefined }, null]) {
      const result = await session.invoke(method, options);
      expect(result.outcome).toBe('committed');
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(invoke.mock.calls[invoke.mock.calls.length - 1][0].refresh).toBe(
        true
      );
    }
  });

  it.each([undefined, true, false])(
    'protected Invoke default and explicit %s refresh preserve their baseline contract',
    async (refresh) => {
      const { fixture, session, invoke, response } = await setup();
      const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
      invoke.mockImplementationOnce(async (request) => {
        const full = response(
          { ...request, p: { l: [], d: [] } },
          2,
          'method result'
        );
        return {
          i: request.i,
          sc: request.sc,
          invoke: { k: 'committed' },
          ...(request.refresh
            ? {
                changed: {
                  i: request.i,
                  sc: request.sc,
                  k: 'changed',
                  p: full.p,
                  s: full.s,
                  pm: full.pm,
                  u: [],
                },
              }
            : {}),
        };
      });
      const result = await session.invoke(
        { object, methodType: fixture.m.Organisation.JustDoIt },
        refresh === undefined ? undefined : ({ refresh } as never)
      );
      expect(result.outcome).toBe('committed');
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(result.application.refresh).toBe(
        refresh === false ? 'not-applicable' : 'applied'
      );
      expect(result.refresh.status).toBe(
        refresh === false ? 'not-requested' : 'succeeded'
      );
      expect(object.Name).toBe(
        refresh === false ? 'original' : 'method result'
      );
      expect(invoke.mock.calls[0][0].refresh).toBe(refresh !== false);
      expect(invoke.mock.calls[0][0]).not.toHaveProperty('o');
      expect(invoke).toHaveBeenCalledTimes(1);
    }
  );

  it('captures Invoke refresh options and returns rejection and unknown outcomes without replay', async () => {
    const { fixture, session, read, invoke, response } = await setup();
    read.mockImplementation(async (request) => {
      if (request.k === 'discover') return discovery(request);
      const result = response(request);
      result.p = [
        {
          i: 101,
          c: fixture.m.Organisation.tag,
          v: 1,
          g: [],
          r: [],
          p: { ro: [] },
        },
      ];
      result.s = [];
      result.pm = [];
      return result;
    });
    const object = (await session.pull({ objectId: 101 })).object('C1');
    const method = { object, methodType: fixture.m.Organisation.JustDoIt };
    const options = { refresh: false };
    const operation = session.invoke(method, options);
    options.refresh = true;
    const committed = await operation;
    expect(committed.outcome).toBe('committed');
    expect(invoke.mock.calls[0][0].refresh).toBe(false);
    expect(invoke.mock.calls[0][0]).not.toHaveProperty('o');
    invoke.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      invoke: { k: 'rejected', _e: 'runtime rejected' },
    }));
    const rejected = await session.invoke(method);
    expect(rejected.outcome).toBe('rejected');
    expect(rejected.hasErrors).toBe(true);
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            l: [{ i: 0, o: 101, s: 'unavailable' }],
            p: [],
            s: [],
            pm: [],
            o: { C1: null },
          }
    );
    await session.pull({ objectId: 101 });
    await expect(session.invoke(method)).rejects.toThrow(/unavailable/i);
    expect(invoke).toHaveBeenCalledTimes(2);
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : {
            ...response(request),
            p: [
              {
                i: 101,
                c: fixture.m.Organisation.tag,
                v: 1,
                g: [],
                r: [],
                p: { ro: [] },
              },
            ],
            s: [],
            pm: [],
          }
    );
    await session.pull({ objectId: 101 });
    invoke.mockRejectedValueOnce(new Error('connection dropped'));
    const unknown = await session.invoke(method);
    expect(unknown.outcome).toBe('unknown');
    expect(unknown.reconciliationRequired).toBe(true);
    expect(invoke).toHaveBeenCalledTimes(3);
  });

  it('stages host object getters before final admission so retirement cannot import a baseline', async () => {
    const { database, session } = await setup();
    const factory = database.configuration.objectFactory;
    const create = factory.create.bind(factory);
    const spy = jest.spyOn(factory, 'create').mockImplementation((strategy) => {
      const object = create(strategy);
      Object.defineProperty(object, 'id', {
        get() {
          void database.close();
          return strategy.id;
        },
      });
      return object;
    });
    try {
      let error: Error;
      try {
        await session.pull({ objectId: 101 });
      } catch (failure) {
        error = failure;
      }
      expect(error?.message).toMatch(/retired/i);
      expect(session.instantiate(101)).toBeNull();
    } finally {
      spy.mockRestore();
    }
  });

  it('stages existing-object getters and merge decisions before accepting a refreshed baseline', async () => {
    const { database, session, read, response } = await setup();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const strategy = object.strategy as CommonStrategy;
    object.Name = 'local';
    Object.defineProperty(object, 'strategy', {
      get() {
        void database.close();
        return strategy;
      },
    });
    read.mockImplementation(async (request) =>
      request.k === 'discover'
        ? discovery(request)
        : response(request, 2, 'remote')
    );
    await expect(session.pull({ objectId: 101 })).rejects.toThrow(/retired/i);
    expect(strategy.DatabaseOriginState.databaseRecord.version).toBe(1);
    expect(strategy.DatabaseOriginState.changedRoleByRelationType.size).toBe(1);
  });

  it('stages Push remapping getters before final admission without applying a retired response', async () => {
    const { fixture, database, session, pushRead } = await setup();
    await session.pull({ objectId: 101 });
    const object = session.create<C1>(fixture.m.C1);
    object.Name = 'created';
    const strategy = object.strategy;
    const localId = strategy.id;
    pushRead.mockImplementation(async (request) => {
      Object.defineProperty(object, 'id', {
        get() {
          void database.close();
          return strategy.id;
        },
      });
      return {
        i: request.i,
        sc: request.sc,
        push: { k: 'committed', n: [{ w: localId, d: 202 }] },
      };
    });
    await expect(session.push({ refresh: false })).resolves.toMatchObject({
      outcome: 'committed',
      application: { acknowledgement: 'failed' },
      reconciliationRequired: true,
    });
    expect(strategy.id).toBe(localId);
    expect(session.instantiate(202)).toBeNull();
    expect(pushRead).toHaveBeenCalledTimes(1);
  });
  it.each([{ n: {} }, { n: [null] }, { _v: {} }])(
    'treats malformed mutation envelopes as uncertain without replay: %j',
    async (malformed) => {
      const { session, pushRead } = await setup();
      await session.pull({ objectId: 101 });
      pushRead.mockImplementationOnce(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: { k: 'committed', n: [], ...malformed },
      }));
      const saved = await session.push({ refresh: false });
      expect(saved.outcome).toBe('unknown');
      expect(saved.reconciliationRequired).toBe(true);
      await expect(session.push()).rejects.toThrow(/reconcil/i);
      expect(pushRead).toHaveBeenCalledTimes(1);
    }
  );

  it('keeps new-object tracking when a successful Push omits assigned ID mappings', async () => {
    const { fixture, session, pushRead } = await setup();
    await session.pull({ objectId: 101 });
    const object = session.create<C1>(fixture.m.C1);
    const id = object.id;
    const result = await session.push();
    expect(result.outcome).toBe('unknown');
    expect(result.reconciliationRequired).toBe(true);
    expect(result.creationMappings).toEqual([]);
    expect(object.id).toBe(id);
    expect(session.pushToDatabaseTracker.created.has(object)).toBe(true);
    await expect(session.push()).rejects.toThrow(/reconcil/i);
    expect(pushRead).toHaveBeenCalledTimes(1);
  });

  it('preserves negative new-object access diagnostics and created tracking as structured results', async () => {
    const { fixture, session, pushRead } = await setup();
    await session.pull({ objectId: 101 });
    const object = session.create<C1>(fixture.m.C1);
    const id = object.id;
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'rejected', _a: [id] },
    }));
    const result = await session.push();
    expect(result.outcome).toBe('rejected');
    expect(result.accessErrors).toEqual([object]);
    expect(result.diagnostics.accessErrorIds).toEqual([id]);
    expect(session.pushToDatabaseTracker.created.has(object)).toBe(true);
    pushRead.mockImplementationOnce(async (request) => ({
      i: request.i,
      sc: request.sc,
      push: { k: 'committed', n: [{ w: id, d: 202 }] },
    }));
    await session.push({ refresh: false });
    expect(pushRead.mock.calls[1][0].push.n.map((entry) => entry.w)).toEqual([
      id,
    ]);
    expect(object.id).toBe(202);
  });
  it('replaces custom checkpoint tracking before final admission without invoking its add callback', async () => {
    const { database, session } = await setup();
    const onAdd = jest.fn(() => void database.close());
    class HostStates extends Set<CommonStrategy['DatabaseOriginState']> {
      override add(state: CommonStrategy['DatabaseOriginState']): this {
        onAdd();
        return super.add(state);
      }
    }
    session.changeSetTracker.databaseOriginStates = new HostStates();
    await session.pull({ objectId: 101 });
    expect(onAdd).not.toHaveBeenCalled();
    expect(database.closed).toBe(false);
    expect(session.changeSetTracker.databaseOriginStates.size).toBe(1);
  });

  it('rejects malformed nested mutation derivation diagnostics as uncertain outcomes', async () => {
    for (const diagnostic of [
      {},
      { r: null },
      { r: [null] },
      { r: [{ i: 0, r: 'role' }] },
      { r: [{ i: 101, r: 123 }] },
    ]) {
      const { session, pushRead } = await setup();
      await session.pull({ objectId: 101 });
      pushRead.mockImplementationOnce(async (request) => ({
        i: request.i,
        sc: request.sc,
        push: { k: 'rejected', _d: [diagnostic] },
      }));
      const result = await session.push();
      expect(result.outcome).toBe('unknown');
      expect(result.reconciliationRequired).toBe(true);
      await expect(session.push()).rejects.toThrow(/reconcil/i);
      expect(pushRead).toHaveBeenCalledTimes(1);
    }
  });

  it('preserves declared mutation derivation relations as inspectable copied Push and Invoke result diagnostics', async () => {
    const { fixture, session, pushRead, invoke } = await setup();
    const object = (await session.pull({ objectId: 101 })).object<C1>('C1');
    const created = session.create<C1>(fixture.m.C1);
    const roleTag = fixture.m.C1.Name.relationType.tag;
    for (const operation of ['push', 'invoke']) {
      const response = {
        _d: [
          {
            m: 'Invalid name',
            r: [
              { i: object.id, r: roleTag },
              { i: created.id, r: roleTag },
            ],
          },
          { m: null, r: [] },
        ],
      };
      let result: import('@allors/system/workspace/domain').IResult;
      if (operation === 'push') {
        pushRead.mockImplementationOnce(async (request) => ({
          i: request.i,
          sc: request.sc,
          push: { k: 'rejected', ...response },
        }));
        const saved = await session.push();
        expect(saved.outcome).toBe('rejected');
        expect(saved.diagnostics.derivationErrors[1].message).toBeNull();
        result = saved;
        expect(pushRead).toHaveBeenCalledTimes(1);
      } else {
        invoke.mockImplementationOnce(async (request) => ({
          i: request.i,
          sc: request.sc,
          invoke: { k: 'rejected', ...response },
        }));
        const invoked = await session.invoke([]);
        expect(invoked.outcome).toBe('rejected');
        expect(invoked.diagnostics.derivationErrors[0].relations).toHaveLength(
          2
        );
        expect(invoked.diagnostics.derivationErrors[1].message).toBeNull();
        result = invoked;
        expect(invoke).toHaveBeenCalledTimes(1);
      }
      response._d[0].r[0].i = 999;
      expect(result.derivationErrors[0].message).toBe('Invalid name');
      const roles = result.derivationErrors[0].roles;
      expect(roles.length).toBe(2);
      expect(roles[0].object === object).toBe(true);
      expect(roles[1].object === created).toBe(true);
      expect(
        roles.every(
          (role) => role.relationType === fixture.m.C1.Name.relationType
        )
      ).toBe(true);
      expect(result.derivationErrors[1].roles).toEqual([]);
      expect(session.instantiate(999)).toBeNull();
    }
    session.close();
  });
});

test('revocation refresh includes cached ids and ignores permission-id collisions', () => {
  const database = {
    ranges: { enumerate: (value: number[]) => value },
    grantById: new Map<number, unknown>(),
    revocationById: new Map<number, unknown>([[1, {}]]), // revocation 1 is cached
    permissions: new Set<number>([2]), // decoy: id 2 is a permission, not a revocation
  } as any;

  const context = new ResponseContext(database);
  context.checkForMissingRevocations([1, 2] as any);

  // Both revocations need refreshing, regardless of the revocation cache or the
  // unrelated permission with id 2.
  expect([...context.missingRevocationIds]).toEqual([1, 2]);
});

test('legacy record decoding refreshes distinct cached and uncached access ids across objects', () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = {
    configuration: fixture.createConfiguration(),
    ranges: new DefaultNumberRanges(),
    grantById: new Map([[11, new Grant(1, [])]]),
    revocationById: new Map([[21, new Revocation(1, [])]]),
  };
  const object: SyncResponseObject = {
    i: 101,
    v: 1,
    c: fixture.m.C1.tag,
    g: [11, 12],
    r: [21, 22],
    ro: [],
  };
  const ctx = new ResponseContext(database);
  DatabaseRecord.fromResponse(database, ctx, object);
  DatabaseRecord.fromResponse(database, ctx, {
    ...object,
    i: 102,
    g: [11, 13],
    r: [21, 23],
  });
  expect([...ctx.missingGrantIds]).toEqual([11, 12, 13]);
  expect([...ctx.missingRevocationIds]).toEqual([21, 22, 23]);
  const later = new ResponseContext(database);
  DatabaseRecord.fromResponse(database, later, {
    ...object,
    v: 2,
    g: [11],
    r: [21],
  });
  expect([...later.missingGrantIds]).toEqual([11]);
  expect([...later.missingRevocationIds]).toEqual([21]);
  const empty = new ResponseContext(database);
  expect([...empty.missingGrantIds]).toEqual([]);
  expect([...empty.missingRevocationIds]).toEqual([]);
});

test('an unchanged complete read advertises exact cached bodies without legacy requests', async () => {
  const { fixture, session, read } = await setup();
  await session.pull({ objectId: 101 });
  await session.pull({ objectId: 101 });
  expect(read).toHaveBeenCalledTimes(4);
  const completion = read.mock.calls[3][0] as ReadRequest;
  expect(completion.k).toBe('read');
  expect(completion.h.r).toEqual([
    { i: 101, c: fixture.m.C1.tag, v: 1, g: [0], r: [] },
  ]);
  expect(completion.h.s).toEqual([{ k: 'grant', i: 11, v: 1, h: true }]);
  expect(completion.h.p).toEqual([31, 32]);
  expect(session.instantiate<C1>(101).Name).toBe('original');
});

test('pull forwards the session context and completes permission refresh before returning objects', async () => {
  const { fixture, session, read } = await setup();
  const result = await session.pull({ objectId: 101 });
  expect(result.hasErrors).toBe(false);
  expect(read).toHaveBeenCalledTimes(2);
  expect(
    read.mock.calls.every(([request]) => request.x === session.context)
  ).toBe(true);
  expect(read.mock.calls.map(([request]) => request.k)).toEqual([
    'discover',
    'read',
  ]);
  expect(
    result.object<C1>(fixture.m.C1).strategy.canRead(fixture.m.C1.Name)
  ).toBe(true);
});
