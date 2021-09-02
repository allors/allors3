import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/workspace/meta/system';
import {
  IUnit,
  TypeForParameter,
  Procedure as DataProcedure,
  Pull as DataPull,
  Extent as DataExtent,
  Predicate as DataPredicate,
  Sort as DataSort,
  Result as DataResult,
  Select as DataSelect,
  Node as DataNode,
  IObject,
} from '@allors/workspace/domain/system';
import { Extent, ExtentKind, Predicate, Procedure, Pull, Result, Select, Sort, Node, PredicateKind } from '@allors/protocol/json/system';

export function unitToJson(from: unknown): IUnit {
  if (from == null) {
    return null;
  }

  switch (typeof from) {
    case 'boolean':
    case 'number':
    case 'string':
      return from;
  }

  if (from instanceof Date) {
    return from;
  }

  throw new Error(`Unsupported value: ${from}`);
}

export function procedureToJson(from: DataProcedure): Procedure {
  if (from == null) {
    return null;
  }

  return {
    n: from.name,
    c: collectionToJson(from.collections),
    o: objectsToJson(from.objects),
    v: valuesToJson(from.values),
    p: poolToJson(from.pool),
  };
}

export function pullToJson(from: DataPull): Pull {
  if (from == null) {
    return null;
  }

  return {
    er: extentRefToJson(from.extentRef),
    e: extentToJson(from.extent),
    t: objectTypeToJson(from.objectType),
    o: from.objectId ?? objectToJson(from.object),
    r: resultsToJson(from.results),
    a: argumentsToJson(from.arguments),
  };
}

export function extentToJson(from: DataExtent): Extent {
  if (from == null) {
    return null;
  }

  switch (from.kind) {
    case 'Filter':
      return {
        k: ExtentKind[from.kind],
        t: objectTypeToJson(from.objectType),
        s: sortingsToJson(from.sorting),
        p: predicateToJson(from.predicate),
      };

    case 'Except':
    case 'Intersect':
    case 'Union':
      return {
        k: ExtentKind[from.kind],
        // t: objectTypeToJson(from.objectType),
        s: sortingsToJson(from.sorting),
        o: extentsToJson(from.operands),
      };
  }
}

export function predicateToJson(from: DataPredicate): Predicate {
  if (from == null) {
    return null;
  }

  // TODO: Koen
  switch (from.kind) {
    case 'And':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        ops: predicatesToJson(from.operands),
      };

    case 'Between':
      return {
        k: PredicateKind.Between,
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        vs: from.values?.map((v) => unitToJson(v)),
        pas: pathsToJson(from.paths),
        p: from.parameter,
      };

    case 'ContainedIn':
      return {
        k: PredicateKind.ContainedIn,
        d: from.dependencies,
        a: asAssociationTypeToJson(from.propertyType),
        r: asRoleTypeToJson(from.propertyType),
        vs: from.objects?.map((v) => v.id),
        p: from.parameter,
        e: extentToJson(from.extent),
      };

    case 'Contains':
      return {
        k: PredicateKind.Contains,
        d: from.dependencies,
        a: asAssociationTypeToJson(from.propertyType),
        r: asRoleTypeToJson(from.propertyType),
        ob: from.object?.id,
        p: from.parameter,
      };

    case 'Equals':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        a: asAssociationTypeToJson(from.propertyType),
        r: asRoleTypeToJson(from.propertyType),
        ob: from.object?.id,
        v: unitToJson(from.value),
        pa: roleTypeToJson(from.path),
        p: from.parameter,
      };

    case 'Exists':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        a: asAssociationTypeToJson(from.propertyType),
        r: asRoleTypeToJson(from.propertyType),
        p: from.parameter,
      };

    case 'GreaterThan':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        v: unitToJson(from.value),
        pa: roleTypeToJson(from.path),
        p: from.parameter,
      };

    case 'Instanceof':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        o: objectTypeToJson(from.objectType),
        a: asAssociationTypeToJson(from.propertyType),
        r: asRoleTypeToJson(from.propertyType),
      };

    case 'LessThan':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        v: unitToJson(from.value),
        pa: roleTypeToJson(from.path),
        p: from.parameter,
      };

    case 'Like':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        v: unitToJson(from.value),
        p: from.parameter,
      };

    case 'Not':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        op: predicateToJson(from.operand),
      };

    case 'Or':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        ops: predicatesToJson(from.operands),
      };
  }

  throw new Error('Not implemented yet');
}

function sortingsToJson(from: DataSort[]): Sort[] {
  if (from == null) {
    return null;
  }

  return undefined;
}

function resultToJson(from: DataResult): Result {
  if (from == null) {
    return null;
  }

  return {
    r: from.selectRef,
    s: selectToJson(from.select),
    n: from.name,
    k: from.skip,
    t: from.take,
  };
}

function selectToJson(from: DataSelect): Select {
  if (from == null) {
    return null;
  }

  return {
    a: asAssociationTypeToJson(from.propertyType),
    r: asRoleTypeToJson(from.propertyType),
    n: selectToJson(from.next),
    i: nodesToJson(from.include),
  };
}

function nodeToJson(from: DataNode): Node {
  if (from == null) {
    return null;
  }

  return {
    a: asAssociationTypeToJson(from.propertyType),
    r: asRoleTypeToJson(from.propertyType),
    n: nodesToJson(from.nodes),
  };
}

function argumentsToJson(from: { [name: string]: TypeForParameter }): { [name: string]: string } {
  if (from == null) {
    return null;
  }

  return undefined;
}

export function extentsToJson(from: DataExtent[]): Extent[] {
  return from?.map(extentToJson);
}

export function predicatesToJson(from: DataPredicate[]): Predicate[] {
  return from?.map(predicateToJson);
}

export function resultsToJson(from: DataResult[]): Result[] {
  return from?.map(resultToJson);
}

export function nodesToJson(from: DataNode[]): Node[] {
  return from?.map(nodeToJson);
}

export function objectTypeToJson(from: ObjectType): number {
  return from?.tag;
}

export function roleTypeToJson(from: RoleType): number {
  return from?.relationType.tag;
}

export function asAssociationTypeToJson(from: PropertyType): number {
  if (from?.isAssociationType) {
    return (from as AssociationType).relationType.tag;
  }

  return null;
}

export function asRoleTypeToJson(from: PropertyType): number {
  if (from?.isRoleType) {
    return (from as RoleType).relationType.tag;
  }

  return null;
}

export function objectToJson(from: IObject): number {
  return from?.id;
}

export function extentRefToJson(from: string): string {
  return from;
}

export function collectionToJson(from: { [name: string]: IObject[] } | Map<string, IObject[]>): { [name: string]: number[] } {
  return map<IObject[], number[]>(from, (v) => v.map((w) => w.id));
}

export function objectsToJson(from: { [name: string]: IObject } | Map<string, IObject>): { [name: string]: number } {
  return map<IObject, number>(from, (v) => v.id);
}

export function valuesToJson(from: { [name: string]: IUnit } | Map<string, IUnit>): { [name: string]: IUnit } {
  return map<IUnit, IUnit>(from, (v) => v);
}

export function poolToJson(from: Map<IObject, number>): number[][] {
  if (from == null) {
    return null;
  }

  return Array.from(from, ([obj, version]) => [obj.id, version]);
}

export function pathsToJson(from: RoleType[]): number[] {
  return from?.map((v) => v.relationType.tag);
}

function map<T1, T2>(from: { [name: string]: T1 } | Map<string, T1>, fn: (T1) => T2): { [name: string]: T2 } {
  if (from == null) {
    return null;
  }

  const result = {};
  if (from instanceof Map) {
    for (const [key, value] of from) {
      result[key] = fn(value);
    }
  } else {
    for (const key of Object.keys(from)) {
      result[key] = fn(from[key]);
    }
  }

  return result;
}
