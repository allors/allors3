import { IMetaObject, IMetaPopulation } from '@allors/workspace/system';

export interface IMetaPopulationInternals extends IMetaPopulation {
  onMetaObject(metaObject: IMetaObject): void;
}
