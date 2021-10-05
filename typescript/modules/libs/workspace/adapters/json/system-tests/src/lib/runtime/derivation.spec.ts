import { Pull } from '@allors/workspace/domain/system';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('sessionFullName', async () => {
  const { client, workspace, m } = fixture;
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

  const result = await client.pull(session, [pull]);
  const jane = result.collection<Person>('People')[0];

  expect(jane.SessionFullName).toBeNull();

  session.derive();

  expect(jane.SessionFullName).toBe('Jane Doe');
});
