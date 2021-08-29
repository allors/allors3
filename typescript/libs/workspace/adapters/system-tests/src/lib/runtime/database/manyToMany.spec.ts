import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/core';
import { C1 } from '@allors/workspace/domain/core';

import { Fixture } from '../../Fixture';
import '../../Matchers';

let fixture: Fixture;

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
}

export async function databaseManyToManySetRole() {
  // Single Session
  //#region No push before add
  {
    const { m, client, workspace } = fixture;
    const session1 = workspace.createSession();

    const c1a = session1.create<C1>(m.C1);
    const c1b = session1.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session1);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pullAsync(session1, [{ object: c1a }, { object: c1b }]);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion
}
