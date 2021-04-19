import { IClass, IComposite, IInterface, IMetaObject, IMetaPopulation, IMethodType, IRelationType, IUnit } from '@allors/workspace/system';

export class MetaPopulation implements IMetaPopulation {
  units: IUnit[];
  interfaces: IInterface[];
  classes: IClass[];
  relationTypes: IRelationType[];
  methodTypes: IMethodType[];
  composites: IComposite[];
  find(metaObjectId: string): IMetaObject {
    throw new Error('Method not implemented.');
  }
  findByName(name: string): IComposite {
    throw new Error('Method not implemented.');
  }

}
