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

  directAssociationTypes: Set<IAssociationTypeInternals> = new Set();
  directRoleTypes: Set<IRoleTypeInternals> = new Set();
  directMethodTypes: Readonly<Set<IMethodTypeInternals>> = new Set();

  abstract isClass: boolean;
  abstract classes: Readonly<Set<IClassInternals>>;

  private _associationTypes: Readonly<Set<IAssociationTypeInternals>>;
  private _roleTypes: Readonly<Set<IRoleTypeInternals>>;
  private _methodTypes: Readonly<Set<IMethodTypeInternals>>;
  private _pluralName: string;
  private _origin: Origin;

  get origin() {
    // return (this._origin ??= );
    return this._origin;
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get associationTypes(): Readonly<Set<IAssociationTypeInternals>> {
    return this._associationTypes ??= new Set(this.associationTypeGenerator());
  }
  get roleTypes(): Readonly<Set<IRoleTypeInternals>> {
    return this._roleTypes ??= new Set(this.roleTypeGenerator());
  }

  get methodTypes(): Readonly<Set<IMethodTypeInternals>> {
    return this._methodTypes ??= new Set(this.methodTypeGenerator());
  }

  abstract isAssignableFrom(objectType: ICompositeInternals): boolean;

  constructor(public metaPopulation: IMetaPopulationInternals, public d: ObjectTypeData) {
    this.tag = d[0];
    this.singularName = d[1];
    metaPopulation.onNewComposite(this);
  }

  derive(): void {
    const [, s, d, r, m, p] = this.d;

    this.singularName = s;
    this._pluralName = p;
    this.directSupertypes = d?.length > 0 ? new Set(d?.map((v) => this.metaPopulation.metaObjectByTag[v] as IInterfaceInternals)) : (frozenEmptySet as Set<IInterfaceInternals>);
    r?.forEach((v) => new RelationType(this, v));
    this.directMethodTypes = m?.length > 1 ? new Set(m?.map((v) => new MethodType(this, v))) : (frozenEmptySet as Set<IMethodTypeInternals>);
  }

  deriveSuper(): void {
    this.supertypes = new Set(this.supertypeGenerator());
  }

  onNewAssociationType(associationType: IAssociationTypeInternals) {
    this.directAssociationTypes.add(associationType);
  }

  onNewRoleType(roleType: IRoleTypeInternals) {
    this.directRoleTypes.add(roleType);
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

  *associationTypeGenerator(): IterableIterator<IAssociationTypeInternals> {
    if (this._associationTypes) {
      yield* this._associationTypes.values();
    } else {
      yield* this.directAssociationTypes.values();
      for (const supertype of this.directSupertypes.values()) {
        yield* supertype.associationTypeGenerator();
      }
    }
  }

  *roleTypeGenerator(): IterableIterator<IRoleTypeInternals> {
    if (this._roleTypes) {
      yield* this._roleTypes.values();
    } else {
      yield* this.directRoleTypes.values();
      for (const supertype of this.directSupertypes.values()) {
        yield* supertype.roleTypeGenerator();
      }
    }
  }

  *methodTypeGenerator(): IterableIterator<IMethodTypeInternals> {
    if (this._methodTypes) {
      yield* this._methodTypes.values();
    } else {
      yield* this.directMethodTypes.values();
      for (const supertype of this.directSupertypes.values()) {
        yield* supertype.methodTypeGenerator();
      }
    }
  }
}
