import { ObjectTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { IClassInternals } from './Internals/IClassInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { Composite } from './Composite';

export class Class extends Composite implements IClassInternals {
  readonly isClass = true;
  readonly classes: Readonly<Set<IClassInternals>>;

  constructor(metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    super(metaPopulation, data);
    this.classes = new Set([this]);
  }

  isAssignableFrom(objectType: ICompositeInternals): boolean {
    return this === objectType;
  }
}
