import { AssociationType } from './association-type';
import { PropertyType, PropertyTypeExtension } from './property-type';
import { RelationType } from './relation-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface RoleTypeExtension extends PropertyTypeExtension {}

export interface RoleType extends PropertyType {
  readonly kind: 'RoleType';
  _: RoleTypeExtension;
  singularName: string;
  associationType: AssociationType;
  relationType: RelationType;
  size?: number;
  precision?: number;
  scale?: number;
  isRequired: boolean;
  isUnique: boolean;
  mediaType?: string;
}
