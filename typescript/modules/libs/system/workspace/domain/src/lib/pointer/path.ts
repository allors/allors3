import {
  AssociationType,
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import { IObject } from '../iobject';
import { IStrategy } from '../istrategy';

export interface Path {
  propertyType: PropertyType;
  ofType?: Composite;
  next?: Path;
}

function getComposite(
  strategy: IStrategy,
  propertyType: PropertyType,
  ofType: Composite,
  skipMissing?: boolean
): IObject {
  const composite = propertyType.isRoleType
    ? strategy.getCompositeRole(propertyType as RoleType, skipMissing)
    : strategy.getCompositeAssociation(propertyType as AssociationType);

  if (composite == null || ofType == null) {
    return composite;
  }

  return ofType.isAssignableFrom(composite.strategy.cls) ? composite : null;
}

function getComposites(
  strategy: IStrategy,
  propertyType: PropertyType,
  ofType: Composite,
  skipMissing?: boolean
): Readonly<IObject[]> {
  const composites = propertyType.isRoleType
    ? strategy.getCompositesRole(propertyType as RoleType, skipMissing)
    : strategy.getCompositesAssociation(propertyType as AssociationType);

  if (composites == null || ofType == null) {
    return composites;
  }

  return composites.filter((v) => ofType.isAssignableFrom(v.strategy.cls));
}

function resolveRecursive(
  object: IObject,
  path: Path,
  results: Set<IObject>,
  skipMissing?: boolean
): void {
  if (path.propertyType.isOne) {
    const resolved = getComposite(
      object.strategy,
      path.propertyType,
      path.ofType,
      skipMissing
    );
    if (resolved != null) {
      if (path.next) {
        resolveRecursive(resolved, path.next, results, skipMissing);
      } else {
        results.add(resolved);
      }
    }
  } else {
    const resolveds = getComposites(
      object.strategy,
      path.propertyType,
      path.ofType,
      skipMissing
    );
    if (resolveds != null) {
      if (path.next) {
        for (const resolved of resolveds) {
          resolveRecursive(resolved, path.next, results, skipMissing);
        }
      } else {
        for (const resolved of resolveds) {
          results.add(resolved);
        }
      }
    }
  }
}

export function isPath(path: unknown): path is Path {
  return (path as Path).propertyType !== undefined;
}

export function pathResolve(
  obj: IObject,
  path: Path,
  skipMissing?: boolean
): Set<IObject> {
  const results: Set<IObject> = new Set();
  resolveRecursive(obj, path, results, skipMissing);
  return results;
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
