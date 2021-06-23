import { RoleType } from "../meta/RoleType";
import { UnitTypes } from "../runtime/Types";
import { ParameterizablePredicate } from "./ParameterizablePredicate";

export interface GreaterThan extends ParameterizablePredicate {
  roleType: RoleType;
  value?: UnitTypes;
  path?: RoleType;
}
