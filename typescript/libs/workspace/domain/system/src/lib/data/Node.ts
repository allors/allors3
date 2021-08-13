import { PropertyType } from "@allors/workspace/meta/system";
import { IObject } from "../IObject";

export interface Node {
  kind: 'Node';
  propertyType: PropertyType;
  nodes?: Node[];
}
