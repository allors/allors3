import { Multiplicity } from './multiplicity';
import { AssociationType } from './association-type';
import { MetaObject, MetaObjectExtension } from './meta-object';
import { RoleType } from './role-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface RelationTypeExtension extends MetaObjectExtension {}

export interface RelationType extends MetaObject {
  readonly kind: 'RelationType';
  _: RelationTypeExtension;
  associationType: AssociationType;
  roleType: RoleType;
  multiplicity: Multiplicity;
  isDerived: boolean;
}
