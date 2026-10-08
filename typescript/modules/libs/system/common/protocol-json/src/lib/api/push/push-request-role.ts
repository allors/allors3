export interface PushRequestRole {
  /** RelationType */
  t: string;

  /** SetUnitRole */
  u?: unknown;

  /** SetCompositeRole */
  c?: number;

  /** AddCompositesRole */
  a?: number[];

  /** RemoveCompositesRole */
  r?: number[];
}
