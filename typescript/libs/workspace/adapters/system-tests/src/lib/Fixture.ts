import { MetaPopulation } from '@allors/workspace/meta/system';
import { M } from '@allors/workspace/meta/core';
import { IAsyncDatabaseClient, IReactiveDatabaseClient, ISession, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { ClientAdapter } from './ClientAdapter';
import { C1, C2 } from '@allors/workspace/domain/core';

export const name_c1A = 'c1A';
export const name_c1B = 'c1B';
export const name_c1C = 'c1C';
export const name_c1D = 'c1D';
export const name_c2A = 'c2A';
export const name_c2B = 'c2B';
export const name_c2C = 'c2C';
export const name_c2D = 'c2D';

export class Fixture {
  metaPopulation: MetaPopulation;
  m: M;

  client: IAsyncDatabaseClient;
  constructor(
    public asyncClient: IAsyncDatabaseClient,
    reactiveClient: IReactiveDatabaseClient,
    public workspace: IWorkspace,
    public login: (login: string) => Promise<boolean>,
    public createWorkspace: () => IWorkspace = null,
    public createExclusiveWorkspace: () => IWorkspace = null
  ) {
    this.metaPopulation = workspace.configuration.metaPopulation;
    this.m = this.metaPopulation as MetaPopulation as M;

    this.client = asyncClient ?? new ClientAdapter(reactiveClient);
  }

  async pullC1(session: ISession, name: string): Promise<C1> {
    const { client, m } = this;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: m.C1.Name,
          value: name,
        },
      },
    };

    const result = await client.pullAsync(session, [pull]);
    return result.collection<C1>(m.C1)[0];
  }

  async pullC2(session: ISession, name: string): Promise<C2> {
    const { client, m } = this;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C2,
        predicate: {
          kind: 'Equals',
          propertyType: m.C2.Name,
          value: name,
        },
      },
    };

    const result = await client.pullAsync(session, [pull]);
    return result.collection<C2>(m.C2)[0];
  }
}
