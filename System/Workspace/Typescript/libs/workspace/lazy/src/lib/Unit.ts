import { IUnit, Origin, UnitTags } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export class Unit implements IUnit {
  constructor(public metaPopulation: IMetaPopulationInternals, public tag: number, public singularName: string) {
    this.pluralName = singularName === 'Binary' ? 'Binaries' : singularName + 's';
    metaPopulation.onObjectType(this);
  }

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
  Ÿ;
}
