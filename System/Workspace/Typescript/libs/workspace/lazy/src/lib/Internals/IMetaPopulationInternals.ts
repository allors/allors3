import { IMetaPopulation } from '@allors/workspace/system';
import { IMetaObjectInternals } from './IMetaObjectInternals';
import { IObjectTypeInternals } from './IObjectTypeInternals';

export interface IMetaPopulationInternals extends IMetaPopulation {
  onNew(metaObject: IMetaObjectInternals);
  onNewObjectType(objectType: IObjectTypeInternals);
}
