import { data } from '@allors/default/workspace/meta-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';
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
