import { IAssociationType } from './IAssociationType';
import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMethodType } from './IMethodType';
import { IObjectType } from './IObjectType';
import { IRoleType } from './IRoleType';

export interface IComposite extends IObjectType {
  directSupertypes: Readonly<Set<IInterface>>;
  directAssociationTypes: Readonly<Set<IAssociationType>>;
  directRoleTypes: Readonly<Set<IRoleType>>;
  directMethodTypes: Readonly<Set<IMethodType>>;

  supertypes: Readonly<Set<IInterface>>;
  classes: Readonly<Set<IClass>>;
  associationTypes: Readonly<Set<IAssociationType>>;
  roleTypes: Readonly<Set<IRoleType>>;
  methodTypes: Readonly<Set<IMethodType>>;

  isAssignableFrom(objectType: IComposite): boolean;
}
