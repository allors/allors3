import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/meta/core';

import { ParameterizablePredicate, ParameterizablePredicateArgs } from './ParameterizablePredicate';

export interface InstanceofArgs extends ParameterizablePredicateArgs, Pick<Instanceof, 'propertyType' | 'objectType'> {}

export class Instanceof extends ParameterizablePredicate {
  propertyType: PropertyType;
  instanceObjectType?: ObjectType;

  constructor(propertyType: PropertyType);
  constructor(args: InstanceofArgs);
  constructor(args: InstanceofArgs | PropertyType) {
    super();

    if ((args as PropertyType).objectType) {
      this.propertyType = args as PropertyType;
    } else {
      Object.assign(this, args);
      this.propertyType = (args as InstanceofArgs).propertyType;
    }
  }

  get objectType(): ObjectType {
    return this.propertyType.objectType;
  }

  toJSON(): any {
    return {
      kind: 'Instanceof',
      dependencies: this.dependencies,
      associationType: (this.propertyType instanceof AssociationType) ? this.propertyType.relationType.id : undefined,
      roleType: (this.propertyType instanceof RoleType) ? this.propertyType.relationType.id : undefined,
      parameter: this.parameter,
      objecttype: this.instanceObjectType?.id,
    };
  }
}
