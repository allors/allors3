import { InvokeResponse } from '@allors/system/common/protocol-json';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { CallResult } from './call-result';

/**
 * What an invoke answered: the errors.
 */
export class InvokeResult extends CallResult {
  constructor(metaPopulation: MetaPopulation, response: InvokeResponse) {
    super(metaPopulation, response);
  }
}
