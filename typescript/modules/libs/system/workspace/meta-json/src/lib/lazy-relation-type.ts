import { RelationTypeData } from '@allors/system/common/protocol-json';
import {
  AssociationType,
  Multiplicity,
  ObjectType,
  Origin,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';

import { Lookup } from './utils/lookup';
import { InternalComposite } from './internal/internal-composite';
import { InternalMetaPopulation } from './internal/internal-meta-population';

import { LazyRoleType } from './lazy-role-type';

export class LazyRelationType implements RelationType {
  readonly kind = 'RelationType';
  readonly _ = {};
  metaPopulation: InternalMetaPopulation;

  tag: string;
  multiplicity: Multiplicity;
  origin: Origin;
  isDerived: boolean;

  associationType: AssociationType;
  roleType: RoleType;

  constructor(
    associationObjectType: InternalComposite,
    data: RelationTypeData,
    lookup: Lookup
  ) {
    this.metaPopulation =
      associationObjectType.metaPopulation as InternalMetaPopulation;

    const [t, r] = data;
    const roleObjectType = this.metaPopulation.metaObjectByTag.get(
      r
    ) as ObjectType;

    this.tag = t;
    this.multiplicity = roleObjectType.isUnit
      ? Multiplicity.OneToOne
      : lookup.m.get(t) ?? Multiplicity.ManyToOne;
    this.origin = lookup.o.get(t) ?? Origin.Database;
    this.isDerived = lookup.d.has(t);

    this.metaPopulation.onNew(this);

    this.roleType = new LazyRoleType(
      this,
      associationObjectType,
      roleObjectType,
      this.multiplicity,
      data,
      lookup
    );
    this.associationType = this.roleType.associationType;

    if (this.roleType.objectType.isComposite) {
      (this.roleType.objectType as InternalComposite).onNewAssociationType(
        this.associationType
      );
    }

    (this.associationType.objectType as InternalComposite).onNewRoleType(
      this.roleType
    );
  }
}
