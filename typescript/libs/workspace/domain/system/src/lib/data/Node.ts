import { PropertyType } from "@allors/workspace/meta/system";

export interface Node {
  kind: 'Node';
  propertyType: PropertyType;
  nodes?: Node[];
}
