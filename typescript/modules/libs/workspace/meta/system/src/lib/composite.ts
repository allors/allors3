import { AssociationType } from './association-type';
import { Class } from './class';
import { Dependency } from './dependency';
import { Interface } from './interface';
import { MethodType } from './method-type';
import { ObjectType, ObjectTypeExtension } from './object-type';
import { PropertyType } from './property-type';
import { RoleType } from './role-type';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface CompositeExtension extends ObjectTypeExtension {}

export interface Composite extends ObjectType {
  _: CompositeExtension;
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
  workspaceOriginRoleTypes: Set<RoleType>;

  dependencyByPropertyType: Map<PropertyType, Dependency>;

  isAssignableFrom(objectType: Composite): boolean;
}
