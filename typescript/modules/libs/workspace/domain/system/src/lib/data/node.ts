import { AssociationType, Composite, PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';
import { IStrategy } from '../istrategy';

export interface Node {
  propertyType: PropertyType;
  ofType?: Composite;
  nodes?: Node[];
}

function getComposite(strategy: IStrategy, propertyType: PropertyType, ofType: Composite, skipMissing?: boolean): IObject {
  const composite = propertyType.isRoleType ? strategy.getCompositeRole(propertyType as RoleType, skipMissing) : strategy.getCompositeAssociation(propertyType as AssociationType);

  if (composite == null || ofType == null) {
    return composite;
  }

  return ofType.isAssignableFrom(composite.strategy.cls) ? composite : null;
}

function getComposites(strategy: IStrategy, propertyType: PropertyType, ofType: Composite, skipMissing?: boolean): Readonly<IObject[]> {
  const composites = propertyType.isRoleType ? strategy.getCompositesRole(propertyType as RoleType, skipMissing) : strategy.getCompositesAssociation(propertyType as AssociationType);

  if (composites == null || ofType == null) {
    return composites;
  }

  return composites.filter((v) => ofType.isAssignableFrom(v.strategy.cls));
}

function resolveRecursive(obj: IObject, node: Node, results: Set<IObject>, skipMissing?: boolean): void {
  if (node.propertyType.isOne) {
    const resolved = getComposite(obj.strategy, node.propertyType, node.ofType, skipMissing);
    if (resolved != null) {
      if (node.nodes?.length > 0) {
        for (const subNode of node.nodes) {
          resolveRecursive(resolved, subNode, results);
        }
      } else {
        results.add(resolved);
      }
    }
  } else {
    const resolveds = getComposites(obj.strategy, node.propertyType, node.ofType, skipMissing);
    if (resolveds != null) {
      if (node.nodes?.length > 0) {
        for (const resolved of resolveds) {
          for (const subNode of node.nodes) {
            resolveRecursive(resolved, subNode, results);
          }
        }
      } else {
        for (const resolved of resolveds) {
          results.add(resolved);
        }
      }
    }
  }
}

export function resolve(obj: IObject, node: Node, skipMissing?: boolean): Set<IObject> {
  const results: Set<IObject> = new Set();
  resolveRecursive(obj, node, results, skipMissing);
  return results;
}
