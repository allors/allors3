export class MapMap<k1, k2, v> {
  readonly mapMap: Map<k1, Map<k2, v>>;

  constructor() {
    this.mapMap = new Map();
  }

  has(key1: k1, key2: k2): boolean {
    return this.mapMap.get(key1)?.has(key2) ?? false;
  }

  get(key1: k1, key2: k2): v | undefined {
    return this.mapMap.get(key1)?.get(key2);
  }

  set(key1: k1, key2: k2, value: v | undefined): this {
    let map = this.mapMap.get(key1);

    if (value == null) {
      if (map != null) {
        map.delete(key2);
        if (map.size === 0) {
          this.mapMap.delete(key1);
        }
      }
    } else {
      if (map == null) {
        map = new Map();
        this.mapMap.set(key1, map);
      }

      map.set(key2, value);
    }

    return this;
  }

  remove(key1: k1, key2: k2) {
    const map = this.mapMap.get(key1);
    map?.delete(key2);
    return this;
  }
}
