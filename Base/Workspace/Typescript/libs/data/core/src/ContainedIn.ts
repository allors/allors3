import { AssociationType, ObjectType, PropertyType } from '@allors/meta/core';
import { DatabaseObject, CompositeTypes } from '@allors/workspace/core';

import {
  ParameterizablePredicateArgs,
  ParameterizablePredicate,
} from './ParameterizablePredicate';
import { IExtent } from './IExtent';

export interface ContainedInArgs
  extends ParameterizablePredicateArgs,
    Pick<ContainedIn, 'propertyType' | 'extent' | 'objects'> {}

export class ContainedIn extends ParameterizablePredicate {
  propertyType: PropertyType;
  extent?: IExtent;
  objects?: Array<CompositeTypes>;

  constructor(propertyType: PropertyType);
  constructor(args: ContainedInArgs);
  constructor(args: ContainedInArgs | PropertyType) {
    super();

    if ((args as PropertyType).objectType) {
      this.propertyType = args as PropertyType;
    } else {
      Object.assign(this, args);
      this.propertyType = (args as ContainedInArgs).propertyType;
    }
  }

  get objectType(): ObjectType {
    return this.propertyType.objectType;
  }

  public toJSON(): any {
    if (this.propertyType instanceof AssociationType) {
      return {
        kind: 'ContainedIn',
        associationType: this.propertyType.relationType.id,
        dependencies: this.dependencies,
        parameter: this.parameter,
        extent: this.extent,
        objects: this.objects
          ? this.objects.map((v) =>
              (v as DatabaseObject).id ? (v as DatabaseObject).id : v
            )
          : undefined,
      };
    } else {
      return {
        kind: 'ContainedIn',
        roleType: this.propertyType.relationType.id,
        dependencies: this.dependencies,
        parameter: this.parameter,
        extent: this.extent,
        objects: this.objects
          ? this.objects.map((v) =>
              (v as DatabaseObject).id ? (v as DatabaseObject).id : v
            )
          : undefined,
      };
    }
  }
}
