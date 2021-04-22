import { IClass, IComposite, ClassData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Class extends Composite implements IClass {
  readonly isInterface = false;
  readonly isClass = true;
  readonly classes = [this];

  constructor(metaPopulation: IMetaPopulationInternals, [tag, singularName]) {
    super(metaPopulation, tag, singularName);
  }

  init([, , relationTypes, methodTypes, pluralName]: ClassData): void {
    super.init(relationTypes, methodTypes, pluralName);
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
