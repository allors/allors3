import httpFetch from 'cross-fetch';
import { InvokeReadRequest } from '@allors/system/common/protocol-json';
import {
  Organisation,
  C1,
  Cascader,
  Cascaded,
} from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('callSingle', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await session.pull([pull]);
  const organisation = result.collection<Organisation>(m.Organisation)[0];

  expect(organisation.JustDidIt).toBeFalsy();

  const invokeResult = await session.invoke(organisation.JustDoIt);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await session.pull([{ object: organisation }]);

  expect(organisation.JustDidIt).toBeTruthy();
  expect(organisation.JustDidItDerived).toBeTruthy();
});

test('callMultiple', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await session.pull([pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await session.invoke([
    organisation1.JustDoIt,
    organisation2.JustDoIt,
  ]);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await session.pull([pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
});

test('callMultipleAtomic', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await session.pull([pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await session.invoke([
    organisation1.JustDoIt,
    organisation2.JustDoIt,
  ]);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await session.pull([pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
});

async function protectedMethods(interception?: { mode?: 'corrupt' | 'lose' }) {
  await fixture.login('administrator');
  const requests: { path: string; body: InvokeReadRequest }[] = [];
  const fetch = jest.fn(async (url: string, init: RequestInit) => {
    const path = url.substring(url.lastIndexOf('/') + 1);
    requests.push({ path, body: JSON.parse(init.body as string) });
    const response = await httpFetch(url, init);
    if (path === 'invoke-read' && interception?.mode) {
      const raw = await response.text();
      expect(response.ok).toBe(true);
      const body = JSON.parse(raw);
      expect(body.invoke.k).toBe('committed');
      if (interception.mode === 'lose')
        throw new Error('response lost after server commitment');
      body.changed = { invalid: true };
      return {
        ok: response.ok,
        status: response.status,
        text: async () => JSON.stringify(body),
      };
    }
    return response;
  });
  const owner = await fixture.createConnection(
    { revision: 'core-read-v1' },
    fixture.jsonClient.createTransport(fetch)
  );
  const session = owner.createWorkspace().createSession();
  const pull: Pull = {
    extent: { kind: 'Filter', objectType: fixture.m.Organisation },
  };
  const result = await session.pull(pull);
  expect(result.hasErrors).toBe(false);
  const organisations = result.collection<Organisation>(fixture.m.Organisation);
  expect(organisations.length).toBeGreaterThan(1);
  requests.length = 0;
  fetch.mockClear();
  return { owner, session, organisations, requests, fetch };
}

test.each(['single', 'multiple'])(
  'protected Invoke refreshes %s methods and their derivation effects in one HTTP request',
  async (kind) => {
    const { owner, session, organisations, requests } =
      await protectedMethods();
    try {
      const targets = organisations.slice(0, kind === 'single' ? 1 : 2);
      for (const target of targets) expect(target.JustDidIt).toBeFalsy();
      const result = await session.invoke(
        kind === 'single'
          ? targets[0].JustDoIt
          : targets.map((object) => object.JustDoIt)
      );
      expect(result.outcome).toBe('committed');
      expect(result.refresh.status).toBe('succeeded');
      expect(result.application.acknowledgement).toBe('not-applicable');
      expect(result.application.refresh).toBe('applied');
      expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
      for (const target of targets) {
        expect(target.JustDidIt).toBe(true);
        expect(target.JustDidItDerived).toBe(true);
      }
    } finally {
      await fixture.closeConnection(owner);
    }
  }
);

test('protected Invoke reads committed state while preserving local edits and unsaved creations', async () => {
  const { owner, session, organisations, requests } = await protectedMethods();
  try {
    const object = organisations[0];
    const original = object.Name;
    object.Name = 'local edited name';
    const created = session.create<C1>(fixture.m.C1);
    created.Name = 'local creation';
    const localId = created.id;
    const result = await session.invoke(object.JustDoIt);
    expect(result.outcome).toBe('committed');
    expect(result.application.acknowledgement).toBe('not-applicable');
    expect(object.JustDidIt).toBe(true);
    expect(object.JustDidItDerived).toBe(true);
    expect(object.Name).toBe('local edited name');
    expect(created.id).toBe(localId);
    expect(created.id).toBeLessThan(0);
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    const reader = owner.createWorkspace().createSession();
    await reader.pull({ objectId: object.id });
    expect(reader.instantiate<Organisation>(object.id).Name).toBe(original);
    const saved = await session.push();
    expect(saved.outcome).toBe('committed');
    expect(created.id).toBeGreaterThan(0);
    await reader.pull([{ objectId: object.id }, { objectId: created.id }]);
    expect(reader.instantiate<Organisation>(object.id).Name).toBe(
      'local edited name'
    );
    expect(reader.instantiate<C1>(created.id).Name).toBe('local creation');
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke refresh skip leaves its baseline until an explicit Pull', async () => {
  const { owner, session, organisations, requests } = await protectedMethods();
  try {
    const object = organisations[0];
    const result = await session.invoke(object.JustDoIt, { refresh: false });
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('not-requested');
    expect(result.application.acknowledgement).toBe('not-applicable');
    expect(object.JustDidIt).toBeFalsy();
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    await session.pull({ objectId: object.id });
    expect(object.JustDidIt).toBe(true);
    expect(object.JustDidItDerived).toBe(true);
    expect(requests.map((entry) => entry.path)).toEqual([
      'invoke-read',
      'read',
      'read',
    ]);
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke rolls back earlier method effects when a later target was deleted in the batch', async () => {
  const { owner, session, organisations, requests } = await protectedMethods();
  try {
    const [first, second] = organisations;
    const result = await session.invoke([
      first.JustDoIt,
      second.Delete,
      second.JustDoIt,
    ]);
    expect(result.outcome).toBe('rejected');
    expect(result.hasErrors).toBe(true);
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    const reader = owner.createWorkspace().createSession();
    await reader.pull([{ objectId: first.id }, { objectId: second.id }]);
    expect(reader.instantiate<Organisation>(first.id).JustDidIt).toBeFalsy();
    expect(
      reader.instantiate<Organisation>(first.id).JustDidItDerived
    ).toBeFalsy();
    expect(reader.instantiate(second.id)).not.toBeNull();
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke rolls back earlier method effects when a later method throws', async () => {
  const { owner, session, organisations, requests } = await protectedMethods();
  try {
    const cascader = session.create<Cascader>(fixture.m.Cascader);
    const cascaded = session.create<Cascaded>(fixture.m.Cascaded);
    cascader.Cascaded = cascaded;
    expect((await session.push()).outcome).toBe('committed');
    requests.length = 0;
    const organisation = organisations[0];
    const result = await session.invoke([
      organisation.JustDoIt,
      cascaded.Delete,
    ]);
    expect(result.outcome).toBe('rejected');
    expect(result.hasErrors).toBe(true);
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    const reader = owner.createWorkspace().createSession();
    await reader.pull([
      { objectId: organisation.id },
      { objectId: cascaded.id },
    ]);
    expect(
      reader.instantiate<Organisation>(organisation.id).JustDidIt
    ).toBeFalsy();
    expect(
      reader.instantiate<Organisation>(organisation.id).JustDidItDerived
    ).toBeFalsy();
    expect(reader.instantiate(cascaded.id)).not.toBeNull();
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke keeps a committed HTTP receipt when changed records are corrupted', async () => {
  const { owner, session, organisations, requests } = await protectedMethods({
    mode: 'corrupt',
  });
  try {
    const object = organisations[0];
    object.Name = 'retained';
    const result = await session.invoke(object.JustDoIt);
    expect(result.outcome).toBe('committed');
    expect(result.refresh.status).toBe('failed');
    expect(result.reconciliationRequired).toBe(false);
    expect(object.Name).toBe('retained');
    object.Name = 'editable';
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    const reader = owner.createWorkspace().createSession();
    await reader.pull({ objectId: object.id });
    expect(reader.instantiate<Organisation>(object.id).JustDidIt).toBe(true);
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke reports a lost HTTP receipt as unknown without replay', async () => {
  const { owner, session, organisations, requests } = await protectedMethods({
    mode: 'lose',
  });
  try {
    const object = organisations[0];
    const result = await session.invoke(object.JustDoIt);
    expect(result.outcome).toBe('unknown');
    expect(result.reconciliationRequired).toBe(true);
    await expect(session.invoke(object.JustDoIt)).rejects.toThrow(/reconcil/i);
    await expect(session.push()).rejects.toThrow(/reconcil/i);
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    const reader = owner.createWorkspace().createSession();
    await reader.pull({ objectId: object.id });
    expect(reader.instantiate<Organisation>(object.id).JustDidIt).toBe(true);
    expect(reader.instantiate<Organisation>(object.id).JustDidItDerived).toBe(
      true
    );
  } finally {
    await fixture.closeConnection(owner);
  }
});

test('protected Invoke preserves ordered repeated-target calls with one original expected version', async () => {
  const { owner, session, organisations, requests } = await protectedMethods();
  try {
    const object = organisations[0];
    const version = object.strategy.version;
    const result = await session.invoke([object.JustDoIt, object.JustDoIt]);
    expect(result.outcome).toBe('committed');
    expect(object.JustDidIt).toBe(true);
    expect(object.JustDidItDerived).toBe(true);
    expect(requests.map((entry) => entry.path)).toEqual(['invoke-read']);
    expect(requests[0].body.l).toEqual([
      { i: object.id, v: version, m: fixture.m.Organisation.JustDoIt.tag },
      { i: object.id, v: version, m: fixture.m.Organisation.JustDoIt.tag },
    ]);
  } finally {
    await fixture.closeConnection(owner);
  }
});

afterEach(async () => {
  await fixture?.close();
});
