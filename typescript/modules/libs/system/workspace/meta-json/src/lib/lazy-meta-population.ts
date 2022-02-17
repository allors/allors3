import { MetaData } from '@allors/system/common/protocol-json';

import { Lookup } from './utils/lookup';
import { InternalMetaPopulation } from './internal/internal-meta-population';
import { InternalInterface } from './internal/internal-interface';
import { InternalClass } from './internal/internal-class';
import { InternalComposite } from './internal/internal-composite';

import { LazyPathBuilder } from './builders/lazy-path-builder';
import { LazyTreeBuilder } from './builders/lazy-tree-builder';
import { LazySelectBuilder } from './builders/lazy-select-builder';
import { LazyPullBuilder } from './builders/lazy-pull-builder';
import { LazyResultBuilder } from './builders/lazy-result-builder';

import { LazyUnit } from './lazy-unit';
import { LazyInterface } from './lazy-interface';
import { LazyClass } from './lazy-class';
import {
  Composite,
  Dependency,
  MetaObject,
  MethodType,
  ObjectType,
  PropertyType,
  RelationType,
  Unit,
} from '@allors/system/workspace/meta';

export class LazyMetaPopulation implements InternalMetaPopulation {
  readonly kind = 'MetaPopulation';
  readonly _ = {};
  metaObjectByTag: Map<string, MetaObject> = new Map();
  objectTypeByUppercaseName: Map<string, MetaObject> = new Map();
  units: Set<Unit>;
  interfaces: Set<InternalInterface>;
  classes: Set<InternalClass>;
  composites = new Set<InternalComposite>();
  relationTypes: Set<RelationType>;
  methodTypes: Set<MethodType>;

  dependency: <T extends Composite>(
    objectType: T,
    propertyType: (objectType: T) => PropertyType
  ) => Dependency;

  constructor(data: MetaData) {
    const lookup = new Lookup(data);

    this.units = new Set(
      [
        'Binary',
        'Boolean',
        'DateTime',
        'Decimal',
        'Float',
        'Integer',
        'String',
        'Unique',
      ].map((name, i) => new LazyUnit(this, (i + 1).toString(), name))
    );
    this.interfaces = new Set(
      data.i?.map((v) => new LazyInterface(this, v, lookup)) ?? []
    );
    this.classes = new Set(
      data.c?.map((v) => new LazyClass(this, v, lookup)) ?? []
    );
    this.relationTypes = new Set();
    this.methodTypes = new Set();

    this.composites.forEach((v) => v.derive(lookup));
    this.composites.forEach((v) => v.deriveSuper());
    this.interfaces.forEach((v) => v.deriveSub());
    this.composites.forEach((v) => v.deriveOperand());
    this.composites.forEach((v) => v.deriveOriginRoleType());
    this.composites.forEach((v) => v.derivePropertyTypeByPropertyName());
    this.classes.forEach((v) => v.deriveOverridden(lookup));
    this.composites.forEach((v) => v.deriveDependencies());

    this['pathBuilder'] = new LazyPathBuilder(this);

    this['treeBuilder'] = new LazyTreeBuilder(this);

    this['selectBuilder'] = new LazySelectBuilder(this);

    this['pullBuilder'] = new LazyPullBuilder(this);

    this['resultBuilder'] = new LazyResultBuilder(this);

    this.dependency = (objectType, propertyType) => {
      return objectType.dependencyByPropertyType.get(propertyType(objectType));
    };
  }

  onNew(metaObject: MetaObject) {
    this.metaObjectByTag.set(metaObject.tag, metaObject);
  }

  onNewObjectType(objectType: ObjectType) {
    this.onNew(objectType);
    (this as Record<string, unknown>)[objectType.singularName] = objectType;
    this.objectTypeByUppercaseName.set(
      objectType.singularName.toUpperCase(),
      objectType
    );
  }
  onNewComposite(objectType: InternalComposite) {
    this.onNewObjectType(objectType);
    this.composites.add(objectType);
  }
}
