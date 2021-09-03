import { PropertyType } from './PropertyType';
import { RelationType } from './RelationType';
import { RoleType } from './RoleType';

export interface AssociationType extends PropertyType {
  readonly kind: 'AssociationType';
  
  relationType: RelationType;

  roleType: RoleType;
}
