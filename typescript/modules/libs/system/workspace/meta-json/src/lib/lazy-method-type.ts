import { MethodTypeData } from '@allors/protocol/json/system';
import { MethodType, Origin } from '@allors/system/workspace/meta';

import { InternalComposite } from './internal/internal-composite';
import { InternalMetaPopulation } from './internal/internal-meta-population';

export class LazyMethodType implements MethodType {
  readonly _ = {};
  readonly kind = 'MethodType';
  isRoleType = false;
  isAssociationType = false;
  isMethodType = true;

  metaPopulation: InternalMetaPopulation;
  tag: string;
  name: string;
  origin = Origin.Database;
  operandTag: string;

  constructor(public objectType: InternalComposite, d: MethodTypeData) {
    this.metaPopulation = objectType.metaPopulation as InternalMetaPopulation;
    this.tag = d[0];
    this.name = d[1];
    this.operandTag = this.tag;
    this.metaPopulation.onNew(this);
  }
}
