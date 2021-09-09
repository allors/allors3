import { ObjectType } from '@allors/workspace/meta/system';
import { Between } from './Between';
import { ContainedIn } from './ContainedIn';
import { Contains } from './Contains';
import { Equals } from './Equals';
import { Exists } from './Exists';
import { GreaterThan } from './GreaterThan';
import { Instanceof } from './Instanceof';
import { LessThan } from './LessThan';
import { Like } from './Like';
import { PredicateBase } from './Predicate';

export type ParameterizablePredicate = Between | ContainedIn | Contains | Equals | Exists | GreaterThan | Instanceof | LessThan | Like;

export type ParameterizablePredicateKind = ParameterizablePredicate['kind'];

export interface ParameterizablePredicateBase extends PredicateBase {
  parameter?: string;
}

export function parameterizablePredicateObjectType(predicate: ParameterizablePredicate): ObjectType {
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
