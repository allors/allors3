import { ObjectType, PropertyType } from '@allors/meta/system';
import { SessionObject, CompositeTypes } from '@allors/domain/system';

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
      propertyType: this.propertyType.id,
      parameter: this.parameter,
      object: this.object ? ((this.object as SessionObject).id ? (this.object as SessionObject).id : this.object) : undefined,
    };
  }
}
