import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull, Procedure } from '@allors/workspace/domain/system';
import { Fixture } from '../Fixture';
import { UnitSample } from '@allors/workspace/domain/core';

import '../Matchers';
import '@allors/workspace/domain/core';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initProcedure(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function testUnitSamplesWithNulls() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'TestUnitSamples',
    values: { step: '0' },
  };

  const result = await client.callAsync(session, procedure);

  expect(result.hasErrors).toBeFalsy();

  const unitSample = result.object<UnitSample>(m.UnitSample);

  expect(unitSample.AllorsBinary).toBeUndefined();
  expect(unitSample.AllorsBoolean).toBeUndefined();
  expect(unitSample.AllorsDateTime).toBeUndefined();
  expect(unitSample.AllorsDecimal).toBeUndefined();
  expect(unitSample.AllorsDouble).toBeUndefined();
  expect(unitSample.AllorsInteger).toBeUndefined();
  expect(unitSample.AllorsString).toBeUndefined();
  expect(unitSample.AllorsUnique).toBeUndefined();
}

export async function testUnitSamplesWithValues() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'TestUnitSamples',
    values: { step: '1' },
  };

  const result = await client.callAsync(session, procedure);

  expect(result.hasErrors).toBeFalsy();

  const unitSample = result.object<UnitSample>(m.UnitSample);

  expect(unitSample.AllorsBinary).toEqual('QVFJRA==');
  expect(unitSample.AllorsBoolean).toBeTruthy();
  expect(new Date(unitSample.AllorsDateTime)).toEqual(new Date('Tue Mar 27 1973 00:00:00 GMT+0000'));
  expect(unitSample.AllorsDecimal).toEqual('12.34');
  expect(unitSample.AllorsDouble).toEqual(123);
  expect(unitSample.AllorsInteger).toEqual(1000);
  expect(unitSample.AllorsString).toEqual('a string');
  expect(unitSample.AllorsUnique).toEqual('2946cf37-71be-4681-8fe6-d0024d59beff');
}

export async function nonExistingProcedure() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const procedure: Procedure = {
    name: 'ThisIsWrong',
    values: { step: '2' },
  };

  const result = await client.callAsync(session, procedure);
  expect(result.hasErrors).toBeTruthy();
}
