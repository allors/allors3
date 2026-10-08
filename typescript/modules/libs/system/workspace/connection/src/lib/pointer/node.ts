import { Composite, PropertyType } from '@allors/system/workspace/meta';

export interface Node {
  propertyType: PropertyType;
  ofType?: Composite;
  nodes?: Node[];
}

function resolveLeafs(node: Node, results: Set<Node>): void {
  if (node.nodes.length > 0) {
    for (const child of node.nodes) {
      resolveLeafs(child, results);
    }
  } else {
    results.add(node);
  }
}

export function nodeLeafs(node: Node): Set<Node> {
  const results: Set<Node> = new Set();
  resolveLeafs(node, results);
  return results;
}
