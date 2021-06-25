import { PropertyType } from "@allors/workspace/meta/system";

export interface Node {
  kind: 'Like';
  propertyType: PropertyType;
  nodes?: Node[];
}
