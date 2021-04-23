import { IComposite, IMethodType, Origin, MethodTypeData } from '@allors/workspace/system';
import { Composite } from './Composite';
import { MetaPopulation } from './MetaPopulation';

export class MethodType implements IMethodType {
  readonly metaPopulation: MetaPopulation;
  objectType: IComposite;
  tag: number;
  origin: Origin;
  operandId: string;
  name: string;

  constructor(a: Composite, d: MethodTypeData) {
    this.tag = d[0];
    this.metaPopulation = a.metaPopulation;
    this.metaPopulation.onNew(this);
  }
}
