import { IMetaPopulation, IUnit, Origin, UnitTags } from '@allors/workspace/system';

export class Unit implements IUnit {
  pluralName: string;
  origin = Origin.Database;
  isUnit = true;
  isComposite = false;
  isInterface = false;
  isClass = false;
  isBinary = this.tag === UnitTags.Binary;
  isBoolean = this.tag === UnitTags.Boolean;
  isDecimal = this.tag === UnitTags.Decimal;
  isDateTime = this.tag === UnitTags.DateTime;
  isFloat = this.tag === UnitTags.Float;
  isInteger = this.tag === UnitTags.Integer;
  isString = this.tag === UnitTags.String;
  isUnique = this.tag === UnitTags.Unique;

  constructor(public metaPopulation: IMetaPopulation, public tag: number, public singularName: string) {
    metaPopulation[singularName] = this;
    this.pluralName = singularName === 'Binary' ? 'Binaries' : singularName + 's';
  }
}
