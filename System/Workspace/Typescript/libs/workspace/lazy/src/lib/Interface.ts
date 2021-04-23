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
    const subtypes = (this.metaPopulation.composites as ICompositeInternals[]).filter((v) => v.supertypes.has(this));
    this.subtypes = subtypes.length > 0 ? new Set(subtypes) : (frozenEmptySet as Readonly<Set<ICompositeInternals>>);
    const classes = subtypes.filter((v) => v.isClass);
    this.classes = classes.length > 0 ? new Set(classes) : (frozenEmptySet as Readonly<Set<IClassInternals>>);
  }

  isAssignableFrom(objectType: ICompositeInternals): boolean {
    return this === objectType || this.subtypes.has(objectType);
  }
}
