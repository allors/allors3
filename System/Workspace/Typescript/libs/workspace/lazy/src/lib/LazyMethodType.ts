import { Origin, MethodTypeData } from '@allors/workspace/system';
import { InternalComposite } from './internal/InternalComposite';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalMethodType } from './internal/InternalMethodType';

export class LazyMethodType implements InternalMethodType {
  readonly metaPopulation: InternalMetaPopulation;
  objectType: InternalComposite;
  tag: number;
  origin: Origin;
  operandTag: number;
  name: string;

  constructor(a: InternalComposite, d: MethodTypeData) {
    this.tag = d[0];
    this.metaPopulation = a.metaPopulation as InternalMetaPopulation;
    this.metaPopulation.onNew(this);
  }
}
