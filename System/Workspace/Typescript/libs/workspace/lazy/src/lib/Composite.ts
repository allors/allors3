import { IAssociationType, IClass, IComposite, IInterface, IMethodType, IRoleType, Origin } from '@allors/workspace/system';
import { ObjectTypeData } from './MetaData';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export abstract class Composite implements IComposite {
  private _p: string;

  constructor(public metaPopulation: IMetaPopulationInternals, private data: ObjectTypeData) {
    this.tag = data.t;
    this.singularName = data.s;
    metaPopulation.onObjectType(this);
  }

  readonly tag: number;
  readonly singularName: string;

  isUnit = false;
  isComposite = true;
  abstract isInterface: boolean;
  abstract isClass: boolean;

  get pluralName() {
    return (this._p ??= this.data.p ?? pluralize(this.singularName));
  }
  origin: Origin;
  isSynced: boolean;

  directSupertypes: IInterface[];
  supertypes: IInterface[];
  classes: IClass[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  workspaceRoleTypes: IRoleType[];
  databaseRoleTypes: IRoleType[];
  methodTypes: IMethodType[];

  abstract isAssignableFrom(objectType: IComposite): boolean;
}

function pluralize(singularName: string): string {
  throw new Error('Function not implemented.');
}
