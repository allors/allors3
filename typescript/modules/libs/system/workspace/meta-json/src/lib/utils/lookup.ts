import { MetaData } from '@allors/system/common/protocol-json';
import { Multiplicity, Origin } from '@allors/system/workspace/meta';

export class Lookup {
  o: Map<string, Origin>;
  m: Map<string, Multiplicity>;
  d: Set<string>;
  r: Set<string>;
  t: Map<string, string>;
  or: Map<string, string[]>;
  rel: Set<string>;

  constructor(data: MetaData) {
    this.m = new Map();
    data.m?.forEach((v, i) => {
      const multiplicity =
        i == 0
          ? Multiplicity.OneToOne
          : i == 1
          ? Multiplicity.OneToMany
          : Multiplicity.ManyToMany;
      v.forEach((w) => this.m.set(w, multiplicity));
    });

    this.o = new Map();
    data.o?.forEach((v) => {
      this.o.set(v, Origin.Session);
    });

    this.d = new Set(data.d ?? []);
    this.r = new Set(data.r ?? []);

    this.t = new Map();
    if (data.t) {
      for (const mediaType in data.t) {
        data.t[mediaType].forEach((v) => this.t.set(v, mediaType));
      }
    }

    this.or = new Map();
    if (data.or) {
      for (const [classTag, roleTypeTags] of data.or) {
        this.or.set(classTag, roleTypeTags);
      }
    }

    this.rel = new Set(data.rel ?? []);
  }
}
