import { AssociationType } from './AssociationType';
import { Class } from './Class';
import { Interface } from './Interface';
import { MethodType } from './MethodType';
import { ObjectType } from './ObjectType';
import { RoleType } from './RoleType';

export interface Composite extends ObjectType {
  directSupertypes: Readonly<Set<Interface>>;
  directAssociationTypes: Readonly<Set<AssociationType>>;
  directRoleTypes: Readonly<Set<RoleType>>;
  directMethodTypes: Readonly<Set<MethodType>>;

  supertypes: Readonly<Set<Interface>>;
  classes: Readonly<Set<Class>>;
  associationTypes: Readonly<Set<AssociationType>>;
  roleTypes: Readonly<Set<RoleType>>;
  methodTypes: Readonly<Set<MethodType>>;

  isAssignableFrom(objectType: Composite): boolean;
}
