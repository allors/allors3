import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1 } from '@allors/workspace/domain/default';

import { SingleSessionContext } from '../../../context/SingleSessionContext';
import { MultipleSessionContext } from '../../../context/MultipleSessionContext';
import { databaseModes } from '../../../context/modes/DatabaseMode';
import { Fixture } from '../../../../Fixture';
import '../../../Matchers';

let fixture: Fixture;
let singleSessionContext: SingleSessionContext;
let multipleSessionContext: MultipleSessionContext;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDatabaseOneToOne(
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

const pushes = [
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  async () => {},
  async (session) => await fixture.client.pushAsync(session),
];

function* contextFactories() {
  yield () => singleSessionContext;
  yield () => new SingleSessionContext(fixture, 'Single');
  yield () => multipleSessionContext;
  yield () => new MultipleSessionContext(fixture, 'Multiple');
}

export async function databaseOneToOneSetRole() {
  for (const push of pushes) {
    for (const mode1 of databaseModes) {
      for (const mode2 of databaseModes) {
        for (const contextFactory of contextFactories()) {
          const ctx = contextFactory();
          const { session1, session2 } = ctx;
          const { m, client } = fixture;

          const c1x_1 = await ctx.create<C1>(session1, m.C1, mode1);
          const c1y_2 = await ctx.create<C1>(session2, m.C1, mode2);

          expect(c1x_1).toBeDefined();
          expect(c1y_2).toBeDefined();

          await client.pushAsync(session2);
          const result = await client.pullAsync(session1, { object: c1y_2 });

          const c1y_1 = result.objects.values().next().value as C1;

          expect(c1y_1).toBeDefined();

          if (!c1x_1.canWriteC1C1One2One) {
            await client.pullAsync(session1, { object: c1x_1 });
          }

          c1x_1.C1C1One2One = c1y_1;

          expect(c1x_1.C1C1One2One).toBe(c1y_1);
          expect(c1y_1.C1WhereC1C1One2One).toBe(c1x_1);

          await push(session1);

          expect(c1x_1.C1C1One2One).toBe(c1y_1);
          expect(c1y_1.C1WhereC1C1One2One).toBe(c1x_1);
        }
      }
    }
  }
}

export async function databaseOneToOneRemoveRole() {
  for (const push of pushes) {
    for (const mode1 of databaseModes) {
      for (const mode2 of databaseModes) {
        for (const contextFactory of contextFactories()) {
          const ctx = contextFactory();
          const { session1, session2 } = ctx;
          const { m, client } = fixture;

          const c1x_1 = await ctx.create<C1>(session1, m.C1, mode1);
          const c1y_2 = await ctx.create<C1>(session2, m.C1, mode2);

          expect(c1x_1).toBeDefined();
          expect(c1y_2).toBeDefined();

          await client.pushAsync(session2);
          const result = await client.pullAsync(session1, { object: c1y_2 });

          const c1y_1 = result.objects.values().next().value as C1;

          expect(c1y_1).toBeDefined();

          if (!c1x_1.canWriteC1C1One2Manies) {
            await client.pullAsync(session1, { object: c1x_1 });
          }

          c1x_1.C1C1One2One = c1y_1;

          await push(session1);

          if (!c1x_1.canWriteC1C1One2Manies) {
            await client.pullAsync(session1, { object: c1x_1 });
          }

          c1x_1.C1C1One2One = null;

          expect(c1x_1.C1C1One2One).toBeNull();
          expect(c1y_1.C1WhereC1C1One2One).toBeNull();

          await push(session1);

          expect(c1x_1.C1C1One2One).toBeNull();
          expect(c1y_1.C1WhereC1C1One2One).toBeNull();
        }
      }
    }
  }
}
