import { Pull, IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';

import { Fixture, name_c1C } from '../../../Fixture';
import '../../Matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDatabaseUnit(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function databaseUnit() {
  const { client, workspace, m } = fixture;
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
        select: {
          include: [
            {
              propertyType: m.C2.C1WhereC1C2One2Many,
            },
          ],
        },
      },
    ],
  };

  const result = await client.pullAsync(session, [pull]);

  const c2s = result.collection('C2s');

  const c2C = c2s[0];

  const c1WhereC1C2One2Many = c2C.C1WhereC1C2One2Many;

  expect(c1WhereC1C2One2Many).toBeDefined();
  expect(c1WhereC1C2One2Many.Name).toBe(name_c1C);
}
