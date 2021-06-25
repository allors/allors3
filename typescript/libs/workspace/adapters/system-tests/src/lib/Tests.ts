import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';
import { Database } from '@allors/workspace/adapters/system';
import { IWorkspace } from '@allors/workspace/domain/system';

export abstract class Tests {
  metaPopulation: MetaPopulation;
  m: M;
  workspace: IWorkspace;

  constructor(public database: Database, public login: (login: string) => Promise<boolean>) {
    this.metaPopulation = new LazyMetaPopulation(data);
    this.m = (this.metaPopulation as MetaPopulation) as M;
    this.workspace = database.createWorkspace();
  }
}
