export interface PushRequestRole {
  /** RelationType */
  t: number;

  /** SetUnitRole */
  u: string;

  /** SetCompositeRole */
  c: number;

  /** AddCompositesRole */
  a: number[];

  /** RemoveCompositesRole */
  r: number[];
}
