import { RoleType } from "@allors/workspace/meta/system";
import { UnitType } from "../runtime/Types";
import { ParameterizablePredicateBase } from "./ParameterizablePredicate";

export interface GreaterThan extends ParameterizablePredicateBase {
  kind: 'GreaterThan';
  roleType: RoleType;
  value?: UnitType;
  path?: RoleType;
}
