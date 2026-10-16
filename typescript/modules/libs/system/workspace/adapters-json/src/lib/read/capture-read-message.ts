/** Own message data without invoking accessors or JSON hooks; scoped units may include Date. */
export function captureReadMessage(
  value: unknown,
  convertDates = false,
  ancestors = new Set<object>()
): unknown {
  if (
    value === null ||
    value === undefined ||
    typeof value === 'string' ||
    typeof value === 'boolean'
  )
    return value;
  if (typeof value === 'number' && Number.isFinite(value)) return value;
  if (convertDates && value instanceof Date)
    return Date.prototype.toISOString.call(value);
  if (typeof value !== 'object')
    throw new Error('Invalid read message data. Supply finite JSON values.');
  if (ancestors.has(value) || ancestors.size >= 64)
    throw new Error(
      'Invalid read message nesting. Supply acyclic JSON data with at most 64 containers.'
    );
  const array = Array.isArray(value);
  const prototype = Object.getPrototypeOf(value);
  if (
    prototype !== (array ? Array.prototype : Object.prototype) &&
    prototype !== null
  )
    throw new Error('Invalid read message object. Supply plain JSON data.');
  ancestors.add(value);
  try {
    const result = array ? [] : Object.create(null);
    for (const key of Reflect.ownKeys(value)) {
      if (array && key === 'length') continue;
      const descriptor = Object.getOwnPropertyDescriptor(value, key);
      if (
        typeof key !== 'string' ||
        !descriptor ||
        !('value' in descriptor) ||
        !descriptor.enumerable
      )
        throw new Error(
          'Invalid read message accessor or hidden property. Supply plain enumerable data properties.'
        );
      if (
        array &&
        (!/^(?:0|[1-9]\d*)$/.test(key) || Number(key) >= value.length)
      )
        throw new Error(
          'Invalid read message array property. Supply only JSON array elements.'
        );
      Object.defineProperty(result, key, {
        value: captureReadMessage(descriptor.value, convertDates, ancestors),
        enumerable: true,
        configurable: true,
        writable: true,
      });
    }
    if (
      array &&
      (result.length !== value.length ||
        Object.keys(result).length !== value.length)
    )
      throw new Error(
        'Invalid sparse read message array. Supply every JSON array element.'
      );
    return result;
  } finally {
    ancestors.delete(value);
  }
}
