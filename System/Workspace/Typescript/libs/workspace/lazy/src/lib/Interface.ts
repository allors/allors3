import { IComposite, IInterface } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';
import { ObjectTypeData } from './MetaData';

export class Interface extends Composite implements IInterface {
  constructor(metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  directSubtypes: IComposite;
  subtypes: IComposite;

  isInterface = true;
  isClass = false;

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
