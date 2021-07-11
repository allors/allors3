import { RoleType } from "@allors/workspace/meta/system";
import { IUnit } from "../runtime/Types";
import { ParameterizablePredicateBase } from "./ParameterizablePredicate";

export interface GreaterThan extends ParameterizablePredicateBase {
  kind: 'GreaterThan';
  roleType: RoleType;
  value?: IUnit;
  path?: RoleType;
}
