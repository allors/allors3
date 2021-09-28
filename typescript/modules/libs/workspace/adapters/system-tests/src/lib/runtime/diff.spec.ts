import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient, Pull, IUnitDiff } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1 } from '@allors/workspace/domain/default';

import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDiff(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}
export async function databaseUnitDiff() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await client.pullAsync(session, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await client.pushAsync(session);

  result = await client.pullAsync(session, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(1);

  const diff = diffs[0] as IUnitDiff;

  expect(diff.originalRole).toBe('X');
  expect(diff.changedRole).toBe('Y');
  expect(diff.relationType.roleType).toBe(m.C1.C1AllorsString);
}

export async function databaseUnitDiffAfterReset() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await client.pullAsync(session, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await client.pushAsync(session);

  result = await client.pullAsync(session, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  c1a_2.strategy.reset();

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(0);
}

export async function databaseUnitDiffAfterDoubleReset() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await client.pullAsync(session, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';

  await client.pushAsync(session);

  result = await client.pullAsync(session, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';

  c1a_2.strategy.reset();
  c1a_2.strategy.reset();

  const diffs = c1a_2.strategy.diff();

  expect(diffs.length).toBe(0);
}

export async function databaseMultipleUnitDiff() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await client.pullAsync(session, [pull]);
  const c1a_1 = result.collection<C1>('C1s')[0];

  c1a_1.C1AllorsString = 'X';
  c1a_1.C1AllorsInteger = 1;

  await client.pushAsync(session);

  result = await client.pullAsync(session, [pull]);
  const c1a_2 = result.collection<C1>('C1s')[0];

  c1a_2.C1AllorsString = 'Y';
  c1a_2.C1AllorsInteger = 2;

  const diffs = c1a_2.strategy.diff() as IUnitDiff[];

  expect(diffs.length).toBe(2);

  const stringDiff = diffs.find((v) => v.relationType.roleType === m.C1.C1AllorsString);

  expect(stringDiff.originalRole).toBe('X');
  expect(stringDiff.changedRole).toBe('Y');

  const intDiff = diffs.find((v) => v.relationType.roleType === m.C1.C1AllorsInteger);

  expect(intDiff.originalRole).toBe(1);
  expect(intDiff.changedRole).toBe(2);
}

export async function workspaceUnitDiff() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  const result = await client.pullAsync(session, [pull]);
  const c1a = result.collection<C1>('C1s')[0];

  c1a.WorkspaceAllorsString = 'X';

  session.pushToWorkspace();
  session.pullFromWorkspace();

  c1a.WorkspaceAllorsString = 'Y';

  const diffs = c1a.strategy.diff();

  expect(diffs.length).toBe(1);

  const diff = diffs[0] as IUnitDiff;

  expect(diff.originalRole).toBe('X');
  expect(diff.changedRole).toBe('Y');
  expect(diff.relationType.roleType).toBe(m.C1.WorkspaceAllorsString);
}

export async function workspaceUnitDiffAfterReset() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  const result = await client.pullAsync(session, [pull]);
  const c1a = result.collection<C1>('C1s')[0];

  c1a.WorkspaceAllorsString = 'X';

  session.pushToWorkspace();
  session.pullFromWorkspace();

  c1a.WorkspaceAllorsString = 'Y';

  c1a.strategy.reset();

  const diffs = c1a.strategy.diff();

  expect(diffs.length).toBe(0);
}

// TODO: More variations
