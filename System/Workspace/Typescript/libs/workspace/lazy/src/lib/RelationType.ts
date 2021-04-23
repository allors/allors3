import { IObjectType, IRelationType, Multiplicity, Origin, RelationTypeData } from '@allors/workspace/system';
import { AssociationType } from './AssociationType';
import { Composite } from './Composite';
import { MetaPopulation } from './MetaPopulation';
import { RoleType } from './RoleType';

export class RelationType implements IRelationType {
  readonly metaPopulation: MetaPopulation;
  readonly associationType: AssociationType;
  readonly roleType: RoleType;
  readonly isDerived: boolean;

  multiplicity: Multiplicity;
  tag: number;
  origin: Origin;

  constructor(associationObjectType: Composite, [t, r, s, x, o, p, d, q, u, m]: RelationTypeData) {
    this.tag = t;
    this.metaPopulation = associationObjectType.metaPopulation;
    this.metaPopulation.onNew(this);

    this.isDerived = d ?? false;
    this.origin = o ?? Origin.Database;
    const roleObjectType = this.metaPopulation.metaObjectByTag[r] as IObjectType;

    this.multiplicity = roleObjectType.isUnit ? Multiplicity.OneToOne : (x as number) ?? Multiplicity.ManyToOne;
    const oneTo = (this.multiplicity & 2) == 0;
    const toOne = (this.multiplicity & 1) == 0;

    this.roleType = new RoleType(this, roleObjectType, toOne, s, q, u, m, p, x);
    this.associationType = new AssociationType(this.roleType, associationObjectType, oneTo);
  }
}
