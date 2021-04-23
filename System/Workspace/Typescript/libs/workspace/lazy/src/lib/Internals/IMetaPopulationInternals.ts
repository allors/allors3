import { IMetaPopulation } from '@allors/workspace/system';
import { IMetaObjectInternals } from './IMetaObjectInternals';
import { IObjectTypeInternals } from './IObjectTypeInternals';
import { ICompositeInternals } from './ICompositeInternals';

export interface IMetaPopulationInternals extends IMetaPopulation {
  onNew(metaObject: IMetaObjectInternals): void;
  onNewObjectType(objectType: IObjectTypeInternals): void;

  onNewComposite(objectType: ICompositeInternals);
}
