import { AssociationType } from './association-type';
import { Class } from './class';
import { Dependency } from './dependency';
import { Interface } from './interface';
import { MethodType } from './method-type';
import { ObjectType } from './object-type';
import { PropertyType } from './property-type';
import { RoleType } from './role-type';

export interface Composite extends ObjectType {
  directSupertypes: Set<Interface>;
  directAssociationTypes: Set<AssociationType>;
  directRoleTypes: Set<RoleType>;
  directMethodTypes: Set<MethodType>;
  propertyTypeByPropertyName: Map<string, PropertyType>;

  supertypes: Set<Interface>;
  classes: Set<Class>;
  associationTypes: Set<AssociationType>;
  roleTypes: Set<RoleType>;
  methodTypes: Set<MethodType>;

  databaseOriginRoleTypes: Set<RoleType>;

  dependencyByPropertyType: Map<PropertyType, Dependency>;

  isRelationship: boolean;

  isAssignableFrom(objectType: Composite): boolean;
}
