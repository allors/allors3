import { IClass, IComposite, IInterface, IMetaObject, IMetaPopulation, IMethodType, IRelationType, IUnit } from '@allors/workspace/system';
import { Interface } from './Interface';
import { MetaData } from './MetaData';
import { Unit } from './Unit';

export class MetaPopulation implements IMetaPopulation {
  units: IUnit[];
  interfaces: IInterface[];
  classes: IClass[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
  composites: IComposite[];
  metaObjectByTag: IMetaObject[];
  metaObjectByName: { [name: string]: IMetaObject };

  constructor(metaData: MetaData) {
    this.units = ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name));
    this.interfaces = metaData.i?.map((i) => new Interface(this, i)) ?? [];
  }
}
