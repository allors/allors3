import { MetaData, Multiplicity, Origin } from '@allors/workspace/system';
import { InternalMetaPopulation } from '../internal/InternalMetaPopulation';
import { InternalRelationType } from '../internal/InternalRelationType';

export class Lookup {
  m: Map<number, Multiplicity>;
  o: Map<number, Origin>;
  d: Set<number>;
  r: Set<number>;
  u: Set<number>;
  t: Map<number, string>;

  constructor({ metaObjectByTag }: InternalMetaPopulation, data: MetaData) {
    const r = (tag: number) => metaObjectByTag.get(tag) as InternalRelationType;

    this.m = new Map();
    data.m?.forEach((v, i) => {
      const multiplicity = i == 0 ? Multiplicity.OneToOne : i == 1 ? Multiplicity.OneToMany : Multiplicity.ManyToMany;
      v.forEach((w) => this.m.set(w, multiplicity));
    });

    this.o = new Map();
    data.o?.forEach((v, i) => {
      const origin = i ? Origin.Workspace : Origin.Session;
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
  }
}
