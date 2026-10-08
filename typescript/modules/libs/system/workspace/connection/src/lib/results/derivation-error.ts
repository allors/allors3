import { ResponseDerivationError } from '@allors/system/common/protocol-json';
import { MetaPopulation, RelationType } from '@allors/system/workspace/meta';

/**
 * A role a derivation error names: the object's id and the relation type.
 */
export interface DerivationRole {
  objectId: number;

  relationType: RelationType;
}

/**
 * An error a derivation on the server raised: its message and the roles it names.
 */
export class DerivationError {
  readonly message: string;

  readonly roles: DerivationRole[];

  constructor(metaPopulation: MetaPopulation, error: ResponseDerivationError) {
    this.message = error.m;
    this.roles =
      error.r?.map((v) => ({
        objectId: v.i,
        relationType: metaPopulation.metaObjectByTag.get(v.r) as RelationType,
      })) ?? [];
  }
}
