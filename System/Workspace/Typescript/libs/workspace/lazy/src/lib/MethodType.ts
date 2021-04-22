import { IComposite, IMethodType, Origin, MethodTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export class MethodType implements IMethodType {
  constructor(public metaPopulation: IMetaPopulationInternals, private data: MethodTypeData) {
    metaPopulation.onMetaObject(this);
  }
  objectType: IComposite;
  tag: number;
  origin: Origin;
  operandId: string;
  name: string;
}
