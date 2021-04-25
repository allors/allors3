import { Origin, pluralize, ObjectTypeData } from '@allors/workspace/system';
import { frozenEmptySet } from './utils/frozenEmptySet';
import { InternalComposite } from './internal/InternalComposite';
import { InternalInterface } from './internal/InternalInterface';
import { InternalAssociationType } from './internal/InternalAssociationType';
import { InternalRoleType } from './internal/InternalRoleType';
import { InternalMethodType } from './internal/InternalMethodType';
import { InternalClass } from './internal/InternalClass';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { LazyRelationType } from './LazyRelationType';
import { LazyMethodType } from './LazyMethodType';

export abstract class LazyComposite implements InternalComposite {
  isUnit = false;
  isComposite = true;
  readonly tag: number;
  singularName: string;

  directSupertypes: Readonly<Set<InternalInterface>>;
  supertypes: Readonly<Set<InternalInterface>>;

  directAssociationTypes: Set<InternalAssociationType> = new Set();
  directRoleTypes: Set<InternalRoleType> = new Set();
  directMethodTypes: Readonly<Set<InternalMethodType>> = new Set();

  abstract isClass: boolean;
  abstract classes: Readonly<Set<InternalClass>>;

  private _associationTypes: Readonly<Set<InternalAssociationType>>;
  private _roleTypes: Readonly<Set<InternalRoleType>>;
  private _methodTypes: Readonly<Set<InternalMethodType>>;
  private _pluralName: string;
  private _origin: Origin;

  get origin() {
    // return (this._origin ??= );
    return this._origin;
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get associationTypes(): Readonly<Set<InternalAssociationType>> {
    return this._associationTypes ??= new Set(this.associationTypeGenerator());
  }
  get roleTypes(): Readonly<Set<InternalRoleType>> {
    return this._roleTypes ??= new Set(this.roleTypeGenerator());
  }

  get methodTypes(): Readonly<Set<InternalMethodType>> {
    return this._methodTypes ??= new Set(this.methodTypeGenerator());
  }

  abstract isAssignableFrom(objectType: InternalComposite): boolean;

  constructor(public metaPopulation: InternalMetaPopulation, public d: ObjectTypeData) {
    this.tag = d[0];
    this.singularName = d[1];
    metaPopulation.onNewComposite(this);
  }

  derive(): void {
    const [, s, d, r, m, p] = this.d;

    this.singularName = s;
    this._pluralName = p;
    this.directSupertypes = d?.length > 0 ? new Set(d?.map((v) => this.metaPopulation.metaObjectByTag[v] as InternalInterface)) : (frozenEmptySet as Set<InternalInterface>);
    r?.forEach((v) => new LazyRelationType(this, v));
    this.directMethodTypes = m?.length > 1 ? new Set(m?.map((v) => new LazyMethodType(this, v))) : (frozenEmptySet as Set<InternalMethodType>);
  }

  deriveSuper(): void {
    this.supertypes = new Set(this.supertypeGenerator());
  }

  onNewAssociationType(associationType: InternalAssociationType) {
    this.directAssociationTypes.add(associationType);
  }

  onNewRoleType(roleType: InternalRoleType) {
    this.directRoleTypes.add(roleType);
  }

  *supertypeGenerator(): IterableIterator<InternalInterface> {
    if (this.supertypes) {
      yield* this.supertypes.values();
    } else {
      for (const supertype of this.directSupertypes.values()) {
        yield supertype;
        yield* supertype.supertypeGenerator();
      }
    }
  }

  *associationTypeGenerator(): IterableIterator<InternalAssociationType> {
    if (this._associationTypes) {
      yield* this._associationTypes.values();
    } else {
      yield* this.directAssociationTypes.values();
      for (const supertype of this.directSupertypes.values()) {
        yield* supertype.associationTypeGenerator();
      }
    }
  }

  *roleTypeGenerator(): IterableIterator<InternalRoleType> {
    if (this._roleTypes) {
      yield* this._roleTypes.values();
    } else {
      yield* this.directRoleTypes.values();
      for (const supertype of this.directSupertypes.values()) {
        yield* supertype.roleTypeGenerator();
      }
    }
  }

  *methodTypeGenerator(): IterableIterator<InternalMethodType> {
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
