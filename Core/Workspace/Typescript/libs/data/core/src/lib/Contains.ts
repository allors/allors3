import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/meta/core';
import { DatabaseObject, CompositeTypes } from '@allors/workspace/core';

import { ParameterizablePredicateArgs, ParameterizablePredicate } from './ParameterizablePredicate';

export interface ContainsArgs extends ParameterizablePredicateArgs, Pick<Contains, 'propertyType' | 'object'> {}

export class Contains extends ParameterizablePredicate {
  propertyType: PropertyType;
  object?: CompositeTypes;

  constructor(propertyType: PropertyType);
  constructor(args: ContainsArgs);
  constructor(args: ContainsArgs | PropertyType) {
    super();

    if ((args as PropertyType).objectType) {
      this.propertyType = args as PropertyType;
    } else {
      Object.assign(this, args);
      this.propertyType = (args as ContainsArgs).propertyType;
    }
  }

  get objectType(): ObjectType {
    return this.propertyType.objectType;
  }

  public toJSON(): any {
    return {
      kind: 'Contains',
      dependencies: this.dependencies,
      associationType: (this.propertyType instanceof AssociationType) ? this.propertyType.relationType.id : undefined,
      roleType: (this.propertyType instanceof RoleType) ? this.propertyType.relationType.id : undefined,
      parameter: this.parameter,
      object: this.object ? ((this.object as DatabaseObject).id ? (this.object as DatabaseObject).id : this.object) : undefined,
    };
  }
}
