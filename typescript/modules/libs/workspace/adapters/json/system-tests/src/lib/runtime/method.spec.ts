import { Organisation } from '@allors/workspace/domain/default';
import { Pull } from '@allors/workspace/domain/system';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('callSingle', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pull(session, [pull]);
  const organisation = result.collection<Organisation>(m.Organisation)[0];

  expect(organisation.JustDidIt).toBeFalsy();

  const invokeResult = await client.invoke(session, organisation.JustDoIt);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pull(session, [{ object: organisation }]);

  expect(organisation.JustDidIt).toBeTruthy();
  expect(organisation.JustDidItDerived).toBeTruthy();
});

test('callMultiple', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pull(session, [pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await client.invoke(session, [organisation1.JustDoIt, organisation2.JustDoIt]);

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pull(session, [pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
});

test('callMultipleIsolated', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
    },
  };

  let result = await client.pull(session, [pull]);
  const organisation1 = result.collection<Organisation>(m.Organisation)[0];
  const organisation2 = result.collection<Organisation>(m.Organisation)[1];

  expect(organisation1.JustDidIt).toBeFalsy();

  const invokeResult = await client.invoke(session, [organisation1.JustDoIt, organisation2.JustDoIt], { isolated: true });

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await client.pull(session, [pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
});
