import { Multiplicity, Origin, RelationTypeData } from '@allors/workspace/system';
import { IAssociationTypeInternals } from './Internals/IAssociationTypeInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { IObjectTypeInternals } from './Internals/IObjectTypeInternals';
import { IRelationTypeInternals } from './Internals/IRelationTypeInternals';
import { IRoleTypeInternals } from './Internals/IRoleTypeInternals';
import { AssociationType } from './AssociationType';
import { RoleType } from './RoleType';

export class RelationType implements IRelationTypeInternals {
  readonly metaPopulation: IMetaPopulationInternals;
  readonly tag: number;
  readonly origin: Origin;
  readonly associationType: IAssociationTypeInternals;
  readonly roleType: IRoleTypeInternals;
  readonly isDerived: boolean;
  readonly multiplicity: Multiplicity;

  constructor(associationObjectType: ICompositeInternals, [t, r, s, x, o, p, d, q, u, m]: RelationTypeData) {
    this.tag = t;
    this.metaPopulation = associationObjectType.metaPopulation as IMetaPopulationInternals;
    this.metaPopulation.onNew(this);

    this.isDerived = d ?? false;
    this.origin = o ?? Origin.Database;
    const roleObjectType = this.metaPopulation.metaObjectByTag[r] as IObjectTypeInternals;

    this.multiplicity = roleObjectType.isUnit ? Multiplicity.OneToOne : (x as number) ?? Multiplicity.ManyToOne;
    const oneTo = (this.multiplicity & 2) == 0;
    const toOne = (this.multiplicity & 1) == 0;

    this.roleType = new RoleType(this, roleObjectType, toOne, s, q, u, m, p, x);
    this.associationType = new AssociationType(this.roleType, associationObjectType, oneTo);
    this.roleType.associationType = this.associationType;

    if (this.roleType.objectType.isComposite) {
      (this.roleType.objectType as ICompositeInternals).onNewAssociationType(this.associationType);
    }

    (this.associationType.objectType as ICompositeInternals).onNewRoleType(this.roleType);
  }
}
