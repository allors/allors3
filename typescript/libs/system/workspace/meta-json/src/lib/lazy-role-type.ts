import { RelationTypeData } from '@allors/system/common/protocol-json';
import {
  Origin,
  pluralize,
  Multiplicity,
  RoleType,
  AssociationType,
  RelationType,
  ObjectType,
  Unit,
} from '@allors/system/workspace/meta';

import { Lookup } from './utils/lookup';
import { InternalComposite } from './internal/internal-composite';

import { LazyAssociationType } from './lazy-association-type';

export class LazyRoleType implements RoleType {
  readonly kind = 'RoleType';
  readonly _ = {};
  isRoleType = true;
  isAssociationType = false;
  isMethodType = false;

  objectType: ObjectType;
  isOne: boolean;
  isMany: boolean;
  origin: Origin;
  name: string;
  singularName: string;
  isDerived: boolean;
  isRequired: boolean;
  size?: number;
  precision?: number;
  scale?: number;
  mediaType?: string;
  operandTag: string;

  associationType: AssociationType;

  private _pluralName?: string;

  constructor(
    public relationType: RelationType,
    associationObjectType: InternalComposite,
    roleObjectType: ObjectType,
    multiplicity: Multiplicity,
    data: RelationTypeData,
    lookup: Lookup
  ) {
    this.isOne = (multiplicity & 1) == 0;
    this.isMany = !this.isOne;
    this.origin = relationType.origin;
    this.operandTag = relationType.tag;
    this.objectType = roleObjectType;

    this.isDerived = lookup.d.has(this.relationType.tag);
    this.isRequired = lookup.r.has(this.relationType.tag);
    this.mediaType = lookup.t.get(this.relationType.tag);

    const [, , v0, v1, v2, v3] = data;

    this.singularName =
      (!Number.isInteger(v0) ? (v0 as string) : undefined) ??
      this.objectType.singularName;
    this._pluralName = !Number.isInteger(v1) ? (v1 as string) : undefined;

    if (this.objectType.isUnit) {
      const unit = this.objectType as Unit;
      if (unit.isString || unit.isBinary || unit.isDecimal) {
        let sizeOrScale = undefined;
        let precision = undefined;

        if (Number.isInteger(v0)) {
          sizeOrScale = v0 as number;
          precision = v1 as number;
        } else if (Number.isInteger(v1)) {
          sizeOrScale = v1 as number;
          precision = v2 as number;
        } else {
          sizeOrScale = v2 as number;
          precision = v3 as number;
        }

        if (unit.isString || unit.isBinary) {
          this.size = sizeOrScale;
        }
        if (unit.isDecimal) {
          this.scale = sizeOrScale;
          this.precision = precision;
        }
      }
    }

    this.name = this.isOne ? this.singularName : this.pluralName;

    this.associationType = new LazyAssociationType(
      this,
      associationObjectType,
      multiplicity
    );
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }
}
