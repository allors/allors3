import { AssociationType } from './AssociationType';
import { PropertyType } from './PropertyType';
import { RelationType } from './RelationType';

export interface RoleType extends PropertyType {

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
