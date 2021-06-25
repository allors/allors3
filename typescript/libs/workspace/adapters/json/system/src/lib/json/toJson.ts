import { AssociationType, ObjectType, PropertyType, RoleType } from '@allors/workspace/meta/system';
import {
  UnitTypes,
  Procedure as DataProcedure,
  Pull as DataPull,
  Extent as DataExtent,
  Predicate as DataPredicate,
  Sort as DataSort,
  Result as DataResult,
  Select as DataSelect,
  Step as DataStep,
  Node as DataNode,
  ParameterTypes,
  IObject,
} from '@allors/workspace/domain/system';
import { Extent, ExtentKind, Predicate, Procedure, Pull, Result, Select, Sort, Step, Node, PredicateKind } from '@allors/protocol/json/system';

export function unitToJson(value: unknown): UnitTypes {
  switch (typeof value) {
    case 'boolean':
    case 'number':
    case 'string':
      return value;
  }

  if (value instanceof Date) {
    return value;
  }

  throw new Error(`Unsupported value: ${value}`);
}

export function procedureToJson(from: DataProcedure): Procedure {
  return undefined;
}

export function pullToJson(from: DataPull): Pull {
  return {
    er: extentRefToJson(from.extentRef),
    e: extentToJson(from.extent),
    t: objectTypeToJson(from.objectType),
    o: objectToJson(from.object),
    r: resultsToJson(from.results),
    a: argumentsToJson(from.arguments),
  };
}

export function extentToJson(from: DataExtent): Extent {
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
  switch (from.kind) {
    case 'And':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        ops: predicatesToJson(from.operands),
      };

    case 'GreaterThan':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        v: unitToJson(from.value),
        pa: roleTypeToJson(from.path),
      };

    case 'LessThan':
      return {
        k: PredicateKind[from.kind],
        d: from.dependencies,
        r: roleTypeToJson(from.roleType),
        v: unitToJson(from.value),
        pa: roleTypeToJson(from.path),
      };
  }
}

function sortingsToJson(sorting: DataSort[]): Sort[] {
  return undefined;
}
function resultToJson(from: DataResult): Result {
  return {
    r: from.selectRef,
    s: selectToJson(from.select),
    n: from.name,
    k: from.skip,
    t: from.take,
  };
}

function selectToJson(from: DataSelect): Select {
  return {
    s: stepToJson(from.step),
    i: nodesToJson(from.include),
  };
}

function stepToJson(from: DataStep): Step {
  return {
    a: asAssociationTypeToJson(from.propertyType),
    r: asRoleTypeToJson(from.propertyType),
    n: stepToJson(from.next),
    i: nodesToJson(from.include),
  };
}

function nodeToJson(from: DataNode): Node {
  return {
    a: asAssociationTypeToJson(from.propertyType),
    r: asRoleTypeToJson(from.propertyType),
    n: nodesToJson(from.nodes),
  };
}

function argumentsToJson(args: { [name: string]: ParameterTypes }): { [name: string]: string } {
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
  if (from.isAssociationType) {
    return (from as AssociationType).relationType.tag;
  }
}

export function asRoleTypeToJson(from: PropertyType): number {
  if (from.isRoleType) {
    return (from as RoleType).relationType.tag;
  }
}

export function objectToJson(from: IObject): number {
  return from?.id;
}

export function extentRefToJson(from: string): string {
  return from;
}
