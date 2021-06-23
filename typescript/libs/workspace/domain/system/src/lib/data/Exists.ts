import { PropertyType } from "../meta/PropertyType";
import { ParameterizablePredicate } from "./ParameterizablePredicate";

export interface Exists extends ParameterizablePredicate {
  propertyType: PropertyType;
}
