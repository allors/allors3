import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/meta/core';
import { DatabaseObject, CompositeTypes, UnitTypes, serialize} from '@allors/workspace/core';

import { ParameterizablePredicateArgs, ParameterizablePredicate } from './ParameterizablePredicate';

export interface EqualsArgs extends ParameterizablePredicateArgs, Pick<Equals, 'propertyType' | 'value' | 'object'> {}

export class Equals extends ParameterizablePredicate {
  public propertyType: PropertyType;
  public value?: UnitTypes;
  public object?: CompositeTypes;

  constructor(propertyType: PropertyType);
  constructor(args: EqualsArgs);
  constructor(args: EqualsArgs | PropertyType) {
    super();

    if ((args as PropertyType).objectType) {
      this.propertyType = args as PropertyType;
    } else {
      Object.assign(this, args);
      this.propertyType = (args as EqualsArgs).propertyType;
    }
  }

  get objectType(): ObjectType {
    return this.propertyType.objectType;
  }

  public toJSON(): any {
    return {
      kind: 'Equals',
      dependencies: this.dependencies,
      associationType: (this.propertyType instanceof AssociationType) ? this.propertyType.relationType.id : undefined,
      roleType: (this.propertyType instanceof RoleType) ? this.propertyType.relationType.id : undefined,
      parameter: this.parameter,
      value: serialize(this.value),
      object: this.object && (this.object as DatabaseObject).id ? (this.object as DatabaseObject).id : this.object,
    };
  }
}
