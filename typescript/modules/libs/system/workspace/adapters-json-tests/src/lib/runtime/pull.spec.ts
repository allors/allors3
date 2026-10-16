import {
  C1,
  Denied,
  I12,
  Organisation,
} from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
import {
  Fixture,
  name_c1A,
  name_c1B,
  name_c1C,
  name_c1D,
  name_c2A,
  name_c2B,
  name_c2C,
  name_c2D,
} from '../fixture';
import '../matchers';
import {
  PushReadRequest,
  PushReadResponse,
  ReadRequest,
  ReadResponse,
  ReadSuccess,
} from '@allors/system/common/protocol-json';
import { ReadHttpTransport } from '@allors/system/workspace/adapters-json';
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries -- Inspect immutable cache publication through a normal HTTP connection.
import { ReadStore } from '../../../../adapters-json/src/lib/read/read-store';
import httpFetch from 'cross-fetch';
import { Agent } from 'http';
import { PullResult } from '@allors/system/workspace/adapters-json';
import { Session as JsonSession } from '@allors/system/workspace/adapters-json';
import { Strategy as CommonStrategy } from '@allors/system/workspace/adapters';

describe('scoped test owner HTTP', () => {
  const scope = { workspace: 'Default', revision: 'core-read-v1' };

  test('bootstrap permits immediate Push and captures identity across fixture login for all endpoints', async () => {
    const credential = fixture.jsonClient.testUserId;
    const userId = fixture.jsonClient.userId;
    const fetch = jest.fn((url: string, init: RequestInit) =>
      httpFetch(url, init)
    );
    const owner = await fixture.createConnection(
      scope,
      fixture.jsonClient.createTransport(fetch)
    );
    const session = owner.createWorkspace().createSession();
    const sibling = owner.createWorkspace().createSession();
    try {
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(
        fetch.mock.calls.map(([, init]) => JSON.parse(init.body as string).k)
      ).toEqual(['discover', 'read']);
      await fixture.login('noacl');
      expect(fixture.jsonClient.testUserId).not.toBe(credential);
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'scoped owner immediate creation';
      const beforePush = fetch.mock.calls.length;
      expect((await session.push()).hasErrors).toBe(false);
      expect(created.id).toBeGreaterThan(0);
      expect(fetch.mock.calls.length - beforePush).toBe(1);
      const read = await session.pull({ objectId: created.id });
      expect(read.hasErrors).toBe(false);
      expect(read.scope.userId).toBe(userId);
      expect(session.instantiate<C1>(created.id).Name).toBe(created.Name);
      const beforeInvoke = fetch.mock.calls.length;
      const invoked = await session.invoke([]);
      expect(invoked.hasErrors).toBe(false);
      expect(invoked.outcome).toBe('committed');
      expect(invoked.application.acknowledgement).toBe('not-applicable');
      expect(invoked.application.refresh).toBe('applied');
      expect(invoked.refresh.status).toBe('succeeded');
      expect(fetch.mock.calls.length - beforeInvoke).toBe(1);
      created.Name = 'scoped owner saved with original identity';
      const beforeSave = fetch.mock.calls.length;
      const saved = await session.push({ refresh: true });
      expect(saved.outcome).toBe('committed');
      expect(saved.application.refresh).toBe('applied');
      expect(fetch.mock.calls.length - beforeSave).toBe(1);
      expect(
        new Set(
          fetch.mock.calls.map(([url]) =>
            url.substring(url.lastIndexOf('/') + 1)
          )
        )
      ).toEqual(new Set(['read', 'invoke-read', 'push-read']));
      expect(
        fetch.mock.calls.every(
          ([, init]) =>
            new Headers(init.headers).get('X-Allors-TestUser') === credential
        )
      ).toBe(true);
      const replacementFetch = jest.fn((url: string, init: RequestInit) =>
        httpFetch(url, init)
      );
      const replacement = await fixture.createConnection(
        scope,
        fixture.jsonClient.createTransport(replacementFetch)
      );
      try {
        expect(replacementFetch).toHaveBeenCalledTimes(2);
        const replacementSession = replacement
          .createWorkspace()
          .createSession();
        expect(replacementSession.instantiate(created.id)).toBeNull();
        expect((await replacementSession.pull([])).scope.userId).toBe(
          fixture.jsonClient.userId
        );
        expect(
          replacementFetch.mock.calls.every(
            ([, init]) =>
              new Headers(init.headers).get('X-Allors-TestUser') ===
              fixture.jsonClient.testUserId
          )
        ).toBe(true);
      } finally {
        await fixture.closeConnection(replacement);
      }
      await fixture.closeConnection(owner);
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      expect(
        (created.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
          fixture.m.C1.Name
        )
      ).toBe('scoped owner saved with original identity');
      expect(() => session.create(fixture.m.C1)).toThrow(/closed/i);
    } finally {
      await fixture.closeConnection(owner);
    }
  });

  test.each(['acknowledged', 'lost'])(
    'disposal drains an actually committed %s protected Push response before closing all Sessions',
    async (receipt) => {
      let release: () => void;
      let dispatched: () => void;
      const gate = new Promise<void>((resolve) => (release = resolve));
      const arrived = new Promise<void>((resolve) => (dispatched = resolve));
      const fetch = jest.fn(async (url: string, init: RequestInit) => {
        const response = await httpFetch(url, init);
        const text = await response.text();
        if (url.endsWith('/push-read')) {
          expect(JSON.parse(text).push.k).toBe('committed');
          dispatched();
          await gate;
          if (receipt === 'lost') throw new Error('Committed response lost');
        }
        return {
          ok: response.ok,
          status: response.status,
          text: async () => text,
        };
      });
      const transport = fixture.jsonClient.createTransport(fetch);
      const dispose = jest.spyOn(transport, 'dispose');
      const owner = await fixture.createConnection(scope, transport);
      const session = owner.createWorkspace().createSession();
      const sibling = owner.createWorkspace().createSession();
      const created = session.create<C1>(fixture.m.C1);
      created.Name = `owner drain ${receipt}`;
      const localId = created.id;
      const saving = session.push({ refresh: true });
      await arrived;
      const queued = sibling.pull([]).then(
        () => undefined,
        (error: Error) => error
      );
      let finished = false;
      const disposal = fixture.closeConnection(owner).then(() => {
        finished = true;
      });
      try {
        await Promise.resolve();
        expect(finished).toBe(false);
        expect(session.closed).toBe(false);
        expect(sibling.closed).toBe(false);
        expect(dispose).not.toHaveBeenCalled();
        expect(() => owner.createWorkspace().createSession()).toThrow(
          /closed|ready/i
        );
        release();
        const saved = await saving;
        expect(saved.outcome).toBe(
          receipt === 'acknowledged' ? 'committed' : 'unknown'
        );
        expect(saved.reconciliationRequired).toBe(true);
        expect((await queued).message).toMatch(/retired/i);
        await disposal;
        expect(dispose).toHaveBeenCalledTimes(1);
        expect(session.closed).toBe(true);
        expect(sibling.closed).toBe(true);
        expect(fetch).toHaveBeenCalledTimes(3);
        expect(created.id).toBe(localId);
        expect(
          (created.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
            fixture.m.C1.Name
          )
        ).toBe(`owner drain ${receipt}`);
        expect(created.strategy.diff()).toHaveLength(1);
        const observer = await fixture.createConnection(scope);
        try {
          const observed = await observer
            .createWorkspace()
            .createSession()
            .pull({
              extent: {
                kind: 'Filter',
                objectType: fixture.m.C1,
                predicate: {
                  kind: 'Equals',
                  propertyType: fixture.m.C1.Name,
                  value: `owner drain ${receipt}`,
                },
              },
            });
          expect(observed.collection<C1>(fixture.m.C1)).toHaveLength(1);
          expect(observed.collection<C1>(fixture.m.C1)[0].id).toBeGreaterThan(
            0
          );
        } finally {
          await fixture.closeConnection(observer);
        }
      } finally {
        release();
        await disposal;
      }
    }
  );

  test('failed HTTP bootstrap closes its temporary Session and disposes resources without retry', async () => {
    const fetch = jest.fn((url: string, init: RequestInit) =>
      httpFetch(url, init)
    );
    const transport = fixture.jsonClient.createTransport(fetch);
    const dispose = jest.spyOn(transport, 'dispose');
    const close = jest.spyOn(JsonSession.prototype, 'close');
    try {
      await expect(
        fixture.createConnection(
          { ...scope, revision: 'unknown-owner-revision' },
          transport
        )
      ).rejects.toThrow(/retired/i);
      expect(fetch).toHaveBeenCalledTimes(1);
      expect(fetch.mock.calls[0][0]).toBe(`${fixture.jsonClient.baseUrl}read`);
      expect(close).not.toHaveBeenCalled();
      expect(dispose).toHaveBeenCalledTimes(1);
    } finally {
      close.mockRestore();
    }
  });
});

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('andGreaterThanLessThan', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'And',
        operands: [
          {
            kind: 'GreaterThan',
            roleType: m.C1.C1AllorsInteger,
            value: 0,
          },
          {
            kind: 'LessThan',
            roleType: m.C1.C1AllorsInteger,
            value: 2,
          },
        ],
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);

  //  Interface
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'And',
        operands: [
          {
            kind: 'GreaterThan',
            roleType: m.I12.I12AllorsInteger,
            value: 0,
          },
          {
            kind: 'LessThan',
            roleType: m.I12.I12AllorsInteger,
            value: 2,
          },
        ],
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const i12s = result.collection<I12>(m.I12);
  expect(i12s).toEqualObjects([name_c1B, name_c2B]);
});

test('associationMany2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Empty
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'Nothing here!',
          },
        },
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(0);

  //  Full
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
        },
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);

  //  Filtered
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationMany2ManyContains', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Contains',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        object: c1c,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C]);
});

test('associationMany2ManyExists', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationMany2OneContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2One,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationMany2OneContains', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Contains',
        propertyType: m.C2.C1sWhereC1C2Many2One,
        object: c1c,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
});

test('associationOne2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1WhereC1C2One2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationOne2ManyEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2Many,
        object: c1b,
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);

  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2Many,
        object: c1c,
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C, name_c2D]);
});

test('associationOne2ManyExists', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1WhereC1C2One2Many,
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);

  //  Interface
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.I2.I1WhereI1I2One2Many,
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2ManyInstanceof', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.C2.C1WhereC1C2One2Many,
        objectType: m.C1,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2OneContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1WhereC1C2One2One,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationOne2OneEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2One,
        object: c1b,
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);

  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2One,
        object: c1c,
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
});

test('associationOne2OneExists', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Exists',
        propertyType: m.C1.C1WhereC1C1One2One,
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);

  //  Class
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1WhereC1C2One2One,
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2OneInstanceof', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  // Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.I12.I12WhereI12I12One2One,
        objectType: m.C1,
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1D, name_c2B, name_c2C]);

  // Interface
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.I12.I12WhereI12I12One2One,
        objectType: m.I2,
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1B, name_c1C, name_c2D]);
});

test('objectEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          object: c1c,
        },
      },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1C]);
  }

  // TODO:
  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.C1,
  //       predicate: {
  //         kind: 'Equals',
  //         parameter: 'obj',
  //       },
  //     },
  //     arguments: { obj: c1c },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);

  //   const c1s = result.collection(m.C1);

  //   expect(c1s).toEqualObjects([name_c1C]);
  // }
});

test('extentInterface', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([
    name_c1A,
    name_c1B,
    name_c1C,
    name_c1D,
    name_c2A,
    name_c2B,
    name_c2C,
    name_c2D,
  ]);
});

test('instanceof', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.I12,
  //       predicate: {
  //         kind: 'Instanceof',
  //         objectType: m.C1,
  //       },
  //     },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);

  //   const i12s = result.collection(m.I12);

  //   expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
  // }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.I12,
        predicate: {
          kind: 'Instanceof',
          parameter: 'type',
        },
      },
      arguments: { type: m.C1 },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const i12s = result.collection(m.I12);

    expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
  }
});

test('notEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Not',
        operand: {
          kind: 'Equals',
          object: c1c,
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B, name_c1D]);
});

test('orEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Or',
        operands: [
          {
            kind: 'Equals',
            object: c1b,
          },
          {
            kind: 'Equals',
            object: c1c,
          },
        ],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('operatorExcept', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Except',
      operands: [
        {
          kind: 'Filter',
          objectType: m.I12,
        },
        {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Instanceof',
            objectType: m.C2,
          },
        },
      ],
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
});

test('operatorIntersect', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Intersect',
      operands: [
        {
          kind: 'Filter',
          objectType: m.I12,
        },
        {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Instanceof',
            objectType: m.C2,
          },
        },
      ],
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c2A, name_c2B, name_c2C, name_c2D]);
});

test('operatorUnion', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Union',
      operands: [
        {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.Name,
            value: 'c1A',
          },
        },
        {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.Name,
            value: 'c1B',
          },
        },
      ],
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B]);
});

test('roleDateTimeBetweenPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDateTime,
        paths: [m.C1.C1DateTimeBetweenA, m.C1.C1DateTimeBetweenB],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDateTimeBetweenValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDateTime,
        values: [
          new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
          new Date('Sat Jan 01 2000 00:00:06 GMT+0000'),
        ],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleDateTimeGreaterThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDateTime,
        path: m.C1.C1DateTimeGreaterThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDateTimeGreaterThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDateTimeLessThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDateTime,
        path: m.C1.C1DateTimeLessThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDateTimeLessThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:05 GMT+0000'),
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDateTimeEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDecimalBetweenPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDecimal,
        paths: [m.C1.C1DecimalBetweenA, m.C1.C1DecimalBetweenB],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDecimalBetweenValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDecimal,
        values: ['2.1', '2.3'],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDecimalGreaterThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDecimal,
        path: m.C1.C1DecimalGreaterThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('roleDecimalGreaterThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDecimal,
        value: '1.5',
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDecimalLessThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDecimal,
        path: m.C1.C1DecimalLessThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDecimalLessThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDecimal,
        value: '1.9',
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDecimalEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDecimal,
        value: '2.2',
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleBetweenPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDouble,
        paths: [m.C1.C1DoubleBetweenA, m.C1.C1DoubleBetweenB],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDoubleBetweenValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDouble,
        values: [2.1, 2.3],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleGreaterThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDouble,
        path: m.C1.C1DoubleGreaterThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('roleDoubleGreaterThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDouble,
        value: 1.5,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleLessThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDouble,
        path: m.C1.C1DoubleLessThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDoubleLessThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDouble,
        value: 1.9,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDoubleEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDouble,
        value: 2.2,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerBetweenPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsInteger,
        paths: [m.C1.C1IntegerBetweenA, m.C1.C1IntegerBetweenB],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerBetweenValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsInteger,
        values: [1, 2],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleIntegerGreaterThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsInteger,
        path: m.C1.C1IntegerGreaterThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleIntegerGreaterThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsInteger,
        value: 1,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerLessThanPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsInteger,
        path: m.C1.C1IntegerLessThan,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleIntegerLessThanValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleIntegerEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerExist', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Exists',
        propertyType: m.C1.C1AllorsInteger,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleStringEqualsPath', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsString,
        path: m.C1.C1AllorsStringEquals,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
});

test('roleStringEqualsValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: m.C1.C1AllorsString,
          value: 'ᴀbra',
        },
      },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: m.C1.C1AllorsString,
          parameter: 'str',
        },
      },
      arguments: { str: 'ᴀbra' },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }
});

test('roleStringLike', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Like',
        roleType: m.C1.C1AllorsString,
        value: 'ᴀ%',
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleUniqueEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsUnique,
        value: '8B3C4978-72D3-40BA-B302-114EB331FE04',
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleMany2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  //  Empty
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'Nothing here!',
          },
        },
      },
    },
  };

  let result = await session.pull([pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(0);

  //  Full
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
        },
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  let c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);

  //  Filtered
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleMany2ManyContains', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c2c = await fixture.pullC2(session, name_c2C);

  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.C1,
  //       predicate: {
  //         kind: 'Contains',
  //         propertyType: m.C1.C1C2Many2Manies,
  //         object: c2c,
  //       },
  //     },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);

  //   const c1s = result.collection(m.C1);

  //   expect(c1s).toEqualObjects([name_c1C, name_c1D]);
  // }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Contains',
          propertyType: m.C1.C1C2Many2Manies,
          parameter: 'obj',
        },
      },
      arguments: { obj: c2c },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1C, name_c1D]);
  }
});

test('roleOne2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12One2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleOne2ManyContains', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c2d = await fixture.pullC2(session, name_c2D);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Contains',
        propertyType: m.C1.C1C2One2Manies,
        object: c2d,
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
});

test('roleMany2OneContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'ContainedIn',
          propertyType: m.C1.C1I12Many2One,
          extent: {
            kind: 'Filter',
            objectType: m.I12,
            predicate: {
              kind: 'Equals',
              propertyType: m.I12.I12AllorsString,
              value: 'ᴀbra',
            },
          },
        },
      },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }

  {
    const objectsPull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.I12,
        predicate: {
          kind: 'Equals',
          propertyType: m.I12.I12AllorsString,
          value: 'ᴀbra',
        },
      },
    };

    const objectsResult = await session.pull([objectsPull]);
    const objects = objectsResult.collection(m.I12);

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'ContainedIn',
          propertyType: m.C1.C1I12Many2One,
          parameter: 'objects',
        },
      },
      arguments: {
        objects,
      },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }
});

test('roleOne2OneContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12One2One,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('withResultName', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
    results: [
      {
        name: 'IetsAnders',
      },
    ],
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection('IetsAnders');

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('pullWithObjectId', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a_1 = await fixture.pullC1(session, name_c1A);

  const pull: Pull = {
    objectId: c1a_1.id,
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(1);

  const c1a_2 = result.object(m.C1);

  expect([c1a_2]).toEqualObjects([name_c1A]);
});

test('pullWithInclude', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
    },
    results: [
      {
        include: [
          {
            propertyType: m.C1.C1C2One2One,
          },
        ],
      },
    ],
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);

  const c1s = result.collection<C1>(m.C1);

  const c1b = c1s.find((v) => v.Name === name_c1B);
  const c1c = c1s.find((v) => v.Name === name_c1C);
  const c1d = c1s.find((v) => v.Name === name_c1D);

  expect(c1b.C1C2One2One.Name).toBe(name_c2B);
  expect(c1c.C1C2One2One.Name).toBe(name_c2C);
  expect(c1d.C1C2One2One.Name).toBe(name_c2D);
});

describe('protected read HTTP entrypoint', () => {
  function request(id: string): ReadRequest {
    return {
      v: 1,
      i: id,
      k: 'read',
      x: 'typescript-http',
      p: {
        l: [
          {
            e: { k: 1, t: fixture.m.C1.tag },
            r: [
              {
                n: 'found',
                i: [{ r: fixture.m.C1.C1C2One2One.relationType.tag }],
              },
            ],
          },
          {
            o: Number.MAX_SAFE_INTEGER,
            t: fixture.m.C1.tag,
            r: [{ n: 'missing', i: [] }],
          },
        ],
        d: [],
      },
    };
  }

  function transport() {
    const credential = fixture.jsonClient.testUserId;
    const fetch = jest.fn((url: string, init: RequestInit) =>
      httpFetch(url, {
        ...init,
        headers: {
          ...(init.headers as Record<string, string>),
          'X-Allors-TestUser': credential,
        },
        agent: new Agent({ keepAlive: false }),
      } as RequestInit)
    );
    return {
      read: new ReadHttpTransport(`${fixture.jsonClient.baseUrl}read`, fetch),
      fetch,
    };
  }

  test('scoped Session HTTP discovers then completes reads and merges exact baselines with protected Push remapping', async () => {
    const fetch = jest.fn((url: string, init: RequestInit) =>
      httpFetch(url, init)
    );
    const database = await fixture.createConnection(
      { revision: 'core-read-v1' },
      fixture.jsonClient.createTransport(fetch)
    );
    expect(
      fetch.mock.calls.map(([, init]) => JSON.parse(init.body as string).k)
    ).toEqual(['discover', 'read']);
    fetch.mockClear();
    const session = database.createWorkspace().createSession();
    const selection: Pull = {
      extent: { kind: 'Filter', objectType: fixture.m.C1 },
      results: [{ name: 'objects' }],
    };
    const first = await session.pull(selection);
    expect(first.hasErrors).toBe(false);
    expect(first.collection('objects').length).toBeGreaterThan(0);
    expect(fetch).toHaveBeenCalledTimes(2);
    expect(
      fetch.mock.calls.map(([, init]) => JSON.parse(init.body as string).k)
    ).toEqual(['discover', 'read']);
    expect(fetch.mock.calls.every(([url]) => url.endsWith('/read'))).toBe(true);
    const object = first.collection<C1>('objects')[0];
    const baseline = object.Name;
    object.Name = 'scoped edited';
    await session.pull({ objectId: object.id });
    expect(object.Name).toBe('scoped edited');
    expect(object.strategy.diff()[0]).toEqual(
      expect.objectContaining({ originalRole: baseline })
    );
    await session.push({ refresh: false });
    await session.pull({ objectId: object.id });
    expect(object.Name).toBe('scoped edited');
    const created = session.create<C1>(fixture.m.C1);
    created.Name = 'scoped created';
    expect(created.id).toBeLessThan(0);
    await session.push();
    expect(created.id).toBeGreaterThan(0);
    await session.pull({ objectId: created.id });
    expect(created.Name).toBe('scoped created');
    const empty = await session.pull([]);
    expect(empty.hasErrors).toBe(false);
    const absent = await session.pull({
      objectId: Number.MAX_SAFE_INTEGER,
      results: [{ name: 'absent' }],
    });
    expect(absent.hasErrors).toBe(false);
    expect(absent.outcomes[0].s).toBe('unavailable');
    expect(absent.object('absent')).toBeNull();
    session.close();
    void database.close();
  });

  test('scoped Session HTTP preserves inline arguments optional parameters and empty results', async () => {
    const fetch = jest.fn((url: string, init: RequestInit) =>
      httpFetch(url, init)
    );
    const database = await fixture.createConnection(
      { revision: 'core-read-v1' },
      fixture.jsonClient.createTransport(fetch)
    );
    expect(
      fetch.mock.calls.map(([, init]) => JSON.parse(init.body as string).k)
    ).toEqual(['discover', 'read']);
    fetch.mockClear();
    const session = database.createWorkspace().createSession();
    const query: Pull = {
      extent: {
        kind: 'Filter',
        objectType: fixture.m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: fixture.m.C1.Name,
          parameter: 'name',
        },
      },
      arguments: { name: name_c1A },
      results: [{ name: 'named' }],
    };
    const selected = await session.pull(query);
    expect(selected.hasErrors).toBe(false);
    expect(
      selected.collection<C1>('named').map((object) => object.Name)
    ).toEqual([name_c1A]);
    const optional = await session.pull({ ...query, arguments: {} });
    expect(optional.hasErrors).toBe(false);
    expect(optional.collection('named').length).toBeGreaterThan(1);
    const empty = await session.pull({ ...query, results: [] });
    expect(empty.hasErrors).toBe(false);
    expect(JSON.parse(fetch.mock.calls[5][1].body as string).p.l[0].r).toEqual(
      []
    );
    const noOutputs = await session.pull({
      objectId: selected.collection('named')[0].id,
      results: [],
    });
    expect(noOutputs.collections.size).toBe(0);
    expect(noOutputs.objects.size).toBe(0);
    expect(fetch).toHaveBeenCalledTimes(8);
    session.close();
    void database.close();
  });

  function complete(
    value: Awaited<ReturnType<ReadHttpTransport['read']>>
  ): ReadSuccess {
    expect(value.k).toBe('complete');
    if (value.k !== 'complete') throw new Error(JSON.stringify(value));
    return value;
  }

  test('connection HTTP cold and cached reads always discover then return equivalent complete closures', async () => {
    const { fetch } = transport();
    const owner = await fixture.createConnection(
      {
        revision: 'core-read-v1',
        context: 'typescript-http',
      },
      fixture.jsonClient.createTransport(fetch)
    );
    expect(fetch).toHaveBeenCalledTimes(2);
    expect(
      fetch.mock.calls.map(([, init]) => JSON.parse(init.body as string).k)
    ).toEqual(['discover', 'read']);
    fetch.mockClear();
    const session = owner.createWorkspace().createSession();
    const query: Pull[] = [
      {
        extent: { kind: 'Filter', objectType: fixture.m.C1 },
        results: [
          {
            name: 'found',
            include: [{ propertyType: fixture.m.C1.C1C2One2One }],
          },
        ],
      },
      {
        objectId: Number.MAX_SAFE_INTEGER,
        objectType: fixture.m.C1,
        results: [{ name: 'missing', include: [] }],
      },
    ];
    const batches: ReturnType<ReadStore['publish']>[] = [];
    const caches: ReadStore[] = [];
    const publish = ReadStore.prototype.publish;
    const spy = jest
      .spyOn(ReadStore.prototype, 'publish')
      .mockImplementation(function (publication) {
        caches.push(this);
        const batch = publish.call(this, publication);
        batches.push(batch);
        return batch;
      });
    try {
      const cold = await session.pull(query);
      const cached = await session.pull(query);
      const discovered = await session.pull(query);
      expect(cached.hasErrors).toBe(false);
      expect(batches[1].records).toEqual(batches[0].records);
      expect(batches[1].security).toEqual(batches[0].security);
      expect(batches[1].permissions).toEqual(batches[0].permissions);
      expect(batches[2].records).toEqual(batches[0].records);
      expect(discovered.collections).toEqual(cold.collections);
      expect(discovered.object('missing')).toBeNull();
      expect(discovered.outcomes[1].s).toBe('unavailable');
      expect(fetch).toHaveBeenCalledTimes(6);
      const sent = fetch.mock.calls.map(
        ([, init]) => JSON.parse(init.body as string) as ReadRequest
      );
      expect(sent.map((entry) => entry.k)).toEqual([
        'discover',
        'read',
        'discover',
        'read',
        'discover',
        'read',
      ]);
      expect(sent[3].h.r.length).toBe(batches[0].records.length);
      expect(sent[5].p).toEqual(sent[4].p);
      expect(
        fetch.mock.calls.every(
          ([url]) => url === `${fixture.jsonClient.baseUrl}read`
        )
      ).toBe(true);
      await owner.close();
      expect(caches[0].getRecord(batches[0].records[0])).toBeUndefined();
      expect(batches[2].records.length).toBeGreaterThan(0);
      expect(discovered.collection('found').length).toBeGreaterThan(0);
    } finally {
      spy.mockRestore();
      await owner.close();
    }
  });

  test('returns full included records, permission closure, and unavailable input in one request', async () => {
    const { read, fetch } = transport();
    const result = complete(await read.read(request('typescript-cold')));
    expect(result.sc).toMatchObject({
      u: fixture.jsonClient.userId,
      w: 'Default',
      r: 'core-read-v1',
      x: 'typescript-http',
    });
    expect(result.sc.p.length).toBeGreaterThan(0);
    expect(result.c.found.length).toBeGreaterThan(0);
    expect(result.o.missing).toBeNull();
    expect(result.l).toEqual([
      { i: 0, o: null, s: 'resolved' },
      { i: 1, o: Number.MAX_SAFE_INTEGER, s: 'unavailable' },
    ]);
    expect(result.p.some((record) => record.c === fixture.m.C1.tag)).toBe(true);
    expect(result.p.some((record) => record.c === fixture.m.C2.tag)).toBe(true);
    expect(result.p.every((record) => Array.isArray(record.p.ro))).toBe(true);
    expect(result.s.length).toBeGreaterThan(0);
    expect(result.s.every((security) => Array.isArray(security.p.p))).toBe(
      true
    );
    const permissionIds = new Set(
      result.s.reduce<number[]>((ids, security) => ids.concat(security.p.p), [])
    );
    expect(new Set(result.pm.map((permission) => permission.i))).toEqual(
      permissionIds
    );
    expect(result.pm.every((permission) => permission.p.c != null)).toBe(true);
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(fetch.mock.calls[0][0]).toBe(`${fixture.jsonClient.baseUrl}read`);
  });

  test('reuses exact held records, security bodies, and permission descriptors with equivalent closure', async () => {
    const { read, fetch } = transport();
    const cold = request('typescript-full');
    const full = complete(await read.read(cold));
    const cached: ReadRequest = {
      ...cold,
      i: 'typescript-reused',
      sc: full.sc,
      h: {
        r: full.p.map(({ i, c, v, g, r }) => ({ i, c, v, g, r })),
        s: full.s.map(({ k, i, v }) => ({ k, i, v, h: true })),
        p: full.pm.map(({ i }) => i),
      },
    };
    const reused = complete(await read.read(cached));
    expect(reused.p.every((record) => record.p.r != null)).toBe(true);
    expect(reused.s.every((security) => security.p.r != null)).toBe(true);
    expect(reused.pm.every((permission) => permission.p.r != null)).toBe(true);
    expect({
      ...reused,
      i: full.i,
      p: reused.p.map((record) => ({ ...record, p: full.p[record.p.r].p })),
      s: reused.s.map((security) => ({
        ...security,
        p: full.s[security.p.r].p,
      })),
      pm: reused.pm.map((permission) => ({
        ...permission,
        p: full.pm[permission.p.r].p,
      })),
    }).toEqual(full);
    expect(fetch).toHaveBeenCalledTimes(2);
    expect(
      fetch.mock.calls.every(
        ([url]) => url === `${fixture.jsonClient.baseUrl}read`
      )
    ).toBe(true);
  });

  test('discovers headers and then reads full payloads without claiming unheld bodies', async () => {
    const { read, fetch } = transport();
    const discover: ReadRequest = {
      ...request('typescript-discover'),
      k: 'discover',
    };
    const discovered = await read.read(discover);
    expect(discovered.k).toBe('discovery');
    if (discovered.k !== 'discovery')
      throw new Error(JSON.stringify(discovered));
    const result = complete(
      await read.read({
        ...discover,
        i: 'typescript-after-discovery',
        k: 'read',
        sc: discovered.sc,
      })
    );
    expect(result.p.map(({ i, c, v, g, r }) => ({ i, c, v, g, r }))).toEqual(
      discovered.p
    );
    expect(result.p.every((record) => Array.isArray(record.p.ro))).toBe(true);
    expect(result.s.every((security) => Array.isArray(security.p.p))).toBe(
      true
    );
    expect(fetch).toHaveBeenCalledTimes(2);
  });

  test('returns scope mismatch without retrying or accepting cached results', async () => {
    const { read, fetch } = transport();
    const cold = request('typescript-scope');
    const full = complete(await read.read(cold));
    const mismatch = await read.read({
      ...cold,
      i: 'typescript-other-population',
      sc: { ...full.sc, p: 'different-population' },
    });
    expect(mismatch.k).toBe('error');
    expect(mismatch).not.toHaveProperty('p');
    expect(fetch).toHaveBeenCalledTimes(2);
  });
});

describe('scoped protected Push HTTP integration', () => {
  async function connection() {
    const requests: { url: string; body: ReadRequest | PushReadRequest }[] = [];
    const responses: { url: string; body: ReadResponse | PushReadResponse }[] =
      [];
    const interception: {
      afterResponse?: (url: string, body: string) => Promise<string | void>;
    } = {};
    const fetch = jest.fn(async (url: string, init: RequestInit) => {
      requests.push({ url, body: JSON.parse(init.body as string) });
      const response = await httpFetch(url, init);
      const body = await response.text();
      responses.push({ url, body: JSON.parse(body) });
      // Faults run after a complete real HTTP response; the server has already
      // established its outcome before a selected read part or receipt is lost.
      const intercepted = await interception.afterResponse?.(url, body);
      return {
        ok: response.ok,
        status: response.status,
        text: async () =>
          typeof intercepted === 'string' ? intercepted : body,
      };
    });
    const database = await fixture.createConnection(
      { revision: 'core-read-v1' },
      fixture.jsonClient.createTransport(fetch)
    );
    expect(requests.map(({ body }) => (body as ReadRequest).k)).toEqual([
      'discover',
      'read',
    ]);
    fetch.mockClear();
    requests.length = 0;
    responses.length = 0;
    return {
      database,
      session: database.createWorkspace().createSession(),
      fetch,
      requests,
      responses,
      interception,
      clear: () => {
        fetch.mockClear();
        requests.length = 0;
        responses.length = 0;
      },
    };
  }

  function c1s(): Pull {
    return {
      extent: { kind: 'Filter', objectType: fixture.m.C1 },
      results: [{ name: 'objects' }],
    };
  }

  test.each([true, false])(
    'ordinary Push default and explicit skip preserve their baseline contract',
    async (refresh) => {
      const { session, database, fetch, requests, clear } = await connection();
      try {
        const existing = (await session.pull(c1s())).collection<C1>(
          'objects'
        )[0];
        const original = existing.Name;
        session.checkpoint();
        existing.Name = 'ordinary protected saved';
        const created = session.create<C1>(fixture.m.C1);
        created.Name = 'ordinary protected created';
        const localId = created.id;
        clear();

        const saved = refresh
          ? await session.push()
          : await session.push({ refresh: false });

        expect(saved.outcome).toBe('committed');
        expect(saved.hasErrors).toBe(false);
        expect(saved.application.acknowledgement).toBe('applied');
        expect(saved.refresh.status).toBe(
          refresh ? 'succeeded' : 'not-requested'
        );
        expect(saved.application.refresh).toBe(
          refresh ? 'applied' : 'not-applicable'
        );
        expect(saved.creationMappings).toEqual([
          { workspaceId: localId, databaseId: created.id },
        ]);
        expect(created.id).toBeGreaterThan(0);
        expect(session.instantiate(created.id)).toBe(created);
        expect(session.instantiate(localId)).toBeNull();
        expect(fetch).toHaveBeenCalledTimes(1);
        expect(requests[0].url).toBe(`${fixture.jsonClient.baseUrl}push-read`);
        expect((requests[0].body as PushReadRequest).returnChanged).toBe(
          refresh
        );
        expect(existing.strategy.canWrite(fixture.m.C1.Name)).toBe(refresh);
        expect(created.strategy.canWrite(fixture.m.C1.Name)).toBe(refresh);
        if (refresh) existing.Name = 'later local edit';
        session.reset();
        expect(existing.Name).toBe(
          refresh ? 'ordinary protected saved' : original
        );
        if (!refresh) {
          expect(existing.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
          const read = await session.pull([
            { objectId: existing.id, results: [{ name: 'existing' }] },
            { objectId: created.id, results: [{ name: 'created' }] },
          ]);
          expect(read.object('existing')).toBe(existing);
          expect(read.object('created')).toBe(created);
          expect(existing.Name).toBe('ordinary protected saved');
          expect(created.Name).toBe('ordinary protected created');
          expect(existing.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
          expect(created.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
          expect(fetch).toHaveBeenCalledTimes(3);
        }
      } finally {
        session.close();
        void database.close();
      }
    }
  );

  test('changed-only save uses one HTTP request and remaps a new object in place', async () => {
    const { session, database, fetch, requests, responses, clear } =
      await connection();
    try {
      const existing = (await session.pull(c1s())).collection<C1>('objects')[0];
      existing.Name = 'PushRead existing';
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'PushRead created';
      const localId = created.id;
      expect(localId).toBeLessThan(0);
      clear();

      const saved = await session.push({ refresh: true });

      expect(saved.outcome).toBe('committed');
      expect(saved.application.refresh).toBe('applied');
      expect(saved.refresh.result.hasErrors).toBe(false);
      expect(saved.refresh.result.collections.size).toBe(0);
      expect(saved.refresh.result.objects.size).toBe(0);
      expect(saved.discovery.status).toBe('not-requested');
      expect(saved.pendingQuery).toBeUndefined();
      expect(saved.reconciliationRequired).toBe(false);
      expect(fetch).toHaveBeenCalledTimes(1);
      expect(requests[0].url).toBe(`${fixture.jsonClient.baseUrl}push-read`);
      expect((requests[0].body as PushReadRequest).push.n[0].w).toBe(localId);
      const response = responses[0].body as PushReadResponse;
      expect(response.push.n).toContainEqual({ w: localId, d: created.id });
      expect(created.id).toBeGreaterThan(0);
      expect(session.instantiate(created.id)).toBe(created);
      expect(session.instantiate(localId)).toBeNull();
      expect(session.instantiate(existing.id)).toBe(existing);
      expect(existing.Name).toBe('PushRead existing');
      expect(created.Name).toBe('PushRead created');
      expect(existing.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
      expect(created.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
      expect(session.pushToDatabaseTracker.created).toBeNull();
      expect(session.pushToDatabaseTracker.changed).toBeNull();

      const observer = (await fixture.createWorkspace()).createSession();
      const persisted = await observer.pull([
        { objectId: existing.id, results: [{ name: 'existing' }] },
        { objectId: created.id, results: [{ name: 'created' }] },
      ]);
      expect(persisted.object<C1>('existing').Name).toBe('PushRead existing');
      expect(persisted.object<C1>('created').Name).toBe('PushRead created');
    } finally {
      session.close();
      void database.close();
    }
  });

  test('bundled query completes on demand in a second HTTP request using exact refreshed record bodies', async () => {
    const { session, database, fetch, requests, responses, clear } =
      await connection();
    try {
      const existing = (await session.pull(c1s())).collection<C1>('objects')[0];
      existing.Name = 'PushRead refreshed';
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'PushRead included creation';
      const query = c1s();
      clear();

      const saved = await session.push({
        refresh: true,
        discover: query,
      });

      expect(saved.outcome).toBe('committed');
      expect(saved.application.refresh).toBe('applied');
      expect(saved.discovery.status).toBe('succeeded');
      expect(fetch).toHaveBeenCalledTimes(1);
      const initial = requests[0].body as PushReadRequest;
      const pushed = responses[0].body as PushReadResponse;
      expect(pushed.changed.k).toBe('changed');
      if (pushed.changed.k !== 'changed')
        throw new Error('Expected changed records.');
      const changedResponse = pushed.changed;
      expect(pushed.changed.p.map(({ i }) => i)).toEqual(
        expect.arrayContaining([existing.id, created.id])
      );
      // Changing the caller-owned selection does not change the captured query.
      query.results[0].name = 'mutated after save';
      const final = await saved.pendingQuery.complete();

      expect(final.hasErrors).toBe(false);
      expect(final.collection('objects')).toEqual(
        expect.arrayContaining([existing, created])
      );
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(requests.map(({ url }) => url)).toEqual([
        `${fixture.jsonClient.baseUrl}push-read`,
        `${fixture.jsonClient.baseUrl}read`,
      ]);
      const completedRequest = requests[1].body as ReadRequest;
      const completedResponse = responses[1].body as ReadSuccess;
      expect(completedRequest.k).toBe('read');
      expect(completedRequest.p).toEqual(initial.discover);
      expect(completedResponse.k).toBe('complete');
      for (const changed of pushed.changed.p) {
        const heldIndex = completedRequest.h.r.findIndex(
          (record) => record.i === changed.i && record.v === changed.v
        );
        expect(heldIndex).toBeGreaterThanOrEqual(0);
        expect(completedRequest.h.r[heldIndex]).toMatchObject({
          i: changed.i,
          c: changed.c,
          v: changed.v,
        });
        for (const provenance of ['g', 'r'] as const) {
          expect(
            completedRequest.h.r[heldIndex][provenance].map((index) => {
              const { k, i, v } = completedRequest.h.s[index];
              return { k, i, v };
            })
          ).toEqual(
            changed[provenance].map((index) => {
              const { k, i, v } = changedResponse.s[index];
              return { k, i, v };
            })
          );
        }
        const payload = completedResponse.p.find(({ i }) => i === changed.i).p;
        expect(payload.r).toBe(heldIndex);
        expect(payload.ro == null).toBe(true);
      }
      await expect(saved.pendingQuery.complete()).rejects.toThrow(/once|used/i);
      expect(fetch).toHaveBeenCalledTimes(2);
    } finally {
      session.close();
      void database.close();
    }
  });

  test('final HTTP read observes concurrent membership and data while preserving a local merge conflict', async () => {
    const { session, database, fetch, clear } = await connection();
    try {
      const objects = (await session.pull(c1s())).collection<C1>('objects');
      const leaving = objects.find(({ Name }) => Name === name_c1A);
      const entering = objects.find(({ Name }) => Name === name_c1B);
      leaving.Name = 'saved name';
      leaving.C1AllorsString = 'selected by PushRead';
      clear();
      const saved = await session.push({
        refresh: true,
        discover: [
          {
            extent: {
              kind: 'Filter',
              objectType: fixture.m.C1,
              predicate: {
                kind: 'Equals',
                propertyType: fixture.m.C1.C1AllorsString,
                value: 'selected by PushRead',
              },
            },
            results: [{ name: 'members' }],
          },
          { objectId: leaving.id, results: [{ name: 'leaving' }] },
        ],
      });
      expect(saved.discovery.status).toBe('succeeded');
      expect(fetch).toHaveBeenCalledTimes(1);
      leaving.Name = 'local edit after save';
      const writer = (await fixture.createWorkspace()).createSession();
      const remote = await writer.pull([
        { objectId: leaving.id, results: [{ name: 'leaving' }] },
        { objectId: entering.id, results: [{ name: 'entering' }] },
      ]);
      const remoteLeaving = remote.object<C1>('leaving');
      remoteLeaving.Name = 'concurrent name';
      remoteLeaving.C1AllorsString = 'left the selection';
      remote.object<C1>('entering').C1AllorsString = 'selected by PushRead';
      expect((await writer.push()).hasErrors).toBe(false);

      const final = await saved.pendingQuery.complete();

      expect(final.collection('members')).toEqual([entering]);
      expect(final.object('leaving')).toBe(leaving);
      expect(final.mergeErrors).toContain(leaving);
      expect(leaving.Name).toBe('local edit after save');
      expect(leaving.C1AllorsString).toBe('selected by PushRead');
      expect(entering.C1AllorsString).toBe('selected by PushRead');
      expect(fetch).toHaveBeenCalledTimes(2);
    } finally {
      session.close();
      void database.close();
    }
  });

  test.each(['grant', 'revocation'] as const)(
    'final HTTP read refreshes concurrent %s permission changes',
    async (kind) => {
      await fixture.login('administrator');
      const { session, database, fetch, clear } = await connection();
      try {
        const c1 = (await session.pull(c1s())).collection<C1>('objects')[0];
        const target =
          kind === 'grant'
            ? c1
            : (
                await session.pull({
                  extent: { kind: 'Filter', objectType: fixture.m.Denied },
                })
              ).collection<Denied>(fixture.m.Denied)[0];
        const role =
          kind === 'grant'
            ? fixture.m.C1.C1AllorsString
            : fixture.m.Denied.DefaultWorkspaceProperty;
        expect(target.strategy.canRead(role)).toBe(true);
        c1.Name = 'saved before security change';
        clear();
        const saved = await session.push({
          refresh: true,
          discover: { objectId: target.id, results: [{ name: 'target' }] },
        });
        expect(saved.outcome).toBe('committed');
        expect(saved.discovery.status).toBe('succeeded');
        expect(target.strategy.canRead(role)).toBe(true);
        if (kind === 'grant') {
          await fixture.jsonClient.removeC1StringReadGrant(target.id);
        } else {
          await fixture.jsonClient.addDeniedPropertyReadRevocation(target.id);
        }

        const final = await saved.pendingQuery.complete();

        expect(final.hasErrors).toBe(false);
        expect(final.object('target')).toBe(target);
        expect(target.strategy.canRead(role)).toBe(false);
        expect(fetch).toHaveBeenCalledTimes(2);
      } finally {
        session.close();
        void database.close();
      }
    }
  );

  test('final HTTP read reports concurrent deletion as unavailable and removes query membership', async () => {
    const { session, database, fetch, clear } = await connection();
    try {
      const selection: Pull = {
        extent: { kind: 'Filter', objectType: fixture.m.Organisation },
        results: [{ name: 'organisations' }],
      };
      const target = (await session.pull(selection)).collection<Organisation>(
        'organisations'
      )[0];
      target.Name = 'saved before deletion';
      clear();
      const saved = await session.push({
        refresh: true,
        discover: [
          selection,
          { objectId: target.id, results: [{ name: 'target' }] },
        ],
      });
      expect(saved.outcome).toBe('committed');
      expect(saved.discovery.status).toBe('succeeded');
      const writer = (await fixture.createWorkspace()).createSession();
      const remote = (
        await writer.pull({ objectId: target.id })
      ).object<Organisation>(fixture.m.Organisation);
      expect((await writer.invoke(remote.Delete)).hasErrors).toBe(false);

      const final = await saved.pendingQuery.complete();

      expect(final.hasErrors).toBe(false);
      expect(final.object('target')).toBeNull();
      expect(final.collection('organisations')).not.toContain(target);
      expect((final as PullResult).outcomes).toContainEqual({
        i: 1,
        o: target.id,
        s: 'unavailable',
      });
      expect(target.strategy.canRead(fixture.m.Organisation.Name)).toBe(false);
      expect(target.strategy.canWrite(fixture.m.Organisation.Name)).toBe(false);
      expect(fetch).toHaveBeenCalledTimes(2);
    } finally {
      session.close();
      void database.close();
    }
  });

  test.each(['changed', 'discovery'] as const)(
    'a failed %s part after real HTTP commit preserves the independent outcome',
    async (part) => {
      const { session, database, fetch, interception, responses, clear } =
        await connection();
      try {
        const target = (await session.pull(c1s())).collection<C1>('objects')[0];
        target.Name = `saved despite ${part} failure`;
        clear();
        interception.afterResponse = async (url, body) => {
          if (!url.endsWith('/push-read')) return undefined;
          const response = JSON.parse(body) as PushReadResponse;
          expect(response.push.k).toBe('committed');
          response[part] = {
            i: response.i,
            sc: response.sc,
            k: 'error',
            e: [{ c: 'test_read_failure', m: 'Refresh this read separately.' }],
          };
          return JSON.stringify(response);
        };

        const saved = await session.push({
          refresh: true,
          discover: { objectId: target.id, results: [{ name: 'target' }] },
        });

        expect((responses[0].body as PushReadResponse).push.k).toBe(
          'committed'
        );
        expect(saved.outcome).toBe('committed');
        expect(saved[part === 'changed' ? 'refresh' : 'discovery'].status).toBe(
          'failed'
        );
        expect(
          saved[part === 'changed' ? 'refresh' : 'discovery'].errors[0].message
        ).toMatch(/Refresh this read separately/);
        expect(saved.reconciliationRequired).toBe(false);
        expect(fetch).toHaveBeenCalledTimes(1);
        if (part === 'changed') {
          expect(saved.discovery.status).toBe('succeeded');
          expect(target.strategy.canWrite(fixture.m.C1.Name)).toBe(false);
          const final = await saved.pendingQuery.complete();
          expect(final.object('target')).toBe(target);
          expect(target.Name).toBe('saved despite changed failure');
          expect(target.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
          expect(fetch).toHaveBeenCalledTimes(2);
        } else {
          expect(saved.application.refresh).toBe('applied');
          expect(saved.pendingQuery).toBeUndefined();
          expect(target.Name).toBe('saved despite discovery failure');
          expect(target.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
        }
      } finally {
        session.close();
        void database.close();
      }
    }
  );

  test('server rejects invalid discovery before writing over HTTP and leaves local changes editable', async () => {
    const { session, database, fetch, clear } = await connection();
    try {
      const target = (await session.pull(c1s())).collection<C1>('objects')[0];
      const original = target.Name;
      target.Name = 'saved despite invalid query';
      clear();
      const saved = await session.push({
        refresh: true,
        discover: {
          extent: {
            kind: 'Filter',
            objectType: fixture.m.C1,
            predicate: {
              kind: 'Equals',
              propertyType: fixture.m.C1.C1AllorsInteger,
              value: 'not-an-integer',
            },
          },
        },
      });
      expect(saved.outcome).toBe('rejected');
      expect(saved.refresh.status).toBe('unavailable');
      expect(saved.discovery.status).toBe('unavailable');
      expect(saved.pendingQuery).toBeUndefined();
      expect(saved.reconciliationRequired).toBe(false);
      expect(target.Name).toBe('saved despite invalid query');
      expect(target.strategy.canWrite(fixture.m.C1.Name)).toBe(true);
      expect(fetch).toHaveBeenCalledTimes(1);
      const observer = (await fixture.createWorkspace()).createSession();
      const persisted = await observer.pull({ objectId: target.id });
      expect(persisted.object<C1>(fixture.m.C1).Name).toBe(original);
    } finally {
      session.close();
      void database.close();
    }
  });

  test('lost HTTP receipt after server commit leaves an unknown outcome without replay', async () => {
    const { session, database, fetch, responses, interception, clear } =
      await connection();
    try {
      await session.pull([]);
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'committed once before response loss';
      const localId = created.id;
      clear();
      interception.afterResponse = async (url) => {
        if (url.endsWith('/push-read'))
          throw new Error('Test: committed HTTP response lost.');
      };

      const saved = await session.push({ refresh: true });

      const committed = responses[0].body as PushReadResponse;
      expect(committed.push.k).toBe('committed');
      expect(committed.push.n).toHaveLength(1);
      expect(saved.outcome).toBe('unknown');
      expect(saved.reconciliationRequired).toBe(true);
      expect(session.saving).toBe(false);
      expect(created.id).toBe(localId);
      expect(created.Name).toBe('committed once before response loss');
      expect(() => session.create(fixture.m.C1)).toThrow(/reconcil/i);
      await expect(session.push({ refresh: false })).rejects.toThrow(
        /reconcil/i
      );
      await expect(session.push()).rejects.toThrow(/reconcil/i);
      expect(fetch).toHaveBeenCalledTimes(1);
      const observer = (await fixture.createWorkspace()).createSession();
      const persisted = await observer.pull({
        extent: {
          kind: 'Filter',
          objectType: fixture.m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: fixture.m.C1.Name,
            value: 'committed once before response loss',
          },
        },
      });
      const objects = persisted.collection<C1>(fixture.m.C1);
      expect(objects).toHaveLength(1);
      expect(objects[0].id).toBe(committed.push.n[0].d);
    } finally {
      session.close();
      void database.close();
    }
  });

  test.each(['close', 'retire'] as const)(
    'pending HTTP completion rejects after %s without another request',
    async (lifetime) => {
      const { session, database, fetch, clear } = await connection();
      try {
        const target = (await session.pull(c1s())).collection<C1>('objects')[0];
        target.Name = 'committed before lifetime ended';
        clear();
        const saved = await session.push({
          refresh: true,
          discover: { objectId: target.id },
        });
        expect(saved.outcome).toBe('committed');
        expect(saved.discovery.status).toBe('succeeded');
        if (lifetime === 'close') session.close();
        else void database.close();

        await expect(saved.pendingQuery.complete()).rejects.toThrow(
          /closed|retired/i
        );

        expect(fetch).toHaveBeenCalledTimes(1);
      } finally {
        session.close();
        void database.close();
      }
    }
  );

  test('retirement after server HTTP commitment retains the receipt and prevents local application', async () => {
    const { session, database, fetch, interception, clear } =
      await connection();
    try {
      await session.pull([]);
      const created = session.create<C1>(fixture.m.C1);
      created.Name = 'committed before retirement';
      const localId = created.id;
      clear();
      interception.afterResponse = async (url) => {
        if (url.endsWith('/push-read')) void database.close();
      };

      const saved = await session.push({ refresh: true });

      expect(saved.outcome).toBe('committed');
      expect(saved.creationMappings).toHaveLength(1);
      expect(saved.reconciliationRequired).toBe(true);
      expect(saved.application.errors[0].message).toMatch(/retired/i);
      expect(session.saving).toBe(false);
      expect(created.id).toBe(localId);
      expect(fetch).toHaveBeenCalledTimes(1);
      const observer = (await fixture.createWorkspace()).createSession();
      const persisted = await observer.pull({
        objectId: saved.creationMappings[0].databaseId,
      });
      expect(persisted.object<C1>(fixture.m.C1).Name).toBe(
        'committed before retirement'
      );
    } finally {
      session.close();
      void database.close();
    }
  });
});

afterEach(async () => {
  await fixture?.close();
});

test('normal Fixture identity replacement drains managed connections and keeps explicit independent ownership', async () => {
  const originalId = fixture.jsonClient.testUserId;
  const originalRoot = fixture.databaseConnection;
  const originalWorkspace = fixture.workspace;
  const rootSession = originalWorkspace.createSession() as JsonSession;
  const ordinaryWorkspace = await fixture.createWorkspace();
  const ordinary = ordinaryWorkspace.createSession() as JsonSession;
  const retained = rootSession.create<C1>(fixture.m.C1);
  retained.Name = 'unsaved original identity';
  const explicit = await fixture.createConnection({ revision: 'core-read-v1' });
  const explicitSession = explicit.createWorkspace().createSession();
  let release: () => void;
  let arrived: () => void;
  const held = new Promise<void>((resolve) => (release = resolve));
  const started = new Promise<void>((resolve) => (arrived = resolve));
  const fetch = jest.fn(async (url: string, init: RequestInit) => {
    const response = await httpFetch(url, init);
    if (hold && url.endsWith('/read')) {
      arrived();
      await held;
    }
    return response;
  });
  let hold = false;
  const transport = fixture.jsonClient.createTransport(fetch);
  const dispose = jest.spyOn(transport, 'dispose');
  const managed = await fixture.createConnection(
    { revision: 'core-read-v1' },
    transport,
    true
  );
  const active = managed.createWorkspace().createSession();
  expect(fetch).toHaveBeenCalledTimes(2);
  hold = true;
  const reading = active.pull([]).catch((error: Error) => error);
  await started;
  let finished = false;
  const login = fixture.login('noacl').then((value) => {
    finished = true;
    return value;
  });
  try {
    await Promise.resolve();
    expect(finished).toBe(false);
    expect(dispose).not.toHaveBeenCalled();
    expect(originalRoot.closed).toBe(true);
    release();
    await reading;
    expect(await login).toBe(true);
    expect(dispose).toHaveBeenCalledTimes(1);
    expect(rootSession.closed).toBe(true);
    expect(ordinary.closed).toBe(true);
    expect(active.closed).toBe(true);
    expect(fixture.jsonClient.testUserId).not.toBe(originalId);
    expect(fixture.databaseConnection).not.toBe(originalRoot);
    expect((await fixture.workspace.createSession().pull([])).hasErrors).toBe(
      false
    );
    expect(explicit.closed).toBe(false);
    expect((await explicitSession.pull([])).hasErrors).toBe(false);
    expect(
      (retained.strategy as CommonStrategy).DatabaseOriginState.getUnitRole(
        fixture.m.C1.Name
      )
    ).toBe('unsaved original identity');
    expect(retained.strategy.diff()).toHaveLength(1);
    await fixture.closeConnection(explicit);
    expect(explicitSession.closed).toBe(true);
  } finally {
    release();
    await login;
  }
});

test.each(['acknowledged', 'lost'] as const)(
  'normal close drains an actually committed %s Invoke receipt before releasing the owned HTTP transport',
  async (receipt) => {
    await fixture.login('administrator');
    let release: () => void;
    let arrived: () => void;
    const held = new Promise<void>((resolve) => (release = resolve));
    const started = new Promise<void>((resolve) => (arrived = resolve));
    const fetch = jest.fn(async (url: string, init: RequestInit) => {
      const response = await httpFetch(url, init);
      const body = await response.text();
      if (url.endsWith('/invoke-read')) {
        expect(JSON.parse(body).invoke.k).toBe('committed');
        arrived();
        await held;
        if (receipt === 'lost')
          throw new Error('committed Invoke response lost');
      }
      return {
        ok: response.ok,
        status: response.status,
        text: async () => body,
      };
    });
    const transport = fixture.jsonClient.createTransport(fetch);
    const dispose = jest.spyOn(transport, 'dispose');
    const connection = await fixture.createConnection(
      { revision: 'core-read-v1' },
      transport
    );
    const session = connection.createWorkspace().createSession();
    const sibling = connection.createWorkspace().createSession();
    const result = await session.pull({
      extent: { kind: 'Filter', objectType: fixture.m.Organisation },
    });
    const organisation = result.collection<Organisation>(
      fixture.m.Organisation
    )[0];
    const id = organisation.id;
    fetch.mockClear();
    const invoking = session.invoke(organisation.JustDoIt);
    await started;
    const queued = sibling.pull([]).catch((error: Error) => error);
    let finished = false;
    const closing = fixture
      .closeConnection(connection)
      .then(() => (finished = true));
    try {
      await Promise.resolve();
      expect(finished).toBe(false);
      expect(dispose).not.toHaveBeenCalled();
      expect(session.closed).toBe(false);
      release();
      const invoked = await invoking;
      expect(invoked.outcome).toBe(
        receipt === 'acknowledged' ? 'committed' : 'unknown'
      );
      expect(invoked.reconciliationRequired).toBe(receipt === 'lost');
      expect(invoked.application.acknowledgement).toBe('not-applicable');
      expect(await queued).toBeInstanceOf(Error);
      await closing;
      expect(session.closed).toBe(true);
      expect(sibling.closed).toBe(true);
      expect(dispose).toHaveBeenCalledTimes(1);
      expect(fetch).toHaveBeenCalledTimes(1);
      const observer = (await fixture.createWorkspace()).createSession();
      const observed = await observer.pull({
        objectId: id,
        results: [{ name: 'saved' }],
      });
      expect(observed.object<Organisation>('saved').JustDidIt).toBe(true);
      expect(observed.object<Organisation>('saved').JustDidItDerived).toBe(
        true
      );
    } finally {
      release();
      await closing;
    }
  }
);
