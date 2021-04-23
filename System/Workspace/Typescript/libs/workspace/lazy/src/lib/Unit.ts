import { IUnit, Origin, UnitTags } from '@allors/workspace/system';
import { MetaPopulation } from './MetaPopulation';

export class Unit implements IUnit {
  isUnit = true;
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

  constructor(public metaPopulation: MetaPopulation, public tag: number, public singularName: string) {
    this.pluralName = singularName === 'Binary' ? 'Binaries' : singularName + 's';
    metaPopulation.onNewObjectType(this);
  }
}
