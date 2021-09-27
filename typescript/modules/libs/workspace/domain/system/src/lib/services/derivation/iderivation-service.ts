import { IDerivation } from "../../derivation/iderivation";
import { ISession } from "../../isession";

export interface IDerivationService {
  create(session: ISession): IDerivation;
}
