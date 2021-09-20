import { ObjectTypeData } from '@allors/protocol/json/system';
import { Lookup } from './utils/Lookup';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { LazyComposite } from './LazyComposite';
import { RelationType, RoleType } from '@allors/workspace/meta/system';

export class LazyClass extends LazyComposite implements InternalClass {
  readonly kind = 'Class';
  readonly isInterface = false;
  readonly isClass = true;
  readonly classes: Set<InternalClass>;

  overriddenRequiredRoleTypes: RoleType[];

  overriddenUniqueRoleTypes: RoleType[];

  requiredRoleTypes: Set<RoleType>;

  uniqueRoleTypes: Set<RoleType>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData, lookup: Lookup) {
    super(metaPopulation, data, lookup);
    this.classes = new Set([this]);
  }

  deriveOverridden(lookup: Lookup): void {
    this.overriddenRequiredRoleTypes = lookup.or.has(this.tag) ? [...lookup.or.get(this.tag)].map((v) => (this.metaPopulation.metaObjectByTag.get(v) as RelationType).roleType) : [];
    this.overriddenUniqueRoleTypes = lookup.ou.has(this.tag) ? [...lookup.or.get(this.tag)].map((v) => (this.metaPopulation.metaObjectByTag.get(v) as RelationType).roleType) : [];

    this.requiredRoleTypes = new Set([...this.roleTypes].filter((v) => v.isRequired).concat(this.overriddenRequiredRoleTypes));
    this.uniqueRoleTypes = new Set([...this.roleTypes].filter((v) => v.isRequired).concat(this.overriddenUniqueRoleTypes));
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this.tag === objectType?.tag;
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
