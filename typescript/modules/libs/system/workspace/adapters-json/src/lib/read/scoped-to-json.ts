import { Pull as JsonPull } from '@allors/system/common/protocol-json';
import {
  IObject,
  Pull,
  TypeForParameter,
} from '@allors/system/workspace/domain';
import { MetaObject } from '@allors/system/workspace/meta';
import { pullToJson } from '../json/to-json';
import { captureReadMessage } from './capture-read-message';

/** Capture supported native units before queueing without invoking custom JSON hooks. */
export function captureScopedMessage(value: unknown): unknown {
  return captureReadMessage(value, true);
}

export function captureScopedPull(pull: Pull): JsonPull {
  const ancestors = new Set<object>();
  type Kind =
    | 'pull'
    | 'extent'
    | 'predicate'
    | 'result'
    | 'select'
    | 'node'
    | 'sort';
  const fields: Record<Kind, string[]> = {
    pull: [
      'extent',
      'objectType',
      'object',
      'objectId',
      'results',
      'arguments',
    ],
    extent: ['kind', 'objectType', 'sorting', 'predicate', 'operands'],
    predicate: [
      'kind',
      'dependencies',
      'operands',
      'operand',
      'roleType',
      'propertyType',
      'values',
      'paths',
      'parameter',
      'objects',
      'objectIds',
      'objectId',
      'object',
      'extent',
      'value',
      'path',
      'objectType',
    ],
    result: ['select', 'include', 'name', 'skip', 'take'],
    select: ['propertyType', 'ofType', 'next', 'include'],
    node: ['propertyType', 'nodes'],
    sort: ['roleType', 'sortDirection'],
  };
  const extentFields: Record<string, string[]> = {
    Filter: ['kind', 'objectType', 'sorting', 'predicate'],
    Except: ['kind', 'sorting', 'operands'],
    Intersect: ['kind', 'sorting', 'operands'],
    Union: ['kind', 'sorting', 'operands'],
  };
  const predicateFields: Record<string, string[]> = {
    And: ['operands'],
    Or: ['operands'],
    Not: ['operand'],
    Instanceof: ['objectType', 'propertyType', 'parameter'],
    Exists: ['propertyType', 'parameter'],
    Equals: [
      'propertyType',
      'objectId',
      'object',
      'value',
      'path',
      'parameter',
    ],
    Contains: ['propertyType', 'objectId', 'object', 'parameter'],
    ContainedIn: [
      'propertyType',
      'objects',
      'objectIds',
      'parameter',
      'extent',
    ],
    Between: ['roleType', 'values', 'paths', 'parameter'],
    GreaterThan: ['roleType', 'value', 'path', 'parameter'],
    LessThan: ['roleType', 'value', 'path', 'parameter'],
    Like: ['roleType', 'value', 'parameter'],
  };
  const check = (value: unknown, kind: Kind): void => {
    if (value == null) return;
    if (
      typeof value !== 'object' ||
      (Object.getPrototypeOf(value) !== Object.prototype &&
        Object.getPrototypeOf(value) !== null)
    )
      throw new Error(
        'Supply plain inline query declarations without custom query objects.'
      );
    if (ancestors.has(value) || ancestors.size >= 64)
      throw new Error('Supply acyclic query declarations.');
    ancestors.add(value);
    try {
      const object = value as Record<string, unknown>;
      for (const key of Reflect.ownKeys(value)) {
        const descriptor = Object.getOwnPropertyDescriptor(value, key);
        if (
          typeof key !== 'string' ||
          !descriptor?.enumerable ||
          !('value' in descriptor)
        )
          throw new Error(
            'Supply declaration data properties without accessors.'
          );
        if (!fields[kind].includes(key))
          throw new Error(
            `Unsupported ${kind} field ${key}. Supply a supported inline query declaration.`
          );
      }
      const admitted =
        kind === 'extent'
          ? extentFields[object.kind as string]
          : kind === 'predicate' && predicateFields[object.kind as string]
          ? ['kind', 'dependencies', ...predicateFields[object.kind as string]]
          : kind === 'predicate'
          ? undefined
          : fields[kind];
      if (!admitted)
        throw new Error(
          'Unsupported legacy custom query. Supply a supported inline query declaration.'
        );
      for (const key of Object.keys(object)) {
        if (!admitted.includes(key))
          throw new Error(
            `Unsupported ${String(
              object.kind ?? kind
            )} field ${key}. Supply a supported declarative query.`
          );
      }
      if (
        kind === 'predicate' &&
        object.kind === 'Exists' &&
        object.parameter != null
      )
        throw new Error(
          'Exists parameters are unsupported by protected reads. Supply a property-only Exists predicate.'
        );
      const child = (name: string, childKind: Kind, many = false): void => {
        const target = object[name];
        if (target == null) return;
        if (!many) {
          check(target, childKind);
          return;
        }
        if (!Array.isArray(target))
          throw new Error(`Supply an array for ${name}.`);
        for (let index = 0; index < target.length; index++) {
          const descriptor = Object.getOwnPropertyDescriptor(target, index);
          if (!descriptor || !('value' in descriptor))
            throw new Error('Supply dense query arrays without accessors.');
          check(descriptor.value, childKind);
        }
      };
      switch (kind) {
        case 'pull':
          child('extent', 'extent');
          child('results', 'result', true);
          break;
        case 'extent':
          child('predicate', 'predicate');
          child('operands', 'extent', true);
          child('sorting', 'sort', true);
          break;
        case 'predicate':
          child('operand', 'predicate');
          child('operands', 'predicate', true);
          child('extent', 'extent');
          break;
        case 'result':
          child('select', 'select');
          child('include', 'node', true);
          break;
        case 'select':
          child('next', 'select');
          child('include', 'node', true);
          break;
        case 'node':
          child('nodes', 'node', true);
          break;
      }
      if (kind === 'pull' && object.arguments != null) {
        for (const name of Reflect.ownKeys(object.arguments as object)) {
          const descriptor = Object.getOwnPropertyDescriptor(
            object.arguments,
            name
          );
          if (
            typeof name !== 'string' ||
            !descriptor?.enumerable ||
            !('value' in descriptor)
          )
            throw new Error('Supply plain argument values without accessors.');
        }
      }
    } finally {
      ancestors.delete(value);
    }
  };
  if (!pull) throw new Error('Supply a declarative Pull selection.');
  check(pull, 'pull');
  if (
    (pull.objectId != null && pull.objectId <= 0) ||
    (pull.object && pull.object.id <= 0)
  )
    throw new Error(
      'Scoped Pull requires positive database object IDs. Push new objects before reading them.'
    );
  const result = pullToJson({ ...pull, arguments: undefined });
  if (pull.arguments != null) {
    // Argument names are data, including names inherited by ordinary objects.
    const arguments_: Record<string, unknown> = Object.create(null);
    for (const name of Object.keys(pull.arguments)) {
      const value = pull.arguments[name];
      arguments_[name] = Array.isArray(value)
        ? value.map(argumentToJson)
        : argumentToJson(value);
    }
    // The legacy DTO types arguments as strings; scoped validation admits JSON values.
    result.a = arguments_ as JsonPull['a'];
  }
  return result;
}

function argumentToJson(value: TypeForParameter): unknown {
  const id = (value as IObject)?.id;
  if (id != null && id <= 0)
    throw new Error(
      'Scoped query arguments require positive database IDs. Save unsaved objects before referencing them in a query.'
    );
  if (id) return id;
  const tag = (value as MetaObject)?.tag;
  return tag || value;
}
