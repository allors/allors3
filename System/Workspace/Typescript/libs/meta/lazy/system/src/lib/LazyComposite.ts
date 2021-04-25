import { Origin, pluralize, ObjectTypeData, Multiplicity } from '@allors/workspace/system';
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
import { Lookup } from './utils/Lookup';

export abstract class LazyComposite implements InternalComposite {
  isUnit = false;
  isComposite = true;
  readonly tag: number;
  singularName: string;

  directSupertypes: Set<InternalInterface>;
  supertypes: Set<InternalInterface>;

  directAssociationTypes: Set<InternalAssociationType> = new Set();
  directRoleTypes: Set<InternalRoleType> = new Set();
  directMethodTypes: Set<InternalMethodType> = new Set();

  abstract isClass: boolean;
  abstract classes: Set<InternalClass>;

  private _associationTypes?: Set<InternalAssociationType>;
  private _roleTypes?: Set<InternalRoleType>;
  private _methodTypes?: Set<InternalMethodType>;
  private _origin?: Origin;
  private _pluralName?: string;

  get origin() {
    return (this._origin ??= Origin.Database);
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get associationTypes(): Set<InternalAssociationType> {
    return (this._associationTypes ??= new Set(this.associationTypeGenerator()));
  }
  get roleTypes(): Set<InternalRoleType> {
    return (this._roleTypes ??= new Set(this.roleTypeGenerator()));
  }

  get methodTypes(): Set<InternalMethodType> {
    return (this._methodTypes ??= new Set(this.methodTypeGenerator()));
  }

  abstract isAssignableFrom(objectType: InternalComposite): boolean;

  constructor(public metaPopulation: InternalMetaPopulation, public d: ObjectTypeData) {
    this.tag = d[0];
    this.singularName = d[1];
    metaPopulation.onNewComposite(this);
    this.directSupertypes = frozenEmptySet as Set<InternalInterface>;
    this.supertypes = frozenEmptySet as Set<InternalInterface>;
    this.directMethodTypes = frozenEmptySet as Set<InternalMethodType>;
  }

  derive(lookup: Lookup): void {
    const [, s, d, r, m, p] = this.d;

    this.singularName = s;
    this._pluralName = p;
    if (d) {
      this.directSupertypes = new Set(d?.map((v) => this.metaPopulation.metaObjectByTag.get(v) as InternalInterface));
    }
    r?.forEach((v) => new LazyRelationType(this, v, lookup));
    if (m) {
      this.directMethodTypes = new Set(m?.map((v) => new LazyMethodType(this, v)));
    }
  }

  deriveSuper(): void {
    if (this.directSupertypes.size > 0) {
      this.supertypes = new Set(this.supertypeGenerator());
    }
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
