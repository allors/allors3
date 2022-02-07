import { PropertyType } from './property-type';
import { RelationType } from './relation-type';
import { RoleType } from './role-type';

export interface AssociationType extends PropertyType {
  readonly kind: 'AssociationType';
  relationType: RelationType;
  roleType: RoleType;
}
