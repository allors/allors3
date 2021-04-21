import { IAssociationType } from './IAssociationType';
import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMethodType } from './IMethodType';
import { IObjectType } from './IObjectType';
import { IRoleType } from './IRoleType';

export interface IComposite extends IObjectType {
  directSupertypes: IInterface[];

  supertypes: IInterface[];

  classes: IClass[];

  associationTypes: IAssociationType[];

  roleTypes: IRoleType[];

  workspaceRoleTypes: IRoleType[];

  databaseRoleTypes: IRoleType[];

  methodTypes: IMethodType[];

  isSynced: boolean;

  isAssignableFrom(objectType: IComposite): boolean;
}
