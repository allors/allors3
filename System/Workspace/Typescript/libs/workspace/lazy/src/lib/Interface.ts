import { ObjectTypeData } from '@allors/workspace/system';
import { frozenEmptySet } from './Internals/frozenEmptySet';
import { IClassInternals } from './Internals/IClassInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IInterfaceInternals } from './Internals/IInterfaceInternals';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Interface extends Composite implements IInterfaceInternals {
  readonly isClass = false;

  subtypes: Readonly<Set<ICompositeInternals>>;
  classes: Readonly<Set<IClassInternals>>;

  constructor(metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  deriveSub(): void {
    this.subtypes = new Set();
    this.classes = new Set();
    this.metaPopulation.composites.forEach((v) => {
      if (v.supertypes.has(this)) {
        this.subtypes.add(v as ICompositeInternals);
        if(v.isClass){
          this.classes.add(v as IClassInternals);
        }
      }
    });
  }

  isAssignableFrom(objectType: ICompositeInternals): boolean {
    return this === objectType || this.subtypes.has(objectType);
  }
}
