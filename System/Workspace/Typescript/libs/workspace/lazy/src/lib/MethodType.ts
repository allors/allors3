import { Origin, MethodTypeData } from '@allors/workspace/system';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { IMethodTypeInternals } from './Internals/IMethodTypeInternals';

export class MethodType implements IMethodTypeInternals {
  readonly metaPopulation: IMetaPopulationInternals;
  objectType: ICompositeInternals;
  tag: number;
  origin: Origin;
  operandTag: number;
  name: string;

  constructor(a: ICompositeInternals, d: MethodTypeData) {
    this.tag = d[0];
    this.metaPopulation = a.metaPopulation as IMetaPopulationInternals;
    this.metaPopulation.onNew(this);
  }
}
