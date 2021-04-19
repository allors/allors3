import { Origin } from '../Origin';
import { IMetaPopulation } from './IMetaPopulation';

export interface IMetaObject {
  metaPopulation: IMetaPopulation;

  id: string;

  origin: Origin;

  hasDatabaseOrigin: boolean;

  hasWorkspaceOrigin: boolean;

  hasSessionOrigin: boolean;
}
