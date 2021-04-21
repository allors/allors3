import { IMetaObject, IMethodType, IObjectType, IRelationType, IUnit } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IClassInternals } from './Internals/IClassInternals';
import { IInterfaceInternals } from './Internals/IInterfaceInternals';
import { Class } from './Class';
import { Interface } from './Interface';
import { MetaData } from './MetaData';
import { Unit } from './Unit';

export class MetaPopulation implements IMetaPopulationInternals {
  constructor(metaData: MetaData) {
    this.units = ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name));
    this.interfaces = metaData.i?.map((i) => new Interface(this, i)) ?? [];
    this.classes = metaData.i?.map((i) => new Class(this, i)) ?? [];
    this.composites = this.classes.concat(this.interfaces);
    this.composites.forEach((v) => v.init());
  }

  units: IUnit[];
  interfaces: IInterfaceInternals[];
  classes: IClassInternals[];
  composites: ICompositeInternals[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
  metaObjectByTag: IMetaObject[] = [];
  metaObjectByName: { [name: string]: IObjectType } = {};

  onObjectType(objectType: IObjectType): void {
    this.metaObjectByName[objectType.singularName] = objectType;
    this.metaObjectByTag[objectType.tag] = objectType;
    this[objectType.singularName] = objectType;
  }
}
