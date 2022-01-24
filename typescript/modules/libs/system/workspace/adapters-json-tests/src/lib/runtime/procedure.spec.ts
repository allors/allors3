import { UnitSample } from '@allors/default/workspace/domain';
import { Procedure } from '@allors/system/workspace/domain';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('testUnitSamplesWithNulls', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'TestUnitSamples',
    values: { step: '0' },
  };

  const result = await session.call(procedure);

  expect(result.hasErrors).toBeFalsy();

  const unitSample = result.object<UnitSample>(m.UnitSample);

  expect(unitSample.AllorsBinary).toBeNull();
  expect(unitSample.AllorsBoolean).toBeNull();
  expect(unitSample.AllorsDateTime).toBeNull();
  expect(unitSample.AllorsDecimal).toBeNull();
  expect(unitSample.AllorsDouble).toBeNull();
  expect(unitSample.AllorsInteger).toBeNull();
  expect(unitSample.AllorsString).toBeNull();
  expect(unitSample.AllorsUnique).toBeNull();
});

test('testUnitSamplesWithValues', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'TestUnitSamples',
    values: { step: '1' },
  };

  const result = await session.call(procedure);

  expect(result.hasErrors).toBeFalsy();

  const unitSample = result.object<UnitSample>(m.UnitSample);

  expect(unitSample.AllorsBinary).toEqual('QVFJRA==');
  expect(unitSample.AllorsBoolean).toBeTruthy();
  expect(new Date(unitSample.AllorsDateTime)).toEqual(
    new Date('Tue Mar 27 1973 00:00:00 GMT+0000')
  );
  expect(unitSample.AllorsDecimal).toEqual('12.34');
  expect(unitSample.AllorsDouble).toEqual(123);
  expect(unitSample.AllorsInteger).toEqual(1000);
  expect(unitSample.AllorsString).toEqual('a string');
  expect(unitSample.AllorsUnique).toEqual(
    '2946cf37-71be-4681-8fe6-d0024d59beff'
  );
});

test('nonExistingProcedure', async () => {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'ThisIsWrong',
    values: { step: '2' },
  };

  const result = await session.call(procedure);
  expect(result.hasErrors).toBeTruthy();
});
