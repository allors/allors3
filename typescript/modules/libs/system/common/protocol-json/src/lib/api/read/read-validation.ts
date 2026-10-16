import {
  InvokeReadRequest,
  InvokeReadReceipt,
  InvokeReadResponse,
} from '../invoke-read/invoke-read';
import {
  PushReadRequest,
  PushReadReceipt,
  PushReadResponse,
  PushReadChanged,
} from '../push-read/push-read';
import { ReadPull } from './read-pull';
import { ReadRecordHeader } from './read-record-header';
import { ReadRequest } from './read-request';
import { ReadResponse } from './read-response';
import { ReadScope } from './read-scope';
import { ReadSecurityHeader } from './read-security-header';

type Check = (value: unknown, path: string) => void;
type Bag = { [name: string]: unknown };

/** Decode raw JSON before duplicate or unsupported fields can be discarded. */
export function decodeReadRequest(json: string): ReadRequest {
  const value: unknown = parse(json);
  validateReadRequest(value);
  return value;
}

/** Validate a response against the exact request whose hints it references. */
export function decodeReadResponse(
  json: string,
  request: ReadRequest
): ReadResponse {
  const value: unknown = parse(json);
  validateReadResponse(value, request);
  return value;
}

/** Validate an already decoded tree; use decodeReadRequest to also detect duplicate JSON keys. */
export function validateReadRequest(
  value: unknown
): asserts value is ReadRequest {
  finiteTree(value, '$request');
  shape(value, '$request', {
    v: oneOf(1),
    i: text,
    sc: optional(scope),
    x: optional(context),
    k: oneOf('discover', 'read'),
    p: pull,
    h: optional(cache),
  });
  const request = value as ReadRequest;
  if (request.sc && !sameContext(request.sc.x, request.x)) {
    invalid('$request.sc.x', 'Use the same context as the request.');
  }
  if (request.h) {
    const { s, r, p } = request.h;
    if (!request.sc && (s.length || r.length || p.length)) {
      invalid(
        '$request.sc',
        'Supply the expected scope before advertising cache entries.'
      );
    }
    securityKeys(s, '$request.h.s');
    provenance(r, s, '$request.h.r');
    unique(
      r.map((record) => recordKey(record, s)),
      '$request.h.r'
    );
    unique(p, '$request.h.p');
  }
}

/** Structural and table validation cannot establish authentication or cache possession. */
export function validateReadResponse(
  value: unknown,
  request: ReadRequest
): asserts value is ReadResponse {
  validateReadRequest(request);
  const root = object(value, '$response');
  oneOf('discovery', 'complete', 'error')(root.k, '$response.k');
  if (root.k === 'error') {
    shape(value, '$response', {
      i: text,
      sc: optional(scope),
      k: oneOf('error'),
      e: array(error),
    });
  } else if (root.k === 'discovery') {
    shape(value, '$response', {
      i: text,
      sc: scope,
      k: oneOf('discovery'),
      p: array(recordHeader),
      s: array(securityHeader),
    });
  } else {
    shape(value, '$response', {
      i: text,
      sc: scope,
      k: oneOf('complete'),
      l: array(input),
      c: dictionary(array(id)),
      o: dictionary(nullable(id)),
      p: array(record),
      s: array(security),
      pm: array(permission),
    });
  }
  const response = value as ReadResponse;
  if (response.i !== request.i) {
    invalid('$response.i', 'Echo the original request ID.');
  }
  if (response.k === 'error') {
    if (!response.e.length) {
      invalid('$response.e', 'Supply at least one actionable error.');
    }
    response.e.forEach((entry, i) => {
      if (entry.o) unique(entry.o, `$response.e[${i}].o`);
      if (entry.i != null)
        reference(entry.i, request.p.l, `$response.e[${i}].i`);
    });
    return;
  }
  if (response.k !== (request.k === 'discover' ? 'discovery' : 'complete')) {
    invalid(
      '$response.k',
      'Return the success kind corresponding to the request kind.'
    );
  }
  if (!sameContext(response.sc.x, request.x)) {
    invalid('$response.sc.x', 'Return the original request context.');
  }
  if (request.sc && scopeKey(response.sc) !== scopeKey(request.sc)) {
    invalid(
      '$response.sc',
      'Return the expected scope or an explicit failure.'
    );
  }
  unique(
    response.p.map((entry) => entry.i),
    '$response.p'
  );
  securityKeys(response.s, '$response.s');
  const referencedSecurity = provenance(response.p, response.s, '$response.p');
  if (referencedSecurity.size !== response.s.length) {
    invalid(
      '$response.s',
      'Return only security entries referenced by the record pool.'
    );
  }
  if (response.k === 'discovery') return;

  if (response.l.length !== request.p.l.length) {
    invalid(
      '$response.l',
      'Return exactly one outcome for each request selection.'
    );
  }
  unique(
    response.l.map((entry) => entry.i),
    '$response.l'
  );
  response.l.forEach((entry, i) => {
    const selection = reference(entry.i, request.p.l, `$response.l[${i}].i`);
    if ((entry.o ?? null) !== (selection.o ?? null)) {
      invalid(
        `$response.l[${i}].o`,
        'Echo the target of the corresponding selection.'
      );
    }
    if (entry.s === 'unavailable' && selection.o == null) {
      invalid(
        `$response.l[${i}].s`,
        'Use unavailable only for an explicit object target.'
      );
    }
  });
  const pool = new Set(response.p.map((entry) => entry.i));
  for (const [name, members] of Object.entries(response.c)) {
    unique(members, `$response.c.${name}`);
    members.forEach((member) =>
      memberOfPool(member, pool, `$response.c.${name}`)
    );
  }
  for (const [name, member] of Object.entries(response.o)) {
    if (member != null) memberOfPool(member, pool, `$response.o.${name}`);
  }
  response.p.forEach((entry, i) => {
    if (entry.p.r != null) {
      const cached = reference(
        entry.p.r,
        request.h?.r,
        `$response.p[${i}].p.r`
      );
      if (recordKey(entry, response.s) !== recordKey(cached, request.h.s)) {
        invalid(
          `$response.p[${i}].p.r`,
          'Reuse only a record with identical ID, class, version and security provenance.'
        );
      }
    }
  });
  unique(
    response.pm.map((entry) => entry.i),
    '$response.pm'
  );
  const permissions = new Set(response.pm.map((entry) => entry.i));
  const requiredPermissions = new Set<number>();
  let reusedSecurity = false;
  response.s.forEach((entry, i) => {
    if (entry.p.r != null) {
      reusedSecurity = true;
      const cached = reference(
        entry.p.r,
        request.h?.s,
        `$response.s[${i}].p.r`
      );
      if (!cached.h || securityKey(cached) !== securityKey(entry)) {
        invalid(
          `$response.s[${i}].p.r`,
          'Reuse only the exact security kind, ID and version advertised with a held body.'
        );
      }
    } else {
      unique(entry.p.p, `$response.s[${i}].p.p`);
      entry.p.p.forEach((permissionId) => {
        if (!permissions.has(permissionId)) {
          invalid(
            `$response.s[${i}].p.p`,
            'Include every referenced permission descriptor in pm.'
          );
        }
        requiredPermissions.add(permissionId);
      });
    }
  });
  if (!reusedSecurity && requiredPermissions.size !== permissions.size) {
    invalid(
      '$response.pm',
      'Return only permission descriptors referenced by the security closure.'
    );
  }
  response.pm.forEach((entry, i) => {
    if (
      entry.p.r != null &&
      reference(entry.p.r, request.h?.p, `$response.pm[${i}].p.r`) !== entry.i
    ) {
      invalid(
        `$response.pm[${i}].p.r`,
        'Reuse the request permission entry with the same ID.'
      );
    }
  });
}

function invalid(path: string, remedy: string): never {
  throw new Error(`${path}: ${remedy}`);
}

function object(value: unknown, path: string): Bag {
  if (value == null || typeof value !== 'object' || Array.isArray(value)) {
    invalid(path, 'Supply a JSON object.');
  }
  const prototype = Object.getPrototypeOf(value);
  if (prototype !== null && prototype !== Object.prototype) {
    invalid(path, 'Supply a plain JSON object.');
  }
  return value as Bag;
}

function shape(
  value: unknown,
  path: string,
  checks: { [name: string]: Check }
): Bag {
  const result = object(value, path);
  for (const name of Object.keys(result)) {
    if (!Object.prototype.hasOwnProperty.call(checks, name)) {
      invalid(
        `${path}.${name}`,
        'Remove the unsupported field and supply a supported inline query declaration.'
      );
    }
  }
  for (const [name, check] of Object.entries(checks)) {
    check(
      Object.prototype.hasOwnProperty.call(result, name)
        ? result[name]
        : undefined,
      `${path}.${name}`
    );
  }
  return result;
}

function optional(check: Check): Check {
  return (value, path) => {
    if (value != null) check(value, path);
  };
}

function nullable(check: Check): Check {
  return (value, path) => {
    if (value !== null) check(value, path);
  };
}

function array(check: Check): Check {
  return (value, path) => {
    if (!Array.isArray(value))
      invalid(path, 'Supply an array, using [] when empty.');
    for (let i = 0; i < value.length; i++) check(value[i], `${path}[${i}]`);
  };
}

function dictionary(check: Check): Check {
  return (value, path) => {
    for (const [name, entry] of Object.entries(object(value, path))) {
      check(entry, `${path}.${name}`);
    }
  };
}

function oneOf(...allowed: unknown[]): Check {
  return (value, path) => {
    if (!allowed.includes(value))
      invalid(path, `Supply one of ${allowed.join(', ')}.`);
  };
}

function text(value: unknown, path: string): void {
  if (typeof value !== 'string' || !/[^\s\u0085]/.test(value))
    invalid(path, 'Supply a nonempty string.');
}

function context(value: unknown, path: string): void {
  if (typeof value !== 'string')
    invalid(path, 'Supply a string or null context.');
}

function integer(
  value: unknown,
  path: string,
  minimum: number,
  maximum: number
): void {
  if (
    typeof value !== 'number' ||
    !Number.isSafeInteger(value) ||
    value < minimum ||
    value > maximum
  ) {
    invalid(
      path,
      `Supply an exactly representable integer from ${minimum} through ${maximum}.`
    );
  }
}

function id(value: unknown, path: string): void {
  integer(value, path, 1, Number.MAX_SAFE_INTEGER);
}

function version(value: unknown, path: string): void {
  integer(value, path, 0, Number.MAX_SAFE_INTEGER);
}

function index(value: unknown, path: string): void {
  integer(value, path, 0, 2147483647);
}

function unit(value: unknown, path: string): void {
  if (
    value !== null &&
    typeof value !== 'string' &&
    typeof value !== 'boolean' &&
    !(typeof value === 'number' && Number.isFinite(value))
  ) {
    invalid(path, 'Supply null, a string, a Boolean, or a finite number.');
  }
}

function jsonData(value: unknown, path: string): void {
  if (Array.isArray(value)) array(jsonData)(value, path);
  else if (value != null && typeof value === 'object')
    dictionary(jsonData)(value, path);
  else unit(value, path);
}

function scope(value: unknown, path: string): void {
  shape(value, path, {
    p: text,
    u: id,
    w: text,
    r: text,
    x: optional(context),
  });
}

function pull(value: unknown, path: string): void {
  shape(value, path, { l: array(selection), d: array(dependency) });
}

function selection(value: unknown, path: string): void {
  shape(value, path, {
    e: optional(extent),
    t: optional(text),
    o: optional(id),
    r: optional(array(result)),
    a: optional(dictionary(jsonData)),
  });
}

function dependency(value: unknown, path: string): void {
  const entry = shape(value, path, {
    o: text,
    a: optional(text),
    r: optional(text),
  });
  if ((entry.a != null) === (entry.r != null))
    invalid(path, 'Supply exactly one association or role dependency.');
}

function extent(value: unknown, path: string): void {
  shape(value, path, {
    k: oneOf(1, 2, 3, 4),
    o: optional(array(extent)),
    t: optional(text),
    p: optional(predicate),
    s: optional(array(sort)),
  });
}

function predicate(value: unknown, path: string): void {
  shape(value, path, {
    k: oneOf(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12),
    a: optional(text),
    r: optional(text),
    o: optional(text),
    p: optional(text),
    d: optional(array(text)),
    op: optional(predicate),
    ops: optional(array(predicate)),
    ob: optional(id),
    obs: optional(array(id)),
    v: optional(unit),
    vs: optional(array(unit)),
    pa: optional(text),
    pas: optional(array(text)),
    e: optional(extent),
  });
}

function result(value: unknown, path: string): void {
  shape(value, path, {
    s: optional(select),
    i: optional(array(node)),
    n: optional(text),
    k: optional(index),
    t: optional(index),
  });
}

function select(value: unknown, path: string): void {
  shape(value, path, {
    a: optional(text),
    r: optional(text),
    o: optional(text),
    n: optional(select),
    i: optional(array(node)),
  });
}

function node(value: unknown, path: string): void {
  shape(value, path, {
    a: optional(text),
    r: optional(text),
    n: optional(array(node)),
  });
}

function sort(value: unknown, path: string): void {
  shape(value, path, { r: optional(text), d: optional(oneOf(1, 2)) });
}

function cache(value: unknown, path: string): void {
  shape(value, path, {
    s: array(securityHint),
    r: array(recordHeader),
    p: array(id),
  });
}

function securityHeader(value: unknown, path: string): void {
  shape(value, path, { k: oneOf('grant', 'revocation'), i: id, v: version });
}

function securityHint(value: unknown, path: string): void {
  shape(value, path, {
    k: oneOf('grant', 'revocation'),
    i: id,
    v: version,
    h: oneOf(true, false),
  });
}

function security(value: unknown, path: string): void {
  shape(value, path, {
    k: oneOf('grant', 'revocation'),
    i: id,
    v: version,
    p: securityPayload,
  });
}

function recordHeader(value: unknown, path: string): void {
  shape(value, path, {
    i: id,
    c: text,
    v: version,
    g: array(index),
    r: array(index),
  });
}

function record(value: unknown, path: string): void {
  shape(value, path, {
    i: id,
    c: text,
    v: version,
    g: array(index),
    r: array(index),
    p: recordPayload,
  });
}

function recordPayload(value: unknown, path: string): void {
  const entry = shape(value, path, {
    ro: optional(array(role)),
    r: optional(index),
  });
  if ((entry.ro != null) === (entry.r != null))
    invalid(path, 'Supply exactly one full role body or reuse index.');
  if (entry.ro)
    unique(
      (entry.ro as Bag[]).map((item) => item.t),
      `${path}.ro`
    );
}

function role(value: unknown, path: string): void {
  const entry = shape(value, path, {
    t: text,
    c: optional(array(id)),
    o: optional(id),
    v: optional(unit),
  });
  if ([entry.c, entry.o, entry.v].filter((item) => item != null).length > 1)
    invalid(path, 'Supply at most one collection, object, or unit value.');
  if (entry.c) {
    const members = entry.c as number[];
    for (let i = 1; i < members.length; i++) {
      if (members[i - 1] >= members[i])
        invalid(`${path}.c`, 'Supply sorted, distinct positive object IDs.');
    }
  }
}

function securityPayload(value: unknown, path: string): void {
  const entry = shape(value, path, {
    p: optional(array(id)),
    r: optional(index),
  });
  if ((entry.p != null) === (entry.r != null))
    invalid(path, 'Supply exactly one full permission-ID body or reuse index.');
}

function permission(value: unknown, path: string): void {
  shape(value, path, { i: id, p: permissionPayload });
}

function permissionPayload(value: unknown, path: string): void {
  const entry = shape(value, path, {
    c: optional(text),
    o: optional(oneOf(1, 2, 4, 8)),
    t: optional(text),
    r: optional(index),
  });
  if (entry.r != null) {
    if (entry.c != null || entry.o != null || entry.t != null)
      invalid(
        path,
        'Supply a reuse index without a full permission descriptor.'
      );
  } else {
    text(entry.c, `${path}.c`);
    oneOf(1, 2, 4, 8)(entry.o, `${path}.o`);
    if (entry.o === 1) {
      if (entry.t != null)
        invalid(`${path}.t`, 'Omit the operand of a Create permission.');
    } else text(entry.t, `${path}.t`);
  }
}

function input(value: unknown, path: string): void {
  shape(value, path, {
    i: index,
    o: optional(id),
    s: oneOf('resolved', 'unavailable'),
  });
}

function error(value: unknown, path: string): void {
  shape(value, path, {
    c: text,
    m: text,
    o: optional(array(id)),
    i: optional(index),
  });
}

function unique<T>(values: T[], path: string): void {
  if (new Set(values).size !== values.length)
    invalid(path, 'Remove duplicate entries.');
}

function reference<T>(value: number, table: T[], path: string): T {
  if (!table || value >= table.length)
    invalid(path, 'Reference an existing entry in the corresponding table.');
  return table[value];
}

function securityKey(entry: ReadSecurityHeader): string {
  return JSON.stringify([entry.k, entry.i, entry.v]);
}

function securityKeys(entries: ReadSecurityHeader[], path: string): void {
  unique(entries.map(securityKey), path);
}

function provenance(
  records: ReadRecordHeader[],
  securities: ReadSecurityHeader[],
  path: string
): Set<number> {
  const referenced = new Set<number>();
  records.forEach((entry, i) => {
    for (const [name, kind] of [
      ['g', 'grant'],
      ['r', 'revocation'],
    ] as const) {
      unique(entry[name], `${path}[${i}].${name}`);
      entry[name].forEach((position) => {
        const security = reference(
          position,
          securities,
          `${path}[${i}].${name}`
        );
        if (security.k !== kind)
          invalid(`${path}[${i}].${name}`, `Reference only ${kind} entries.`);
        referenced.add(position);
      });
    }
  });
  return referenced;
}

function recordKey(
  entry: ReadRecordHeader,
  securities: ReadSecurityHeader[]
): string {
  return JSON.stringify([
    entry.i,
    entry.c,
    entry.v,
    entry.g.map((i) => securityKey(securities[i])).sort(),
    entry.r.map((i) => securityKey(securities[i])).sort(),
  ]);
}

function sameContext(left: string, right: string): boolean {
  return (left ?? null) === (right ?? null);
}

function scopeKey(entry: ReadScope): string {
  return JSON.stringify([entry.p, entry.u, entry.w, entry.r, entry.x ?? null]);
}

function memberOfPool(value: number, pool: Set<number>, path: string): void {
  if (!pool.has(value))
    invalid(path, 'Include every named output object in the record pool.');
}

function parse(
  json: string,
  receiptOnly: boolean | 'changed' | 'discovery' = false
): unknown {
  const skip = (path: string): boolean => {
    const part = /^\$\.(changed|discovery)(?:\.|\[|$)/.exec(path)?.[1];
    return (
      !!part && !!receiptOnly && (receiptOnly === true || receiptOnly !== part)
    );
  };
  unicode(json, '$');
  let value: unknown;
  try {
    value = JSON.parse(json);
  } catch {
    invalid('$', 'Supply valid JSON without comments or trailing commas.');
  }
  // JSON.parse owns grammar validation. This token scan only preserves object-key
  // information that JSON.parse discards, including escaped-equivalent names.
  const tokens =
    /"(?:[^"\\]|\\[\s\S])*"|[{}[\]:,]|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?/g;
  const stack: Array<{
    path: string;
    keys?: Set<string>;
    key?: boolean;
    property?: string;
    index: number;
  }> = [];
  let token: RegExpExecArray;
  while ((token = tokens.exec(json)) !== null) {
    const current = stack[stack.length - 1];
    const path = !current
      ? '$'
      : current.keys
      ? `${current.path}.${current.property}`
      : `${current.path}[${current.index}]`;
    const item = token[0];
    const stringValue: string = item[0] === '"' ? JSON.parse(item) : undefined;
    const independent = skip(path);
    if (stringValue !== undefined && !independent) {
      unicode(stringValue, current?.key ? current.path : path);
    }
    if ((item === '{' || item === '[') && stack.length >= 64) {
      invalid(path, 'Limit JSON nesting to 64 objects and arrays.');
    }
    if (
      !independent &&
      /^-?\d/.test(item) &&
      Number.isInteger(Number(item)) &&
      !exactInteger(item)
    ) {
      invalid(
        path,
        'Supply numeric values that do not round a fractional value into an integer.'
      );
    }
    if (item === '{')
      stack.push({ path, keys: new Set<string>(), key: true, index: 0 });
    else if (item === '[') stack.push({ path, index: 0 });
    else if (item === '}' || item === ']') stack.pop();
    else if (item === ',' && current) {
      if (current.keys) current.key = true;
      else current.index++;
    } else if (item === ':' && current) current.key = false;
    else if (item[0] === '"' && current?.key) {
      const key = stringValue;
      if (current.keys.has(key) && !skip(current.path))
        invalid(
          `${current.path}.${key}`,
          'Remove the duplicate JSON property.'
        );
      current.keys.add(key);
      current.property = key;
    }
  }
  return value;
}

function exactInteger(token: string): boolean {
  const parts = /^-?(\d+)(?:\.(\d+))?(?:[eE]([+-]?\d+))?$/.exec(token);
  const fraction = parts[2] ?? '';
  const digits = parts[1] + fraction;
  const fractionalPlaces = fraction.length - Number(parts[3] ?? 0);
  if (fractionalPlaces <= 0 || /^0+$/.test(digits)) return true;
  const trailingZeros = /0*$/.exec(digits)[0].length;
  return trailingZeros >= fractionalPlaces;
}

function unicode(value: string, path: string): void {
  for (let i = 0; i < value.length; i++) {
    const code = value.charCodeAt(i);
    if (code >= 0xd800 && code <= 0xdbff) {
      const next = value.charCodeAt(++i);
      if (!(next >= 0xdc00 && next <= 0xdfff)) {
        invalid(
          path,
          'Supply paired Unicode surrogates; lone high surrogates lose data during decoding.'
        );
      }
    } else if (code >= 0xdc00 && code <= 0xdfff) {
      invalid(
        path,
        'Supply paired Unicode surrogates; lone low surrogates lose data during decoding.'
      );
    }
  }
}

function finiteTree(
  value: unknown,
  path: string,
  ancestors = new Set<object>(),
  depth = 0
): void {
  if (value == null || typeof value !== 'object') return;
  if (depth >= 64)
    invalid(path, 'Limit JSON nesting to 64 objects and arrays.');
  if (ancestors.has(value))
    invalid(path, 'Remove cyclic references from the original request.');
  ancestors.add(value);
  if (Array.isArray(value)) {
    for (let i = 0; i < value.length; i++) {
      finiteTree(value[i], `${path}[${i}]`, ancestors, depth + 1);
    }
  } else {
    for (const [name, entry] of Object.entries(value)) {
      finiteTree(entry, `${path}.${name}`, ancestors, depth + 1);
    }
  }
  ancestors.delete(value);
}

/** Decode the strict save request before mutation is admitted. */
export function decodePushReadRequest(json: string): PushReadRequest {
  const value = parse(json);
  validatePushReadRequest(value);
  return value;
}

/** Preserve the mutation receipt without depending on independent read payloads. */
export function decodePushReadReceipt(
  json: string,
  request: PushReadRequest
): PushReadReceipt {
  const value = parse(json, true);
  validatePushReadReceipt(value, request);
  return { i: value.i, sc: value.sc, push: value.push };
}

/** Strictly decode one independent read part without admitting malformed data in that part. */
export function decodePushReadPart(
  json: string,
  request: PushReadRequest,
  part: 'changed' | 'discovery'
): PushReadResponse {
  const value = parse(json, part) as PushReadResponse;
  validatePushReadReceipt(value, request);
  const selected: PushReadResponse = {
    i: value.i,
    sc: value.sc,
    push: value.push,
    [part]: value[part],
  };
  validatePushReadResponse(
    selected,
    part === 'changed'
      ? { ...request, discover: undefined }
      : { ...request, returnChanged: false }
  );
  return selected;
}

export function decodePushReadResponse(
  json: string,
  request: PushReadRequest
): PushReadResponse {
  const value = parse(json);
  validatePushReadResponse(value, request);
  return value;
}

export function validatePushReadRequest(
  value: unknown
): asserts value is PushReadRequest {
  finiteTree(value, '$request');
  const request = shape(value, '$request', {
    v: oneOf(1),
    i: text,
    sc: scope,
    x: optional(context),
    push: (push, path) =>
      shape(push, path, {
        n: optional(array(pushNew)),
        o: optional(array(pushExisting)),
      }),
    returnChanged: optional(oneOf(true, false)),
    discover: optional(pull),
  }) as unknown as PushReadRequest;
  validateReadRequest(pushReadRequest(request, 'discover', request.discover));
  const newIds = new Set((request.push.n ?? []).map((entry) => entry.w));
  unique(
    (request.push.n ?? []).map((entry) => entry.w),
    '$request.push.n'
  );
  unique(
    (request.push.o ?? []).map((entry) => entry.d),
    '$request.push.o'
  );
  for (const [field, entries] of [
    ['n', request.push.n],
    ['o', request.push.o],
  ] as const) {
    for (const [entryIndex, entry] of (entries ?? []).entries()) {
      const path = `$request.push.${field}[${entryIndex}].r`;
      unique(
        (entry.r ?? []).map((role) => role.t),
        path
      );
      for (const [roleIndex, role] of (entry.r ?? []).entries()) {
        const rolePath = `${path}[${roleIndex}]`;
        if (
          Number(role.u != null) +
            Number(role.c != null) +
            Number(role.a != null || role.r != null) >
          1
        )
          invalid(
            rolePath,
            'Supply one unit, composite, or collection-update alternative.'
          );
        const reference = (item: number, itemPath: string) => {
          integer(
            item,
            itemPath,
            -Number.MAX_SAFE_INTEGER,
            Number.MAX_SAFE_INTEGER
          );
          if (!item || (item < 0 && !newIds.has(item)))
            invalid(
              itemPath,
              'Reference a positive database ID or a negative workspace ID declared by this push.'
            );
        };
        if (role.c != null) reference(role.c, `${rolePath}.c`);
        for (const name of ['a', 'r'] as const)
          if (role[name] != null) {
            unique(role[name], `${rolePath}.${name}`);
            role[name].forEach((item, i) =>
              reference(item, `${rolePath}.${name}[${i}]`)
            );
          }
      }
    }
  }
}

export function validatePushReadReceipt(
  value: unknown,
  request: PushReadRequest
): asserts value is PushReadReceipt {
  validatePushReadRequest(request);
  const response = shape(value, '$response', {
    i: text,
    sc: optional(scope),
    push: pushOutcome,
    changed: () => undefined,
    discovery: () => undefined,
  }) as unknown as PushReadReceipt;
  const submittedNewIds = new Set(
    (request.push.n ?? []).map((entry) => entry.w)
  );
  const diagnosticIds = [
    ...(response.push._a ?? []),
    ...(response.push._m ?? []),
    ...([] as number[]).concat(
      ...(response.push.e ?? []).map((entry) => entry.o ?? [])
    ),
  ];
  if (diagnosticIds.some((entry) => entry < 0 && !submittedNewIds.has(entry)))
    invalid(
      '$response.push',
      'Reference only negative workspace IDs declared by this push.'
    );
  if (response.i !== request.i)
    invalid('$response.i', 'Echo the original request correlation ID.');
  if (response.push.k === 'committed') {
    scope(response.sc, '$response.sc');
    if (scopeKey(response.sc) !== scopeKey(request.sc))
      invalid(
        '$response.sc',
        'Keep the scope admitted for the write in the committed receipt.'
      );
    const expected = (request.push.n ?? []).map((entry) => entry.w);
    const returned = response.push.n.map((entry) => entry.w);
    unique(returned, '$response.push.n');
    unique(
      response.push.n.map((entry) => entry.d),
      '$response.push.n'
    );
    if (
      expected.length !== returned.length ||
      expected.some((entry) => !returned.includes(entry))
    )
      invalid(
        '$response.push.n',
        'Return exactly one mapping for every submitted new object.'
      );
  }
}

export function validatePushReadResponse(
  value: unknown,
  request: PushReadRequest
): asserts value is PushReadResponse {
  validatePushReadReceipt(value, request);
  const response = value as PushReadResponse;
  for (const part of ['changed', 'discovery'] as const) {
    const requested =
      response.push.k === 'committed' &&
      (part === 'changed'
        ? request.returnChanged === true
        : request.discover != null);
    if (requested !== (response[part] != null))
      invalid(
        `$response.${part}`,
        'Return each requested read outcome exactly once after commit, and no other read parts.'
      );
    if (!requested) continue;
    const result = response[part];
    const original = pushReadRequest(
      request,
      part === 'changed' ? 'read' : 'discover',
      part === 'changed' ? null : request.discover
    );
    if (part !== 'changed' || result.k === 'error') {
      validateReadResponse(result, original);
      continue;
    }
    validateAffected(result, original);
  }
}

function validateAffected(result: unknown, original: ReadRequest): void {
  shape(result, '$response.changed', {
    i: text,
    sc: scope,
    k: oneOf('changed'),
    p: array(record),
    s: array(security),
    pm: array(permission),
    u: array(id),
  });
  const changed = result as PushReadChanged;
  const complete = {
    i: changed.i,
    sc: changed.sc,
    k: 'complete',
    l: [],
    c: {},
    o: {},
    p: changed.p,
    s: changed.s,
    pm: changed.pm,
  };
  validateReadResponse(complete, original);
  unique(changed.u, '$response.changed.u');
  const present = new Set(changed.p.map((entry) => entry.i));
  if (changed.u.some((entry) => present.has(entry)))
    invalid(
      '$response.changed.u',
      'An affected ID cannot have both a full record and an unavailable outcome.'
    );
}

function pushReadRequest(
  request: PushReadRequest,
  kind: 'read' | 'discover',
  value?: ReadPull
): ReadRequest {
  return {
    v: request.v,
    i: request.i,
    sc: request.sc,
    x: request.x,
    k: kind,
    p: value ?? { l: [], d: [] },
  };
}

function pushNew(value: unknown, path: string): void {
  shape(value, path, {
    w: (item, itemPath) =>
      integer(item, itemPath, -Number.MAX_SAFE_INTEGER, -1),
    t: text,
    r: optional(array(pushRole)),
  });
}

function pushExisting(value: unknown, path: string): void {
  shape(value, path, { d: id, v: version, r: optional(array(pushRole)) });
}

function pushRole(value: unknown, path: string): void {
  const reference: Check = (item, itemPath) =>
    integer(item, itemPath, -Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER);
  shape(value, path, {
    t: text,
    u: optional(unit),
    c: optional(reference),
    a: optional(array(reference)),
    r: optional(array(reference)),
  });
}

function pushOutcome(value: unknown, path: string, invocation = false): void {
  const outcome = shape(value, path, {
    k: oneOf('committed', 'rejected', 'unknown'),
    ...(invocation
      ? {}
      : {
          n: optional(
            array((entry, entryPath) =>
              shape(entry, entryPath, {
                w: (item, itemPath) =>
                  integer(item, itemPath, -Number.MAX_SAFE_INTEGER, -1),
                d: id,
              })
            )
          ),
        }),
    e: optional(
      array((entry, entryPath) => {
        shape(entry, entryPath, {
          c: text,
          m: text,
          o: optional(array(invocation ? id : pushDiagnosticId)),
          i: optional(index),
        });
        if ((entry as Bag).i != null)
          invalid(
            entryPath + '.i',
            'Mutation diagnostics have no query selection index.'
          );
      })
    ),
    _e: optional(text),
    _v: optional(array(id)),
    _a: optional(array(invocation ? id : pushDiagnosticId)),
    _m: optional(array(invocation ? id : pushDiagnosticId)),
    _d: optional(
      array((entry, entryPath) =>
        shape(entry, entryPath, {
          m: optional((message, messagePath) => {
            if (typeof message !== 'string')
              invalid(
                messagePath,
                'Supply a derivation message string, null, or omit it.'
              );
          }),
          r: array((relation, relationPath) =>
            shape(relation, relationPath, {
              i: (item, itemPath) => {
                integer(
                  item,
                  itemPath,
                  -Number.MAX_SAFE_INTEGER,
                  Number.MAX_SAFE_INTEGER
                );
                if (item === 0)
                  invalid(
                    itemPath,
                    'Use a database or workspace ID, never zero.'
                  );
              },
              r: text,
            })
          ),
        })
      )
    ),
  });
  for (const field of ['_v', '_a', '_m'] as const)
    if (outcome[field] != null)
      unique(outcome[field] as number[], path + '.' + field);
  if (outcome.e != null) {
    if (!(outcome.e as unknown[]).length)
      invalid(path + '.e', 'Supply nonempty diagnostics or omit them.');
    for (const entry of outcome.e as Array<{ o?: number[] }>)
      if (entry.o) unique(entry.o, path + '.e.o');
  }
  if (outcome.k === 'committed') {
    if (
      outcome._e ||
      ['_v', '_a', '_m', '_d'].some(
        (field) => (outcome[field] as unknown[])?.length
      )
    )
      invalid(
        path,
        'A committed outcome cannot also report legacy mutation failures.'
      );
    if (!invocation && outcome.n == null)
      invalid(
        path + '.n',
        'Supply the committed new-ID mappings, using [] when empty.'
      );
  } else {
    if (outcome.n != null)
      invalid(path + '.n', 'Return new-ID mappings only after a known commit.');
    if (
      !outcome._e &&
      !['e', '_v', '_a', '_m', '_d'].some(
        (field) => (outcome[field] as unknown[])?.length
      )
    )
      invalid(
        path,
        'Explain a rejected or unknown mutation outcome with actionable diagnostics.'
      );
  }
}

function pushDiagnosticId(value: unknown, path: string): void {
  integer(value, path, -Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER);
  if (value === 0) invalid(path, 'Use a database or workspace ID, never zero.');
}

/** Capture a protected atomic method batch before any effects are admitted. */
export function decodeInvokeReadRequest(json: string): InvokeReadRequest {
  const value = parse(json);
  validateInvokeReadRequest(value);
  return value;
}
export function validateInvokeReadRequest(
  value: unknown
): asserts value is InvokeReadRequest {
  finiteTree(value, '$request');
  const request = shape(value, '$request', {
    v: oneOf(1),
    i: text,
    sc: scope,
    x: optional(context),
    l: array((entry, path) => shape(entry, path, { i: id, v: id, m: text })),
    refresh: oneOf(true, false),
  }) as unknown as InvokeReadRequest;
  validateReadRequest(invokeReadRequest(request));
  const versions = new Map<number, number>();
  for (const entry of request.l) {
    if (versions.has(entry.i) && versions.get(entry.i) !== entry.v)
      invalid(
        '$request.l',
        'Use the same original expected version for every invocation of a target.'
      );
    versions.set(entry.i, entry.v);
  }
}
/** Decode only the independently trusted receipt, preserving later refresh failures. */
export function decodeInvokeReadReceipt(
  json: string,
  request: InvokeReadRequest
): InvokeReadReceipt {
  const value = parse(json, true);
  validateInvokeReadReceipt(value, request);
  return { i: value.i, sc: value.sc, invoke: value.invoke };
}
export function validateInvokeReadReceipt(
  value: unknown,
  request: InvokeReadRequest
): asserts value is InvokeReadReceipt {
  validateInvokeReadRequest(request);
  const response = shape(value, '$response', {
    i: text,
    sc: optional(scope),
    invoke: (entry, path) => pushOutcome(entry, path, true),
    changed: () => undefined,
  }) as unknown as InvokeReadReceipt;
  if (response.i !== request.i)
    invalid('$response.i', 'Echo the original request correlation ID.');
  if (response.invoke.k === 'committed') scope(response.sc, '$response.sc');
  if (response.sc != null && scopeKey(response.sc) !== scopeKey(request.sc))
    invalid(
      '$response.sc',
      'Return the exact expected population, user, workspace, revision and context.'
    );
}
export function decodeInvokeReadResponse(
  json: string,
  request: InvokeReadRequest
): InvokeReadResponse {
  const value = parse(json);
  validateInvokeReadResponse(value, request);
  return value;
}
export function validateInvokeReadResponse(
  value: unknown,
  request: InvokeReadRequest
): asserts value is InvokeReadResponse {
  validateInvokeReadReceipt(value, request);
  const response = value as InvokeReadResponse;
  const requested = response.invoke.k === 'committed' && request.refresh;
  if (requested !== (response.changed != null))
    invalid(
      '$response.changed',
      'Return affected refresh exactly once when requested after commitment, and no other read parts.'
    );
  if (!requested) return;
  const original = invokeReadRequest(request);
  if (response.changed.k === 'error')
    validateReadResponse(response.changed, original);
  else validateAffected(response.changed, original);
}
function invokeReadRequest(request: InvokeReadRequest): ReadRequest {
  return {
    v: request.v,
    i: request.i,
    sc: request.sc,
    x: request.x,
    k: 'read',
    p: { l: [], d: [] },
  };
}
