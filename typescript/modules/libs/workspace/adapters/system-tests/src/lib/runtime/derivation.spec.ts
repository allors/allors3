import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { Person } from '@allors/workspace/domain/default';

import { Fixture } from '../Fixture';
import '../Matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDerivation(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function sessionFullName() {
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

  const result = await client.pullAsync(session, [pull]);
  const jane = result.collection<Person>('People')[0];

  expect(jane.SessionFullName).toBeNull();

  session.services.derive();

  expect(jane.SessionFullName).toBe('Jane Doe');
}

