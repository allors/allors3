import { ObjectType } from '@allors/system/workspace/meta';
import { Between } from './between';
import { ContainedIn } from './contained-in';
import { Contains } from './contains';
import { Equals } from './equals';
import { Exists } from './exists';
import { GreaterThan } from './greater-than';
import { Instanceof } from './instance-of';
import { LessThan } from './less-than';
import { Like } from './like';
import { PredicateBase } from './predicate';

export type ParameterizablePredicate =
  | Between
  | ContainedIn
  | Contains
  | Equals
  | Exists
  | GreaterThan
  | Instanceof
  | LessThan
  | Like;

export type ParameterizablePredicateKind = ParameterizablePredicate['kind'];

export interface ParameterizablePredicateBase extends PredicateBase {
  parameter?: string;
}

export function parameterizablePredicateObjectType(
  predicate: ParameterizablePredicate
): ObjectType {
  if (predicate == null) {
    return null;
  }

  switch (predicate.kind) {
    case 'Between':
    case 'GreaterThan':
    case 'LessThan':
    case 'Like':
      return predicate.roleType.objectType;
    case 'ContainedIn':
    case 'Contains':
    case 'Equals':
    case 'Exists':
    case 'Instanceof':
      return predicate.propertyType.objectType;
  }
}
