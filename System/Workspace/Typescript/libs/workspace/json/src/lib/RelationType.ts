import { IAssociationType, IMetaPopulation, IRelationType, IRoleType, Multiplicity, Origin } from '@allors/workspace/system';

export class RelationType implements IRelationType {
  associationType: IAssociationType;
  roleType: IRoleType;
  multiplicity: Multiplicity;
  isDerived: boolean;
  isSynced: boolean;
  metaPopulation: IMetaPopulation;
  id: string;
  origin: Origin;
  hasDatabaseOrigin: boolean;
  hasWorkspaceOrigin: boolean;
  hasSessionOrigin: boolean;
}
