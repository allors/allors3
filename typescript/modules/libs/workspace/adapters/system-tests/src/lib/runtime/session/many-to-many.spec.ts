import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1 } from '@allors/workspace/domain/default';

import { Fixture } from '../../fixture'
import '../../matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initSessionManyToMany(
  asyncClient: IAsyncDatabaseClient,
  reactiveClient: IReactiveDatabaseClient,
  workspace: IWorkspace,
  login: (login: string) => Promise<boolean>,
  createWorkspace: () => IWorkspace,
  createExclusiveWorkspace: () => IWorkspace
) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login, createWorkspace, createExclusiveWorkspace);
}

export async function sessionManyToManySetRole() {
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addSessionC1Many2Many(c1b);

    expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);

    await client.pullAsync(session, { object: c1a });

    expect(c1a.SessionC1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereSessionC1Many2Many).toEqual([c1a]);
  }
}
