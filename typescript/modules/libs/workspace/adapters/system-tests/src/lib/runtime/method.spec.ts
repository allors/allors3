import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { Organisation } from '@allors/workspace/domain/default';

import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initMethod(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function callSingle() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pullAsync(session, [pull]);
  const organisation = result.collection<Organisation>(m.Organisation)[0];

  expect(organisation.JustDidIt).toBeFalsy();

  const invokeResult = await client.invokeAsync(session, organisation.JustDoIt);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pullAsync(session, [{ object: organisation }]);

  expect(organisation.JustDidIt).toBeTruthy();
  expect(organisation.JustDidItDerived).toBeTruthy();
}

export async function callMultiple() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pullAsync(session, [pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await client.invokeAsync(session, [organisation1.JustDoIt, organisation2.JustDoIt]);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pullAsync(session, [pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
}

export async function callMultipleIsolated() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pullAsync(session, [pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await client.invokeAsync(session, [organisation1.JustDoIt, organisation2.JustDoIt], { isolated: true });

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pullAsync(session, [pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
}
