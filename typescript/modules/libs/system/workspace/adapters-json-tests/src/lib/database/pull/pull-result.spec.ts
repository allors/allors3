import { data } from '@allors/default/workspace/meta-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { Fixture } from '../../fixture';

test('a pull response preserves scalar result values through JSON serialization', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const response = JSON.stringify({
    p: [],
    g: [],
    r: [],
    v: {
      allorsBinary: 'AQID',
      allorsBoolean: true,
      allorsDateTime: new Date('1973-03-27T00:00:00Z'),
      allorsDecimal: '12.34',
      allorsDouble: 123,
      allorsInteger: 1000,
      allorsString: 'a string',
      allorsUnique: '2946cf37-71be-4681-8fe6-d0024d59beff',
    },
  });
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue(JSON.parse(response)),
    sync: jest.fn(),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();

  const result = await session.pull([]);

  expect(result.hasErrors).toBeFalsy();
  expect(result.values.size).toBe(8);
  expect(result.value('allorsBinary')).toEqual('AQID');
  expect(result.value('allorsBoolean')).toBe(true);
  expect(new Date(result.value('allorsDateTime') as string)).toEqual(
    new Date('1973-03-27T00:00:00Z')
  );
  expect(result.value('allorsDecimal')).toEqual('12.34');
  expect(result.value('allorsDouble')).toEqual(123);
  expect(result.value('allorsInteger')).toEqual(1000);
  expect(result.value('allorsString')).toEqual('a string');
  expect(result.value('allorsUnique')).toEqual(
    '2946cf37-71be-4681-8fe6-d0024d59beff'
  );
  expect(client.sync).not.toHaveBeenCalled();
  expect(client.access).not.toHaveBeenCalled();
  expect(client.permission).not.toHaveBeenCalled();
});

test('Sync replaces named objects, ordered repeated collections and values from Pull', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const first = { i: 101, v: 2, c: fixture.m.C1.tag, g: [], r: [], ro: [] };
  const last = { ...first, i: 102 };
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({
      p: [first],
      o: { selected: 101 },
      c: { ordered: [101] },
      v: { count: 1 },
      g: [],
      r: [],
    }),
    sync: jest.fn().mockResolvedValue({
      result: {
        o: { selected: 102 },
        c: { ordered: [102, 103, 102] },
        v: { count: 3 },
      },
      o: [last, { ...last, i: 103 }],
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

  const result = await session.pull({ objectId: 101 });

  expect(result.object('selected').id).toBe(102);
  expect(result.collection('ordered').map((v) => v.id)).toEqual([
    102, 103, 102,
  ]);
  expect(result.value('count')).toBe(3);
  expect(() => session.instantiate(101)).toThrow(
    'Object 101 is not in this session. Pull it before instantiating it.'
  );
  expect(client.sync).toHaveBeenCalledWith(
    expect.objectContaining({
      pull: expect.objectContaining({ l: expect.any(Array) }),
      o: [],
      g: [],
      r: [],
    })
  );
});

test('an object missing from the final selection remains in an existing session', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = { i: 101, v: 2, c: fixture.m.C1.tag, g: [], r: [], ro: [] };
  const client: IDatabaseJsonClient = {
    pull: jest
      .fn()
      .mockResolvedValueOnce({ p: [object], g: [], r: [] })
      .mockResolvedValue({
        p: [{ ...object, v: 3 }],
        o: { selected: 101 },
        g: [],
        r: [],
      }),
    sync: jest
      .fn()
      .mockResolvedValueOnce({
        result: { o: { selected: 101 } },
        o: [object],
        g: [],
        r: [],
        p: [],
      })
      .mockResolvedValue({
        result: { o: { selected: 0 }, c: { ordered: [] }, v: { count: 0 } },
        o: [],
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
  const original = (await session.pull({ objectId: 101 })).object('selected');
  const result = await session.pull({ objectId: 101 });
  expect(result.object('selected')).toBeNull();
  expect(result.collection('ordered')).toEqual([]);
  expect(result.value('count')).toBe(0);
  expect(session.instantiate(101)).toBe(original);
});

test('Sync retains the original Pull arguments and dependencies and owns its final result buffers', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object = { i: 101, v: 2, c: fixture.m.C1.tag, g: [], r: [], ro: [] };
  const response = {
    result: { c: { selected: [101, 101] }, v: { items: [1, 2] } },
    o: [object],
    g: [],
    r: [],
    p: [],
  };
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({ p: [object], g: [], r: [] }),
    sync: jest.fn().mockResolvedValue(response),
    access: jest.fn(),
    permission: jest.fn(),
    push: jest.fn(),
    invoke: jest.fn(),
  };
  database.client = client;
  const session = database.createWorkspace().createSession();
  session.context = 'original';
  session.activate([
    {
      objectType: fixture.m.C1,
      roleType: fixture.m.C1.Name,
      dependencies: [
        { objectType: fixture.m.C1, propertyType: fixture.m.C1.Name },
      ],
      derive: () => null,
    },
  ]);
  const date = new Date('2026-01-01T00:00:00Z');
  const loading = session.pull({ extentRef: 'selection', arguments: { date } });
  date.setUTCFullYear(2027);
  session.context = 'changed';
  const result = await loading;
  const sent = (client.pull as jest.Mock).mock.calls[0][0];
  expect((client.sync as jest.Mock).mock.calls[0][0].pull).toEqual(sent);
  expect(sent.x).toBe('original');
  expect(sent.l[0].a.date).toBe('2026-01-01T00:00:00.000Z');
  expect(sent.d).toEqual([
    { o: fixture.m.C1.tag, r: fixture.m.C1.Name.relationType.tag },
  ]);
  response.result.c.selected.splice(0);
  response.result.v.items.push(3);
  expect(result.collection('selected').map((v) => v.id)).toEqual([101, 101]);
  expect(result.value('items')).toEqual([1, 2]);
});
