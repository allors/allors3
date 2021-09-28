import { RelationTypeData } from '@allors/protocol/json/system';
import { Multiplicity, Origin } from '@allors/workspace/meta/system';

import { Lookup } from './utils/lookup';
import { InternalAssociationType } from './internal/internal-association-type';
import { InternalComposite } from './internal/internal-composite';
import { InternalMetaPopulation } from './internal/internal-meta-population';
import { InternalObjectType } from './internal/internal-object-type';
import { InternalRelationType } from './internal/internal-relation-type';
import { InternalRoleType } from './internal/internal-role-type';

import { LazyRoleType } from './lazy-role-type';

export class LazyRelationType implements InternalRelationType {
  readonly kind = 'RelationType';
  readonly metaPopulation: InternalMetaPopulation;

  readonly tag: string;
  readonly multiplicity: Multiplicity;
  readonly origin: Origin;
  readonly isDerived: boolean;

  readonly associationType: InternalAssociationType;
  readonly roleType: InternalRoleType;

  constructor(associationObjectType: InternalComposite, data: RelationTypeData, lookup: Lookup) {
    this.metaPopulation = associationObjectType.metaPopulation as InternalMetaPopulation;

    const [t, r] = data;
    const roleObjectType = this.metaPopulation.metaObjectByTag.get(r) as InternalObjectType;

    this.tag = t;
    this.multiplicity = roleObjectType.isUnit ? Multiplicity.OneToOne : lookup.m.get(t) ?? Multiplicity.ManyToOne;
    this.origin = lookup.o.get(t) ?? Origin.Database;
    this.isDerived = lookup.d.has(t);

    this.metaPopulation.onNew(this);

    this.roleType = new LazyRoleType(this, associationObjectType, roleObjectType, this.multiplicity, data, lookup);
    this.associationType = this.roleType.associationType;

    if (this.roleType.objectType.isComposite) {
      (this.roleType.objectType as InternalComposite).onNewAssociationType(this.associationType);
    }

    (this.associationType.objectType as InternalComposite).onNewRoleType(this.roleType);
  }
}
