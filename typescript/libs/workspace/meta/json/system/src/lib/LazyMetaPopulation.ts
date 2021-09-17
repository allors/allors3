import { MetaData } from '@allors/protocol/json/system';
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
import { LazyTreeBuilder } from './builders/LazyTreeBuilder';
import { LazySelectBuilder } from './builders/LazySelectBuilder';
import { LazyPullBuilder } from './builders/LazyPullBuilder';
import { LazyResultBuilder } from './builders/LazyResultBuilder';

export class LazyMetaPopulation implements InternalMetaPopulation {
  readonly kind = 'MetaPopulation';
  readonly metaObjectByTag: Map<string, InternalMetaObject> = new Map();
  readonly units: Set<InternalUnit>;
  readonly interfaces: Set<InternalInterface>;
  readonly classes: Set<InternalClass>;
  readonly composites = new Set<InternalComposite>();
  readonly relationTypes: Set<InternalRelationType>;
  readonly methodTypes: Set<InternalMethodType>;

  constructor(data: MetaData) {
    const lookup = new Lookup(data);

    this.units = new Set(['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].map((name, i) => new LazyUnit(this, (i + 1).toString(), name)));
    this.interfaces = new Set(data.i?.map((v) => new LazyInterface(this, v, lookup)) ?? []);
    this.classes = new Set(data.c?.map((v) => new LazyClass(this, v, lookup)) ?? []);
    this.relationTypes = new Set();
    this.methodTypes = new Set();

    this.composites.forEach((v) => v.derive(lookup));
    this.composites.forEach((v) => v.deriveSuper());
    this.interfaces.forEach((v) => v.deriveSub());
    this.composites.forEach((v) => v.deriveOperand());
    this.composites.forEach((v) => v.deriveOriginRoleType());
    this.composites.forEach((v) => v.derivePropertyTypeByPropertyName());
    this.classes.forEach((v) => v.deriveOverridden(lookup));

    this['treeBuilder'] = new LazyTreeBuilder(this);

    this['selectBuilder'] = new LazySelectBuilder(this);

    this['pullBuilder'] = new LazyPullBuilder(this);

    this['resultBuilder'] = new LazyResultBuilder(this);
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
