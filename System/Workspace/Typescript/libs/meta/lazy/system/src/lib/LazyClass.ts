import { ObjectTypeData } from '@allors/workspace/system';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { LazyComposite } from './LazyComposite';

export class LazyClass extends LazyComposite implements InternalClass {
  readonly isClass = true;
  readonly classes: Set<InternalClass>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
    this.classes = new Set([this]);
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this === objectType;
  }
}
