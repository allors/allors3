import { IMetaPopulation } from './IMetaPopulation';

export interface IMetaObject {
  MetaPopulation: IMetaPopulation;

  Id: string;

  Origin: Origin;

  HasDatabaseOrigin: boolean;

  HasWorkspaceOrigin: boolean;

  HasSessionOrigin: boolean;
}
