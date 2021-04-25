import { Multiplicity, Origin, RelationTypeData } from '@allors/workspace/system';
import { InternalAssociationType } from './internal/InternalAssociationType';
import { InternalComposite } from './internal/InternalComposite';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalObjectType } from './internal/InternalObjectType';
import { InternalRelationType } from './internal/InternalRelationType';
import { InternalRoleType } from './internal/InternalRoleType';
import { LazyAssociationType } from './LazyAssociationType';
import { LazyRoleType } from './LazyRoleType';

export class LazyRelationType implements InternalRelationType {
  readonly metaPopulation: InternalMetaPopulation;
  readonly tag: number;
  readonly origin: Origin;
  readonly associationType: InternalAssociationType;
  readonly roleType: InternalRoleType;
  readonly isDerived: boolean;
  readonly multiplicity: Multiplicity;

  constructor(associationObjectType: InternalComposite, [t, r, s, x, o, p, d, q, u, m]: RelationTypeData) {
    this.tag = t;
    this.metaPopulation = associationObjectType.metaPopulation as InternalMetaPopulation;
    this.metaPopulation.onNew(this);

    this.isDerived = d ?? false;
    this.origin = o ?? Origin.Database;
    const roleObjectType = this.metaPopulation.metaObjectByTag[r] as InternalObjectType;

    this.multiplicity = roleObjectType.isUnit ? Multiplicity.OneToOne : (x as number) ?? Multiplicity.ManyToOne;
    const oneTo = (this.multiplicity & 2) == 0;
    const toOne = (this.multiplicity & 1) == 0;

    this.roleType = new LazyRoleType(this, roleObjectType, toOne, s, q, u, m, p, x);
    this.associationType = new LazyAssociationType(this.roleType, associationObjectType, oneTo);
    this.roleType.associationType = this.associationType;

    if (this.roleType.objectType.isComposite) {
      (this.roleType.objectType as InternalComposite).onNewAssociationType(this.associationType);
    }

    (this.associationType.objectType as InternalComposite).onNewRoleType(this.roleType);
  }
}
