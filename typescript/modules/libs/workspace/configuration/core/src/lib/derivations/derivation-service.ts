import { IDerivation, IDerivationService, ISession } from '@allors/workspace/domain/system';
import { Derivation } from './derivation';
import { Engine } from './engine';

export class DerivationService implements IDerivationService {
  constructor(public engine: Engine, public maxCycles: number = 10) {}

  create(session: ISession): IDerivation {
    return new Derivation(session, this.engine, this.maxCycles);
  }
}
