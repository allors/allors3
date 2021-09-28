import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1 } from '@allors/workspace/domain/default';

import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initMerge(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function databaseMergeError() {
  const { client, workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.Name,
        value: 'C1A',
      },
    },
  };

  let result = await client.pullAsync(session1, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  result = await client.pullAsync(session2, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';
  c1a_2.C1AllorsString = 'Y';

  await client.pushAsync(session2);

  result = await client.pullAsync(session1, [pull]);

  expect(result.hasErrors).toBeTruthy();
  expect(result.mergeErrors.length).toBe(1);

  const mergeError = result.mergeErrors[0];

  expect(mergeError.strategy).toBe(c1a_1.strategy);
}
