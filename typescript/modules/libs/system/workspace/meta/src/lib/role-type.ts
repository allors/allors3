import { AssociationType } from './association-type';
import { PropertyType } from './property-type';
import { RelationType } from './relation-type';

export interface RoleType extends PropertyType {
  readonly kind: 'RoleType';
  _: unknown;
  singularName: string;
  associationType: AssociationType;
  relationType: RelationType;
  size?: number;
  precision?: number;
  scale?: number;
  isRequired: boolean;
  mediaType?: string;
}
