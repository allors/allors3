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
  readonly units: Readonly<Set<IUnitInternals>>;
  readonly interfaces: Readonly<Set<IInterfaceInternals>>;
  readonly classes: Readonly<Set<IClassInternals>>;
  readonly composites = new Set<ICompositeInternals>();
  readonly metaObjectByTag: Map<number, IMetaObjectInternals> = new Map();
  readonly relationTypes: Readonly<Set<IRelationTypeInternals>>;
  readonly methodTypes: Readonly<Set<IMethodTypeInternals>>;

  constructor(data: MetaData) {
    this.units = new Set(['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name)));
    this.interfaces = new Set(data.i?.map((v) => new Interface(this, v)) ?? []);
    this.classes = new Set(data.c?.map((v) => new Class(this, v)) ?? []);

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
  onNewComposite(objectType: ICompositeInternals) {
    this.onNewObjectType(objectType);
    this.composites.add(objectType);
  }
}
