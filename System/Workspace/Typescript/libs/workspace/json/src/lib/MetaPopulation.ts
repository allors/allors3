import { IClass, IComposite, IInterface, IMetaObject, IMethodType, IRelationType, IUnit } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Class } from './Class';
import { Interface } from './Interface';
import { MetaData } from './MetaData';
import { Unit } from './Unit';
import { IObjectType } from '../../../system/src/lib/meta/IObjectType';

export class MetaPopulation implements IMetaPopulationInternals {
  units: IUnit[];
  interfaces: IInterface[];
  classes: IClass[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
  composites: IComposite[];
  metaObjectByTag: IMetaObject[] = [];
  metaObjectByName: { [name: string]: IObjectType } = {};

  constructor(metaData: MetaData) {
    this.units = ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name));
    this.interfaces = metaData.i?.map((i) => new Interface(this, i)) ?? [];
    this.classes = metaData.i?.map((i) => new Class(this, i)) ?? [];
  }

  onObjectType(objectType: IObjectType): void {
    this.metaObjectByName[objectType.singularName] = objectType;
    this.metaObjectByTag[objectType.tag] = objectType;
    this[objectType.singularName] = objectType;
  }
}
