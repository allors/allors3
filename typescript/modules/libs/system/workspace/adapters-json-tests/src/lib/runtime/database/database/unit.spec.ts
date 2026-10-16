import { C1 } from '@allors/default/workspace/domain';
import { Fixture, name_c1C } from '../../../fixture';
import { Pull } from '@allors/system/workspace/domain';
import '../../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('unit', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.Name,
        value: 'c2C',
      },
    },
    results: [
      {
        include: [
          {
            propertyType: m.C2.C1WhereC1C2One2Many,
          },
        ],
      },
    ],
  };

  const result = await session.pull([pull]);

  const c2s = result.collection('C2s');

  const c2C = c2s[0];

  const c1WhereC1C2One2Many = c2C.C1WhereC1C2One2Many;

  expect(c1WhereC1C2One2Many).toBeDefined();
  expect(c1WhereC1C2One2Many.Name).toBe(name_c1C);
});

test('persists and refreshes unit role values', async () => {
  const { m } = fixture;
  const writer = (await fixture.createWorkspace()).createSession();
  const reader = (await fixture.createWorkspace()).createSession();
  const unitSample = writer.create<C1>(m.C1);

  const initialPush = await writer.push();
  expect(initialPush.hasErrors).toBe(false);
  expect(unitSample.id).toBeGreaterThan(0);
  const writerPull = await writer.pull({ objectId: unitSample.id });
  expect(writerPull.hasErrors).toBe(false);

  const initialRead = await reader.pull({ objectId: unitSample.id });
  expect(initialRead.hasErrors).toBe(false);
  const readerObject = initialRead.object<C1>(m.C1);
  expect(readerObject).not.toBe(unitSample);
  expect(readerObject.C1AllorsBinary).toBeNull();
  expect(readerObject.C1AllorsBoolean).toBeNull();
  expect(readerObject.C1AllorsDateTime).toBeNull();
  expect(readerObject.C1AllorsDecimal).toBeNull();
  expect(readerObject.C1AllorsDouble).toBeNull();
  expect(readerObject.C1AllorsInteger).toBeNull();
  expect(readerObject.C1AllorsString).toBeNull();
  expect(readerObject.C1AllorsUnique).toBeNull();

  unitSample.C1AllorsBinary = 'AQID';
  unitSample.C1AllorsBoolean = true;
  unitSample.C1AllorsDateTime = new Date('1973-03-27T00:00:00Z');
  unitSample.C1AllorsDecimal = '12.34';
  unitSample.C1AllorsDouble = 123;
  unitSample.C1AllorsInteger = 1000;
  unitSample.C1AllorsString = 'a string';
  unitSample.C1AllorsUnique = '2946cf37-71be-4681-8fe6-d0024d59beff';

  const changedPush = await writer.push();
  expect(changedPush.hasErrors).toBe(false);
  const refreshed = await reader.pull({ objectId: unitSample.id });
  expect(refreshed.hasErrors).toBe(false);
  expect(refreshed.object<C1>(m.C1)).toBe(readerObject);
  expect(readerObject.C1AllorsBinary).toEqual('AQID');
  expect(readerObject.C1AllorsBoolean).toBeTruthy();
  expect(new Date(readerObject.C1AllorsDateTime)).toEqual(
    new Date('Tue Mar 27 1973 00:00:00 GMT+0000')
  );
  expect(readerObject.C1AllorsDecimal).toEqual('12.34');
  expect(readerObject.C1AllorsDouble).toEqual(123);
  expect(readerObject.C1AllorsInteger).toEqual(1000);
  expect(readerObject.C1AllorsString).toEqual('a string');
  expect(readerObject.C1AllorsUnique).toEqual(
    '2946cf37-71be-4681-8fe6-d0024d59beff'
  );
});

test('persists and refreshes removed unit roles', async () => {
  const { m } = fixture;
  const writer = (await fixture.createWorkspace()).createSession();
  const reader = (await fixture.createWorkspace()).createSession();
  const unitSample = writer.create<C1>(m.C1);
  unitSample.C1AllorsBinary = 'AQID';
  unitSample.C1AllorsBoolean = true;
  unitSample.C1AllorsDateTime = new Date('1973-03-27T00:00:00Z');
  unitSample.C1AllorsDecimal = '12.34';
  unitSample.C1AllorsDouble = 123;
  unitSample.C1AllorsInteger = 1000;
  unitSample.C1AllorsString = 'a string';
  unitSample.C1AllorsUnique = '2946cf37-71be-4681-8fe6-d0024d59beff';

  const initialPush = await writer.push();
  expect(initialPush.hasErrors).toBe(false);
  expect(unitSample.id).toBeGreaterThan(0);
  const writerPull = await writer.pull({ objectId: unitSample.id });
  expect(writerPull.hasErrors).toBe(false);

  const initialRead = await reader.pull({ objectId: unitSample.id });
  expect(initialRead.hasErrors).toBe(false);
  const readerObject = initialRead.object<C1>(m.C1);
  expect(readerObject).not.toBe(unitSample);
  expect(readerObject.C1AllorsBinary).toEqual('AQID');
  expect(readerObject.C1AllorsBoolean).toBeTruthy();
  expect(new Date(readerObject.C1AllorsDateTime)).toEqual(
    new Date('Tue Mar 27 1973 00:00:00 GMT+0000')
  );
  expect(readerObject.C1AllorsDecimal).toEqual('12.34');
  expect(readerObject.C1AllorsDouble).toEqual(123);
  expect(readerObject.C1AllorsInteger).toEqual(1000);
  expect(readerObject.C1AllorsString).toEqual('a string');
  expect(readerObject.C1AllorsUnique).toEqual(
    '2946cf37-71be-4681-8fe6-d0024d59beff'
  );

  unitSample.C1AllorsBinary = null;
  unitSample.C1AllorsBoolean = null;
  unitSample.C1AllorsDateTime = null;
  unitSample.C1AllorsDecimal = null;
  unitSample.C1AllorsDouble = null;
  unitSample.C1AllorsInteger = null;
  unitSample.C1AllorsString = null;
  unitSample.C1AllorsUnique = null;

  const changedPush = await writer.push();
  expect(changedPush.hasErrors).toBe(false);
  const refreshed = await reader.pull({ objectId: unitSample.id });
  expect(refreshed.hasErrors).toBe(false);
  expect(refreshed.object<C1>(m.C1)).toBe(readerObject);
  expect(readerObject.C1AllorsBinary).toBeNull();
  expect(readerObject.C1AllorsBoolean).toBeNull();
  expect(readerObject.C1AllorsDateTime).toBeNull();
  expect(readerObject.C1AllorsDecimal).toBeNull();
  expect(readerObject.C1AllorsDouble).toBeNull();
  expect(readerObject.C1AllorsInteger).toBeNull();
  expect(readerObject.C1AllorsString).toBeNull();
  expect(readerObject.C1AllorsUnique).toBeNull();
});

afterEach(async () => {
  await fixture?.close();
});
