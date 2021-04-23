import { Origin, pluralize, ObjectTypeData } from '@allors/workspace/system';
import { frozenEmptySet } from './Internals/frozenEmptySet';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IInterfaceInternals } from './Internals/IInterfaceInternals';
import { IAssociationTypeInternals } from './Internals/IAssociationTypeInternals';
import { IRoleTypeInternals } from './Internals/IRoleTypeInternals';
import { IMethodTypeInternals } from './Internals/IMethodTypeInternals';
import { IClassInternals } from './Internals/IClassInternals';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { RelationType } from './RelationType';
import { MethodType } from './MethodType';

export abstract class Composite implements ICompositeInternals {
  isUnit = false;
  isComposite = true;
  readonly tag: number;
  singularName: string;

  directSupertypes: Readonly<Set<IInterfaceInternals>>;
  supertypes: Readonly<Set<IInterfaceInternals>>;

  directAssociationTypes: IAssociationTypeInternals[] = [];
  directRoleTypes: IRoleTypeInternals[] = [];
  directMethodTypes: IMethodTypeInternals[];

  abstract isClass: boolean;
  abstract classes: Readonly<Set<IClassInternals>>;

  private _associationTypes: IAssociationTypeInternals[];
  private _roleTypes: IRoleTypeInternals[];
  private _methodTypes: IMethodTypeInternals[];
  private _pluralName: string;
  private _origin: Origin;

  get origin() {
    // return (this._origin ??= );
    return this._origin;
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get associationTypes(): IAssociationTypeInternals[] {
    return (this._associationTypes ??= this.directAssociationTypes.concat(...this.directSupertypes.map((v) => v.associationTypes)));
  }

  get roleTypes(): IRoleTypeInternals[] {
    return (this._roleTypes ??= this.directRoleTypes.concat(...this.directSupertypes.map((v) => v.roleTypes)));
  }

  get methodTypes(): IMethodTypeInternals[] {
    return (this._methodTypes ??= this.directMethodTypes.concat(...this.directSupertypes.map((v) => v.methodTypes)));
  }

  abstract isAssignableFrom(objectType: ICompositeInternals): boolean;

  constructor(public metaPopulation: IMetaPopulationInternals, public d: ObjectTypeData) {
    this.tag = d[0];
    this.singularName = d[1];
    metaPopulation.onNewObjectType(this);
  }

  derive(): void {
    const [, s, d, r, m, p] = this.d;

    this.singularName = s;
    this._pluralName = p;
    this.directSupertypes = d?.length > 0 ? new Set(d?.map((v) => this.metaPopulation.metaObjectByTag[v] as IInterfaceInternals)) : (frozenEmptySet as Set<IInterfaceInternals>);
    r?.forEach((v) => new RelationType(this, v));
    this.directMethodTypes = m?.map((v) => new MethodType(this, v)) ?? [];
  }

  deriveSuper(): void {
    this.supertypes = new Set(this.supertypeGenerator());
  }

  onNewAssociationType(associationType: IAssociationTypeInternals) {
    this.directAssociationTypes.push(associationType);
  }

  onNewRoleType(roleType: IRoleTypeInternals) {
    this.directRoleTypes.push(roleType);
  }

  *supertypeGenerator(): IterableIterator<IInterfaceInternals> {
    if (this.supertypes) {
      yield* this.supertypes.values();
    } else {
      for (const supertype of this.directSupertypes.values()) {
        yield supertype;
        yield* supertype.supertypeGenerator();
      }
    }
  }
}
