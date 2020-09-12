export function* map(iterator, mapping) {
  while (true) {
    const result = iterator.next();
    if (result.done) {
      break;
    }
    yield mapping(result.value);
  }
}

export function* filter(iterator, predicate) {
  for (const v of iterator) {
    if (predicate(v)) yield v;
  }
}

export function* except(iterator, predicate) {
  for (const v of iterator) {
    if (!predicate(v)) yield v;
  }
}

export function forEach(iterator, fn): void {
  for (const v of iterator) {
    fn(v);
  }
}

export function some(iterator, predicate): boolean {
  for (const v of iterator) {
    if (predicate(v)) return true;
  }

  return false;
}
