import { IComposite, IInterface } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';
import { ObjectTypeData } from './MetaData';

export class Interface extends Composite implements IInterface {
  directSubtypes: IComposite;
  subtypes: IComposite;

  constructor(metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
