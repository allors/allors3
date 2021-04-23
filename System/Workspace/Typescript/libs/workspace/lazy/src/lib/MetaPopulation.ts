import { MetaData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { IMetaObjectInternals } from './Internals/IMetaObjectInternals';
import { IUnitInternals } from './Internals/IUnitInternals';
import { IInterfaceInternals } from './Internals/IInterfaceInternals';
import { IClassInternals } from './Internals/IClassInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IRelationTypeInternals } from './Internals/IRelationTypeInternals';
import { IMethodTypeInternals } from './Internals/IMethodTypeInternals';
import { IObjectTypeInternals } from './Internals/IObjectTypeInternals';
import { Unit } from './Unit';
import { Interface } from './Interface';
import { Class } from './Class';

export class MetaPopulation implements IMetaPopulationInternals {
  readonly units: IUnitInternals[];
  readonly interfaces: IInterfaceInternals[];
  readonly classes: IClassInternals[];
  readonly composites: ICompositeInternals[];
  readonly metaObjectByTag: IMetaObjectInternals[] = [];
  readonly relationTypes: IRelationTypeInternals[];
  readonly methodTypes: IMethodTypeInternals[];

  constructor(data: MetaData) {
    this.units = ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name));
    this.interfaces = data.i?.map((v) => new Interface(this, v)) ?? [];
    this.classes = data.c?.map((v) => new Class(this, v)) ?? [];
    this.composites = (this.interfaces as ICompositeInternals[]).concat(this.classes);

    this.composites.forEach((v) => v.derive());
    this.composites.forEach((v) => v.deriveSuper());
    this.interfaces.forEach((v) => v.deriveSub());
  }

  onNew(metaObject: IMetaObjectInternals) {
    this.metaObjectByTag[metaObject.tag] = metaObject;
  }

  onNewObjectType(objectType: IObjectTypeInternals) {
    this.onNew(objectType);
    this[objectType.singularName] = objectType;
  }
}
