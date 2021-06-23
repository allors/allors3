import { MethodTypeData } from '@allors/protocol/json/system';
import { Origin } from '@allors/workspace/meta/system';
import { InternalComposite } from './internal/InternalComposite';
import { InternalMetaPopulation } from './internal/InternalMetaPopulation';
import { InternalMethodType } from './internal/InternalMethodType';

export class LazyMethodType implements InternalMethodType {
  readonly isRoleType = false;
  readonly isAssociationType = false;
  readonly isMethodType = true;

  readonly metaPopulation: InternalMetaPopulation;
  readonly tag: number;
  readonly name: string;
  readonly origin = Origin.Database;
  readonly operandTag: number;

  constructor(public objectType: InternalComposite, d: MethodTypeData) {
    this.metaPopulation = objectType.metaPopulation as InternalMetaPopulation;
    this.tag = d[0];
    this.name = d[1];
    this.operandTag = this.tag;
    this.metaPopulation.onNew(this);
  }
}
