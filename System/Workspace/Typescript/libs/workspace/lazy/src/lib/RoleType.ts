import { IAssociationType, IObjectType, IRoleType, IUnit, Origin, pluralize } from '@allors/workspace/system';
import { RelationType } from './RelationType';

export class RoleType implements IRoleType {
  readonly isMany: boolean;
  readonly origin: Origin;
  readonly name: string;
  readonly operandId: string;
  readonly size: number;
  readonly precision: number;
  readonly scale: number;

  associationType: IAssociationType;

  constructor(
    public relationType: RelationType,
    public objectType: IObjectType,
    public isOne: boolean,
    public singularName: string,
    public isRequired: boolean = false,
    public isUnique: boolean = false,
    public mediaType: string,
    private _pluralName: string,
    sizeOrScaleAndPrecision: number | string
  ) {
    this.isMany = !this.isOne;

    if (this.objectType.isUnit && !!sizeOrScaleAndPrecision) {
      const unit = this.objectType as IUnit;
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
  }
  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }
}
