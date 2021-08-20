import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/core';
import { C1 } from '@allors/workspace/domain/core';

import { Fixture } from '../../../Fixture';
import { SingleSessionContext } from '../../context/SingleSessionContext';
import { MultipleSessionContext } from '../../context/MultipleSessionContext';
import '../../Matchers';
import { DatabaseMode } from '../../context/modes/DatabaseMode';

let fixture: Fixture;
let singleSessionContext: SingleSessionContext;
let multipleSessionContext: MultipleSessionContext;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDatabaseManyToMany(
  asyncClient: IAsyncDatabaseClient,
  reactiveClient: IReactiveDatabaseClient,
  workspace: IWorkspace,
  login: (login: string) => Promise<boolean>,
  createWorkspace: () => IWorkspace,
  createExclusiveWorkspace: () => IWorkspace
) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login, createWorkspace, createExclusiveWorkspace);
  singleSessionContext = new SingleSessionContext(fixture, 'Single shared');
  multipleSessionContext = new MultipleSessionContext(fixture, 'Multiple shared');
}

function* contextFactories() {
  yield () => singleSessionContext;
  yield () => new SingleSessionContext(fixture, 'Single');
  yield () => multipleSessionContext;
  yield () => new MultipleSessionContext(fixture, 'Multiple');
}

export async function databaseManyToManySetRole() {
  for (const contextFactory of contextFactories()) {
    const ctx = contextFactory();
    const { session1, session2 } = ctx;
    const { m } = fixture;

    const c1x_1 = await ctx.create<C1>(session1, m.C1, DatabaseMode.NoPush);
    const c1y_2 = await ctx.create<C1>(session2, m.C1, DatabaseMode.NoPush);

    expect(c1x_1).toBeDefined();
    expect(c1y_2).toBeDefined();
  }
}
