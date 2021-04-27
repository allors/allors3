export interface Extent {
  /** Kind */
  k: ExtentKind;

  /** Operands */
  o: Extent[];

  /** ObjectType */
  t: number;

  /** Predicate */
  p: Predicate;

  /** Sorting */
  s: Sort[];
}
