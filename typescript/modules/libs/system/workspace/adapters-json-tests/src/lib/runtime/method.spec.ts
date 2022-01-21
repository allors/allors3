import { Organisation } from '@allors/default/workspace/domain';
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

test('callMultipleIsolated', async () => {
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

  const invokeResult = await session.invoke(
    [organisation1.JustDoIt, organisation2.JustDoIt],
    { isolated: true }
  );

  expect(invokeResult.hasErrors).toBeFalsy();

  result = await session.pull([pull]);

  expect(organisation1.JustDidIt).toBeTruthy();
  expect(organisation1.JustDidItDerived).toBeTruthy();

  expect(organisation2.JustDidIt).toBeTruthy();
  expect(organisation2.JustDidItDerived).toBeTruthy();
});
