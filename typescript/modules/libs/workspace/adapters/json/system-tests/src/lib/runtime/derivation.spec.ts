import { Organisation, Person } from '@allors/workspace/domain/default';
import { Pull } from '@allors/workspace/domain/system';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('personDisplayName', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const rules = workspace.rules.filter((v) => v.id === '63fd1d4973ee4e7ab170f39a5e71a917');
  session.activate(rules);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Person,
      predicate: {
        kind: 'Equals',
        propertyType: m.Person.FirstName,
        value: 'Jane',
      },
    },
  };

  const result = await session.pull([pull]);
  const jane = result.collection<Person>(m.Person)[0];

  expect(jane.DisplayName).toBeNull();

  const validation = session.derive();
  expect(validation.errors).toHaveLength(0);

  expect(jane.DisplayName).toBe('Jane Doe');
});

test('personDisplayNameNotActivated', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Person,
      predicate: {
        kind: 'Equals',
        propertyType: m.Person.FirstName,
        value: 'Jane',
      },
    },
  };

  const result = await session.pull([pull]);
  const jane = result.collection<Person>(m.Person)[0];

  expect(jane.DisplayName).toBeNull();

  const validation = session.derive();

  expect(validation.errors).toHaveLength(1);

  expect(jane.DisplayName).toBeNull();
});

test('organisationDisplayName', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession('organisationDisplayName');

  const rules = workspace.rules.filter((v) => v.id === '000c900fc851453285519f3601758e6f');
  session.activate(rules);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.Organisation,
      predicate: {
        kind: 'Equals',
        propertyType: m.Organisation.Name,
        value: 'Acme',
      },
    },
  };

  const result = await session.pull([pull]);
  const acme = result.collection<Organisation>(m.Organisation)[0];

  expect(acme.DisplayName).toBeNull();

  session.derive();

  expect(acme.DisplayName).toBe('Acme owned by Jane');
});
