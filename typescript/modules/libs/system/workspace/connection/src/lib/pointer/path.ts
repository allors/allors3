import { Composite, PropertyType } from '@allors/system/workspace/meta';

export interface Path {
  propertyType: PropertyType;
  ofType?: Composite;
  next?: Path;
}

export function isPath(path: unknown): path is Path {
  return (path as Path).propertyType != null;
}

export function pathLeaf(path: Path): Path {
  let next = path;
  while (next.next) {
    next = next.next;
  }

  return next;
}

export function pathObjectType(path: Path): Composite {
  const leaf = pathLeaf(path);
  return leaf.ofType ?? (leaf.propertyType.objectType as Composite);
}

export function pathTag(path: Path): string {
  let tag: string;

  let next = path;
  while (next.next) {
    tag = `${tag ? `_${tag}` : tag}${next.propertyType.relationType.tag}`;
    next = next.next;
  }

  return tag;
}
