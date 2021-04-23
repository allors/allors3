import { IMetaObject, IMetaPopulation, IObjectType, MetaData } from '@allors/workspace/system';
import { Class } from './Class';
import { Interface } from './Interface';
import { Unit } from './Unit';
import { RelationType } from './RelationType';
import { MethodType } from './MethodType';
import { Composite } from './Composite';

export class MetaPopulation implements IMetaPopulation {
  readonly units: Unit[];
  readonly interfaces: Interface[];
  readonly classes: Class[];
  readonly composites: Composite[];
  readonly metaObjectByTag: IMetaObject[] = [];
  readonly relationTypes: RelationType[];
  readonly methodTypes: MethodType[];

  constructor(data: MetaData) {
    this.units = ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new Unit(this, i + 1, name));
    this.interfaces = data.i?.map((v) => new Interface(this, v)) ?? [];
    this.classes = data.c?.map((v) => new Class(this, v)) ?? [];
    this.composites = (this.interfaces as Composite[]).concat(this.classes);
    this.composites.forEach((v) => v.init());
    // data.h?.forEach(([sup, subs]) => {
    //   const supertype = this.metaObjectByTag[sup] as IInterface;
    //   const directSubtypes = subs?.map((w) => this.metaObjectByTag[w] as IComposite) ?? [];
    //   supertype.directSubtypes = directSubtypes;
    //   directSubtypes.forEach((v) => {
    //     if (v.directSupertypes) {
    //       v.directSupertypes.push(supertype);
    //     } else {
    //       v.directSupertypes = [supertype];
    //     }
    //   });
    // });
    Object.freeze(this.metaObjectByTag);
    Object.freeze(this.units);
    Object.freeze(this.interfaces);
    Object.freeze(this.classes);
    Object.freeze(this.relationTypes);
    Object.freeze(this.methodTypes);
  }

  onNew(metaObject: IMetaObject) {
    this.metaObjectByTag[metaObject.tag] = metaObject;
  }

  onNewObjectType(objectType: IObjectType) {
    this.onNew(objectType);
    this[objectType.singularName] = objectType;
  }
}
