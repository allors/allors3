import { IClass, IComposite, ObjectTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Class extends Composite implements IClass {
  readonly isInterface = false;
  readonly isClass = true;
  readonly classes = [this];

  constructor(metaPopulation: IMetaPopulationInternals, [tag, singularName, r, m, pluralName]: ObjectTypeData) {
    super(metaPopulation, tag, singularName, r, m, pluralName);
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
