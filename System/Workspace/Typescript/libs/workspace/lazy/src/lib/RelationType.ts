import { IAssociationType, IRelationType, IRoleType, Multiplicity, Origin, RelationTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export class RelationType implements IRelationType {
  constructor(public metaPopulation: IMetaPopulationInternals, private data: RelationTypeData){
    metaPopulation.onMetaObject(this);
  }

  associationType: IAssociationType;
  roleType: IRoleType;
  multiplicity: Multiplicity;
  isDerived: boolean;
  isSynced: boolean;
  tag: number;
  origin: Origin;

}
