import { Composite, Dependency, PropertyType } from '@allors/workspace/meta/system';

export class LazyDependency implements Dependency {
  constructor(public readonly objectType: Composite, public readonly propertyType: PropertyType) {}
}
