import { Origin, pluralize, Multiplicity } from '@allors/workspace/system';
import { InternalAssociationType } from './internal/InternalAssociationType';
import { InternalObjectType } from './internal/InternalObjectType';
import { InternalRelationType } from './internal/InternalRelationType';
import { InternalRoleType } from './internal/InternalRoleType';
import { InternalUnit } from './internal/InternalUnit';
import { LazyAssociationType } from './LazyAssociationType';
import { InternalComposite } from './internal/InternalComposite';

export class LazyRoleType implements InternalRoleType {
  readonly isOne: boolean;
  readonly isMany: boolean;
  readonly origin: Origin;
  readonly name: string;
  readonly operandTag: number;
  readonly size?: number;
  readonly precision?: number;
  readonly scale?: number;

  associationType: InternalAssociationType;

  constructor(
    public relationType: InternalRelationType,

    associationObjectType: InternalComposite,
    public objectType: InternalObjectType,
    multiplicity: Multiplicity,
    private _singularName?: string,
    public isRequired: boolean = false,
    public isUnique: boolean = false,
    public mediaType?: string,
    private _pluralName?: string,
    sizeOrScaleAndPrecision?: number | string,
  ) {
    this.isOne = (multiplicity & 2) == 0;
    this.isMany = !this.isOne;
    this.origin = relationType.origin;
    this.operandTag = relationType.tag;

    if (this.objectType.isUnit && !!sizeOrScaleAndPrecision) {
      const unit = this.objectType as InternalUnit;
      if (unit.isString) {
        this.size = sizeOrScaleAndPrecision as number;
      }
      if (unit.isDecimal) {
        const [scale, precision] = (sizeOrScaleAndPrecision as string).split(':');
        if (scale) {
          this.scale = Number(scale);
        }
        if (precision) {
          this.precision = Number(precision);
        }
      }
    }

    this.name = this.isOne ? this.singularName : this.pluralName;

    this.associationType = new LazyAssociationType(this, associationObjectType, multiplicity);
  }

  get singularName() {
    return (this._singularName ??= this.objectType.singularName);
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }
}
