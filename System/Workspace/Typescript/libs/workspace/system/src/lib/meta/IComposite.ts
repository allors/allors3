import { IAssociationType } from './IAssociationType';
import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMethodType } from './IMethodType';
import { IObjectType } from './IObjectType';
import { IRoleType } from './IRoleType';

export interface IComposite extends IObjectType {
  directSupertypes: IInterface[];
  directAssociationTypes: IAssociationType[];
  directRoleTypes: IRoleType[];
  directMethodTypes: IMethodType[];

  supertypes: IInterface[];
  classes: IClass[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  methodTypes: IMethodType[];

  isAssignableFrom(objectType: IComposite): boolean;
}
