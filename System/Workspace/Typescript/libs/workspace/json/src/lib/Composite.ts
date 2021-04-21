import { IAssociationType, IClass, IComposite, IInterface, IMetaPopulation, IMethodType, IRoleType, Origin } from '@allors/workspace/system';
import { ObjectTypeData } from './MetaData';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export abstract class Composite implements IComposite {
  tag: number;
  singularName: string;
  origin: Origin;
  pluralName: string;
  isUnit: boolean;
  isComposite: boolean;
  isInterface: boolean;
  isClass: boolean;
  directSupertypes: IInterface[];
  supertypes: IInterface[];
  classes: IClass[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  workspaceRoleTypes: IRoleType[];
  databaseRoleTypes: IRoleType[];
  methodTypes: IMethodType[];
  isSynced: boolean;

  constructor(public metaPopulation: IMetaPopulationInternals, data: ObjectTypeData) {
    this.tag = data.t;
    this.singularName = data.s;
    metaPopulation.onObjectType(this);
  }

  abstract isAssignableFrom(objectType: IComposite): boolean;
}
