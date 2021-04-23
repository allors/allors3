import { Origin, UnitTags } from '@allors/workspace/system';
import { IUnitInternals } from './Internals/IUnitInternals';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export class Unit implements IUnitInternals {
  isUnit = true;
  isComposite = false;
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

  constructor(public metaPopulation: IMetaPopulationInternals, public tag: number, public singularName: string) {
    this.pluralName = singularName === 'Binary' ? 'Binaries' : singularName + 's';
    metaPopulation.onNewObjectType(this);
  }
}
