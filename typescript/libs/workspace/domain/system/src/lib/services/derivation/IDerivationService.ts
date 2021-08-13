import { IDerivation } from "../../derivation/IDerivation";
import { ISession } from "../../ISession";

export interface IDerivationService {
  create(session: ISession): IDerivation;
}
