import { IAssociationType, IMetaPopulation, IRelationType, IRoleType, Multiplicity, Origin, RelationTypeData } from '@allors/workspace/system';
import { Composite } from './Composite';

export class RelationType implements IRelationType {
  readonly metaPopulation: IMetaPopulation;

  associationType: IAssociationType;
  roleType: IRoleType;
  multiplicity: Multiplicity;
  isDerived: boolean;
  isSynced: boolean;
  tag: number;
  origin: Origin;

  constructor(a: Composite, d: RelationTypeData) {
    this.tag = d[0];
    this.metaPopulation = a.metaPopulation;
    this.metaPopulation.metaObjectByTag[this.tag] = this;
  }
}
