import { ContainedIn } from './ContainedIn';
import { And } from './And';
import { Between } from './Between';
import { Contains } from './Contains';
import { Equals } from './Equals';
import { Exists } from './Exists';
import { GreaterThan } from './GreaterThan';
import { Instanceof } from './Instanceof';
import { LessThan } from './LessThan';
import { Like } from './Like';
import { Not } from './Not';
import { Or } from './Or';

export type Predicate = And | Between | ContainedIn | Contains | Equals | Exists | GreaterThan | Instanceof | LessThan | Like | Not | Or ;

export type PredicateKind = Predicate['kind'];

export interface PredicateBase {
  dependencies?: string[];
}
