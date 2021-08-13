import { IValidation } from "../derivation/IValidation";
import { ISession } from "../ISession";

export interface ISessionServices {
  onInit(session: ISession): void;

  derive(): IValidation;
}
