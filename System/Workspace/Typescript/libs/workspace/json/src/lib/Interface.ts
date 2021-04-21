import { IAssociationType, IClass, IComposite, IInterface, IMetaPopulation, IMethodType, IRoleType, Origin } from '@allors/workspace/system';
import { ObjectTypeData } from './MetaData';

export class Interface implements IInterface {
  tag: number;
  origin: Origin;
  singularName: string;
  pluralName: string;
  isUnit: boolean;
  isComposite: boolean;
  isInterface: boolean;
  isClass: boolean;
  directSupertypes: IInterface[];
  supertypes: IInterface[];
  directSubtypes: IComposite;
  subtypes: IComposite;
  classes: IClass[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  workspaceRoleTypes: IRoleType[];
  databaseRoleTypes: IRoleType[];
  methodTypes: IMethodType[];
  isSynced: boolean;

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error("Not Implemented yet");
  }

  constructor(public metaPopulation:IMetaPopulation, data: ObjectTypeData){
    this.tag = data.t;
    this.singularName = data.s;
    metaPopulation[this.singularName] = this;
  }

}
