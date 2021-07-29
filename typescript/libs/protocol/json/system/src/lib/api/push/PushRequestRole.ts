import { IUnit } from "@allors/workspace/domain/system";

export interface PushRequestRole {
  /** RelationType */
  t: number;

  /** SetUnitRole */
  u?: IUnit;

  /** SetCompositeRole */
  c?: number;

  /** AddCompositesRole */
  a?: number[];

  /** RemoveCompositesRole */
  r?: number[];
}
