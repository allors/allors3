import { IMetaPopulation, IObjectType } from '@allors/workspace/system';

export interface IMetaPopulationInternals extends IMetaPopulation {
  onObjectType(objectType: IObjectType): void;
}
