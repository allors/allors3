import { And } from '../And';
import { Between } from '../Between';
import { ContainedIn } from '../ContainedIn';
import { Contains } from '../Contains';
import { Equals } from '../Equals';
import { Except } from '../Except';
import { Exists } from '../Exists';
import { Extent } from '../Extent';
import { GreaterThan } from '../GreaterThan';
import { Instanceof } from '../Instanceof';
import { Intersect } from '../Intersect';
import { LessThan } from '../LessThan';
import { Like } from '../Like';
import { Select } from '../Select';
import { Node } from '../Node';
import { Not } from '../Not';
import { Or } from '../Or';
import { Pull } from '../Pull';
import { Result } from '../Result';
import { Sort } from '../Sort';
import { Step } from '../Step';
import { Union } from '../Union';
import { Procedure } from '../Procedure';

export interface IVisitor {
  VisitAnd(visited: And): void;

  VisitBetween(visited: Between): void;

  VisitContainedIn(visited: ContainedIn): void;

  VisitContains(visited: Contains): void;

  VisitEquals(visited: Equals): void;

  VisitExcept(visited: Except): void;

  VisitExists(visited: Exists): void;

  VisitExtent(visited: Extent): void;

  VisitSelect(visited: Select): void;

  VisitGreaterThan(visited: GreaterThan): void;

  VisitInstanceOf(visited: Instanceof): void;

  VisitIntersect(visited: Intersect): void;

  VisitLessThan(visited: LessThan): void;

  VisitLike(visited: Like): void;

  VisitNode(visited: Node): void;

  VisitNot(visited: Not): void;

  VisitOr(visited: Or): void;

  VisitPull(visited: Pull): void;

  VisitResult(visited: Result): void;

  VisitSort(visited: Sort): void;

  VisitStep(visited: Step): void;

  VisitUnion(visited: Union): void;

  VisitProcedure(procedure: Procedure): void;
}
