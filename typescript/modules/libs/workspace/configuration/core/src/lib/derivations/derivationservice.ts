import { IDerivation, IDerivationService, ISession } from "@allors/workspace/domain/system";
import { Derivation } from "./Derivation";
import { Engine } from "./Engine";

export class DerivationService implements IDerivationService {
  constructor(public engine: Engine, public maxCycles: number = 10) {}

  create(session: ISession): IDerivation {
    return new Derivation(session, this.engine, this.maxCycles);
  }
}
