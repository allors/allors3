import { IClass, IComposite } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { IClassInternals } from './Internals/IClassInternals';
import { Composite } from './Composite';
import { ObjectTypeData } from './MetaData';

export class Class extends Composite implements IClassInternals {
  constructor(metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  isInterface = false;
  isClass = true;

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
