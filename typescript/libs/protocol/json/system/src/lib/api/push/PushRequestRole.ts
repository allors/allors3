import { UnitTypes } from "@allors/workspace/system";

export interface PushRequestRole {
  /** RelationType */
  t: number;

  /** SetUnitRole */
  u?: UnitTypes;

  /** SetCompositeRole */
  c?: number;

  /** AddCompositesRole */
  a?: number[];

  /** RemoveCompositesRole */
  r?: number[];
}
