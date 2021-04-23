import { IClass, IComposite, ObjectTypeData } from '@allors/workspace/system';
import { Composite } from './Composite';
import { MetaPopulation } from './MetaPopulation';

export class Class extends Composite implements IClass {
  readonly classes = [this];

  constructor(metaPopulation: MetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  isAssignableFrom(objectType: IComposite): boolean {
    return this === objectType;
  }
}
