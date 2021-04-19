import { IAssociationType } from './IAssociationType';
import { IClass } from './IClass';
import { IInterface } from './IInterface';
import { IMethodType } from './IMethodType';
import { IObjectType } from './IObjectType';
import { IRoleType } from './IRoleType';

export interface IComposite extends IObjectType {
  IsSynced: boolean;

  DirectSupertypes: IInterface[];

  Supertypes: IInterface[];

  Classes: IClass[];

  AssociationTypes: IAssociationType[];

  RoleTypes: IRoleType[];

  WorkspaceRoleTypes: IRoleType[];

  DatabaseRoleTypes: IRoleType[];

  MethodTypes: IMethodType[];

  IsAssignableFrom(objectType: IComposite): bool;
}
