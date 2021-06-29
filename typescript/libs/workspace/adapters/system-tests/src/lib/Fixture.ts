import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';
import { Database } from '@allors/workspace/adapters/system';
import { IWorkspace } from '@allors/workspace/domain/system';

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
  workspace: IWorkspace;

  constructor(public database: Database, public login: (login: string) => Promise<boolean>) {
    this.metaPopulation = new LazyMetaPopulation(data);
    this.m = (this.metaPopulation as MetaPopulation) as M;
    this.workspace = database.createWorkspace();
  }
}
