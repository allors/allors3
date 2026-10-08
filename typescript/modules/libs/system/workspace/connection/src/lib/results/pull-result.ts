import { PullResponse } from '@allors/system/common/protocol-json';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { CallResult } from './call-result';

/**
 * What a pull answered, in ids: the named objects, the named collections, the named values
 * and the pool of every object the answer names, whose records the connection holds. The
 * names are kept in upper case so that they compare without regard to case.
 */
export class PullResult extends CallResult {
  private _objects: Map<string, number>;

  private _collections: Map<string, number[]>;

  private _values: Map<string, unknown>;

  private _pool: number[];

  constructor(metaPopulation: MetaPopulation, private readonly pullResponse: PullResponse) {
    super(metaPopulation, pullResponse);
  }

  get objects(): Map<string, number> {
    return (this._objects ??= byName(this.pullResponse.o));
  }

  get collections(): Map<string, number[]> {
    return (this._collections ??= byName(this.pullResponse.c));
  }

  /**
   * The values as the wire delivered them.
   */
  get values(): Map<string, unknown> {
    return (this._values ??= byName(this.pullResponse.v));
  }

  /**
   * The ids of every object the answer names, in objects, collections and includes.
   */
  get pool(): number[] {
    return (this._pool ??= this.pullResponse.p?.map((v) => v.i) ?? []);
  }
}

function byName<T>(from: { [name: string]: T } | null | undefined): Map<string, T> {
  const map = new Map<string, T>();
  if (from != null) {
    for (const name of Object.keys(from)) {
      map.set(name.toUpperCase(), from[name]);
    }
  }

  return map;
}
