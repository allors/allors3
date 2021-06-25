import { ObjectType } from '@allors/workspace/meta/system';
import { UnitTypes, Procedure as DataProcedure, Pull as DataPull, IExtent as DataExtent, Predicate as DataPredicate, Sort as DataSort, ParameterTypes } from '@allors/workspace/domain/system';
import { Extent, Predicate, Procedure, Pull, Result, Sort } from '@allors/protocol/json/system';

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
    er: from.extentRef,
    e: extentToJson(from.extent),
    t: from.objectType?.tag,
    o: from.object?.id,
    r: resultsToJson(from.results),
    a: argumentsToJson(from.arguments),
  };
}

export function extentToJson(from: DataExtent): Extent {
  return {
    k: undefined,

    // o: extentsToJson(from.operands),

    // t: objectTypeToJson(from.objectType),

    // p: predicateToJson(from.predicate),

    s: sortingsToJson(from.sorting),
  };
}

export function extentsToJson(from: DataExtent[]): Extent[] {
  return from?.map(extentToJson);
}

export function objectTypeToJson(from: ObjectType): number {
  return from?.tag;
}

export function predicateToJson(from: DataPredicate): Predicate {
  return {
    k: undefined,

    /** AssociationType */
    a: undefined,

    /** RoleType */
    r: undefined,

    /** ObjectType */
    o: undefined,

    /** Parameter */
    p: undefined,

    /** Dependencies */
    d: undefined,

    /** Operand */
    op: undefined,

    /** Operands */
    ops: undefined,

    /** Object */
    ob: undefined,

    /** Objects */
    obs: undefined,

    /** Value */
    v: undefined,

    /** Values */
    vs: undefined,

    /** Path */
    pa: undefined,

    /** Paths */
    pas: undefined,

    /** Extent */
    e: undefined,
  };
}
function sortingsToJson(sorting: DataSort[]): Sort[] {
  return undefined;
}
function resultsToJson(result: any): Result[] {
  return undefined;
}

function argumentsToJson(args: { [name: string]: ParameterTypes; }): { [name: string]: string; } {
  return undefined;
}

