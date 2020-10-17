import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/meta/core';

import { ParameterizablePredicate, ParameterizablePredicateArgs } from './ParameterizablePredicate';

export interface ExistArgs extends ParameterizablePredicateArgs, Pick<Exists, 'propertyType'> {}

export class Exists extends ParameterizablePredicate {
  propertyType: PropertyType;

  constructor(propertyType: PropertyType);
  constructor(args: ExistArgs);
  constructor(args: ExistArgs | PropertyType) {
    super();

    if ((args as PropertyType).objectType) {
      this.propertyType = args as PropertyType;
    } else {
      Object.assign(this, args);
      this.propertyType = (args as ExistArgs).propertyType;
    }
  }

  get objectType(): ObjectType {
    return this.propertyType.objectType;
  }

  public toJSON(): any {
    return {
      kind: 'Exists',
      dependencies: this.dependencies,
      associationType: (this.propertyType instanceof AssociationType) ? this.propertyType.relationType.id : undefined,
      roleType: (this.propertyType instanceof RoleType) ? this.propertyType.relationType.id : undefined,
      parameter: this.parameter,
    };
  }
}
