import { PropertyType } from "@allors/workspace/meta/system";
import { ParameterizablePredicateBase } from "./ParameterizablePredicate";

export interface Exists extends ParameterizablePredicateBase {
  kind: 'Exists';

  propertyType: PropertyType;
}
