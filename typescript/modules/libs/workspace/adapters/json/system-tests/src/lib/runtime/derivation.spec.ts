import { Organisation, Person } from '@allors/workspace/domain/default';
import { Pull } from '@allors/workspace/domain/system';
import { OrganisationDisplayNameRule, PersonDisplayNameRule } from '@allors/workspace/derivations/core-custom';
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

  const rules = workspace.configuration.rules.filter((v) => v instanceof PersonDisplayNameRule);
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
});

test('organisationDisplayName', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();
  const rules = workspace.configuration.rules.filter((v) => v instanceof OrganisationDisplayNameRule);
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

  expect(acme.DisplayName).toBe('Acme owned by Jane');
});
