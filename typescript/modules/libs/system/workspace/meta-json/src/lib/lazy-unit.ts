import { Origin, Unit, UnitTags } from '@allors/system/workspace/meta';
import { InternalMetaPopulation } from './internal/internal-meta-population';

export class LazyUnit implements Unit {
  readonly kind = 'Unit';
  readonly _ = {};
  isUnit = true;
  isComposite = false;
  isInterface = false;
  isClass = false;
  pluralName: string;
  origin = Origin.Database;
  isBinary = this.tag === UnitTags.Binary;
  isBoolean = this.tag === UnitTags.Boolean;
  isDecimal = this.tag === UnitTags.Decimal;
  isDateTime = this.tag === UnitTags.DateTime;
  isFloat = this.tag === UnitTags.Float;
  isInteger = this.tag === UnitTags.Integer;
  isString = this.tag === UnitTags.String;
  isUnique = this.tag === UnitTags.Unique;

  constructor(
    public metaPopulation: InternalMetaPopulation,
    public tag: string,
    public singularName: string
  ) {
    this.pluralName =
      singularName === 'Binary' ? 'Binaries' : singularName + 's';
    metaPopulation.onNewObjectType(this);
  }
}
