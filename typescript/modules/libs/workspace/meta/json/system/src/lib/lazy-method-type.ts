import { MethodTypeData } from '@allors/protocol/json/system';
import { Origin } from '@allors/workspace/meta/system';

import { InternalComposite } from './internal/internal-composite';
import { InternalMetaPopulation } from './internal/internal-meta-population';
import { InternalMethodType } from './internal/internal-method-type';

export class LazyMethodType implements InternalMethodType {
  readonly kind = 'MethodType';
  readonly isRoleType = false;
  readonly isAssociationType = false;
  readonly isMethodType = true;

  readonly metaPopulation: InternalMetaPopulation;
  readonly tag: string;
  readonly name: string;
  readonly origin = Origin.Database;
  readonly operandTag: string;

  constructor(public objectType: InternalComposite, d: MethodTypeData) {
    this.metaPopulation = objectType.metaPopulation as InternalMetaPopulation;
    this.tag = d[0];
    this.name = d[1];
    this.operandTag = this.tag;
    this.metaPopulation.onNew(this);
  }
}
