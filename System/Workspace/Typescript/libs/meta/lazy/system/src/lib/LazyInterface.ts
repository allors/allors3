import { ObjectTypeData } from '@allors/workspace/system';
import { InternalAssociationType } from './internal/InternalAssociationType';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { InternalInterface } from './internal/InternalInterface';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalRoleType } from './internal/InternalRoleType';
import { LazyComposite } from './LazyComposite';

export class LazyInterface extends LazyComposite implements InternalInterface {
  readonly isClass = false;

  subtypes: Set<InternalComposite>;
  classes: Set<InternalClass>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
    this.subtypes = new Set();
    this.classes = new Set();
  }

  deriveSub(): void {
    this.metaPopulation.composites.forEach((v) => {
      if (v.supertypes.has(this)) {
        this.subtypes.add(v as InternalComposite);
        if (v.isClass) {
          this.classes.add(v as InternalClass);
        }
      }
    });
  }

  /**
   * @override
   */
  onNewAssociationType(associationType: InternalAssociationType) {
    super.onNewAssociationType(associationType);
    for (const subtype of this.subtypes) {
      ((subtype as unknown) as Record<string, unknown>)[associationType.singularName] = associationType;
    }
  }

  /**
   * @override
   */
  onNewRoleType(roleType: InternalRoleType) {
    super.onNewRoleType(roleType);
    for (const subtype of this.subtypes) {
      ((subtype as unknown) as Record<string, unknown>)[roleType.singularName] = roleType;
    }
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this === objectType || this.subtypes.has(objectType);
  }
}
