import { Node, Path } from '@allors/system/workspace/domain';

export interface LinkType {
  paths: Path[];
  tree: Node[];
  label: string;
}
