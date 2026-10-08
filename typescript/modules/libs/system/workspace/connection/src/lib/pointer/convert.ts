import { Select } from '../data/select';
import { Node } from './node';
import { Path } from './path';

function toPathsRecursive(node: Node, nodePath: Node[], paths: Path[]): void {
  if (node.nodes?.length > 0) {
    for (const subnode of node.nodes) {
      toPathsRecursive(subnode, [...nodePath, subnode], paths);
    }
  } else {
    const path: Path = {
      propertyType: nodePath[0].propertyType,
      ofType: nodePath[0].ofType,
    };

    let next = path;
    for (let i = 1; i < nodePath.length; i++) {
      next = next.next = {
        propertyType: nodePath[i].propertyType,
        ofType: nodePath[i].ofType,
      };
    }

    paths.push(path);
  }
}

export function toPaths(tree: Node[]): Path[] {
  const paths: Path[] = [];

  for (const node of tree) {
    const nodePath: Node[] = [node];
    toPathsRecursive(node, nodePath, paths);
  }

  return paths;
}

function toNodeRecursive(parent: Node, path: Path): Node {
  const node: Node = {
    propertyType: path.propertyType,
    ofType: path.ofType,
  };

  if (parent) {
    parent.nodes = [node];
  }

  if (path.next) {
    toNodeRecursive(node, path.next);
  }

  return node;
}

export function toNode(path: Path): Node {
  return toNodeRecursive(null, path);
}

function toSelectRecursive(parent: Select, path: Path): Select {
  const select: Select = {
    propertyType: path.propertyType,
    ofType: path.ofType,
  };

  if (parent) {
    parent.next = select;
  }

  if (path.next) {
    toSelectRecursive(select, path.next);
  }

  return select;
}

export function toSelect(path: Path): Select {
  return toSelectRecursive(null, path);
}
