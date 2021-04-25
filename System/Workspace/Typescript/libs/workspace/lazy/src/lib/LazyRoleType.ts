import { Origin, pluralize } from '@allors/workspace/system';
import { InternalAssociationType } from './internal/InternalAssociationType';
import { InternalObjectType } from './internal/InternalObjectType';
import { InternalRelationType } from './internal/InternalRelationType';
import { InternalRoleType } from './internal/InternalRoleType';
import { InternalUnit } from './internal/InternalUnit';

export class LazyRoleType implements InternalRoleType {
  readonly isMany: boolean;
  readonly origin: Origin;
  readonly name: string;
  readonly operandTag: number;
  readonly size: number;
  readonly precision: number;
  readonly scale: number;

  associationType: InternalAssociationType;

  constructor(
    public relationType: InternalRelationType,
    public objectType: InternalObjectType,
    public isOne: boolean,
    public singularName: string,
    public isRequired: boolean = false,
    public isUnique: boolean = false,
    public mediaType: string,
    private _pluralName: string,
    sizeOrScaleAndPrecision: number | string
  ) {
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
  }
  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }
}
