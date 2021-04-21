import { IAssociationType, IClass, IComposite, IInterface, IMetaPopulation, IMethodType, IRoleType, Origin } from '@allors/workspace/system';

export class Class implements IClass {
  isSynced: boolean;
  directSupertypes: IInterface[];
  supertypes: IInterface[];
  classes: IClass[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  workspaceRoleTypes: IRoleType[];
  databaseRoleTypes: IRoleType[];
  methodTypes: IMethodType[];
  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
  isUnit: boolean;
  isComposite: boolean;
  isInterface: boolean;
  isClass: boolean;
  singularName: string;
  pluralName: string;
  metaPopulation: IMetaPopulation;
  id: string;
  origin: Origin;
  hasDatabaseOrigin: boolean;
  hasWorkspaceOrigin: boolean;
  hasSessionOrigin: boolean;
}
