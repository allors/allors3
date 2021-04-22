import { IComposite, IMetaPopulation, IMethodType, Origin } from '@allors/workspace/system';

export class MethodType implements IMethodType {
  objectType: IComposite;
  metaPopulation: IMetaPopulation;
  tag: number;
  origin: Origin;
  operandId: string;
}
