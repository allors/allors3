import { UnitType } from "@allors/workspace/domain/system";

export interface PushRequestRole {
  /** RelationType */
  t: number;

  /** SetUnitRole */
  u?: UnitType;

  /** SetCompositeRole */
  c?: number;

  /** AddCompositesRole */
  a?: number[];

  /** RemoveCompositesRole */
  r?: number[];
}
