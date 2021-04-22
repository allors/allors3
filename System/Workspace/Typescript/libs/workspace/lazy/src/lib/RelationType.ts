import { IAssociationType, IMetaPopulation, IRelationType, IRoleType, Multiplicity, Origin } from '@allors/workspace/system';

export class RelationType implements IRelationType {
  associationType: IAssociationType;
  roleType: IRoleType;
  multiplicity: Multiplicity;
  isDerived: boolean;
  isSynced: boolean;
  metaPopulation: IMetaPopulation;
  tag: number;
  origin: Origin;
 
}
