import { MetaData, Multiplicity, Origin } from '@allors/workspace/system';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalMetaObject } from './internal/InternalMetaObject';
import { InternalUnit } from './internal/InternalUnit';
import { InternalInterface } from './internal/InternalInterface';
import { InternalClass } from './internal/InternalClass';
import { InternalComposite } from './internal/InternalComposite';
import { InternalRelationType } from './internal/InternalRelationType';
import { InternalMethodType } from './internal/InternalMethodType';
import { InternalObjectType } from './internal/InternalObjectType';
import { LazyUnit } from './LazyUnit';
import { LazyInterface } from './LazyInterface';
import { LazyClass } from './LazyClass';
import { Lookup } from './utils/Lookup';

export class LazyMetaPopulation implements InternalMetaPopulation {
  readonly metaObjectByTag: Map<number, InternalMetaObject> = new Map();
  readonly units: Set<InternalUnit>;
  readonly interfaces: Set<InternalInterface>;
  readonly classes: Set<InternalClass>;
  readonly composites = new Set<InternalComposite>();
  readonly relationTypes: Set<InternalRelationType>;
  readonly methodTypes: Set<InternalMethodType>;

  constructor(data: MetaData) {
    this.units = new Set(['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new LazyUnit(this, i + 1, name)));
    this.interfaces = new Set(data.i?.map((v) => new LazyInterface(this, v)) ?? []);
    this.classes = new Set(data.c?.map((v) => new LazyClass(this, v)) ?? []);
    this.relationTypes = new Set();
    this.methodTypes = new Set();

    const lookup = new Lookup(this, data);

    this.composites.forEach((v) => v.derive(lookup));
    this.composites.forEach((v) => v.deriveSuper());
    this.interfaces.forEach((v) => v.deriveSub());
  }

  onNew(metaObject: InternalMetaObject) {
    this.metaObjectByTag.set(metaObject.tag, metaObject);
  }

  onNewObjectType(objectType: InternalObjectType) {
    this.onNew(objectType);
    (this as Record<string, unknown>)[objectType.singularName] = objectType;
  }
  onNewComposite(objectType: InternalComposite) {
    this.onNewObjectType(objectType);
    this.composites.add(objectType);
  }
}
