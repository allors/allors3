import { ObjectTypeData } from '@allors/workspace/system';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { InternalInterface } from './internal/InternalInterface';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { LazyComposite } from './LazyComposite';

export class LazyInterface extends LazyComposite implements InternalInterface {
  readonly isClass = false;

  subtypes: Readonly<Set<InternalComposite>>;
  classes: Readonly<Set<InternalClass>>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
  }

  deriveSub(): void {
    this.subtypes = new Set();
    this.classes = new Set();
    this.metaPopulation.composites.forEach((v) => {
      if (v.supertypes.has(this)) {
        this.subtypes.add(v as InternalComposite);
        if(v.isClass){
          this.classes.add(v as InternalClass);
        }
      }
    });
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this === objectType || this.subtypes.has(objectType);
  }
}
