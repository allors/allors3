import { PropertyType } from "../meta/PropertyType";

export interface Node {
  propertyType: PropertyType;
  nodes?: Node[];
}
