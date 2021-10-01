import { PropertyType, PropertyTypeExtension } from './property-type';
import { RelationType } from './relation-type';
import { RoleType } from './role-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface AssociationTypeExtension extends PropertyTypeExtension {}

export interface AssociationType extends PropertyType {
  readonly kind: 'AssociationType';
  _: AssociationTypeExtension;
  relationType: RelationType;
  roleType: RoleType;
}
