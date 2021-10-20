export type IRange<T> = Array<T> | undefined;

export abstract class Ranges<T> {
  abstract compare(x: T, y: T): number;

  private _has(set: Array<T>, value: T): boolean {
    let j = 0;
    let length = set.length;
    while (j < length) {
      const i = (length + j - 1) >> 1;
      const compared = this.compare(value, set[i]);
      if (compared > 0) {
        j = i + 1;
      } else if (compared < 0) {
        length = i;
      } else {
        return true;
      }
    }

    return false;
  }

  private *_add(set: Array<T>, value: T) {
    let inserted = false;

    for (const current of set) {
      if (!inserted && this.compare(value, current) < 0) {
        inserted = true;
        yield value;
      }
      yield current;
    }

    if (!inserted) {
      yield value;
    }
  }

  private *_remove(set: Array<T>, value: T) {
    for (const current of set) {
      if (this.compare(current, value) != 0) {
        yield current;
      }
    }
  }

  importFrom(set?: T[] | ReadonlyArray<T>): IRange<T> {
    if (Array.isArray(set)) {
      return [...set].sort(this.compare);
    }

    return undefined;
  }

  *enumerate<T>(set: IRange<T>) {
    if (Array.isArray(set)) {
      return yield* set;
    } else if (set != null) {
      return yield set;
    }
  }

  save<T>(set: IRange<T>) {
    if (Array.isArray(set)) {
      if (set.length > 0) {
        return set;
      }
    } else if (set != null) {
      return [set];
    }

    return null;
  }

  has(set: IRange<T>, value: T): boolean {
    if (set == null) {
      return false;
    }

    if (set.length == 1) {
      return this.compare(set[0], value) === 0;
    }

    return this._has(set, value);
  }

  equals(self: IRange<T>, other: IRange<T>): boolean {
    if (self == null) {
      return other == null;
    }

    if (other == null) {
      return false;
    }

    if (self.length != other.length) {
      return false;
    }

    for (let i = 0; i < self.length; i++) {
      if (this.compare(self[i], other[i]) !== 0) {
        return false;
      }
    }

    return true;
  }

  add(set: IRange<T>, value: T): Exclude<IRange<T>, undefined> {
    if (!Array.isArray(set)) {
      return [value];
    }

    if (!this._has(set, value)) {
      return Array.from(this._add(set, value));
    }

    return set;
  }

  remove(set: IRange<T>, value: T): IRange<T> {
    if (Array.isArray(set) && this._has(set, value)) {
      if (set.length === 1) {
        return undefined;
      }

      return Array.from(this._remove(set, value));
    }

    return set;
  }

  difference(self: IRange<T>, other: IRange<T>): IRange<T> {
    if (self == null || other == null) {
      return self;
    }

    const diff = self.filter((v) => !this._has(other, v));
    return diff.length > 0 ? diff : undefined;
  }

  properSubset(self: IRange<T>, other: IRange<T>): boolean {
    if (other == null) {
      return false;
    }

    if (self == null) {
      return true;
    }

    if (self.length >= other.length) {
      return false;
    }

    for (const element of self) {
      if (!this._has(other, element)) {
        return false;
      }
    }

    return true;
  }
}
