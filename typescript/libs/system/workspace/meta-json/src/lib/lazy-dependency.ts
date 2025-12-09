import {
  Composite,
  Dependency,
  PropertyType,
} from '@allors/system/workspace/meta';

export class LazyDependency implements Dependency {
  constructor(
    public readonly objectType: Composite,
    public readonly propertyType: PropertyType
  ) {}
}
