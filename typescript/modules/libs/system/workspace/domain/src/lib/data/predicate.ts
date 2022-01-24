import { ContainedIn } from './contained-in';
import { And } from './and';
import { Between } from './between';
import { Contains } from './contains';
import { Equals } from './equals';
import { Exists } from './exists';
import { GreaterThan } from './greater-than';
import { Instanceof } from './instance-of';
import { LessThan } from './less-than';
import { Like } from './like';
import { Not } from './not';
import { Or } from './or';

export type Predicate = And | Between | ContainedIn | Contains | Equals | Exists | GreaterThan | Instanceof | LessThan | Like | Not | Or;

export type PredicateKind = Predicate['kind'];

export interface PredicateBase {
  dependencies?: string[];
}
