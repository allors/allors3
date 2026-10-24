import { data } from '@allors/default/workspace/meta-json';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import {
  IDatabaseJsonClient,
  ResponseContext,
} from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { Fixture } from '../../fixture';

test('a pull response preserves scalar result values through JSON serialization', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const response = JSON.stringify({
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

test('synchronization refreshes distinct cached and uncached access ids across objects', () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  database.accessResponse({
    g: [{ i: 11, v: 1, p: [] }],
    r: [{ i: 21, v: 1, p: [] }],
  });

  const object: SyncResponseObject = {
    i: 101,
    v: 1,
    c: fixture.m.C1.tag,
    g: [11, 12],
    r: [21, 22],
    ro: [],
  };

  expect(
    database.onSyncResponse({
      o: [object, { ...object, i: 102, g: [11, 13], r: [21, 23] }],
    })
  ).toEqual({ g: [11, 12, 13], r: [21, 22, 23] });

  // A later response only refreshes the ids referenced by its own objects.
  expect(
    database.onSyncResponse({
      o: [{ ...object, v: 2, g: [11], r: [21] }],
    })
  ).toEqual({ g: [11], r: [21] });
  expect(database.onSyncResponse({ o: [] })).toBeNull();
});

test('an unchanged pull does not repeat synchronization or access requests', async () => {
  const fixture = new Fixture();
  fixture.metaPopulation = new LazyMetaPopulation(data);
  const database = fixture.createDatabaseConnection();
  const object: SyncResponseObject = {
    i: 101,
    v: 1,
    c: fixture.m.C1.tag,
    g: [11],
    r: [21],
    ro: [],
  };
  const client: IDatabaseJsonClient = {
    pull: jest.fn().mockResolvedValue({ p: [object] }),
    sync: jest.fn().mockResolvedValue({ o: [object] }),
    access: jest.fn().mockResolvedValue({
      g: [{ i: 11, v: 1, p: [] }],
      r: [{ i: 21, v: 1, p: [] }],
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
  expect(client.access).toHaveBeenCalledTimes(1);
  expect(client.access).toHaveBeenCalledWith({ g: [11], r: [21] });
  expect(client.permission).not.toHaveBeenCalled();
});
