import { ObjectTypeData } from '@allors/protocol/json/system';
import { Lookup } from './utils/Lookup';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { LazyComposite } from './LazyComposite';

export class LazyClass extends LazyComposite implements InternalClass {
  readonly kind = 'Class';
  readonly isInterface = false;
  readonly isClass = true;
  readonly classes: Set<InternalClass>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData, lookup: Lookup) {
    super(metaPopulation, data, lookup);
    this.classes = new Set([this]);
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this === objectType;
  }

  derivePropertyTypeByPropertyName() {
    this.propertyTypeByPropertyName = new Map();

    for (const roleType of this.roleTypes) {
      this.propertyTypeByPropertyName.set(roleType.name, roleType);
    }

    for (const associationType of this.associationTypes) {
      this.propertyTypeByPropertyName.set(associationType.name, associationType);
    }
  }
}
