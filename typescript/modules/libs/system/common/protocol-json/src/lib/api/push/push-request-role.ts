import { IUnit } from '@allors/system/workspace/domain';

export interface PushRequestRole {
  /** RelationType */
  t: string;

  /** SetUnitRole */
  u?: IUnit;

  /** SetCompositeRole */
  c?: number;

  /** AddCompositesRole */
  a?: number[];

  /** RemoveCompositesRole */
  r?: number[];
}
