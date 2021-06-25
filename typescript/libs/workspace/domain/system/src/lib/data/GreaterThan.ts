import { RoleType } from "@allors/workspace/meta/system";
import { UnitTypes } from "../runtime/Types";
import { ParameterizablePredicateBase } from "./ParameterizablePredicate";

export interface GreaterThan extends ParameterizablePredicateBase {
  kind: 'GreaterThan';
  roleType: RoleType;
  value?: UnitTypes;
  path?: RoleType;
}
