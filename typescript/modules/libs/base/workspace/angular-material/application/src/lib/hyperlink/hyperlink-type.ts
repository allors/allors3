import { Node, Path } from '@allors/system/workspace/domain';

export interface HyperlinkType {
  paths: Path[];
  tree: Node[];
  label: string;
}
