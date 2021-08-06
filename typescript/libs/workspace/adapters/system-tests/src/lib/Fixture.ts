import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';
import { IAsyncDatabaseClient, IPullResult, IReactiveDatabaseClient, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { ClientAdapter } from './ClientAdapter';

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
  constructor(public asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, public workspace: IWorkspace, public login: (login: string) => Promise<boolean>) {
    this.metaPopulation = workspace.configuration.metaPopulation;
    this.m = this.metaPopulation as MetaPopulation as M;

    this.client = asyncClient ?? new ClientAdapter(reactiveClient);
  }
}
