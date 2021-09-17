import { MetaData } from '@allors/protocol/json/system';
import { Multiplicity, Origin } from '@allors/workspace/meta/system';

export class Lookup {
  o: Map<string, Origin>;
  m: Map<string, Multiplicity>;
  d: Set<string>;
  r: Set<string>;
  u: Set<string>;
  t: Map<string, string>;
  or: Map<string, string[]>;
  ou: Map<string, string[]>;

  constructor(data: MetaData) {
    this.m = new Map();
    data.m?.forEach((v, i) => {
      const multiplicity = i == 0 ? Multiplicity.OneToOne : i == 1 ? Multiplicity.OneToMany : Multiplicity.ManyToMany;
      v.forEach((w) => this.m.set(w, multiplicity));
    });

    this.o = new Map();
    data.o?.forEach((v, i) => {
      const origin = i == 0 ? Origin.Workspace : Origin.Session;
      v.forEach((w) => this.o.set(w, origin));
    });

    this.d = new Set(data.d ?? []);
    this.r = new Set(data.r ?? []);
    this.u = new Set(data.u ?? []);

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

    this.ou = new Map();
    if (data.ou) {
      for (const [classTag, roleTypeTags] of data.or) {
        this.or.set(classTag, roleTypeTags);
      }
    }
  }
}
