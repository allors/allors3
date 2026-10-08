import { PushResponse } from '@allors/system/common/protocol-json';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { CallResult } from './call-result';

/**
 * What a push answered: the errors, and for every new object the database id the server
 * gave it.
 */
export class PushResult extends CallResult {
  private _databaseIdByWorkspaceId: Map<number, number>;

  constructor(metaPopulation: MetaPopulation, private readonly pushResponse: PushResponse) {
    super(metaPopulation, pushResponse);
  }

  get databaseIdByWorkspaceId(): Map<number, number> {
    return (this._databaseIdByWorkspaceId ??= new Map(
      this.pushResponse.n?.map((v) => [v.w, v.d]) ?? []
    ));
  }
}
