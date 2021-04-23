import { IAssociationType, IClass, IComposite, IInterface, IMethodType, IRoleType, Origin, pluralize, ObjectTypeData } from '@allors/workspace/system';
import { MetaPopulation } from './MetaPopulation';
import { MethodType } from './MethodType';
import { RelationType } from './RelationType';

export abstract class Composite implements IComposite {
  isUnit = false;
  readonly tag: number;
  singularName: string;

  directSupertypes: IInterface[];
  directAssociationTypes: IAssociationType[];
  directRoleTypes: IRoleType[];
  directMethodTypes: IMethodType[];

  abstract classes: IClass[];

  private _supertypes: IInterface[];
  private _associationTypes: IAssociationType[];
  private _roleTypes: IRoleType[];
  private _methodTypes: IMethodType[];
  private _pluralName: string;
  private _origin: Origin;

  constructor(public metaPopulation: MetaPopulation, public d: ObjectTypeData) {
    this.tag = d[0];
    this.singularName = d[1];
    metaPopulation.onNewObjectType(this);
  }

  get origin() {
    // return (this._origin ??= );
    return this._origin;
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get supertypes(): IInterface[] {
    return this._supertypes ?? this.directSupertypes.concat(...this.directSupertypes.map((v) => v.supertypes));
  }

  get associationTypes(): IAssociationType[] {
    return this._associationTypes ?? this.directAssociationTypes.concat(...this.directSupertypes.map((v) => v.associationTypes));
  }

  get roleTypes(): IRoleType[] {
    return this._roleTypes ?? this.directRoleTypes.concat(...this.directSupertypes.map((v) => v.roleTypes));
  }

  get methodTypes(): IMethodType[] {
    return this._methodTypes ?? this.directMethodTypes.concat(...this.directSupertypes.map((v) => v.methodTypes));
  }

  abstract isAssignableFrom(objectType: IComposite): boolean;

  init(): void {
    const [,s,d,r,m,p] = this.d;

    this.singularName = s;
    this._pluralName = p;
    this.directSupertypes = d?.map((v) => this.metaPopulation.metaObjectByTag[v] as IInterface) ?? [];
    const relationTypes = r?.map((v)=> new RelationType(this, v)) ?? [];
    this.directMethodTypes = m?.map((v)=> new MethodType(this, v)) ?? [];
  }
}
